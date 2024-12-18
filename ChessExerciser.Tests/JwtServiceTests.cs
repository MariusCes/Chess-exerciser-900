using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using Moq;
using backend.Controllers;
using backend.Models.Domain;
using System.Security.Claims;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();

        // Set up mock configuration values with a valid 32-byte key
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("ThisIsASecretKey12345678901234567890");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("TestIssuer");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("TestAudience");

        _jwtService = new JwtService(_mockConfiguration.Object);
    }


    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            UserName = "TestUser",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);

        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));

        var jwtToken = handler.ReadJwtToken(token);
        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());

        // Validate claims
        var claims = jwtToken.Claims.ToList();
        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Name && c.Value == user.UserName);

        // Validate expiration
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ThrowsException_WhenKeyIsMissing()
    {
        // Arrange
        var user = new User
        {
            Id = "12345",
            UserName = "TestUser",
            Email = "test@example.com"
        };

        // Remove the `Jwt:Key` from the mock configuration
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns((string)null);

        var jwtServiceWithMissingKey = new JwtService(_mockConfiguration.Object);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => jwtServiceWithMissingKey.GenerateToken(user));
    }

    [Fact]
    public void GenerateToken_ThrowsException_WhenUserIsNull()
    {
        // Arrange
        User user = null;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _jwtService.GenerateToken(user));
    }
}
