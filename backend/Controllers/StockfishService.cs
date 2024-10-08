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
