using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Controllers;
using backend.DTOs;
using backend.Models.Domain;
using backend.Utilities;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using backend.Models.ViewModels;
using CHESSPROJ.Controllers;

namespace ChessExerciser.Tests
{
    public class ChessControllerTests
    {
        private readonly Mock<IStockfishService> _mockStockfishService;
        private readonly Mock<IDatabaseUtilities> _mockDbUtilities;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<ILogger<ChessController>> _mockLogger;
        private readonly ChessController _controller;

        public ChessControllerTests()
        {
            _mockStockfishService = new Mock<IStockfishService>();
            _mockDbUtilities = new Mock<IDatabaseUtilities>();
            _mockJwtService = new Mock<IJwtService>();
            _mockLogger = new Mock<ILogger<ChessController>>();

            _controller = new ChessController(
                _mockStockfishService.Object,
                _mockDbUtilities.Object,
                _mockLogger.Object,
                _mockJwtService.Object
            );

            // Mock user context
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id")
            }, "mock"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreateGame_ReturnsOk_WhenGameIsCreatedSuccessfully()
        {
            // Arrange
            var request = new CreateGameReqDto (3,2 );
            _mockDbUtilities.Setup(x => x.AddGame(It.IsAny<Game>())).ReturnsAsync(true);

            // Act
            var result = await _controller.CreateGame(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostCreateGameResponseDTO>(okResult.Value);
            Assert.NotNull(response.GameId);
        }

        [Fact]
        public async Task CreateGame_ReturnsServerError_WhenDatabaseFails()
        {
            // Arrange
            var request = new CreateGameReqDto ( 3,2 );
            _mockDbUtilities.Setup(x => x.AddGame(It.IsAny<Game>())).ReturnsAsync(false);

            // Act
            var result = await _controller.CreateGame(request);

            // Assert
            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetMovesHistory_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            string gameId = "nonexistent-game-id";
            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.GetMovesHistory(gameId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetMovesHistory_ReturnsOk_WhenGameExists()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { MovesArraySerialized = "[\"e2e4\",\"e7e5\"]" };
            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);

            // Act
            var result = await _controller.GetMovesHistory(gameId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetMovesHistoryResponseDTO>(okResult.Value);
            Assert.Equal(new List<string> { "e2e4", "e7e5" }, response.MovesArray);
        }

        [Fact]
        public async Task MakeMove_ReturnsNotFound_WhenGameDoesNotExist()
        {
            // Arrange
            string gameId = "nonexistent-game-id";
            var moveDto = new MoveDto ( "e2e4", TimeSpan.Parse("01:30:00") );
            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync((Game)null);

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game_not_found", notFoundResult.Value);
        }

        [Fact]
        public async Task MakeMove_ReturnsBadRequest_WhenMoveIsEmpty()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { GameId = Guid.NewGuid(), IsRunning = true, MovesArraySerialized = "[]" };
            var gameState = new GameState { CurrentLives = 3 };
            var moveDto = new MoveDto ("", TimeSpan.Parse("01:30:00") ); // Empty move

            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);
            _mockDbUtilities.Setup(x => x.GetStateById(gameId)).ReturnsAsync(gameState);

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Move_notation_cannot_be_empty", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task MakeMove_ReturnsOk_WhenMoveIsValid()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { GameId = Guid.NewGuid(), IsRunning = true, MovesArraySerialized = "[]" };
            var gameState = new GameState { CurrentLives = 3 };
            var moveDto = new MoveDto ("e2e4" , TimeSpan.Parse("01:30:00"));

            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);
            _mockDbUtilities.Setup(x => x.GetStateById(gameId)).ReturnsAsync(gameState);
            _mockStockfishService.Setup(x => x.IsMoveCorrect(It.IsAny<string>(), moveDto.move)).Returns(true);
            _mockStockfishService.Setup(x => x.GetBestMove()).Returns("e7e5");
            _mockStockfishService.Setup(x => x.GetFen()).Returns("some-fen-string");

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.False(response.WrongMove);
            Assert.Equal("e7e5", response.BotMove);
            Assert.Equal("some-fen-string", response.FenPosition);
        }

        [Fact]
        public async Task MakeMove_ReturnsOk_WhenMoveIsInvalid_AndReducesLives()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { GameId = Guid.NewGuid(), IsRunning = true, MovesArraySerialized = "[]" };
            var gameState = new GameState { CurrentLives = 3, CurrentBlackout = 2 };
            var moveDto = new MoveDto ("invalid-move" , TimeSpan.Parse("01:30:00"));

            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);
            _mockDbUtilities.Setup(x => x.GetStateById(gameId)).ReturnsAsync(gameState);
            _mockStockfishService.Setup(x => x.IsMoveCorrect(It.IsAny<string>(), moveDto.move)).Returns(false);

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.True(response.WrongMove);
            Assert.Equal(2, response.Lives); // Lives should decrement from 3 to 2
            Assert.True(response.IsRunning); // Game should still be running
        }

        [Fact]
        public async Task MakeMove_EndsGame_WhenLivesReachZero()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { GameId = Guid.NewGuid(), IsRunning = true, MovesArraySerialized = "[]" };
            var gameState = new GameState { CurrentLives = 1 };
            var moveDto = new MoveDto ("invalid-move", TimeSpan.Parse("01:30:00"));

            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);
            _mockDbUtilities.Setup(x => x.GetStateById(gameId)).ReturnsAsync(gameState);
            _mockStockfishService.Setup(x => x.IsMoveCorrect(It.IsAny<string>(), moveDto.move)).Returns(false);

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.True(response.WrongMove);
            Assert.Equal(0, response.Lives);
            Assert.False(response.IsRunning); // Game should end
        }

        [Fact]
        public async Task MakeMove_HandlesBlackoutCorrectly()
        {
            // Arrange
            string gameId = "test-game-id";
            var game = new Game { GameId = Guid.NewGuid(), IsRunning = true, MovesArraySerialized = "[]" };
            var gameState = new GameState { CurrentLives = 3, CurrentBlackout = 1 };
            var moveDto = new MoveDto ("e2e4", TimeSpan.Parse("01:30:00"));

            _mockDbUtilities.Setup(x => x.GetGameById(gameId)).ReturnsAsync(game);
            _mockDbUtilities.Setup(x => x.GetStateById(gameId)).ReturnsAsync(gameState);
            _mockStockfishService.Setup(x => x.IsMoveCorrect(It.IsAny<string>(), moveDto.move)).Returns(true);
            _mockStockfishService.Setup(x => x.GetBestMove()).Returns("e7e5");
            _mockStockfishService.Setup(x => x.GetFen()).Returns("fen-position");

            // Act
            var result = await _controller.MakeMove(gameId, moveDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.True(response.TurnBlack); // Because CurrentBlackout should have triggered a blackout
        }

        [Fact]
        public async Task GetUserGames_ReturnsEmpty_WhenUserHasNoGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { UserId = "other-user-id", MovesArraySerialized = "[\"e2e4\"]" }
            };

            _mockDbUtilities.Setup(x => x.GetGamesList()).ReturnsAsync(games);

            // Act
            var result = await _controller.GetUserGames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var userGames = Assert.IsType<List<Game>>(okResult.Value);
            Assert.Empty(userGames);
        }

        [Fact]
        public async Task GetUserGames_ReturnsGames_WhenUserHasGames()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game { UserId = "test-user-id", MovesArraySerialized = "[\"e2e4\"]" },
                new Game { UserId = "test-user-id", MovesArraySerialized = "[]" },
                new Game { UserId = "other-user-id", MovesArraySerialized = "[\"d2d4\"]" }
            };

            _mockDbUtilities.Setup(x => x.GetGamesList()).ReturnsAsync(games);

            // Act
            var result = await _controller.GetUserGames();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var userGames = Assert.IsType<List<Game>>(okResult.Value);
            // Should return 2 games (even those without moves) because we're not filtering here
            Assert.Equal(2, userGames.Count);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var model = new RegisterViewModel 
            { 
                UserName = "testuser", 
                Email = "test@example.com", 
                Password = "Passw0rd!", 
                ConfirmPassword = "Passw0rd!" 
            };
            _mockDbUtilities.Setup(x => x.AddUser(model)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Serialize the actual response
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(okResult.Value);

            // Expected serialized response
            var expectedJson = System.Text.Json.JsonSerializer.Serialize(new { message = "Registration successful" });

            // Compare serialized responses
            Assert.Equal(expectedJson, jsonResponse);
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelIsInvalid()
        {
            // Arrange
            var model = new RegisterViewModel { UserName = "", Email = "", Password = "", ConfirmPassword = "" };
            _controller.ModelState.AddModelError("Error", "Invalid data");

            // Act
            var result = await _controller.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Deserialize the SerializableError for validation
            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);

            // Validate the error key and message
            Assert.True(serializableError.ContainsKey("Error"));
            var errorMessages = serializableError["Error"] as string[];
            Assert.NotNull(errorMessages);
            Assert.Contains("Invalid data", errorMessages);
        }


        [Fact]
        public async Task Register_ReturnsBadRequest_WhenDatabaseFailsToAddUser()
        {
            // Arrange
            var model = new RegisterViewModel { UserName = "testuser", Email = "test@example.com", Password = "Passw0rd!", ConfirmPassword = "Passw0rd!" };
            _mockDbUtilities.Setup(x => x.AddUser(model)).ReturnsAsync(false);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Bad_request", badRequestResult.Value.ToString());
        }
        
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var model = new LoginViewModel { Email = "test@example.com", Password = "Passw0rd!" };
            var user = new User { Id = "test-user-id", UserName = "testuser", Email = "test@example.com" };
            _mockDbUtilities.Setup(x => x.LogInUser(model)).ReturnsAsync(true);
            _mockDbUtilities.Setup(x => x.GetUserByEmail(model)).ReturnsAsync(user);
            _mockJwtService.Setup(x => x.GenerateToken(user)).Returns("valid-jwt-token");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            // Serialize and inspect the response
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            Assert.Equal("valid-jwt-token", response["token"]);
            Assert.Equal("testuser", response["UserName"]);
            Assert.Equal("test@example.com", response["Email"]);
        }




        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            // Arrange
            var model = new LoginViewModel { Email = "invalid@example.com", Password = "wrongpass" };
            _mockDbUtilities.Setup(x => x.LogInUser(model)).ReturnsAsync(false);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Serialize the response for inspection
            var json = System.Text.Json.JsonSerializer.Serialize(badRequestResult.Value);

            // Validate against the expected serialized string
            var expectedJson = System.Text.Json.JsonSerializer.Serialize("Invalid credentials");
            Assert.Equal(expectedJson, json);
        }

    }
}
