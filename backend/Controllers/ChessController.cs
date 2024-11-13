using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;
using backend.DTOs;
using backend.Models.Domain;
using System;
using System.Collections.Generic;
using backend.Errors;
using System.Text.Json;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase, IEnumerable<Game>
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


        // /api/chess/create-game?skillLevel=10 smth like that for harder
        [HttpGet("create-game")]
        public IActionResult CreateGame([FromQuery] int SkillLevel = 5) 
        {
            _stockfishService.SetLevel(SkillLevel); //default set to 5, need to see what level does
            Game game = new Game(Guid.NewGuid(), 1, 1);
            game.MovesArray = new List<string>();
            game.Lives = 3;
            game.Blackout = 3;
            game.IsRunning = true;
            games.Add(game);
            return Ok(new { GameId = game.GameId });
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
                if(game.Blackout == 0)
                {
                    game.TurnBlack = true;
                    game.Blackout = 3;
                }else{
                    game.TurnBlack = false;
                }
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
                if(game.Blackout == 0)
                {
                    game.TurnBlack = true;
                    game.Blackout = 3;
                }else{
                    game.TurnBlack = false;
                }
                return Ok(new { wrongMove = true, lives = game.Lives, game.IsRunning, game.TurnBlack });
            }
        }

        // Return the list of games
        [HttpGet("games")]
        public IActionResult GetAllGames()
        {
            return Ok(games);
        }

        public IEnumerator<Game> GetEnumerator()
        {
            return games.GetEnumerator(); // Use List<Game>'s built-in enumerator
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
