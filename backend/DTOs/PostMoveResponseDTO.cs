using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class PostMoveResponseDTO
    {
        public Boolean WrongMove { get; set; }
        public string? BotMove { get; set; }
        public string? CurrentPosition { get; set; }
        public string? FenPosition { get; set; }
        public Boolean TurnBlack { get; set; }
        public int Lives { get; set; }
        public Boolean IsRunning { get; set; }
        public int GameWLD { get; set; }
    }
}