using Stockfish.NET;
using backend.Models.Domain;
using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Utilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using backend.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CHESSPROJ.Utilities
{
    public class DatabaseUtilities : IDatabaseUtilities
    {
        private readonly ChessDbContext dbContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public DatabaseUtilities(ChessDbContext dbContext, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.dbContext = dbContext;

            _userManager = userManager;
            _signInManager = signInManager;
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

        public async Task<bool> AddUser(RegisterViewModel newUser)
        {
            using var transaction = await dbContext.Database.BeginTransactionAsync();
            try
            {
                // Create the User entity
                var user = new User
                {
                    UserName = newUser.UserName,
                    Email = newUser.Email,
                    ProfileLifespan = DateTime.UtcNow
                };

                // Create the user with Identity
                var result = await _userManager.CreateAsync(user, newUser.Password);

                if (result.Succeeded)
                {
                    // Create associated UserStats
                    var userStats = new UserStats
                    {
                        UserId = user.Id,
                        Rating = 500,
                        GamesPlayed = 0,
                        GamesWon = 0,
                        GamesLost = 0,
                        GamesDrawn = 0
                    };

                    // Add UserStats to context
                    await dbContext.UserStats.AddAsync(userStats);
                    await dbContext.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return true;
                }

                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> LogInUser(LoginViewModel model)
        {
            //can get user as a nullable instance therefore there are two of the same stuff
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return false;
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (result.Succeeded)
            {
                return true;
            }
            else return false;
        }

        public async Task<User> GetUserByEmail(LoginViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            return user;
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

        public async Task<GameState?> GetStateById(string gameId)
        {
            var state = await dbContext.GameStates.FirstOrDefaultAsync(g => g.GameId.ToString() == gameId);
            if (state == null)
            {
                return null;
            }
            else return state;
        }

        public async Task UpdateGame(Game game, GameState gameState)
        {
            // Ensure the entity is tracked by the context
            var existingGame = await dbContext.Games.FirstOrDefaultAsync(g => g.GameId.ToString() == game.GameId.ToString());
            var existingState = await dbContext.GameStates.FirstOrDefaultAsync(g => g.GameId.ToString() == game.GameId.ToString());
            if (existingGame != null)
            {
                // If the entity exists, update its properties manually (or map the changes)
                existingGame.MovesArraySerialized = game.MovesArraySerialized;
                existingState.TurnBlack = gameState.TurnBlack;
                existingState.CurrentLives = gameState.CurrentLives;
                existingState.CurrentBlackout = gameState.CurrentBlackout;
                existingState.WLD = existingState.WLD;
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