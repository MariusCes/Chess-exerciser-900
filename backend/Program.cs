using backend.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CHESSPROJ.Controllers;
using CHESSPROJ.Services;
using CHESSPROJ.Utilities;
using backend.Utilities;
using Microsoft.Extensions.Logging;
using Serilog;
using backend.Models.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using backend.Controllers;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy — origins from config, semicolon-separated
var corsOrigins = (builder.Configuration["Cors:Origins"] ?? "")
    .Split(';', StringSplitOptions.RemoveEmptyEntries);
if (corsOrigins.Length == 0)
    corsOrigins = new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowStaticWebApp",
        policy =>
        {
            policy.WithOrigins(corsOrigins)
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Read the Stockfish path from configuration (appsettings.json or environment variable)
string stockfishPath = builder.Configuration["StockfishPath"] ?? "stockfish12.exe";

// Register IStockfishService as Scoped — one Stockfish process per request
builder.Services.AddScoped<IStockfishService>(provider =>
{
    if (string.IsNullOrEmpty(stockfishPath))
    {
        throw new InvalidOperationException("Stockfish path is not configured.");
    }
    return new StockfishService(stockfishPath);
});

var connectionString = builder.Configuration.GetConnectionString("ChessPortal");
builder.Services.AddDbContext<ChessDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ChessDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddScoped<IDatabaseUtilities, DatabaseUtilities>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGameService, GameService>();

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

app.UseCors("AllowStaticWebApp");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
public partial class Program { }
