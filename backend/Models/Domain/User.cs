using System;
using backend.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace backend.Models.Domain;

public class User : IdentityUser
{
    public UserStats UserStats { get; set; }
    public ICollection<Game> Games { get; set; } = new List<Game>();
    public DateTime ProfileLifespan { get; set; }
}