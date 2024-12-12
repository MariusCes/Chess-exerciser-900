using System;

namespace backend.Models.Domain;

public class GameConfiguration
{
    public Guid GameId { get; set; }
    public int Difficulty { get; set; }
    public int BotRating { get; set; }
    public int InitialLives { get; set; }
    public int BlackoutFrequency { get; set; }
    public Game Game { get; set; }
}
