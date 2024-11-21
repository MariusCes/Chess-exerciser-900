using Stockfish.NET;
using backend.Models.Domain;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Utilities;
using Microsoft.Extensions.Logging;

namespace CHESSPROJ.Utilities
{
    public class DatabaseUtilities : IDatabaseUtilities
    {
        private readonly ChessDbContext dbContext;
        private readonly ILogger<DatabaseUtilities> logger; 

        public DatabaseUtilities(ChessDbContext dbContext, ILogger<DatabaseUtilities> logger) 
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

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

        public async Task AddUser(User newUser) 
        {
            await dbContext.Users.AddAsync(newUser);
            await dbContext.SaveChangesAsync();
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

        public async Task UpdateGame(Game game) 
        {
            
            await dbContext.SaveChangesAsync();  
        }



        //dbContext.Games.Update(game); // Ensure the game is updated


        // Retrieve all games as a List<Game>
        public async Task<List<Game>> GetGamesList()
        {
            logger.LogInformation("Attempting to retrieve games list");
            logger.LogInformation("Generated SQL Query: {Query}",
                dbContext.Games.ToQueryString());


            List<Game> gamesList = await dbContext.Games.ToListAsync();

            logger.LogInformation("Retrieved {Count} games from database", gamesList.Count);

            return gamesList;
        }
    }
}