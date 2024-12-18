using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using backend.Models.GameStruct;

namespace backend.Models.Domain 
{
    public class Game
    {   
        public Guid GameId { get; set; }
        public string? MovesArraySerialized { get; set; }
        public Boolean IsRunning { get; set; } = true;
        public TimeSpan Duration { get; set; }
        
        // Navigation properties
        public string UserId { get; set; }
        public User User { get; set; }
        public GameConfiguration GameConfiguration { get; set; }
        public GameState GameState { get; set; }

        // Default constructor ensures GameState and GameConfiguration are never null
        public Game()
        {
        }

        public Game(Guid id, int difficulty, int botRating, int lives)
        {
            GameId = id;
            IsRunning = true;

            try
            {
                GameConfiguration = new GameConfiguration();
                GameConfiguration.GameId = id;
                GameConfiguration.Difficulty = difficulty;
                GameConfiguration.BotRating = botRating;
                GameConfiguration.InitialLives = lives;
                GameConfiguration.BlackoutFrequency = GetBlackoutFrequency(difficulty);

                // Initialize GameState
                GameState = new GameState();
                GameState.GameId = id;
                GameState.CurrentLives = lives;
                GameState.CurrentBlackout = GetBlackoutFrequency(difficulty);
                GameState.InitialBlackout = GetBlackoutFrequency(difficulty);
                GameState.TurnBlack = false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize game components: {ex.Message}", ex);
            }
        }

        public static Game CreateGameFactory(Guid guid, int difficulty, int botRating, int lives)
        {
            return new Game(guid, difficulty, botRating, lives);
        }

        public int GetBlackoutFrequency(int difficulty) => difficulty switch
        {
            1 => 6,
            2 => 4,
            3 => 2,
            _ => 3
        };
        
    }
}