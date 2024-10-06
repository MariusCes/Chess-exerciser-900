using System;

namespace backend.Models.Domain;

public class Game
{
    public Guid Id { get; set; }
    public List<string>? MovesArray { get; set; }
    public int Lives { get; set; }
    public int Difficulty { get; set; }
    public int BotRating { get; set; }
    public TimeOnly StartOfGame { get; set; }
    public TimeOnly EndOfGame { get; set; }
    public int WLD { get; set; } //Win - 1 Lose - 0 Draw - 2
}
