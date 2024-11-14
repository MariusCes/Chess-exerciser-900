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

        public void SetPosition(params string[] moves)
        {
            _stockfish.SetPosition(moves);
        }

        public bool IsMoveCorrect(string currentPosition, string move)
        {
            _stockfish.SetPosition(currentPosition); 
            return _stockfish.IsMoveCorrect(move); 
        }

        public string GetFenPosition()
        {
            return _stockfish.GetFenPosition(); 
        }

        public bool IsMoveCorrect(string move)
        {
            return _stockfish.IsMoveCorrect(move);
        }
    }
}