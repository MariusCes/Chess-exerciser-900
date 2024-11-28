using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CHESSPROJ.Controllers;
using backend.Models.Domain;
using backend.DTOs;
using backend.Utilities;
using backend.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;

namespace ChessExerciser.Tests
{
    public class ChessControllerTests
    {
        private readonly Mock<IStockfishService> _mockStockfishService;
        private readonly Mock<IDatabaseUtilities> _mockDbUtilities;
        private readonly ChessController _controller;

        public ChessControllerTests()
        {
            _mockStockfishService = new Mock<IStockfishService>();
            _mockDbUtilities = new Mock<IDatabaseUtilities>();
            _controller = new ChessController(_mockStockfishService.Object, _mockDbUtilities.Object);
        }

        [Fact]
        public async Task CreateGame_ShouldReturnOk_WithGameId()
        {
            // Arrange
            var createGameRequest = new CreateGameReqDto(5, 5);
            _mockDbUtilities.Setup(db => db.AddGame(It.IsAny<Game>())).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateGame(createGameRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Cast the result's value to an anonymous object with GameId
            var response = okResult.Value as object;
            Assert.NotNull(response);

            // Check if the object has the GameId property
            var gameIdProperty = response.GetType().GetProperty("GameId");
            Assert.NotNull(gameIdProperty);
            Assert.NotNull(gameIdProperty.GetValue(response));
        }



        [Fact]
        public async Task CreateGame_ShouldReturnNotFound_WhenDbErrorOccurs()
        {
            // Arrange
            var createGameRequest = new CreateGameReqDto(5, 5);
            _mockDbUtilities.Setup(db => db.AddGame(It.IsAny<Game>())).ReturnsAsync(false);

            // Act
            var result = await _controller.CreateGame(createGameRequest);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game_not_found", notFoundResult.Value);
        }

        [Fact]
        public async Task GetMovesHistory_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _mockDbUtilities.Setup(db => db.GetGameById(It.IsAny<string>())).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.GetMovesHistory("invalid-game-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetMovesHistory_ShouldReturnEmptyList_WhenNoMovesExist()
        {
            // Arrange
            var game = new Game { MovesArraySerialized = JsonSerializer.Serialize(new List<string>()) };
            _mockDbUtilities.Setup(db => db.GetGameById(It.IsAny<string>())).ReturnsAsync(game);

            // Act
            var result = await _controller.GetMovesHistory("valid-game-id");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetMovesHistoryResponseDTO>(okResult.Value);
            var moves = Assert.IsAssignableFrom<List<string>>(response.MovesArray);
            Assert.Empty(moves);
        }

        [Fact]
        public async Task MakeMove_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            _mockDbUtilities.Setup(db => db.GetGameById(It.IsAny<string>())).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.MakeMove("invalid-game-id", new MoveDto("e2e4"));

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game_not_found", notFoundResult.Value);
        }

        [Fact]
        public async Task MakeMove_ShouldReturnBadRequest_WhenMoveIsInvalid()
        {
            // Arrange
            var game = new Game();
            _mockDbUtilities.Setup(db => db.GetGameById(It.IsAny<string>())).ReturnsAsync(game);

            // Act
            var result = await _controller.MakeMove("valid-game-id", new MoveDto(""));

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Move_notation_cannot_be_empty", badRequestResult.Value);
        }

        [Fact]
        public async Task MakeMove_ShouldReturnOk_WithCorrectBotMove()
        {
            // Arrange
            var game = new Game { MovesArraySerialized = JsonSerializer.Serialize(new List<string>()) };
            _mockDbUtilities.Setup(db => db.GetGameById(It.IsAny<string>())).ReturnsAsync(game);
            _mockStockfishService.Setup(s => s.IsMoveCorrect(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockStockfishService.Setup(s => s.GetBestMove()).Returns("e7e5");

            // Act
            var result = await _controller.MakeMove("valid-game-id", new MoveDto("e2e4"));

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);

            Assert.False(response.WrongMove);
            Assert.Equal("e7e5", response.BotMove);
        }

        [Fact]
        public async Task GetAllGames_ShouldReturnGamesList()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { GameId = Guid.NewGuid(), MovesArraySerialized = JsonSerializer.Serialize(new List<string> { "e2e4" }) },
                new Game { GameId = Guid.NewGuid(), MovesArraySerialized = JsonSerializer.Serialize(new List<string> { "e7e5" }) }
            };
            _mockDbUtilities.Setup(db => db.GetGamesList()).ReturnsAsync(games);

            // Act
            var result = await _controller.GetAllGames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetAllGamesResponseDTO>(okResult.Value);
            var gamesList = Assert.IsAssignableFrom<List<Game>>(response.GamesList);
            Assert.Empty(gamesList); //empty games do not count
        }
    }
}