using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;
using backend.Data;
using backend.Models.Domain;

public class ChessDbContextTests
{
    private ChessDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ChessDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
            .Options;

        return new ChessDbContext(options);
    }

    [Fact]
    public void CanAddAndRetrieveUser()
    {
        // Arrange
        using var context = CreateDbContext();
        var user = new User
        {
            Id = "user-1",
            UserName = "TestUser",
            Email = "testuser@example.com",
            ProfileLifespan = DateTime.UtcNow
        };

        // Act
        context.Users.Add(user);
        context.SaveChanges();

        var retrievedUser = context.Users.FirstOrDefault(u => u.Id == "user-1");

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal("TestUser", retrievedUser.UserName);
        Assert.Equal("testuser@example.com", retrievedUser.Email);
    }

    [Fact]
    public void CanAddAndRetrieveGame()
    {
        // Arrange
        using var context = CreateDbContext();
        var user = new User { Id = "user-1", UserName = "TestUser" };
        context.Users.Add(user);
        context.SaveChanges();

        var game = new Game
        {
            GameId = Guid.NewGuid(),
            UserId = "user-1",
            MovesArraySerialized = "[\"e2e4\", \"e7e5\"]",
            IsRunning = true,
            Duration = TimeSpan.FromMinutes(10)
        };

        // Act
        context.Games.Add(game);
        context.SaveChanges();

        var retrievedGame = context.Games.Include(g => g.User).FirstOrDefault(g => g.GameId == game.GameId);

        // Assert
        Assert.NotNull(retrievedGame);
        Assert.Equal(user.Id, retrievedGame.UserId);
        Assert.Equal("[\"e2e4\", \"e7e5\"]", retrievedGame.MovesArraySerialized);
        Assert.True(retrievedGame.IsRunning);
    }

    [Fact]
    public void GameCascadeDeletesWhenUserDeleted()
    {
        // Arrange
        using var context = CreateDbContext();
        var user = new User { Id = "user-1", UserName = "TestUser" };
        context.Users.Add(user);
        context.SaveChanges();

        var game = new Game
        {
            GameId = Guid.NewGuid(),
            UserId = "user-1",
            MovesArraySerialized = "[\"e2e4\"]",
            IsRunning = true
        };
        context.Games.Add(game);
        context.SaveChanges();

        // Act
        context.Users.Remove(user);
        context.SaveChanges();

        var remainingGames = context.Games.Any(g => g.GameId == game.GameId);

        // Assert
        Assert.False(remainingGames); // Game should be deleted when user is deleted
    }

    [Fact]
    public void UserStatsDefaultRatingIs800()
    {
        // Arrange
        using var context = CreateDbContext();
        var userStats = new UserStats { UserId = "user-1" };

        // Act
        context.UserStats.Add(userStats);
        context.SaveChanges();

        var retrievedStats = context.UserStats.FirstOrDefault(us => us.UserId == "user-1");

        // Assert
        Assert.NotNull(retrievedStats);
        Assert.Equal(0, retrievedStats.Rating); // Default value for Rating
    }

    [Fact]
    public void GameStateAndConfigurationAreLinkedToGame()
    {
        // Arrange
        using var context = CreateDbContext();
        var game = new Game { GameId = Guid.NewGuid(), UserId = "user-1", IsRunning = true };
        var gameState = new GameState { GameId = game.GameId, CurrentLives = 3 };
        var gameConfig = new GameConfiguration { GameId = game.GameId, Difficulty = 2 };

        context.Games.Add(game);
        context.GameStates.Add(gameState);
        context.GameConfigurations.Add(gameConfig);
        context.SaveChanges();

        // Act
        var retrievedGame = context.Games
            .Include(g => g.GameState)
            .Include(g => g.GameConfiguration)
            .FirstOrDefault(g => g.GameId == game.GameId);

        // Assert
        Assert.NotNull(retrievedGame);
        Assert.NotNull(retrievedGame.GameState);
        Assert.NotNull(retrievedGame.GameConfiguration);
        Assert.Equal(3, retrievedGame.GameState.CurrentLives);
        Assert.Equal(2, retrievedGame.GameConfiguration.Difficulty);
    }
}
