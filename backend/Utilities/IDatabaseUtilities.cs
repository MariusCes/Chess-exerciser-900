﻿using backend.Models.Domain;
using backend.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace backend.Utilities
{
    public interface IDatabaseUtilities
    {
        public Task<bool> AddGame(Game newGame);
        public Task<bool> AddUser(RegisterViewModel newUser);
        public Task<Game?> GetGameById(string gameId);
        public Task UpdateGame(Game game, GameState gameState);
        public Task<List<Game>> GetGamesList();
        public Task<GameState?> GetStateById(string gameId);
        public Task<bool> LogInUser(LoginViewModel model);
        public Task<User> GetUserByEmail(LoginViewModel model);
        public Task<bool> FindIfUsernameExists(RegisterViewModel model);
        public Task<bool> FindIfEmailExists(RegisterViewModel model);
    }
}
