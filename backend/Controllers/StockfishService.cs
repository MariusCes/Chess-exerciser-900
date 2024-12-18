using CHESSPROJ.Controllers;
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
            _stockfish.Depth = 1; 
            
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


                //cia pasakys mate, jei bus mate, pretty simple, grazina string
        public string GetEvalType()
        {
                var EvalType = _stockfish.GetEvaluation().Type;
                
                return EvalType;
        }
        //cia pasakys kuri puse padare ta mate, jei minusas, tai juoda, jei pliusas, tai balta
        public int GetEvalVal()
        {
                var EvalValue = _stockfish.GetEvaluation().Value;
                
                return EvalValue;
        }
    }
}
