using System;

namespace backend.Models.ViewModels;

public class UserStatist
{
    public int Rating { get; set; }
    public double WinRate { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesDrawn { get; set; }
    public DateOnly ProfileLifespan { get; set; }
}
