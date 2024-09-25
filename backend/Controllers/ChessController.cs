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
        private string currentPOS = "";

        public ChessController(IConfiguration configuration)
        {
            var stockfishPath = configuration["StockfishPath"];
            _stockfishService = new StockfishService(stockfishPath);
        }
 

        [HttpPost("analyze-move")] //http://localhost:5030/api/chess/analyze-move
        public IActionResult AnalyzeMove([FromBody] string move)
        {


            //Some function to have all the moves done in the game
            //example: "e2e4 e7e5 Ng1f3" e2-piece position, e4-where piece will move

           //currentPOS has the board with moves done; move has the move user made;
           //_stockfishService.SetPosition(currentPos, move);

           //EXAMPLE
           _stockfishService.SetPosition(currentPOS, move); //this would be for the user
            currentPOS += $" {move}";


            var bestMove = _stockfishService.GetBestMove(); //gets best move for bot, maybe can be not the best move

            _stockfishService.SetPosition(currentPOS.Trim(), bestMove); //this would be for the bot, it would make the best move
            currentPOS += $" {bestMove}";

            bestMove = _stockfishService.GetBestMove(); 

            var evaluation = _stockfishService.GetEvaluation();
            Console.WriteLine(currentPOS);
            
            //PROGRAM ONLY WORKS FOR THE FIRST MOVE AND THAT IS IT FOR NOW!

            return Ok(new { BestMove = bestMove, Evaluation = evaluation });
        }

    }
}
