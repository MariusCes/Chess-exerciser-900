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
        Blackout = 3; //set to default 3, but should cahnge based on difficulty 
    }



    //factory
    public static Game CreateGameFactory(Guid guid, int difficulty, int botRating, int lives)
    {
        int blackout = difficulty switch
        {
            1 => 2,
            2 => 4,
            3 => 6,
            _ => 3 // Default fallback value
        };

        int newLives = difficulty switch
        {
            1 => 10,
            2 => 8,
            3 => 6,
            _ => 0 // Default fallback value
        };

        return new Game(guid, difficulty, botRating, lives)
        {
            Blackout = blackout,
            Lives = newLives
        };
    }

    //function for the blackout every n moves:
    /*
    if diff is 1, black will be every 2 moves,
    if diff is 2, black will be every 4 moves,
    if diff is 3, black will be every 6 moves,
    otherwise it will be every 3 moves
    */
    public void HandleBlackout()
    {
        Blackout--;
        if (Blackout == 0)
        {
            TurnBlack = true;
            Blackout = Difficulty switch
            {
                1 => 2,
                2 => 4,
                3 => 6,
                _ => 3
            };
        }
        else
        {
            TurnBlack = false;
        }
    }
}
