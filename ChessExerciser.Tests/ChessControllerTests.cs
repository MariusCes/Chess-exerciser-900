using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.DTOs;
using backend.Models.Domain;
using backend.Services;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using backend.Models.ViewModels;
using CHESSPROJ.Controllers;

namespace ChessExerciser.Tests
{
    public class ChessControllerTests
    {
        private readonly Mock<IGameService> _mockGameService;
        private readonly Mock<ILogger<ChessController>> _mockLogger;
        private readonly ChessController _controller;

        public ChessControllerTests()
        {
            _mockGameService = new Mock<IGameService>();
            _mockLogger = new Mock<ILogger<ChessController>>();

            _controller = new ChessController(_mockGameService.Object, _mockLogger.Object);

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
            var request = new CreateGameReqDto(3, 2);
            _mockGameService.Setup(x => x.CreateGameAsync(request, "test-user-id"))
                .ReturnsAsync(new PostCreateGameResponseDTO { GameId = Guid.NewGuid().ToString() });

            var result = await _controller.CreateGame(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostCreateGameResponseDTO>(okResult.Value);
            Assert.NotNull(response.GameId);
        }

        [Fact]
        public async Task CreateGame_ReturnsServerError_WhenDatabaseFails()
        {
            var request = new CreateGameReqDto(3, 2);
            _mockGameService.Setup(x => x.CreateGameAsync(request, "test-user-id"))
                .ThrowsAsync(new backend.Errors.DatabaseOperationException("Failed to add the game to the database."));

            var result = await _controller.CreateGame(request);

            var errorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, errorResult.StatusCode);
        }

        [Fact]
        public async Task GetMovesHistory_ReturnsNotFound_WhenGameDoesNotExist()
        {
            _mockGameService.Setup(x => x.GetMovesHistoryAsync("nonexistent-game-id"))
                .ReturnsAsync((GetMovesHistoryResponseDTO?)null);

            var result = await _controller.GetMovesHistory("nonexistent-game-id");

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Game not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetMovesHistory_ReturnsOk_WhenGameExists()
        {
            _mockGameService.Setup(x => x.GetMovesHistoryAsync("test-game-id"))
                .ReturnsAsync(new GetMovesHistoryResponseDTO { MovesArray = new List<string> { "e2e4", "e7e5" } });

            var result = await _controller.GetMovesHistory("test-game-id");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<GetMovesHistoryResponseDTO>(okResult.Value);
            Assert.Equal(new List<string> { "e2e4", "e7e5" }, response.MovesArray);
        }

        [Fact]
        public async Task MakeMove_ReturnsNotFound_WhenGameDoesNotExist()
        {
            var moveDto = new MoveDto("e2e4", TimeSpan.Parse("01:30:00"));
            _mockGameService.Setup(x => x.MakeMoveAsync("nonexistent-game-id", moveDto))
                .ReturnsAsync((PostMoveResponseDTO?)null);

            var result = await _controller.MakeMove("nonexistent-game-id", moveDto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task MakeMove_ReturnsBadRequest_WhenMoveIsEmpty()
        {
            var moveDto = new MoveDto("", TimeSpan.Parse("01:30:00"));

            var result = await _controller.MakeMove("test-game-id", moveDto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Move_notation_cannot_be_empty", badRequestResult.Value!.ToString());
        }

        [Fact]
        public async Task MakeMove_ReturnsOk_WhenMoveIsValid()
        {
            var moveDto = new MoveDto("e2e4", TimeSpan.Parse("01:30:00"));
            _mockGameService.Setup(x => x.MakeMoveAsync("test-game-id", moveDto))
                .ReturnsAsync(new PostMoveResponseDTO { WrongMove = false, BotMove = "e7e5", FenPosition = "some-fen-string" });

            var result = await _controller.MakeMove("test-game-id", moveDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.False(response.WrongMove);
            Assert.Equal("e7e5", response.BotMove);
            Assert.Equal("some-fen-string", response.FenPosition);
        }

        [Fact]
        public async Task MakeMove_ReturnsOk_WhenMoveIsInvalid_AndReducesLives()
        {
            var moveDto = new MoveDto("invalid-move", TimeSpan.Parse("01:30:00"));
            _mockGameService.Setup(x => x.MakeMoveAsync("test-game-id", moveDto))
                .ReturnsAsync(new PostMoveResponseDTO { WrongMove = true, Lives = 2, IsRunning = true });

            var result = await _controller.MakeMove("test-game-id", moveDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.True(response.WrongMove);
            Assert.Equal(2, response.Lives);
            Assert.True(response.IsRunning);
        }

        [Fact]
        public async Task MakeMove_EndsGame_WhenLivesReachZero()
        {
            var moveDto = new MoveDto("invalid-move", TimeSpan.Parse("01:30:00"));
            _mockGameService.Setup(x => x.MakeMoveAsync("test-game-id", moveDto))
                .ReturnsAsync(new PostMoveResponseDTO { WrongMove = true, Lives = 0, IsRunning = false });

            var result = await _controller.MakeMove("test-game-id", moveDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.Equal(0, response.Lives);
            Assert.False(response.IsRunning);
        }

        [Fact]
        public async Task MakeMove_HandlesBlackoutCorrectly()
        {
            var moveDto = new MoveDto("e2e4", TimeSpan.Parse("01:30:00"));
            _mockGameService.Setup(x => x.MakeMoveAsync("test-game-id", moveDto))
                .ReturnsAsync(new PostMoveResponseDTO { WrongMove = false, TurnBlack = true });

            var result = await _controller.MakeMove("test-game-id", moveDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PostMoveResponseDTO>(okResult.Value);
            Assert.True(response.TurnBlack);
        }

        [Fact]
        public async Task GetUserGames_ReturnsEmpty_WhenUserHasNoGames()
        {
            _mockGameService.Setup(x => x.GetUserGamesAsync("test-user-id"))
                .ReturnsAsync(new List<Game>());

            var result = await _controller.GetUserGames();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userGames = Assert.IsType<List<Game>>(okResult.Value);
            Assert.Empty(userGames);
        }

        [Fact]
        public async Task GetUserGames_ReturnsGames_WhenUserHasGames()
        {
            _mockGameService.Setup(x => x.GetUserGamesAsync("test-user-id"))
                .ReturnsAsync(new List<Game> {
                    new Game { UserId = "test-user-id" },
                    new Game { UserId = "test-user-id" }
                });

            var result = await _controller.GetUserGames();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var userGames = Assert.IsType<List<Game>>(okResult.Value);
            Assert.Equal(2, userGames.Count);
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsRegisteredSuccessfully()
        {
            var model = new RegisterViewModel
            {
                UserName = "testuser",
                Email = "test@example.com",
                Password = "Passw0rd!",
                ConfirmPassword = "Passw0rd!"
            };
            _mockGameService.Setup(x => x.RegisterAsync(model))
                .ReturnsAsync(new RegisterResult(true));

            var result = await _controller.Register(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var expected = System.Text.Json.JsonSerializer.Serialize(new { message = "Registration successful" });
            Assert.Equal(expected, json);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelIsInvalid()
        {
            var model = new RegisterViewModel { UserName = "", Email = "", Password = "", ConfirmPassword = "" };
            _controller.ModelState.AddModelError("Error", "Invalid data");

            var result = await _controller.Register(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(serializableError.ContainsKey("Error"));
            var errorMessages = serializableError["Error"] as string[];
            Assert.NotNull(errorMessages);
            Assert.Contains("Invalid data", errorMessages);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            var model = new RegisterViewModel { UserName = "testuser", Email = "test@example.com", Password = "Passw0rd!", ConfirmPassword = "Passw0rd!" };
            _mockGameService.Setup(x => x.RegisterAsync(model))
                .ReturnsAsync(new RegisterResult(false, "Registration failed"));

            var result = await _controller.Register(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Registration failed", badRequestResult.Value!.ToString());
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var model = new LoginViewModel { Email = "test@example.com", Password = "Passw0rd!" };
            _mockGameService.Setup(x => x.LoginAsync(model))
                .ReturnsAsync(new LoginResult("valid-jwt-token", "testuser", "test@example.com"));

            var result = await _controller.Login(model);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            var response = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            Assert.Equal("valid-jwt-token", response!["token"]);
            Assert.Equal("testuser", response["userName"]);
            Assert.Equal("test@example.com", response["email"]);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenCredentialsAreInvalid()
        {
            var model = new LoginViewModel { Email = "invalid@example.com", Password = "wrongpass" };
            _mockGameService.Setup(x => x.LoginAsync(model))
                .ReturnsAsync((LoginResult?)null);

            var result = await _controller.Login(model);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(badRequestResult.Value);
            Assert.Equal(System.Text.Json.JsonSerializer.Serialize("Invalid credentials"), json);
        }
    }
}
