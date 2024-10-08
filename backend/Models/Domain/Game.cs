using System;
using backend.Models.GameStruct;

namespace backend.Models.Domain;

public class Game
{
    GameStartStruct gameStartStruct;
    public Guid GameId => gameStartStruct.Id;
    public int Difficulty => gameStartStruct.Difficulty;
    public int BotRating => gameStartStruct.BotRating;
    public List<string>? MovesArray { get; set; }
    public int Lives { get; set; }
    public Boolean IsRunning { get; set; }
    public TimeOnly StartOfGame { get; set; }
    public TimeOnly EndOfGame { get; set; }
    public int WLD { get; set; } //Win - 1 Lose - 0 Draw - 2

    public Game(Guid guid, int Difficulty, int BotRating)
    {
        gameStartStruct = new GameStartStruct(guid, Difficulty, BotRating);
    }

}
