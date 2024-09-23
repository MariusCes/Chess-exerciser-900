using System;

namespace backend.Models.Domain;

public class Game
{
    public string[] MovesArray { get; set; }
    public string UsersMove { get; set; }
    public int Lives { get; set; }
    public int Difficulty { get; set; }
    public int BotRating { get; set; }
}
