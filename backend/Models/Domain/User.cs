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

    public User(Guid Id, string Username, string Password) 
    {
        this.Id = Id;
        this.Username = Username;
        this.Password = Password;
    }
}