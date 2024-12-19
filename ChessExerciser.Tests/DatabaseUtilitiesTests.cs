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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ChessExerciser.Tests
{
    public class DatabaseUtilitiesTests
    {
        private readonly ChessDbContext _context;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly DatabaseUtilities _databaseUtilities;

        public DatabaseUtilitiesTests()
        {
            // Setup a unique in-memory database for each test run for isolation
            var options = new DbContextOptionsBuilder<ChessDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new ChessDbContext(options);

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
            var newGame = new Game
            {
                GameId = Guid.NewGuid(),
                UserId = "testUser",
                IsRunning = true
            };

            var result = await _databaseUtilities.AddGame(newGame);

            Assert.True(result);
            Assert.Equal(1, await _context.Games.CountAsync());
        }

        [Fact]
        public async Task AddGame_GameAlreadyExists_ShouldReturnFalse()
        {
            var existingGameId = Guid.NewGuid();
            var existingGame = new Game { GameId = existingGameId, UserId = "testUser" };
            await _context.Games.AddAsync(existingGame);
            await _context.SaveChangesAsync();

            var newGame = new Game { GameId = existingGameId, UserId = "anotherUser" };

            var result = await _databaseUtilities.AddGame(newGame);

            Assert.False(result);
            Assert.Equal(1, await _context.Games.CountAsync());
        }

        [Fact]
        public async Task GetGameById_GameExists_ShouldReturnGame()
        {
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser" };
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();

            var retrievedGame = await _databaseUtilities.GetGameById(gameId.ToString());

            Assert.NotNull(retrievedGame);
            Assert.Equal(gameId, retrievedGame.GameId);
        }

        [Fact]
        public async Task GetGameById_GameDoesNotExist_ShouldReturnNull()
        {
            var retrievedGame = await _databaseUtilities.GetGameById(Guid.NewGuid().ToString());
            Assert.Null(retrievedGame);
        }

        [Fact]
        public async Task LogInUser_ValidCredentials_ShouldReturnTrue()
        {
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

            var result = await _databaseUtilities.LogInUser(loginModel);

            Assert.True(result);
        }

        [Fact]
        public async Task LogInUser_InvalidCredentials_ShouldReturnFalse()
        {
            var loginModel = new LoginViewModel { Email = "unknown@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((User)null);

            var result = await _databaseUtilities.LogInUser(loginModel);

            Assert.False(result);
        }

        [Fact]
        public async Task GetUserByEmail_UserExists_ShouldReturnUser()
        {
            var user = new User { Id = "user123", UserName = "testUser", Email = "test@example.com" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var loginModel = new LoginViewModel { Email = "test@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("test@example.com"))
                .ReturnsAsync(user);

            var retrievedUser = await _databaseUtilities.GetUserByEmail(loginModel);

            Assert.NotNull(retrievedUser);
            Assert.Equal("testUser", retrievedUser.UserName);
        }

        [Fact]
        public async Task GetUserByEmail_UserDoesNotExist_ShouldReturnNull()
        {
            var loginModel = new LoginViewModel { Email = "unknown@example.com", Password = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((User)null);

            var retrievedUser = await _databaseUtilities.GetUserByEmail(loginModel);

            Assert.Null(retrievedUser);
        }

        [Fact]
        public async Task GetGameStateById_StateExists_ShouldReturnState()
        {
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser" };
            var state = new GameState { GameId = gameId, CurrentLives = 3, CurrentBlackout = 2 };
            await _context.Games.AddAsync(game);
            await _context.GameStates.AddAsync(state);
            await _context.SaveChangesAsync();

            var retrievedState = await _databaseUtilities.GetStateById(gameId.ToString());

            Assert.NotNull(retrievedState);
            Assert.Equal(3, retrievedState.CurrentLives);
            Assert.Equal(2, retrievedState.CurrentBlackout);
        }

        [Fact]
        public async Task GetGameStateById_StateNotExists_ShouldReturnNull()
        {
            var retrievedState = await _databaseUtilities.GetStateById(Guid.NewGuid().ToString());
            Assert.Null(retrievedState);
        }

        [Fact]
        public async Task UpdateGame_ExistingGame_ShouldUpdateProperties()
        {
            var gameId = Guid.NewGuid();
            var game = new Game { GameId = gameId, UserId = "testUser", MovesArraySerialized = "[]" };
            var state = new GameState { GameId = gameId, CurrentLives = 3, CurrentBlackout = 2, TurnBlack = false };
            await _context.Games.AddAsync(game);
            await _context.GameStates.AddAsync(state);
            await _context.SaveChangesAsync();

            game.MovesArraySerialized = "[\"e2e4\"]";
            state.TurnBlack = true;
            state.CurrentLives = 1;
            state.CurrentBlackout = 1;

            await _databaseUtilities.UpdateGame(game, state);

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
            await _context.Games.AddAsync(new Game { GameId = Guid.NewGuid(), UserId = "testUser" });
            await _context.Games.AddAsync(new Game { GameId = Guid.NewGuid(), UserId = "testUser2" });
            await _context.SaveChangesAsync();

            var gamesList = await _databaseUtilities.GetGamesList();

            Assert.NotNull(gamesList);
            Assert.Equal(2, gamesList.Count);
        }

        [Fact]
        public async Task GetGamesList_NoGames_ShouldReturnEmptyList()
        {
            var gamesList = await _databaseUtilities.GetGamesList();

            Assert.NotNull(gamesList);
            Assert.Empty(gamesList);
        }

        // Additional Tests for AddUser, FindIfUsernameExists, FindIfEmailExists

        [Fact]
        public async Task AddUser_ValidUser_ShouldReturnTrue()
        {
            var registerModel = new RegisterViewModel
            {
                UserName = "newUser",
                Email = "newUser@example.com",
                Password = "Pass@123",
                ConfirmPassword = "Pass@123"
            };
            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<User, string>((u, p) =>
                {
                    // Manually add the user to the in-memory context since the mock won't do it automatically
                    _context.Users.Add(u);
                    _context.SaveChanges();
                });

            var result = await _databaseUtilities.AddUser(registerModel);

            Assert.True(result);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newUser@example.com");
            Assert.NotNull(user);

            var userStats = await _context.UserStats.FirstOrDefaultAsync(us => us.UserId == user.Id);
            Assert.NotNull(userStats);
            Assert.Equal(500, userStats.Rating);
        }

        [Fact]
        public async Task AddUser_CreateUserFails_ShouldReturnFalse()
        {
            var registerModel = new RegisterViewModel
            {
                UserName = "failUser",
                Email = "failUser@example.com",
                Password = "Pass@123",
                ConfirmPassword = "Pass@123"
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), "Pass@123"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Create failed" }));

            var result = await _databaseUtilities.AddUser(registerModel);

            Assert.False(result);
            Assert.Null(await _context.Users.FirstOrDefaultAsync(u => u.Email == "failUser@example.com"));
            Assert.Equal(0, await _context.UserStats.CountAsync());
        }

        [Fact]
        public async Task FindIfUsernameExists_UserExists_ShouldReturnTrue()
        {
            var user = new User { Id = "userX", UserName = "existingUser", Email = "exist@example.com" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var model = new RegisterViewModel { UserName = "existingUser", Email = "dummy@example.com", Password = "Pass@123", ConfirmPassword = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByNameAsync("existingUser"))
                .ReturnsAsync(user);

            var result = await _databaseUtilities.FindIfUsernameExists(model);
            Assert.True(result);
        }

        [Fact]
        public async Task FindIfUsernameExists_UserNotExists_ShouldReturnFalse()
        {
            var model = new RegisterViewModel { UserName = "nonExistentUser", Email = "dummy@example.com", Password = "Pass@123", ConfirmPassword = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByNameAsync("nonExistentUser"))
                .ReturnsAsync((User)null);

            var result = await _databaseUtilities.FindIfUsernameExists(model);
            Assert.False(result);
        }

        [Fact]
        public async Task FindIfEmailExists_EmailExists_ShouldReturnTrue()
        {
            var user = new User { Id = "userY", UserName = "someUser", Email = "check@example.com" };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var model = new RegisterViewModel { Email = "check@example.com", UserName = "dummy", Password = "Pass@123", ConfirmPassword = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("check@example.com"))
                .ReturnsAsync(user);

            var result = await _databaseUtilities.FindIfEmailExists(model);
            Assert.True(result);
        }

        [Fact]
        public async Task FindIfEmailExists_EmailNotExists_ShouldReturnFalse()
        {
            var model = new RegisterViewModel { Email = "unknown@example.com", UserName = "dummy", Password = "Pass@123", ConfirmPassword = "Pass@123" };

            _userManagerMock
                .Setup(x => x.FindByEmailAsync("unknown@example.com"))
                .ReturnsAsync((User)null);

            var result = await _databaseUtilities.FindIfEmailExists(model);
            Assert.False(result);
        }
    }
}
