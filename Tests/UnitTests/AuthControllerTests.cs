using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_WithNewUser_ReturnsOk()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var jwtService = MockJwtTokenService.CreateService();
        var controller = new AuthController(db, jwtService);
        var registerDto = new AuthController.RegisterDto("John Doe", "john@example.com", "password123");

        // Act
        var result = await controller.Register(registerDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ReturnsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var jwtService = MockJwtTokenService.CreateService();
        db.Users.Add(new User 
        { 
            Email = "existing@example.com", 
            FullName = "Existing User",
            PasswordHash = "hash" 
        });
        await db.SaveChangesAsync();

        var controller = new AuthController(db, jwtService);
        var registerDto = new AuthController.RegisterDto("New User", "existing@example.com", "password123");

        // Act
        var result = await controller.Register(registerDto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var jwtService = MockJwtTokenService.CreateService();
        var controller = new AuthController(db, jwtService);
        
        // First register a user
        var registerDto = new AuthController.RegisterDto("Test User", "test@example.com", "password123");
        await controller.Register(registerDto);

        // Act
        var loginDto = new AuthController.LoginDto("test@example.com", "password123");
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.NotNull(okResult);
        var value = okResult.Value;
        Assert.NotNull(value);
        
        // Check if token property exists
        var tokenProperty = value.GetType().GetProperty("token");
        Assert.NotNull(tokenProperty);
        var token = tokenProperty.GetValue(value) as string;
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var jwtService = MockJwtTokenService.CreateService();
        var controller = new AuthController(db, jwtService);
        var loginDto = new AuthController.LoginDto("nonexistent@example.com", "password123");

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var jwtService = MockJwtTokenService.CreateService();
        var controller = new AuthController(db, jwtService);
        
        // Register a user
        var registerDto = new AuthController.RegisterDto("Test User", "test@example.com", "password123");
        await controller.Register(registerDto);

        // Act - attempt login with wrong password
        var loginDto = new AuthController.LoginDto("test@example.com", "wrongpassword");
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}


