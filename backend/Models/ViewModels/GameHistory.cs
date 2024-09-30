using System;
using backend.Models.Domain;

namespace backend.Models.ViewModels;

public class GameHistory
{
    public Guid Id { get; set; }
    public ICollection<Game> Games {get; set;}
}
