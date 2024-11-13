using System;

namespace backend.Models.GameStruct;

public struct GameStartStruct
{
    public int Difficulty { get; }
    public int BotRating { get; }
    public GameStartStruct(int Difficulty, int BotRating)
    {
        this.Difficulty = Difficulty;
        this.BotRating = BotRating;
    }
}
