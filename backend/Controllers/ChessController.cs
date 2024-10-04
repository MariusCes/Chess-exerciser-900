using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;
using backend.DTOs;
using backend.Models.Domain;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly StockfishService _stockfishService;
        private static List<Game> games = new List<Game>();

        private static string currentPOS = "";
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
            game.Lives = 3;
            games.Add(game);
            return Ok(new { GameId = game.Id}); 
        }
        // GET: api/chess/{gameId}/moves
        [HttpGet("{gameId}/moves")]
        public IActionResult GetMovesHistory(string gameId)
        {
            var game = games.FirstOrDefault(g => g.Id.ToString() == gameId);
            if (game == null)
            {
                return NotFound("Game not found.");
            }
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
        public IActionResult MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {

            Game game = null;

            foreach (var g in games)
{
                if (g.Id.ToString() == gameId) 
                {
                    game = g; 
                    break;
                }
            }
            if (game == null)
            {
                return NotFound("Game not found.");
            }               

            string move = moveNotation.move;
            // Validate move input
            if (string.IsNullOrEmpty(move))
            {
                return BadRequest("Move notation cannot be empty.");
            }

            string currentPosition = string.Join(" ", game.MovesArray);

            if (_stockfishService.IsMoveCorrect(currentPosition, move)) 
            {
                _stockfishService.SetPosition(currentPosition, move);
                game.MovesArray.Add(move);
                string botMove = _stockfishService.GetBestMove();
                _stockfishService.SetPosition(string.Join(" ", game.MovesArray), botMove);
                game.MovesArray.Add(botMove);

                currentPosition = string.Join(" ", game.MovesArray);

                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition });
            }
            else
            {
                game.Lives--; //minus life
                return Ok(new { wrongMove = true, lives = game.Lives });
            }
        }
    
    }
}
