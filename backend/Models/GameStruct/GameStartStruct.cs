using System;

namespace backend.Models.GameStruct;

public struct GameStartStruct
{
    public Guid Id { get; }
    public int Difficulty { get; }
    public int BotRating { get; }
    public GameStartStruct(Guid Id, int Difficulty, int BotRating)
    {
        this.Id = Id;
        this.Difficulty = Difficulty;
        this.BotRating = BotRating;
    }
}
