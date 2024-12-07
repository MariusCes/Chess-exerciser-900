using backend.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CHESSPROJ.Controllers;
using CHESSPROJ.Services;
using CHESSPROJ.Utilities;
using backend.Utilities;
using Microsoft.Extensions.Logging;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()   // Allow all origins
                   .AllowAnyMethod()   // Allow all HTTP methods (GET, POST, etc.)
                   .AllowAnyHeader();  // Allow any headers
        });
});

// Read the Stockfish path from configuration (appsettings.json or environment variable)
string stockfishPath = builder.Configuration["StockfishPath"] ?? "stockfish12.exe";

// Register the IStockfish (Stockfish instance) as Singleton so that it's shared throughout the application
builder.Services.AddScoped<Stockfish.NET.Stockfish>(provider =>
{
    if (string.IsNullOrEmpty(stockfishPath))
    {
        throw new InvalidOperationException("Stockfish path is not configured.");
    }

    return new Stockfish.NET.Stockfish(stockfishPath); // Stockfish is a static-like service
});

// Register IStockfishService (StockfishService) as Scoped so it's injected with a fresh instance per request
builder.Services.AddScoped<IStockfishService>(provider =>
{
    var stockfish = provider.GetRequiredService<Stockfish.NET.Stockfish>(); // Get the singleton instance of Stockfish
    return new StockfishService(stockfish); // Pass the singleton Stockfish to the StockfishService
});

builder.Services.AddDbContext<ChessDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ChessPortal")));
builder.Services.AddScoped<IDatabaseUtilities, DatabaseUtilities>();
builder.Services.AddSingleton<UserSingleton>(provider => UserSingleton.GetInstance()); // mappinam

// Set up Serilog to log to a file
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()  // Optionally log to the console
    .WriteTo.File("logs/myapp-log.txt", rollingInterval: RollingInterval.Day)  // Log to a file
    .CreateLogger();

builder.Host.UseSerilog();  // This replaces the default logging provider with Serilog

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }
