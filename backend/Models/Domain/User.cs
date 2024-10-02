using System;
using backend.Models.ViewModels;

namespace backend.Models.Domain;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public GameHistory UserGameHistory;
    public UserStatist UserStatistics;

    public User(GameHistory userGameHistory, UserStatist userStatist)
    {
        UserGameHistory = userGameHistory;
        UserGameHistory.Id = Id;
        UserStatistics = userStatist;
    }
}
