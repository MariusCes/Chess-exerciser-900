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
        private readonly ILogger<ChessController> logger;

        // Dependency Injection through constructor
        public ChessController(IStockfishService stockfishService, IDatabaseUtilities dbUtilities, ILogger<ChessController> logger)
        {
            _stockfishService = stockfishService;
            this.dbUtilities = dbUtilities;
            this.logger = logger;
        }

        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            _stockfishService.SetLevel(req.aiDifficulty);
            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);

            try
            {
                if (await dbUtilities.AddGame(game))
                {
                    var PostCreateGameResponseDTO = new PostCreateGameResponseDTO {
                    GameId = game.GameId.ToString()
                };
                    return Ok(PostCreateGameResponseDTO);
                }
                else
                {
                    throw new DatabaseOperationException("Failed to add the game to the database.");
                }
            }
            catch (DatabaseOperationException ex)
            {
                logger.LogError(ex, "error while adding game to database {message}", ex.Message);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{gameId}/history")]
        public async Task<IActionResult> GetMovesHistory(string gameId)
        {
            Game game = await dbUtilities.GetGameById(gameId);
            if (game == null)
                return NotFound("Game not found.");

            List<string> moves = new List<string>();
            if (game.MovesArraySerialized != null)
            {
                moves = JsonSerializer.Deserialize<List<string>>(game.MovesArraySerialized);
            }

            var response = new GetMovesHistoryResponseDTO 
            {
                MovesArray = moves
            };
            
            return Ok(response);
}


        // POST: api/chessgame/{gameId}/move
        [HttpPost("{gameId}/move")]
        public async Task<IActionResult> MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {
            Game game = await dbUtilities.GetGameById(gameId);
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

            List<string> MovesArray = new List<string>();

            if (game.MovesArraySerialized != null)
            {
                MovesArray = JsonSerializer.Deserialize<List<string>>(game.MovesArraySerialized);
            }
            string currentPosition = string.Join(" ", MovesArray);

            if (_stockfishService.IsMoveCorrect(currentPosition, move) && game.IsRunning) //kad nebtuu kokiu shenaningans
            {
                _stockfishService.SetPosition(currentPosition, move);
                MovesArray.Add(move);
                string botMove = _stockfishService.GetBestMove();
                _stockfishService.SetPosition(string.Join(" ", MovesArray), botMove);
                MovesArray.Add(botMove);
                string fenPosition = _stockfishService.GetFen();
                currentPosition = string.Join(" ", MovesArray);

                game.HandleBlackout();

                game.MovesArraySerialized = JsonSerializer.Serialize(MovesArray);
                await dbUtilities.UpdateGame(game);
                
                var postMoveResponseDTO = new PostMoveResponseDTO {
                    WrongMove = false,
                    BotMove = botMove,
                    CurrentPosition = currentPosition,
                    FenPosition = fenPosition,
                    TurnBlack = game.TurnBlack
                };

                return Ok(postMoveResponseDTO);
            }
            else
            {
                game.Lives--; //minus life
                if(game.Lives <= 0){
                    game.IsRunning = false; 
                    game.Lives = 0; //kad nebutu negative in db
                    game.WLD = 0;
                }
                game.HandleBlackout();

                await dbUtilities.UpdateGame(game);
                
                var postMoveResponseDTO = new PostMoveResponseDTO {
                    WrongMove = true,
                    Lives = game.Lives,
                    IsRunning = game.IsRunning,
                    TurnBlack = game.TurnBlack
                };
                
                return Ok(postMoveResponseDTO); // we box here :) (fight club reference)
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
            var getAllGamesResponseDTO = new GetAllGamesResponseDTO {
                GamesList = gamesWithMoves
            };
            return Ok(getAllGamesResponseDTO);
            
        }

        [HttpGet("{userId}/games")]
        public async Task<IActionResult> GetUserGames(string userId)
        {
            GamesList gamesList = new GamesList(await dbUtilities.GetGamesList());
            List<Game> userGames = new List<Game>();

            foreach(var game in gamesList)
            {
                if (game.UserId.ToString() == userId)
                {
                    userGames.Add(game);
                }
            }

            return Ok(userGames);
        }
    }
}