using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class GetMovesHistoryResponseDTO
    {
        public required List<string> MovesArray { get; set; }
    }
}