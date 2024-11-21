namespace backend.DTOs;

public record class CreateGameReqDto(
    int aiDifficulty,
    int gameDifficulty
);
