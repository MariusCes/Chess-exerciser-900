using System;

namespace backend.Models.Domain;

public class GameState
{
    public Guid GameId { get; set; }
    public int CurrentLives { get; set; } = 3;
    public int CurrentBlackout { get; set; }
    public Boolean TurnBlack { get; set; }
    public int? WLD { get; set; } = 0;  // Win(1)/Lose(0)/Draw(2)
    public Game Game { get; set; }

    public void HandleBlackout()
        {
            CurrentBlackout--;
            if (CurrentBlackout == 0)
            {
                TurnBlack = true;
            }
            else
            {
                TurnBlack = false;
            }
        }
}
