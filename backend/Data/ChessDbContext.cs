using System;
using backend.Models.Domain;
using backend.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ChessDbContext : DbContext
{
    public ChessDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GameHistory> GameHistories { get; set; }
}
