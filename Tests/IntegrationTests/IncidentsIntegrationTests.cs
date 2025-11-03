using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.IntegrationTests;

public class IncidentsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IncidentsIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var email = $"testuser{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Test User", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        return loginResult!["token"].GetString()!;
    }

    [Fact]
    public async Task CreateIncident_WithAuth_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var incident = new Incident
        {
            Type = "Flood",
            Severity = "High",
            Latitude = -33.9249,
            Longitude = 18.4241,
            Needs = "Food, Water",
            Status = "Open"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetIncidents_ReturnsListOfIncidents()
    {
        // Act
        var response = await _client.GetAsync("/api/incidents");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
        Assert.NotNull(incidents);
    }

    [Fact]
    public async Task GetIncidentById_WithValidId_ReturnsIncident()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        var createResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<Incident>();

        // Act
        var response = await _client.GetAsync($"/api/incidents/{createdIncident!.IncidentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedIncident = await response.Content.ReadFromJsonAsync<Incident>();
        Assert.NotNull(returnedIncident);
        Assert.Equal(createdIncident.IncidentId, returnedIncident.IncidentId);
    }

    [Fact]
    public async Task UpdateIncidentStatus_WithAuth_UpdatesSuccessfully()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var incident = new Incident { Type = "Earthquake", Severity = "High", Status = "Open" };
        var createResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<Incident>();

        var statusDto = new IncidentsController.IncidentStatusDto("InProgress");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/incidents/{createdIncident!.IncidentId}/status?token={token}",
            statusDto
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedIncident = await response.Content.ReadFromJsonAsync<Incident>();
        Assert.Equal("InProgress", updatedIncident!.Status);
    }

    [Fact]
    public async Task ExportIncidentsCsv_ReturnsFile()
    {
        // Act
        var response = await _client.GetAsync("/api/incidents/export");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
    }
}


