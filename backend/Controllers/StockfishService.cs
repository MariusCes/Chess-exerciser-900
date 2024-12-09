using CHESSPROJ.Controllers;
using CHESSPROJ.StockfishServiceExtensions;
using Stockfish.NET;

namespace CHESSPROJ.Services
{


    public class StockfishService : IStockfishService
    {
        private readonly IStockfish _stockfish;

        public StockfishService(IStockfish stockfish)
        {
              _stockfish = stockfish ?? throw new ArgumentNullException(nameof(stockfish));

        }
        public void SetLevel(int level)
        {
            _stockfish.SkillLevel = level;
            _stockfish.Depth = 3;
            
        }

        public void SetPosition(string movesMade, string move)
        {
            _stockfish.SetPosition(movesMade, move);
        }

        public string GetFen() 
        {
            try
            {
                return _stockfish.GetFenPosition(); // Return the FEN position if successful
            }
            catch (Exception ex)
            {
                return "Error getting the FEN" + ex;
            }
        }


        public string GetBestMove()
        {
            return _stockfish.GetBestMove();
        }

        public bool IsMoveCorrect(string currentPosition, string move)
        {

            _stockfish.SetPosition(currentPosition);
            return _stockfish.IsMoveCorrect(move);
        }

        public bool IsCheck()
        {
            return _stockfish.IsCheck();
        }

    }
}
