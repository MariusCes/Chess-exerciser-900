using backend.DTOs;
using backend.Models.Domain;
using backend.Models.ViewModels;

namespace backend.Services
{
    public record LoginResult(string Token, string UserName, string Email);
    public record RegisterResult(bool Success, string? Error = null);

    public interface IGameService
    {
        Task<PostCreateGameResponseDTO> CreateGameAsync(CreateGameReqDto req, string userId);
        Task<PostMoveResponseDTO?> MakeMoveAsync(string gameId, MoveDto moveDto);
        Task<GetMovesHistoryResponseDTO?> GetMovesHistoryAsync(string gameId);
        Task<List<Game>> GetUserGamesAsync(string userId);
        Task<RegisterResult> RegisterAsync(RegisterViewModel model);
        Task<LoginResult?> LoginAsync(LoginViewModel model);
    }
}
