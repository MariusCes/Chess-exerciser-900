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
using backend.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using backend.Controllers;
using System.Security.Claims;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private static ErrorMessages gameNotFound = ErrorMessages.Game_not_found;
        private static ErrorMessages badMove = ErrorMessages.Move_notation_cannot_be_empty;

        private static ErrorMessages registeringError = ErrorMessages.Bad_request;
        private readonly IStockfishService _stockfishService;
        private readonly IDatabaseUtilities dbUtilities;
        private readonly IJwtService _jwtService;
        private readonly ILogger<ChessController> logger;

        // Dependency Injection through constructor
        public ChessController(IStockfishService stockfishService, IDatabaseUtilities dbUtilities, ILogger<ChessController> logger, IJwtService jwtService)
        {
            _stockfishService = stockfishService;
            this.dbUtilities = dbUtilities;
            this.logger = logger;
            _jwtService = jwtService;
        }

        [Authorize]
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            _stockfishService.SetLevel(req.aiDifficulty);
            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            game.UserId = userId;
            try
            {
                if (await dbUtilities.AddGame(game))
                {
                    return Ok(new { GameId = game.GameId });
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

        [Authorize]
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
        [Authorize]
        [HttpPost("{gameId}/move")]
        public async Task<IActionResult> MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {
            Game game = await dbUtilities.GetGameById(gameId);
            if (game == null)
            {
                return NotFound($"{gameNotFound.ToString()}");
            }

            GameState gameState = await dbUtilities.GetStateById(gameId);

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

                gameState.HandleBlackout();

                game.MovesArraySerialized = JsonSerializer.Serialize(MovesArray);
                await dbUtilities.UpdateGame(game, gameState);

                Console.WriteLine($"Before returning - GameState is null: {game.GameState == null}");
                if (game.GameState != null)
                {
                    Console.WriteLine($"CurrentLives in GameState: {game.GameState.CurrentLives}");
                }
                Console.WriteLine($"Lives property returns: {gameState.CurrentLives}");

                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition, fenPosition, gameState.TurnBlack });
            }
            else
            {
                gameState.CurrentLives--; //minus life
                if (gameState.CurrentLives <= 0)
                {
                    game.IsRunning = false;
                    gameState.CurrentLives = 0; //kad nebutu negative in db
                    gameState.WLD = 0;
                }
                gameState.HandleBlackout();

                await dbUtilities.UpdateGame(game, gameState);

                return Ok(new { wrongMove = true, lives = gameState.CurrentLives, game.IsRunning, gameState.TurnBlack }); // we box here :) (fight club reference)
            }
        }

        [Authorize]
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

        [Authorize]
        [HttpGet("{userId}/games")]
        public async Task<IActionResult> GetUserGames(string userId)
        {
            GamesList gamesList = new GamesList(await dbUtilities.GetGamesList());
            List<Game> userGames = new List<Game>();

            foreach (var game in gamesList)
            {
                if (game.UserId.ToString() == userId)
                {
                    userGames.Add(game);
                }
            }

            return Ok(userGames);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await dbUtilities.AddUser(model))
            {
                return Ok(new { message = "Registration successful" });
            }

            return BadRequest($"{registeringError.ToString()}");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (await dbUtilities.LogInUser(model))
            {
                var user = await dbUtilities.GetUserByEmail(model);
                var token = _jwtService.GenerateToken(user);
                return Ok(new { token, user.UserName, user.Email });
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }
    }
}