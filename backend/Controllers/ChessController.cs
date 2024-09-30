using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;
using backend.DTOs;
using backend.Models.Domain;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly StockfishService _stockfishService;
        private static List<Game> games = new List<Game>();
        public ChessController(IConfiguration configuration)
        {
            var stockfishPath = configuration["StockfishPath"];
            _stockfishService = new StockfishService(stockfishPath);
        }

        [HttpPost("create-game")]
        public IActionResult CreateGame()
        {
            Game game = new Game();
            game.Id = Guid.NewGuid();
            game.MovesArray = new List<string>();
            games.Add(game);
            return Ok(new { GameId = game.Id}); 
        }

        // POST: api/chessgame/{gameId}/move
        [HttpPost("{gameId}/move")]
        public IActionResult MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {
            // Validate move input
            if (string.IsNullOrEmpty(moveNotation.move))
            {
                return BadRequest("Move notation cannot be empty.");
            }

            if (true) // in game logic need to validate moveNotation.move if its a good move (FUNCTION FOR IGNAS)
            {
                Game game;
                //Process the move via a service that handles game logic (FUNCTION FOR IGNAS)

                return Ok(result.GameState);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

    }
}
