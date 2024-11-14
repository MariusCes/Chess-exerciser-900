using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using backend.DTOs;
using backend.Models.Domain;
using System;
using System.Collections.Generic;
using backend.Errors;
using System.Text.Json;
using Stockfish.NET;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private static GamesList games = new GamesList(new List<Game>());
        private static ErrorMessages gameNotFound = ErrorMessages.Game_not_found;
        private static ErrorMessages badMove = ErrorMessages.Move_notation_cannot_be_empty;
        private readonly IStockfishService _stockfishService;

        // Dependency Injection through constructor
        public ChessController(IStockfishService stockfishService)
        {
            _stockfishService = stockfishService;
        }


        // /api/chess/create-game?skillLevel=10 smth like that for harder
        [HttpGet("create-game")]
        public IActionResult CreateGame([FromQuery] int SkillLevel = 5) // po kolkas GET req, bet ateityje reikes ir sito
        {
            _stockfishService.SetLevel(SkillLevel); //default set to 5, need to see what level does
            Game game = new Game(Guid.NewGuid(), 1, 1, 3);
            games.Add(game);
            return Ok(new { GameId = game.GameId });
        }

        [HttpGet("{gameId}/history")]
        public IActionResult GetMovesHistory(string gameId)
        {
            var game = games.FirstOrDefault(g => g.GameId.ToString() == gameId);
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
        public IActionResult MakeMove(string gameId, [FromBody] MoveDto moveNotation)       // extractina is JSON post info i MoveDto record'a
        {
            Game game = games.FirstOrDefault(g => g.GameId.ToString() == gameId);
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
            string[] currentPosition = game.MovesArray.ToArray();
            _stockfishService.SetPosition(currentPosition);

            if (_stockfishService.IsMoveCorrect(move))
            {
                game.MovesArray.Add(move);
                currentPosition = game.MovesArray.ToArray();
                _stockfishService.SetPosition(currentPosition);
                string botMove = _stockfishService.GetBestMove();
                game.MovesArray.Add(botMove);
                currentPosition = game.MovesArray.ToArray();
                _stockfishService.SetPosition(currentPosition);


                return Ok(new { wrongMove = false, botMove, currentPosition = currentPosition }); // named args here
            }
            else
            {
                game.Lives--; //minus life
                if (game.Lives == 0)
                {
                    game.IsRunning = false;
                }
                return Ok(new { wrongMove = true, lives = game.Lives, game.IsRunning }); // we box here :) (fight club reference)
            }
        }

        // Return the list of games
        [HttpGet("games")]
        public IActionResult GetAllGames()
        {
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
