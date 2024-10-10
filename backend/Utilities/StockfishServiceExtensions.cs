using Stockfish.NET;

namespace CHESSPROJ.StockfishServiceExtensions
{
    public static class StockfishExtensions
    {
        public static bool IsCheck(this IStockfish stockfish)
        {
            stockfish.SetPosition(stockfish.GetFenPosition());
            stockfish.Depth = 1; // Analyze position
                                 //return stockfish. Contains("check");   // Check for "check" in result

            return false;
        }
    }
}

