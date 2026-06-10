using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using backend.DTOs;
using backend.Errors;
using backend.Models.ViewModels;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<ChessController> _logger;

        public ChessController(IGameService gameService, ILogger<ChessController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                var result = await _gameService.CreateGameAsync(req, userId);
                return Ok(result);
            }
            catch (DatabaseOperationException ex)
            {
                _logger.LogError(ex, "Error while adding game to database: {Message}", ex.Message);
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("{gameId}/history")]
        public async Task<IActionResult> GetMovesHistory(string gameId)
        {
            var result = await _gameService.GetMovesHistoryAsync(gameId);
            if (result == null)
                return NotFound("Game not found.");
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{gameId}/move")]
        public async Task<IActionResult> MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {
            if (string.IsNullOrEmpty(moveNotation.move))
                return BadRequest(ErrorMessages.Move_notation_cannot_be_empty.ToString());

            var result = await _gameService.MakeMoveAsync(gameId, moveNotation);
            if (result == null)
                return NotFound(ErrorMessages.Game_not_found.ToString());

            return Ok(result);
        }

        [Authorize]
        [HttpGet("games")]
        public async Task<IActionResult> GetUserGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var games = await _gameService.GetUserGamesAsync(userId);
            return Ok(games);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _gameService.RegisterAsync(model);
            if (result.Success)
                return Ok(new { message = "Registration successful" });

            return BadRequest(new { message = result.Error });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var result = await _gameService.LoginAsync(model);
            if (result == null)
                return BadRequest("Invalid credentials");

            return Ok(new { token = result.Token, userName = result.UserName, email = result.Email });
        }
    }
}
