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
        public void AddUser(User newUser);
        public Task<Game?> GetGameById(string gameId);
        public void UpdateGame(Game game);
        public Task<List<Game>> GetGamesList();

    }
}
