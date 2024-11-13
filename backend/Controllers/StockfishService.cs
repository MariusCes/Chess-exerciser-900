using Stockfish.NET;

namespace CHESSPROJ.Services
{
    public class StockfishService
    {
        private readonly IStockfish _stockfish;

        public StockfishService(string stockfishPath) //if no parameter used, level will be 5
        {
           _stockfish = new Stockfish.NET.Stockfish(stockfishPath);

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
                return "Error getting the FEN";
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

    }
}
