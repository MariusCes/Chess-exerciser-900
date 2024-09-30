using Microsoft.Extensions.Configuration;
using CHESSPROJ.Services;

class ChessConsoleApp
{
    static string currentPOS = "";

    static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var stockfishPath = configuration["StockfishPath"];
        var stockfishService = new StockfishService(stockfishPath);

        while (true)
        {
            var bestMove = stockfishService.GetBestMove();
            Console.WriteLine("'exit' to quit or move:");
            Console.WriteLine($"best move btw: {bestMove} \n");
            string move = Console.ReadLine();
            
            if (move.ToLower() == "exit") break;

           if (!string.IsNullOrEmpty(move))
            {
                //CHECK IF LEGAL!!! (chess refrence)
                if (stockfishService.IsMoveCorrect(currentPOS, move))
                {
                    ProcessMove(stockfishService, move);
                }
                else
                {
                    Console.WriteLine("Illegal move!\n");
                }
            }
        }
    }




    static void ProcessMove(StockfishService stockfishService, string move)
    {
        stockfishService.SetPosition(currentPOS, move);
        currentPOS += $" {move}";
        //what board is after user

        var bestMove = stockfishService.GetBestMove();
        stockfishService.SetPosition(currentPOS.Trim(), bestMove);
        currentPOS += $" {bestMove}";

        var evaluation = stockfishService.GetEvaluation();

        Console.WriteLine($"\nCurrent Position: {currentPOS} \n");
        Console.WriteLine($"Bot's Best Move: {bestMove}");
        //Console.WriteLine($"Evaluation:\n {evaluation}"); NOT WORKING
    }
}
