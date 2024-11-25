using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using backend.DTOs;
using backend.Models.Domain;
using backend.Errors;
using CHESSPROJ.Utilities;
using System.Text.Json;
using Stockfish.NET;
using backend.Data;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private static ErrorMessages gameNotFound = ErrorMessages.Game_not_found;
        private static ErrorMessages badMove = ErrorMessages.Move_notation_cannot_be_empty;
        private readonly IStockfishService _stockfishService;
        private DatabaseUtilities _dbUtilities;
        private static User demoUser;

        // Dependency Injection through constructor
        public ChessController(IStockfishService stockfishService, ChessDbContext dbContext)
        {
            _stockfishService = stockfishService;
            _dbUtilities = new DatabaseUtilities(dbContext);
            demoUser = new User(Guid.NewGuid(), "BNW", "12amGANG");
            _dbUtilities.AddUser(demoUser);
        }

        //all the creation must be asinc and also game must get difficulty from query, also all the dbContext should be async for ex: dbContext.SaveChanges(); has to be dbContext.SaveChangesAsync();
        // /api/chess/create-game?skillLevel=10 smth like that for harder
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            _stockfishService.SetLevel(req.aiDifficulty); //default set to 5, need to see what level does
            //req.gameDifficulty - nuo kiek iki kiek yra
            //req.aiDifficulty - nuo kiek iki kiek yra

        int blackout = req.gameDifficulty switch
        {
            1 => 2,
            2 => 4,
            3 => 6,
            _ => 3 //jei kazkaip neina, tai duos 3
        };

            
            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);
            

            // add user here. For now its only one (hardcoded)
            game.UserId = demoUser.Id;
            game.User = demoUser; 

            if (await _dbUtilities.AddGame(game)) {
                return Ok(new { GameId = game.GameId });    
            } else {
                return NotFound($"{gameNotFound.ToString()}");        // return "DB error" here
            }
        }

        [HttpGet("{gameId}/history")]
        public async Task<IActionResult> GetMovesHistory(string gameId)
        {
            
            Game game = await _dbUtilities.GetGameById(gameId);
            if (game == null)
                return NotFound("Game not found.");

            var moves = game.MovesArray;
            if (game.MovesArray == null || !game.MovesArray.Any())
            {
                return Ok(new List<string>()); // Return an empty list if there are no moves
            }
            string jsonMoves = JsonSerializer.Serialize(moves);
            MemoryStream memoryStream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(memoryStream))
            {
                writer.Write(jsonMoves);
                writer.Flush();
            }
            memoryStream.Position = 0;
            return new FileStreamResult(memoryStream, "application/json");
        }

        // POST: api/chessgame/{gameId}/move
        [HttpPost("{gameId}/move")]
        public async Task<IActionResult> MakeMove(string gameId, [FromBody] MoveDto moveNotation)       // extractina is JSON post info i MoveDto record'a
        {

            var game = await _dbUtilities.GetGameById(gameId);
            if (game == null)
            {
                return NotFound($"{gameNotFound.ToString()}");
            }

            string move = moveNotation.move;
            // Validate move input
            if (string.IsNullOrEmpty(move))
            {
                return BadRequest($"{badMove.ToString()}");
            }

            string currentPosition = string.Join(" ", game.MovesArray);

            if (_stockfishService.IsMoveCorrect(currentPosition, move))
            {
                _stockfishService.SetPosition(currentPosition, move);
                game.MovesArray.Add(move);
                string botMove = _stockfishService.GetBestMove();
                _stockfishService.SetPosition(string.Join(" ", game.MovesArray), botMove);
                game.MovesArray.Add(botMove);

                string fenPosition = _stockfishService.GetFen();
                currentPosition = string.Join(" ", game.MovesArray);
                game.HandleBlackout();
                _dbUtilities.UpdateGame(game);
                
                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition, fenPosition, game.TurnBlack });
            }
            else
            {
                game.Lives--; //minus life
                
                if (game.Lives <= 0)
                {
                    game.IsRunning = false;
                    game.Lives = 0; //so there are no negative lives in db i hope
                }
                game.HandleBlackout();

                _dbUtilities.UpdateGame(game);
                
                return Ok(new { wrongMove = true, lives = game.Lives, game.IsRunning, game.TurnBlack }); // we box here :) (fight club reference)
            }
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            GamesList games = new GamesList(await _dbUtilities.GetGamesList());
            List<Game> gamesWithMoves = new List<Game>();

            foreach (var game in games.GetCustomEnumerator())
            {
                // custom filtering using IEnumerable
                gamesWithMoves.Add(game);
            }
            return Ok(gamesWithMoves);
        }
    }
}