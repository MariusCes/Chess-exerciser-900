using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using backend.DTOs;
using backend.Models.Domain;
using backend.Errors;
using CHESSPROJ.Utilities;
using System.Text.Json;
using Stockfish.NET;
using backend.Data;
using backend.Utilities;
using Microsoft.Extensions.Logging;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private static ErrorMessages gameNotFound = ErrorMessages.Game_not_found;
        private static ErrorMessages badMove = ErrorMessages.Move_notation_cannot_be_empty;
        private readonly IStockfishService _stockfishService;
        private readonly IDatabaseUtilities dbUtilities;

        // Dependency Injection through constructor
        public ChessController(IStockfishService stockfishService, IDatabaseUtilities dbUtilities)
        {
            _stockfishService = stockfishService;
            this.dbUtilities = dbUtilities;
            //this.dbUtilities.AddUser(demoUser);  hahahafoasfasokf
        }

        // /api/chess/create-game?skillLevel=10 smth like that for harder
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            _stockfishService.SetLevel(req.aiDifficulty); //default set to 5, need to see what level does

            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);

            if (await dbUtilities.AddGame(game)) {
                return Ok(new { GameId = game.GameId });    
            } else {
                return NotFound($"{gameNotFound.ToString()}");        // return "DB error" here
            }
        }

        [HttpGet("{gameId}/history")]
        public async Task<IActionResult> GetMovesHistory(string gameId)
        {
            
            Game game = await dbUtilities.GetGameById(gameId);
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

            var game = await dbUtilities.GetGameById(gameId);
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
                game.Blackout--;
                if (game.Blackout == 0)
                {
                    game.TurnBlack = true;
                    game.Blackout = 3;
                }
                else
                {
                    game.TurnBlack = false;
                }
                
                await dbUtilities.UpdateGame(game);
                
                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition, fenPosition, game.TurnBlack });
            }
            else
            {
                game.Lives--; //minus life
                game.Blackout--;
                if (game.Lives == 0)
                {
                    game.IsRunning = false;
                }
                if (game.Blackout == 0)
                {
                    game.TurnBlack = true;
                    game.Blackout = 3;
                }
                else
                {
                    game.TurnBlack = false;
                }

                await dbUtilities.UpdateGame(game);
                
                return Ok(new { wrongMove = true, lives = game.Lives, game.IsRunning, game.TurnBlack }); // we box here :) (fight club reference)
            }
        }

        [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            GamesList gamesList = new GamesList(await dbUtilities.GetGamesList());

            GamesList games = new GamesList(gamesList);
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
