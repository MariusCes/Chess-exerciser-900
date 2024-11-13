using System;
using backend.Models.Domain;

namespace backend.Models.ViewModels;

public class UserStatist
{
    public Guid StatisticId { get; set; }
    public int Rating { get; set; }
    public double WinRate { get; set; }
    public int GamesPlayed { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesDrawn { get; set; }
    public DateTime ProfileLifespan { get; set; }

    // Foreign key to User
    public Guid UserId { get; set; }

    // Navigation property to the User entity
    public User User { get; set; }
}
