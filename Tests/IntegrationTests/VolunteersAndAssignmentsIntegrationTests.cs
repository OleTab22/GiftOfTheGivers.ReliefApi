using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.IntegrationTests;

public class VolunteersAndAssignmentsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public VolunteersAndAssignmentsIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateVolunteer_WithAuth_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var volunteer = new Volunteer
        {
            FullName = "John Volunteer",
            Email = "john.vol@example.com",
            Phone = "0123456789",
            Skills = "Medical, First Aid",
            HomeBase = "Cape Town",
            Availability = "Available"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdVolunteer = await response.Content.ReadFromJsonAsync<Volunteer>();
        Assert.NotNull(createdVolunteer);
        Assert.Equal("John Volunteer", createdVolunteer.FullName);
    }

    [Fact]
    public async Task GetVolunteerById_WithValidId_ReturnsVolunteer()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var volunteer = new Volunteer 
        { 
            FullName = "Test Vol", 
            Email = "testvol@example.com",
            Skills = "Engineering"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await createResponse.Content.ReadFromJsonAsync<Volunteer>();

        // Act
        var response = await _client.GetAsync($"/api/volunteers/{createdVolunteer!.VolunteerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateAssignment_WithValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        // Create volunteer
        var volunteer = new Volunteer { FullName = "Vol", Email = "vol@e.com", Skills = "Any" };
        var volResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await volResponse.Content.ReadFromJsonAsync<Volunteer>();

        // Create incident
        var incident = new Incident { Type = "Flood", Severity = "High" };
        var incResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await incResponse.Content.ReadFromJsonAsync<Incident>();

        // Create assignment
        var assignment = new Assignment
        {
            VolunteerId = createdVolunteer!.VolunteerId,
            IncidentId = createdIncident!.IncidentId,
            TaskDescription = "Distribute supplies",
            Status = "Assigned"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/assignments?token={token}", assignment);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdAssignment = await response.Content.ReadFromJsonAsync<Assignment>();
        Assert.NotNull(createdAssignment);
    }

    [Fact]
    public async Task GetAssignmentsByVolunteer_ReturnsVolunteerAssignments()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        var volunteer = new Volunteer { FullName = "Multi Vol", Email = "multi@e.com", Skills = "Any" };
        var volResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await volResponse.Content.ReadFromJsonAsync<Volunteer>();

        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        var incResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await incResponse.Content.ReadFromJsonAsync<Incident>();

        // Create multiple assignments
        await _client.PostAsJsonAsync($"/api/assignments?token={token}", 
            new Assignment { VolunteerId = createdVolunteer!.VolunteerId, IncidentId = createdIncident!.IncidentId, TaskDescription = "Task 1" });
        await _client.PostAsJsonAsync($"/api/assignments?token={token}", 
            new Assignment { VolunteerId = createdVolunteer.VolunteerId, IncidentId = createdIncident.IncidentId, TaskDescription = "Task 2" });

        // Act
        var response = await _client.GetAsync($"/api/assignments/by-volunteer/{createdVolunteer.VolunteerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assignments = await response.Content.ReadFromJsonAsync<List<Assignment>>();
        Assert.NotNull(assignments);
        Assert.True(assignments.Count >= 2);
    }

    [Fact]
    public async Task CompleteAssignment_WithAuth_MarksAsCompleted()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        
        var volunteer = new Volunteer { FullName = "Comp Vol", Email = "comp@e.com", Skills = "Any" };
        var volResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await volResponse.Content.ReadFromJsonAsync<Volunteer>();

        var incident = new Incident { Type = "Earthquake", Severity = "High" };
        var incResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await incResponse.Content.ReadFromJsonAsync<Incident>();

        var assignment = new Assignment
        {
            VolunteerId = createdVolunteer!.VolunteerId,
            IncidentId = createdIncident!.IncidentId,
            TaskDescription = "Complete this task",
            Status = "Assigned"
        };
        var assignResponse = await _client.PostAsJsonAsync($"/api/assignments?token={token}", assignment);
        var createdAssignment = await assignResponse.Content.ReadFromJsonAsync<Assignment>();

        var completeDto = new AssignmentsController.CompleteDto(true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/assignments/{createdAssignment!.AssignmentId}/complete?token={token}",
            completeDto
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var completedAssignment = await response.Content.ReadFromJsonAsync<Assignment>();
        Assert.Equal("Completed", completedAssignment!.Status);
        Assert.NotNull(completedAssignment.CompletedDate);
    }
}


