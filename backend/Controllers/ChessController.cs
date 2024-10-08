using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;
using backend.DTOs;
using backend.Models.Domain;
using System;
using System.Collections.Generic;
using backend.Errors;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly StockfishService _stockfishService;
        private static List<Game> games = new List<Game>();
        private static ErrorMessages gameNotFound = ErrorMessages.Game_not_found;
        private static ErrorMessages badMove = ErrorMessages.Move_notation_cannot_be_empty;

        private static string currentPOS = "";
        public ChessController(IConfiguration configuration)
        {
            var stockfishPath = configuration["StockfishPath"];
            _stockfishService = new StockfishService(stockfishPath);
        }

        [HttpGet("create-game")]
        public IActionResult CreateGame()
        {
            Game game = new Game(Guid.NewGuid(), 1, 1);
            game.MovesArray = new List<string>();
            game.Lives = 3;
            games.Add(game);
            return Ok(new { GameId = game.GameId}); 
        }

        // POST: api/chessgame/{gameId}/move
        [HttpPost("{gameId}/move")]
        public IActionResult MakeMove(string gameId, [FromBody] MoveDto moveNotation)
        {

            Game game = null;

            foreach (var g in games)
            {
                if (g.GameId.ToString() == gameId) 
                {
                    game = g; 
                    break;
                }
            }
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
