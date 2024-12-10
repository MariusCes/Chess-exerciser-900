
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using backend.DTOs;
using backend.Models.Domain;
using backend.Data;
using backend.Utilities;
using FluentAssertions;
using Moq;
using Stockfish.NET;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using CHESSPROJ.Controllers;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace ChessExerciser.Tests;

public class ChessControllerIntegrationTests
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly Mock<IStockfishService> _mockStockfishService;
    private readonly Mock<IDatabaseUtilities> _mockDbUtilities;

    public ChessControllerIntegrationTests()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        _mockStockfishService = new Mock<IStockfishService>();
        _mockDbUtilities = new Mock<IDatabaseUtilities>();

        // Setup mock StockfishService
        _mockStockfishService.Setup(s => s.IsMoveCorrect(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        _mockStockfishService.Setup(s => s.GetBestMove())
            .Returns("e7e5");
        _mockStockfishService.Setup(s => s.GetFen())
            .Returns("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");

        // Setup mock DatabaseUtilities
        _mockDbUtilities.Setup(d => d.AddGame(It.IsAny<Game>()))
            .ReturnsAsync(true);

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Remove existing registrations
                    var descriptorsToRemove = services.Where(
                        d => d.ServiceType == typeof(IStockfishService) ||
                             d.ServiceType == typeof(IDatabaseUtilities) ||
                             d.ServiceType == typeof(DbContextOptions<ChessDbContext>)
                    ).ToList();

                    foreach (var descriptor in descriptorsToRemove)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database
                    services.AddDbContext<ChessDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));

                    // Add mocked services
                    services.AddSingleton(_mockStockfishService.Object);
                    services.AddSingleton(_mockDbUtilities.Object);
                });
            });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnGamesList()
    {
        // Arrange
        var gamesList = new List<Game>
        {
            new Game(Guid.NewGuid(), 1, 5, 3, GameConfiguration)
            {
                MovesArraySerialized = JsonSerializer.Serialize(new List<string> { "e2e4" })
            }
        };

        _mockDbUtilities.Setup(d => d.GetGamesList())
            .ReturnsAsync(gamesList);

        // Act
        var response = await _client.GetAsync("/api/chess/games");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var getAllGamesResponseDTO = JsonSerializer.Deserialize<GetAllGamesResponseDTO>(responseContent, _jsonOptions);

        getAllGamesResponseDTO.Should().NotBeNull();
        getAllGamesResponseDTO.GamesList.Should().NotBeNull();
        getAllGamesResponseDTO.GamesList.Should().BeOfType<List<Game>>();
    }

    [Fact]
    public async Task CreateGame_WithValidRequest_ShouldCreateNewGame()
    {
        // Arrange
        var createGameRequest = new CreateGameReqDto(5, 1);

        // Act
        var response = await _client.PostAsync("/api/chess/create-game",
            new StringContent(JsonSerializer.Serialize(createGameRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var createGameResponse = JsonSerializer.Deserialize<PostCreateGameResponseDTO>(responseContent, _jsonOptions);

        createGameResponse.Should().NotBeNull();
        createGameResponse.GameId.Should().NotBeEmpty();
        Guid.TryParse(createGameResponse.GameId, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetMovesHistory_WithValidGameId_ShouldReturnMovesList()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game(gameId, 1, 5, 3, GameConfiguration)
        {
            MovesArraySerialized = JsonSerializer.Serialize(new List<string> { "e2e4" })
        };

        _mockDbUtilities.Setup(d => d.GetGameById(gameId.ToString()))
            .ReturnsAsync(game);

        // Act
        var response = await _client.GetAsync($"/api/chess/{gameId}/history");

        // Assert
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GetMovesHistoryResponseDTO>(responseContent, _jsonOptions);

        result.Should().NotBeNull();
        result.MovesArray.Should().NotBeNull();
        result.MovesArray.Should().ContainSingle();
        result.MovesArray.Should().Contain("e2e4");
    }

    [Fact]
    public async Task MakeMove_WithValidMove_ShouldReturnSuccessAndBotMove()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game(gameId, 1, 5, 3, GameConfiguration)
        {
            MovesArraySerialized = JsonSerializer.Serialize(new List<string>()),
            IsRunning = true,
            Lives = 3
        };

        _mockDbUtilities.Setup(d => d.GetGameById(gameId.ToString()))
            .ReturnsAsync(game);

        _mockDbUtilities.Setup(d => d.UpdateGame(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        var moveRequest = new MoveDto("e2e4");

        // Act
        var response = await _client.PostAsync($"/api/chess/{gameId}/move",
            new StringContent(JsonSerializer.Serialize(moveRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var moveResponse = JsonSerializer.Deserialize<PostMoveResponseDTO>(responseContent, _jsonOptions);

        moveResponse.Should().NotBeNull();
        moveResponse.WrongMove.Should().BeFalse();
        moveResponse.BotMove.Should().Be("e7e5");
        moveResponse.FenPosition.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MakeMove_WithInvalidMove_ShouldReturnWrongMoveResponse()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game(gameId, 1, 5, 3, GameConfiguration)
        {
            MovesArraySerialized = JsonSerializer.Serialize(new List<string>()),
            IsRunning = true,
            Lives = 3
        };

        _mockDbUtilities.Setup(d => d.GetGameById(gameId.ToString()))
            .ReturnsAsync(game);

        _mockDbUtilities.Setup(d => d.UpdateGame(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        _mockStockfishService.Setup(s => s.IsMoveCorrect(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var moveRequest = new MoveDto("invalid");

        // Act
        var response = await _client.PostAsync($"/api/chess/{gameId}/move",
            new StringContent(JsonSerializer.Serialize(moveRequest), Encoding.UTF8, "application/json"));

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var moveResponse = JsonSerializer.Deserialize<PostMoveResponseDTO>(responseContent, _jsonOptions);

        moveResponse.Should().NotBeNull();
        moveResponse.WrongMove.Should().BeTrue();
        moveResponse.Lives.Should().Be(2);
        moveResponse.IsRunning.Should().BeTrue();
    }

    public void Dispose()
    {
        _factory?.Dispose();
        _client?.Dispose();
    }
}
