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

        private async Task<OperationResult<T>> PerformDatabaseOperation<T>(Func<Task<T>> operation) where T : class, new()
        {
            try
            {
                var result = await operation();
                if (result != null)
                {
                    return OperationResult<T>.Success(result);
                }
                else{
                    return OperationResult<T>.Failure($"{gameNotFound}");
                }
            }
            catch (Exception ex)
            {
                    return OperationResult<T>.Failure($"big error: {ex.Message}");
            }
        }


        // /api/chess/create-game?skillLevel=10 smth like that for harder
        [HttpPost("create-game")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameReqDto req)
        {
            _stockfishService.SetLevel(req.aiDifficulty); //default set to 5, need to see what level does

            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);

           //cia testavimui
           /*
            List<string> initialMoves = new List<string>
            {
                "a2a4", "b7b5", "a4b5", "c7c6", "b5c6", "c8b7", "c6b7", "d8a5"
            };

            // Set the initial moves on Stockfish and the game object
            string initialPosition = string.Join(" ", initialMoves);
            _stockfishService.SetPosition(initialPosition, "");

            game.MovesArraySerialized = JsonSerializer.Serialize(initialMoves);
            */
            if (await dbUtilities.AddGame(game))
            {
                return Ok(new { GameId = game.GameId });
            }
            else
            {
                return NotFound($"{gameNotFound.ToString()}");        // return "DB error" here
            }
        }
        

        [HttpGet("{gameId}/history")]
        public async Task<IActionResult> GetMovesHistory(string gameId)
        {
            var result = await PerformDatabaseOperation(async () => await dbUtilities.GetGameById(gameId));

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage); // Handle failure
            }

            var game = result.Value;
            var moves = game.MovesArray ?? new List<string>();
            var response = new GetMovesHistoryResponseDTO
            {
                MovesArray = moves
            };

            return Ok(response); // Handle success
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
                
                
                //cia testing jei mate
                //Win - 1 Lose - 0 Draw - 2

                if(_stockfishService.GetEvalType() == "mate"){
                    if(_stockfishService.GetEvalVal() > 0){
                        //reiskia baltas padare mate
                        game.WLD = 1;
                    }else{
                        //reiskia juodas padare mate
                        game.WLD = 0;
                    }
                game.IsRunning = false;
                //nu jei mate tai game tikrai over
                }
                //ALSO PRIDEJAU PRIE RETURN TA WLD, TAI TURETU GAUTI FRONT TA DALYKA DABAR!!!!!! IR ALSO AR ZAIDIMAS EINA, GAL SIEK TIEK PAKEIST


                return Ok(new { //refractor kad ez skaityt
                    wrongMove = false,
                    botMove,
                    currentPosition = currentPosition,
                    fenPosition,
                    game.TurnBlack,
                    game.WLD,
                    game.IsRunning
                    }); 
            }
            else
            {
                game.Lives--; //minus life
                if(game.Lives <= 0){
                    game.IsRunning = false; 
                    game.Lives = 0; //kad nebutu negative in db
                }
                game.HandleBlackout();

                await dbUtilities.UpdateGame(game);

                //refractorinau return kad lengviau skaityt
                return Ok(new {
                wrongMove = true,
                lives = game.Lives,
                game.IsRunning,
                game.TurnBlack
                }); // we box here :) (fight club reference)
            }
        }


        //idk cia kazkas error dabar ne svarbu, turetu veikti naujausiame main
       /* [HttpGet("games")]
        public async Task<IActionResult> GetAllGames()
        {
            var result = await PerformDatabaseOperation(async () => await dbUtilities.GetGamesList());

            if (!result.IsSuccess)
            {
                return NotFound(result.ErrorMessage); // Handle failure
            }

            GamesList games = new GamesList(gamesList);
            List<Game> gamesWithMoves = new List<Game>();

           foreach (var game in games.GetCustomEnumerator())
            {
                // custom filtering using IEnumerable
                gamesWithMoves.Add(game);
            }
            
            return Ok(gamesWithMoves);
        }
        */
    }
}