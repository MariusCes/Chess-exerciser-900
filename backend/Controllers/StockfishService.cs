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
        }

        public void SetPosition(string movesMade, string move) 
        {
            _stockfish.SetPosition(movesMade, move);
        }

        public void GetFen() //in future maybe some way to see mate 
        {
            _stockfish.GetFenPosition();
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
