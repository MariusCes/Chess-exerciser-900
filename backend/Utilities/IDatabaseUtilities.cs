using backend.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Utilities
{
    public interface IDatabaseUtilities
    {
        public Task<bool> AddGame(Game newGame);
        public Task AddUser(User newUser);
        public Task<Game?> GetGameById(string gameId);
        public Task UpdateGame(Game game);
        public Task<List<Game>> GetGamesList();

    }
}
