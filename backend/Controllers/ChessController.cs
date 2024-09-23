using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;

namespace CHESSPROJ.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChessController : ControllerBase
    {
        private readonly StockfishService _stockfishService;

        public ChessController(IConfiguration configuration)
        {
            var stockfishPath = configuration["StockfishPath"];
            _stockfishService = new StockfishService(stockfishPath);
        }

        [HttpPost("analyze-move")]
        public IActionResult AnalyzeMove([FromBody] string move)
        {

            //string currentPOS = GetCurrentPosition();
            //Some function to have all the moves done in the game
            //example: "e2e4 e7e5 Ng1f3"

           //currentPOS has the board with moves done; move has the move user made;
           //_stockfishService.SetPosition(currentPos, move);

           //EXAMPLE
           _stockfishService.SetPosition("", move); //this would be for the user

            var bestMove = _stockfishService.GetBestMove(); //maybe can be not the best move

            _stockfishService.SetPosition(move, bestMove); //this would be for the bot, it would make the best move

            var evaluation = _stockfishService.GetEvaluation();
            
            //PROGRAM ONLY WORKS FOR THE FIRST MOVE AND THAT IS IT FOR NOW!

            return Ok(new { BestMove = bestMove, Evaluation = evaluation });
        }

    }
}
