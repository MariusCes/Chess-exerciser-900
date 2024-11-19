using Stockfish.NET;
using backend.Models.Domain;
using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace CHESSPROJ.Utilities
{
    public class DatabaseUtilities 
    {
        private readonly ChessDbContext dbContext;

        public DatabaseUtilities(ChessDbContext dbContext) 
        {
            this.dbContext = dbContext;
        }

        //adds a game
        public async Task<bool> AddGame(Game newGame) 
        {
            var game = await GetGameById(newGame.GameId.ToString());
            if (game == null)
            {
                await dbContext.Games.AddAsync(newGame);
                await dbContext.SaveChangesAsync();
                return true;
            }
            else return false;
        }

        public async Task<Game?> GetGameById(string gameId) 
        {
            var game = await dbContext.Games.FirstOrDefaultAsync(g => g.GameId.ToString() == gameId);
            if (game == null)
            {
                return null;
            }
            else return game;
        }

        // update the game in DB.
        public async void UpdateGame(Game game) 
        { 
            await dbContext.SaveChangesAsync();  
        }

        // Retrieve all games as a List<Game>
        public async Task<List<Game>> GetGamesList()
        {
            List<Game> gamesList = await dbContext.Games.ToListAsync();
            return gamesList;
        }
    }
}