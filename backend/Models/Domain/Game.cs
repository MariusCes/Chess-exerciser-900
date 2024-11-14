using System;
using backend.Models.GameStruct;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace backend.Models.Domain;

public class Game
{
    public GameStartStruct gameStartStruct;
    public Guid GameId { get; set; }
    public int Difficulty => gameStartStruct.Difficulty;
    public int BotRating => gameStartStruct.BotRating;
     public string MovesArraySerialized { get; set; }
    public int Lives { get; set; }
    public Boolean IsRunning { get; set; }
    public TimeSpan StartOfGame { get; set; }
    public TimeSpan EndOfGame { get; set; }
    public int WLD { get; set; } //Win - 1 Lose - 0 Draw - 2
    public int Blackout { get; set; }
    public Boolean TurnBlack { get; set; }

    [NotMapped]
    public List<string>? MovesArray
    {
        get => MovesArraySerialized == null ? new List<string>() : JsonSerializer.Deserialize<List<string>>(MovesArraySerialized);
        set => MovesArraySerialized = JsonSerializer.Serialize(value);
    }

    // Foreign key to User
    public Guid UserId { get; set; }

    // Navigation property to the User entity
    public User User { get; set; }

    public Game() { }
    public Game(Guid id, int Difficulty, int BotRating, int lives)
    {
        GameId = id;
        gameStartStruct = new GameStartStruct(Difficulty, BotRating);
        MovesArray = new List<string>();
        Lives = lives;
        IsRunning = true;
    }

    //factory
    public static Game CreateGameFactory(Guid guid, int difficulty, int botRating, int lives)
    {
        return new Game(guid, difficulty, botRating, lives);
    }
}
