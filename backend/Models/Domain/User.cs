using System;
using backend.Models.ViewModels;

namespace backend.Models.Domain;

public class User
{
    public Guid Id { get; set; } // Primary key
    public string Username { get; set; }
    public string Password { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();

    // Parameterless constructor for EF
    public User() { }

    public User(UserStatist userStatist)
    {
        Id = Guid.NewGuid(); // Generate a new Id for the user
    }
}