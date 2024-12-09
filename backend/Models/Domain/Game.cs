using System;
using System.Text.Json;
using backend.Models.GameStruct;

namespace backend.Models.Domain
{
    public class Game
    {
        private GameStartStruct _gameStartStruct;
        
        public Guid GameId { get; set; }
        public string? MovesArraySerialized { get; set; }
        public Boolean IsRunning { get; set; }
        public TimeSpan Duration { get; set; }
        
        // Navigation properties
        public string UserId { get; set; }
        public User User { get; set; }
        public GameConfiguration GameConfiguration { get; set; }
        public GameState GameState { get; set; }

        public Game() { }

        public Game(Guid id, int difficulty, int botRating, int lives)
        {
            GameId = id;
            _gameStartStruct = new GameStartStruct(difficulty, botRating);
            IsRunning = true;
            
            GameConfiguration = new GameConfiguration
            {
                GameId = id,
                Difficulty = difficulty,
                BotRating = botRating,
                InitialLives = lives,
                BlackoutFrequency = GetBlackoutFrequency(difficulty)
            };

            GameState = new GameState
            {
                GameId = id,
                CurrentLives = lives,
                CurrentBlackout = GetBlackoutFrequency(difficulty),
                TurnBlack = false
            };
        }

        private int GetBlackoutFrequency(int difficulty) => difficulty switch
        {
            1 => 2,
            2 => 4,
            3 => 6,
            _ => 3
        };

        public static Game CreateGameFactory(Guid guid, int difficulty, int botRating, int lives)
        {
            return new Game(guid, difficulty, botRating, lives);
        }

        public void HandleBlackout()
        {
            GameState.CurrentBlackout--;
            if (GameState.CurrentBlackout == 0)
            {
                GameState.TurnBlack = true;
                GameState.CurrentBlackout = GameConfiguration.BlackoutFrequency;
            }
            else
            {
                GameState.TurnBlack = false;
            }
        }

        // Helper properties to maintain compatibility with existing code
        public int Lives
        {
            get => GameState.CurrentLives;
            set => GameState.CurrentLives = value;
        }

        public bool TurnBlack
        {
            get => GameState.TurnBlack;
            set => GameState.TurnBlack = value;
        }

        public int Blackout
        {
            get => GameState.CurrentBlackout;
            set => GameState.CurrentBlackout = value;
        }

        public int ?WLD
        {
            get => GameState.WLD;
            set => GameState.WLD = value;
        }
    }
}