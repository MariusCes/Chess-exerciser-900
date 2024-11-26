using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models.Domain;

namespace backend.DTOs
{
    public class GetAllGamesResponseDTO
    {
        public required List<Game> GamesList { get; set; }
    }
}