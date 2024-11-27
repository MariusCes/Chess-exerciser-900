using Stockfish.NET;
using backend.Models.Domain;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CHESSPROJ.Utilities
{
    public class DatabaseUtilities : IDatabaseUtilities
    {
        private readonly ChessDbContext dbContext;
        private User tempUser;

        public DatabaseUtilities(ChessDbContext dbContext, UserSingleton userSingleton) 
        {
            this.dbContext = dbContext;
            tempUser = userSingleton.GetUser();
            dbContext.SaveChanges();
        }

        public async Task<bool> AddGame(Game newGame) 
        {
            // dooooont please
            dbContext.Entry(tempUser).State = EntityState.Unchanged;

            newGame.UserId = tempUser.Id;
            newGame.User = tempUser;

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
            // Ensure the entity is tracked by the context
            var existingGame = await dbContext.Games.FirstOrDefaultAsync(g => g.GameId.ToString() == game.GameId.ToString());

            if (existingGame != null)
            {
                // If the entity exists, update its properties manually (or map the changes)
                existingGame.Blackout = game.Blackout;
                existingGame.Lives = game.Lives;
                existingGame.TurnBlack = game.TurnBlack;
                existingGame.MovesArraySerialized = game.MovesArraySerialized;

                // Set the entity as modified if not already tracked
                dbContext.Entry(existingGame).State = EntityState.Modified;

                // Save changes to the database
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Log the inner exception or inspect its details
                    Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
                }
            }
        }



        //dbContext.Games.Update(game); // Ensure the game is updated


        // Retrieve all games as a List<Game>
        public async Task<List<Game>> GetGamesList()
        {
                dbContext.Games.ToQueryString();

            List<Game> gamesList = await dbContext.Games.ToListAsync();

            return gamesList;
        }
    }
}