using Stockfish.NET;

namespace CHESSPROJ.Services
{
    public class StockfishService
    {
        private readonly IStockfish _stockfish;

        public StockfishService(string stockfishPath)
        {
           _stockfish = new Stockfish.NET.Stockfish(stockfishPath);
        }

        public void SetPosition(string movesMade, string move) 
        {
            _stockfish.SetPosition(movesMade, move);
        }

        public string GetBestMove()
        {
            return _stockfish.GetBestMove();
        }

        public string GetEvaluation()
        {
            var evaluation = _stockfish.GetEvaluation();
            return evaluation.ToString(); //idk how to get eval here

            //it outputs Stockfish.NET.Models.Evaluation"???
        }



        public bool IsMoveCorrect(string currentPosition, string move)
        {
            
            _stockfish.SetPosition(currentPosition);
            return _stockfish.IsMoveCorrect(move);
        }

    }
}
