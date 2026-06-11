using backend.DTOs;
using backend.Errors;
using backend.Models.Domain;
using backend.Models.ViewModels;
using backend.Utilities;
using CHESSPROJ.Controllers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using backend.Controllers;

namespace backend.Services
{
    public class GameService : IGameService
    {
        private readonly IStockfishService _stockfishService;
        private readonly IDatabaseUtilities _dbUtilities;
        private readonly IJwtService _jwtService;
        private readonly ILogger<GameService> _logger;

        public GameService(
            IStockfishService stockfishService,
            IDatabaseUtilities dbUtilities,
            IJwtService jwtService,
            ILogger<GameService> logger)
        {
            _stockfishService = stockfishService;
            _dbUtilities = dbUtilities;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<PostCreateGameResponseDTO> CreateGameAsync(CreateGameReqDto req, string userId)
        {
            _stockfishService.SetLevel(req.aiDifficulty);
            Game game = Game.CreateGameFactory(Guid.NewGuid(), req.gameDifficulty, req.aiDifficulty, 3);
            game.IsRunning = true;
            game.UserId = userId;

            if (!await _dbUtilities.AddGame(game))
            {
                throw new DatabaseOperationException("Failed to add the game to the database.");
            }

            return new PostCreateGameResponseDTO { GameId = game.GameId.ToString() };
        }

        public async Task<PostMoveResponseDTO?> MakeMoveAsync(string gameId, MoveDto moveNotation)
        {
            Game game = await _dbUtilities.GetGameById(gameId);
            if (game == null)
                return null;

            GameState gameState = await _dbUtilities.GetStateById(gameId);
            game.Duration = moveNotation.gameTime;
            string move = moveNotation.move;

            List<string> movesArray = game.MovesArraySerialized != null
                ? JsonSerializer.Deserialize<List<string>>(game.MovesArraySerialized)!
                : new List<string>();

            string currentPosition = string.Join(" ", movesArray);

            if (_stockfishService.IsMoveCorrect(currentPosition, move) && game.IsRunning)
            {
                _stockfishService.SetPosition(currentPosition, move);
                movesArray.Add(move);
                string botMove = _stockfishService.GetBestMove();
                _stockfishService.SetPosition(string.Join(" ", movesArray), botMove);
                movesArray.Add(botMove);
                string fenPosition = _stockfishService.GetFen();
                currentPosition = string.Join(" ", movesArray);

                gameState.HandleBlackout();
                game.MovesArraySerialized = JsonSerializer.Serialize(movesArray);

                if (_stockfishService.GetEvalType() == "mate")
                {
                    gameState.WLD = _stockfishService.GetEvalVal() >= 0 ? 1 : 0;
                    game.IsRunning = false;
                    await _dbUtilities.UpdateGame(game, gameState);

                    return new PostMoveResponseDTO
                    {
                        WrongMove = false,
                        BotMove = botMove,
                        Lives = gameState.CurrentLives,
                        IsRunning = false,
                        TurnBlack = false,
                        FenPosition = fenPosition,
                        GameWLD = (int)gameState.WLD,
                        CurrentPosition = currentPosition,
                        Duration = game.Duration
                    };
                }

                await _dbUtilities.UpdateGame(game, gameState);
                return new PostMoveResponseDTO
                {
                    WrongMove = false,
                    BotMove = botMove,
                    IsRunning = true,
                    CurrentPosition = currentPosition,
                    FenPosition = fenPosition,
                    TurnBlack = gameState.TurnBlack
                };
            }
            else
            {
                gameState.CurrentLives--;
                if (gameState.CurrentLives <= 0)
                {
                    game.IsRunning = false;
                    gameState.CurrentLives = 0;
                    gameState.WLD = 0;
                }
                gameState.HandleBlackout();

                await _dbUtilities.UpdateGame(game, gameState);
                return new PostMoveResponseDTO
                {
                    WrongMove = true,
                    Lives = gameState.CurrentLives,
                    IsRunning = game.IsRunning,
                    TurnBlack = gameState.TurnBlack,
                    GameWLD = (int)gameState.WLD,
                    Duration = game.Duration
                };
            }
        }

        public async Task<GetMovesHistoryResponseDTO?> GetMovesHistoryAsync(string gameId)
        {
            Game game = await _dbUtilities.GetGameById(gameId);
            if (game == null)
                return null;

            List<string> moves = game.MovesArraySerialized != null
                ? JsonSerializer.Deserialize<List<string>>(game.MovesArraySerialized)!
                : new List<string>();

            return new GetMovesHistoryResponseDTO { MovesArray = moves };
        }

        public async Task<List<Game>> GetUserGamesAsync(string userId)
        {
            List<Game> allGames = await _dbUtilities.GetGamesList();
            return allGames.Where(g => g.UserId == userId).ToList();
        }

        public async Task<RegisterResult> RegisterAsync(RegisterViewModel model)
        {
            if (model.Password != model.ConfirmPassword)
                return new RegisterResult(false, "Passwords don't match");

            if (model.Password.Length <= 8)
                return new RegisterResult(false, "Password needs to be at least 9 characters");

            if (!model.Password.Any(char.IsDigit) || !model.Password.Any(char.IsUpper) || !model.Password.Any(char.IsLower))
                return new RegisterResult(false, "Password needs to have a number, an uppercase and a lowercase");

            if (await _dbUtilities.FindIfUsernameExists(model))
                return new RegisterResult(false, "Username already taken");

            if (await _dbUtilities.FindIfEmailExists(model))
                return new RegisterResult(false, "Email already taken");

            if (await _dbUtilities.AddUser(model))
                return new RegisterResult(true);

            return new RegisterResult(false, "Registration failed");
        }

        public async Task<LoginResult?> LoginAsync(LoginViewModel model)
        {
            if (!await _dbUtilities.LogInUser(model))
                return null;

            User user = await _dbUtilities.GetUserByEmail(model);
            string token = _jwtService.GenerateToken(user);
            return new LoginResult(token, user.UserName, user.Email);
        }
    }
}
