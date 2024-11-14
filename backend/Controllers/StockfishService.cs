using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stockfish.NET;
using Stockfish.NET.Models;

namespace CHESSPROJ.Controllers
{
    public class StockfishService : IStockfishService
    {

        private readonly IStockfish _stockfish;

        // Constructor injection
        public StockfishService(IStockfish stockfish)
        {
            _stockfish = stockfish ?? throw new ArgumentNullException(nameof(stockfish));
        }

        public void SetLevel(int level)
        {
            _stockfish.SkillLevel = level; // Assuming SetSkillLevel is a method in Stockfish
        }

        public string GetBestMove()
        {
            return _stockfish.GetBestMove(); // Get the best move from Stockfish
        }

        public void SetPosition(params string[] moves)
        {
            _stockfish.SetPosition(moves);
        }

        public bool IsMoveCorrect(string currentPosition, string move)
        {
            _stockfish.SetPosition(currentPosition); // Set the current position
            return _stockfish.IsMoveCorrect(move); // Validate if the move is correct
        }

        public string GetFenPosition()
        {
            return _stockfish.GetFenPosition(); // Get the FEN string for the current position
        }

        public bool IsMoveCorrect(string move)
        {
            return _stockfish.IsMoveCorrect(move);
        }
    }
}