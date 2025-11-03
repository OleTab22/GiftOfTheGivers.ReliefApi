using System.Net;
using System.Net.Http.Json;
using GiftOfTheGivers.ReliefApi.Controllers;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var registerDto = new AuthController.RegisterDto(
            "Integration Test User",
            $"integration{Guid.NewGuid()}@example.com",
            "password123"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var email = $"logintest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Login Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("token"));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new AuthController.LoginDto("nonexistent@example.com", "wrongpassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_Login_Me_FullAuthFlow()
    {
        // Arrange
        var email = $"fullflow{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Full Flow User", email, "password123");

        // Act 1: Register
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act 2: Login
        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>();
        Assert.NotNull(loginResult);
        var token = loginResult["token"].GetString();
        Assert.NotNull(token);

        // Act 3: Access protected endpoint with token
        var meResponse = await _client.GetAsync($"/api/auth/me?token={token}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
    }
}


