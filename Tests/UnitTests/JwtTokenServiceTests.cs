using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class JwtTokenServiceTests
{
    [Fact]
    public void Create_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var jwtService = MockJwtTokenService.CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test@example.com",
            Role = "User"
        };

        // Act
        var token = jwtService.Create(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        
        // Validate token structure
        var handler = new JwtSecurityTokenHandler();
        Assert.True(handler.CanReadToken(token));
    }

    [Fact]
    public void Create_TokenContainsCorrectClaims()
    {
        // Arrange
        var jwtService = MockJwtTokenService.CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Role = "Admin"
        };

        // Act
        var token = jwtService.Create(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.UserId.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        Assert.Contains(jwtToken.Claims, c => c.Type == "name" && c.Value == user.FullName);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == user.Role);
    }

    [Fact]
    public void Create_TokenHasExpiryTime()
    {
        // Arrange
        var jwtService = MockJwtTokenService.CreateService();
        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Test User",
            Email = "test@example.com",
            Role = "User"
        };

        // Act
        var token = jwtService.Create(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
    }
}


