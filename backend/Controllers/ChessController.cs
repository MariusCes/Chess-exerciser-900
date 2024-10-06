using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;
using backend.DTOs;
using backend.Models.Domain;
using System;
using System.Collections.Generic;

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

            if (_stockfishService.IsMoveCorrect(currentPosition, move)) // in game logic need to validate moveNotation.move if its a good move (FUNCTION FOR IGNAS)
            {
                //player move
                _stockfishService.SetPosition(currentPosition, move);
                game.MovesArray.Add(move); //add the move done

                //bot move
                string botMove = _stockfishService.GetBestMove();
                _stockfishService.SetPosition(string.Join(" ", game.MovesArray), botMove);
                game.MovesArray.Add(botMove);

                currentPosition = string.Join(" ", game.MovesArray);

                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition });
           

                //string botMove; //= Process the move via a service that handles game logic (FUNCTION FOR IGNAS)
            }
            else
            {
                game.Lives--; //minus life
                return Ok(new { wrongMove = true, lives = game.Lives });
            }
        }
    
    }
}
