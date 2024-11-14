using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using CHESSPROJ.Controllers;
using backend.DTOs;
using backend.Models.Domain;
using Stockfish.NET;

namespace ChessControllerTests
{
    public class ChessControllerTests
    {
        private readonly Mock<IStockfishService> _mockStockfishService;
        private readonly ChessController _controller;

        public ChessControllerTests()
        {
            _mockStockfishService = new Mock<IStockfishService>();
            _controller = new ChessController(_mockStockfishService.Object);
        }

        // 1. Test CreateGame method
        [Fact]
        public async Task CreateGame_ShouldReturnOk_WithGameId()
        {
            // Arrange
            int skillLevel = 5;

            // Act
            var result = await _controller.CreateGame(skillLevel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.NotEqual(Guid.Empty, response.GameId); // Ensure GameId is not empty
        }

        // 2. Test GetMovesHistory when game exists
        [Fact]
        public async Task GetMovesHistory_ShouldReturnFileStream_WhenGameExists()
        {
            // Arrange
            var gameId = Guid.NewGuid().ToString();
            var game = new Game(Guid.Parse(gameId), 1, 1, 3)
            {
                MovesArray = new List<string> { "e2e4", "e7e5" }
            };
            ChessController.games.Add(game); // Add game to the static list for testing

            // Act
            var result = await _controller.GetMovesHistory(gameId);

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/json", fileResult.ContentType);
        }

        // 3. Test GetMovesHistory when game does not exist
        [Fact]
        public async Task GetMovesHistory_ShouldReturnNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            var gameId = "non-existent-game-id";

            // Act
            var result = await _controller.GetMovesHistory(gameId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game not found.", notFoundResult.Value);
        }

        // 4. Test MakeMove with valid player move
        [Fact]
        public async Task MakeMove_ShouldReturnBotMove_WhenPlayerMoveIsValid()
        {
            // Arrange
            var gameId = Guid.NewGuid().ToString();
            var moveDto = new MoveDto { move = "e2e4" };
            var game = new Game(Guid.Parse(gameId), 1, 1, 3)
            {
                MovesArray = new List<string>()
            };
            ChessController.games.Add(game); // Add game to the static list for testing

            // Mock Stockfish service behavior
            _mockStockfishService.Setup(service => service.IsMoveCorrect(It.IsAny<string>(), moveDto.move))
                                 .ReturnsAsync(true);
            _mockStockfishService.Setup(service => service.GetBestMove())
                                 .ReturnsAsync("e7e5");

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.False(response.wrongMove);  // Ensure wrongMove is false
            Assert.Equal("e7e5", response.botMove); // Ensure the bot's move is correct
        }

        // 5. Test MakeMove with invalid player move
        [Fact]
        public async Task MakeMove_ShouldReturnError_WhenPlayerMoveIsInvalid()
        {
            // Arrange
            var gameId = Guid.NewGuid().ToString();
            var moveDto = new MoveDto ("invalid-move");
            var game = new Game(Guid.Parse(gameId), 1, 1, 3)
            {
                MovesArray = new List<string>(),
                Lives = 3,
                Blackout = 3
            };
            ChessController.games.Add(game); // Add game to the static list for testing

            // Mock Stockfish service behavior
            _mockStockfishService.Setup(service => service.IsMoveCorrect(It.IsAny<string>(), moveDto.move))
                                 .ReturnsAsync(false);

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            Assert.True(response.wrongMove);  // Ensure wrongMove is true
            Assert.Equal(2, response.lives);  // Ensure lives are decremented
            Assert.False(response.gameIsRunning); // Ensure game is not running if lives = 0
        }

        // 6. Test GetAllGames method
        [Fact]
        public async Task GetAllGames_ShouldReturnAllGames()
        {
            // Arrange
            var game1 = new Game(Guid.NewGuid(), 1, 1, 3);
            var game2 = new Game(Guid.NewGuid(), 1, 1, 3);
            ChessController.games.Add(game1);
            ChessController.games.Add(game2);

            // Act
            var result = await _controller.GetAllGames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var games = Assert.IsAssignableFrom<List<Game>>(okResult.Value);
            Assert.Equal(2, games.Count); // Ensure both games are returned
        }
    }
}
