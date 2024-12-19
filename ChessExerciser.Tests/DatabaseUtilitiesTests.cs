using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using CHESSPROJ.Utilities;
using backend.Models.Domain;
using Microsoft.AspNetCore.Identity;
using backend.Models.ViewModels;

namespace ChessExerciser.Tests
{
    public class DatabaseUtilitiesTests
    {
        private readonly DbContextOptions<ChessDbContext> _dbOptions;
        private readonly ChessDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly DatabaseUtilities _databaseUtilities;

        public DatabaseUtilitiesTests()
        {
            // Setup in-memory database
            _dbOptions = new DbContextOptionsBuilder<ChessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique DB for each test run
                .Options;

            _context = new ChessDbContext(_dbOptions);
            
            // Setup mocks for UserManager and SignInManager
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, 
                null, null, null, null, null, null, null, null
            );

            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null, null, null, null);

            _databaseUtilities = new DatabaseUtilities(_context, _userManagerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public async Task AddGame_GameDoesNotExist_ShouldAddGame()
        {
            // Arrange
            var newGame = new Game
            {
                GameId = Guid.NewGuid(),
                UserId = "testUser",
                IsRunning = true
            };

            // Act
            var result = await _databaseUtilities.AddGame(newGame);

            // Assert
            Assert.True(result);
            Assert.Equal(1, await _context.Games.CountAsync());
        }

        [Fact]
        public async Task AddGame_GameAlreadyExists_ShouldReturnFalse()
        {
            // Arrange
            var existingGameId = Guid.NewGuid();
            var existingGame = new Game { GameId = existingGameId, UserId = "testUser" };
            await _context.Games.AddAsync(existingGame);
            await _context.SaveChangesAsync();

            var newGame = new Game { GameId = existingGameId, UserId = "anotherUser" };

            // Act
            var result = await _databaseUtilities.AddGame(newGame);

            // Assert
            Assert.False(result);
            Assert.Equal(1, await _context.Games.CountAsync());
        }

        [Fact]
        public async Task GetGameById_GameExists_ShouldReturnGame()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser" };
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            // Act
            var retrievedGame = await _databaseUtilities.GetGameById(gameId.ToString());

            // Assert
            Assert.NotNull(retrievedGame);
            Assert.Equal(gameId, retrievedGame.GameId);
        }

        [Fact]
        public async Task GetGameById_GameDoesNotExist_ShouldReturnNull()
        {
            // Act
            var retrievedGame = await _databaseUtilities.GetGameById(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(retrievedGame);
        }
        
        [Fact]
        public async Task LogInUser_ValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser", Email = "test@example.com" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginModel = new LoginViewModel { Email = "test@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);

            _signInManagerMock
                .Setup(x => x.CheckPasswordSignInAsync(user, "Pass@123", false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _databaseUtilities.LogInUser(loginModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task LogInUser_InvalidCredentials_ShouldReturnFalse()
        {
            // Arrange
            var loginModel = new LoginViewModel { Email = "unknown@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((User)null);

            // Act
            var result = await _databaseUtilities.LogInUser(loginModel);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserByEmail_UserExists_ShouldReturnUser()
        {
            // Arrange
            var user = new User { Id = "user123", UserName = "testUser", Email = "test@example.com" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginModel = new LoginViewModel { Email = "test@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);

            // Act
            var retrievedUser = await _databaseUtilities.GetUserByEmail(loginModel);

            // Assert
            Assert.NotNull(retrievedUser);
            Assert.Equal("testUser", retrievedUser.UserName);
        }

        [Fact]
        public async Task GetUserByEmail_UserDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var loginModel = new LoginViewModel { Email = "unknown@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((User)null);

            // Act
            var retrievedUser = await _databaseUtilities.GetUserByEmail(loginModel);

            // Assert
            Assert.Null(retrievedUser);
        }

        [Fact]
        public async Task GetGameStateById_StateExists_ShouldReturnState()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser" };
            var state = new GameState { GameId = gameId, CurrentLives = 3, CurrentBlackout = 2 };
            await _context.Games.AddAsync(game);
            await _context.GameStates.AddAsync(state);
            await _context.SaveChangesAsync();

            // Act
            var retrievedState = await _databaseUtilities.GetStateById(gameId.ToString());

            // Assert
            Assert.NotNull(retrievedState);
            Assert.Equal(3, retrievedState.CurrentLives);
            Assert.Equal(2, retrievedState.CurrentBlackout);
        }

        [Fact]
        public async Task GetGameStateById_StateNotExists_ShouldReturnNull()
        {
            // Act
            var retrievedState = await _databaseUtilities.GetStateById(Guid.NewGuid().ToString());

            // Assert
            Assert.Null(retrievedState);
        }

        [Fact]
        public async Task UpdateGame_ExistingGame_ShouldUpdateProperties()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser", MovesArraySerialized = "[]" };
            var state = new GameState { GameId = gameId, CurrentLives = 3, CurrentBlackout = 2, TurnBlack = false };
            await _context.Games.AddAsync(game);
            await _context.GameStates.AddAsync(state);
            await _context.SaveChangesAsync();

            // Modify properties
            game.MovesArraySerialized = "[\"e2e4\"]";
            state.TurnBlack = true;
            state.CurrentLives = 1;
            state.CurrentBlackout = 1;

            // Act
            await _databaseUtilities.UpdateGame(game, state);

            // Assert - re-fetch from DB
            var updatedGame = await _context.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
            var updatedState = await _context.GameStates.FirstOrDefaultAsync(gs => gs.GameId == gameId);

            Assert.NotNull(updatedGame);
            Assert.NotNull(updatedState);
            Assert.Equal("[\"e2e4\"]", updatedGame.MovesArraySerialized);
            Assert.True(updatedState.TurnBlack);
            Assert.Equal(1, updatedState.CurrentLives);
            Assert.Equal(1, updatedState.CurrentBlackout);
        }

        [Fact]
        public async Task GetGamesList_WhenGamesExist_ShouldReturnListOfGames()
        {
            // Arrange
            await _context.Games.AddAsync(new Game { GameId = Guid.NewGuid(), UserId = "testUser" });
            await _context.Games.AddAsync(new Game { GameId = Guid.NewGuid(), UserId = "testUser2" });
            await _context.SaveChangesAsync();

            // Act
            var gamesList = await _databaseUtilities.GetGamesList();

            // Assert
            Assert.NotNull(gamesList);
            Assert.Equal(2, gamesList.Count);
        }

        [Fact]
        public async Task GetGamesList_NoGames_ShouldReturnEmptyList()
        {
            // Act
            var gamesList = await _databaseUtilities.GetGamesList();

            // Assert
            Assert.NotNull(gamesList);
            Assert.Empty(gamesList);
        }
    }
}
