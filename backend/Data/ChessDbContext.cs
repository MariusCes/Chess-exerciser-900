using System;
using backend.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ChessDbContext : IdentityDbContext<User>
    {
        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<GameConfiguration> GameConfigurations { get; set; }
        public DbSet<GameState> GameStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.ProfileLifespan)
                    .HasDefaultValueSql("GETDATE()");

                entity.HasOne(u => u.UserStats)
                    .WithOne(us => us.User)
                    .HasForeignKey<UserStats>(us => us.UserId);
            });

            // UserStats configuration
            modelBuilder.Entity<UserStats>(entity =>
            {
                entity.HasKey(us => us.UserId);
                entity.Property(us => us.Rating)
                    .HasDefaultValue(800);
            });

            // Game configuration
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.GameId);

                entity.HasOne(g => g.User)
                    .WithMany(u => u.Games)
                    .HasForeignKey(g => g.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Keep the MovesArraySerialized as requested
                entity.Property(g => g.MovesArraySerialized)
                    .HasColumnType("nvarchar(max)");

                entity.HasOne(g => g.GameConfiguration)
                    .WithOne(gc => gc.Game)
                    .HasForeignKey<GameConfiguration>(gc => gc.GameId);

                entity.HasOne(g => g.GameState)
                    .WithOne(gs => gs.Game)
                    .HasForeignKey<GameState>(gs => gs.GameId);
            });

            // GameConfiguration configuration
            modelBuilder.Entity<GameConfiguration>(entity =>
            {
                entity.HasKey(gc => gc.GameId);
            });

            // GameState configuration
            modelBuilder.Entity<GameState>(entity =>
            {
                entity.HasKey(gs => gs.GameId);
            });
        }
    }
}
