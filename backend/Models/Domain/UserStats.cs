using System;

namespace backend.Models.Domain;

public class UserStats 
{
    public string UserId { get; set; }
    public User User { get; set; }
    
    public int Rating { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesDrawn { get; set; }
    
    // WinRate removed as it's calculated
    public double GetWinRate() => GamesPlayed > 0 ? (double)GamesWon / GamesPlayed : 0;
}