using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace backend.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ChessDbContext>
    {
        public ChessDbContext CreateDbContext(string[] args)
        {
            var connectionString = System.Environment.GetEnvironmentVariable("ConnectionStrings__ChessPortal")
                ?? "server=localhost;port=3306;database=chess_exerciser;uid=root;pwd=;";

            var optionsBuilder = new DbContextOptionsBuilder<ChessDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
            return new ChessDbContext(optionsBuilder.Options);
        }
    }
}
