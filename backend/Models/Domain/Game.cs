using System;
using backend.Models.GameStruct;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace backend.Models.Domain;

public class Game
{
    private GameStartStruct _gameStartStruct;
    public Guid GameId { get; set; }
    public int Difficulty { get; private set; }
    public int BotRating { get; private set; }
    public string? MovesArraySerialized { get; set; }
    public int Lives { get; set; }
    public Boolean IsRunning { get; set; }
    public TimeSpan StartOfGame { get; set; }
    public TimeSpan EndOfGame { get; set; }
    public int WLD { get; set; } //Win - 1 Lose - 0 Draw - 2
    public int Blackout { get; set; }
    public Boolean TurnBlack { get; set; }

    // Foreign key to User
    public Guid UserId { get; set; }

    // Navigation property to the User entity
    public User User { get; set; }

    public Game() { }
    public Game(Guid id, int difficulty, int botRating, int lives)
    {
        GameId = id;
        _gameStartStruct = new GameStartStruct(difficulty, botRating);
        this.Difficulty = _gameStartStruct.Difficulty;
        this.BotRating = _gameStartStruct.BotRating;
        BotRating = botRating;
        Lives = lives;
        IsRunning = true;
        TurnBlack = false;
        Blackout = 3;
    }

    //factory
    public static Game CreateGameFactory(Guid guid, int difficulty, int botRating, int lives)
    {
        return new Game(guid, difficulty, botRating, lives);
    }
}
