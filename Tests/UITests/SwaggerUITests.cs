using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UITests;

/// <summary>
/// Functional UI Tests for Swagger Interface
/// Tests user interactions, form submissions, navigation paths, and error-handling
/// </summary>
public class SwaggerUITests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SwaggerUITests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region Functional UI Tests - Swagger Interface Elements

    [Fact]
    public async Task SwaggerUI_LoadsSuccessfully()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/index.html");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("swagger", content.ToLower());
        Assert.Contains("GiftOfTheGivers", content);
    }

    [Fact]
    public async Task SwaggerUI_ApiDocumentationAvailable()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var swaggerDoc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(swaggerDoc.ValueKind != JsonValueKind.Null && swaggerDoc.ValueKind != JsonValueKind.Undefined);
        Assert.True(swaggerDoc.TryGetProperty("info", out var info));
        Assert.True(info.TryGetProperty("title", out var title));
        Assert.Contains("GiftOfTheGivers", title.GetString()!);
    }

    [Fact]
    public async Task SwaggerUI_AllEndpointsDocumented()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/swagger/v1/swagger.json");
        var swaggerDoc = await response.Content.ReadFromJsonAsync<JsonElement>();
        
        // Assert
        Assert.True(swaggerDoc.TryGetProperty("paths", out var paths));
        
        // Verify key endpoints are documented
        var pathsObj = paths.EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains("/api/Auth/register", pathsObj); // Note: Auth is capitalized (controller name)
        Assert.Contains("/api/Auth/login", pathsObj);
        Assert.Contains("/api/Incidents", pathsObj);
        Assert.Contains("/api/Donations", pathsObj);
        Assert.Contains("/api/Volunteers", pathsObj);
        Assert.Contains("/api/Assignments", pathsObj);
    }

    #endregion

    #region Form Submissions - User Interactions

    [Fact]
    public async Task UserRegistration_FormSubmissionWorks()
    {
        // Arrange
        var registerDto = new AuthController.RegisterDto(
            "UI Test User",
            $"uitest{Guid.NewGuid()}@example.com",
            "password123"
        );

        // Act - Simulate form submission
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(result.TryGetProperty("userId", out _));
        Assert.True(result.TryGetProperty("fullName", out var fullName));
        Assert.Equal("UI Test User", fullName.GetString());
    }

    [Fact]
    public async Task UserLogin_FormSubmissionWorks()
    {
        // Arrange
        var email = $"logintest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Login Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");

        // Act - Simulate login form submission
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("token"));
        Assert.StartsWith("eyJ", result["token"].GetString()!);
    }

    [Fact]
    public async Task CreateIncident_FormSubmissionWithAuth()
    {
        // Arrange - Get auth token
        var email = $"incidenttest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Incident Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        var incident = new Incident
        {
            Type = "Flood",
            Severity = "High",
            Latitude = -33.9249,
            Longitude = 18.4241,
            Needs = "Food, Water, Shelter",
            Status = "Open"
        };

        // Act - Simulate form submission with authentication
        var response = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var createdIncident = await response.Content.ReadFromJsonAsync<Incident>();
        Assert.NotNull(createdIncident);
        Assert.Equal("Flood", createdIncident.Type);
        Assert.NotEqual(Guid.Empty, createdIncident.IncidentId);
    }

    [Fact]
    public async Task CreateDonation_FormSubmissionWithAuth()
    {
        // Arrange - Get auth token
        var email = $"donationtest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Donation Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        var donation = new Donation
        {
            DonorName = "Generous Donor",
            DonorEmail = "donor@example.com",
            ItemName = "Blankets",
            Quantity = 100,
            Unit = "pieces",
            Location = "Warehouse A",
            Status = "Pledged"
        };

        // Act - Simulate form submission
        var response = await _client.PostAsJsonAsync($"/api/donations?token={token}", donation);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var createdDonation = await response.Content.ReadFromJsonAsync<Donation>();
        Assert.NotNull(createdDonation);
        Assert.Equal("Blankets", createdDonation.ItemName);
    }

    [Fact]
    public async Task CreateVolunteer_FormSubmissionWithAuth()
    {
        // Arrange - Get auth token
        var email = $"volunteertest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Volunteer Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        var volunteer = new Volunteer
        {
            FullName = "Jane Volunteer",
            Email = "jane.vol@example.com",
            Phone = "0123456789",
            Skills = "Medical, First Aid",
            HomeBase = "Cape Town",
            Availability = "Available"
        };

        // Act - Simulate form submission
        var response = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        var createdVolunteer = await response.Content.ReadFromJsonAsync<Volunteer>();
        Assert.NotNull(createdVolunteer);
        Assert.Equal("Jane Volunteer", createdVolunteer.FullName);
    }

    #endregion

    #region Navigation Paths - Endpoint Access

    [Fact]
    public async Task Navigation_GetIncidentsList()
    {
        // Act - Navigate to incidents list
        var response = await _client.GetAsync("/api/incidents");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
        Assert.NotNull(incidents);
    }

    [Fact]
    public async Task Navigation_GetIncidentById()
    {
        // Arrange - Create incident first
        var email = $"navtest{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Nav Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        var createResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<Incident>();

        // Act - Navigate to specific incident
        var response = await _client.GetAsync($"/api/incidents/{createdIncident!.IncidentId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var retrievedIncident = await response.Content.ReadFromJsonAsync<Incident>();
        Assert.NotNull(retrievedIncident);
        Assert.Equal(createdIncident.IncidentId, retrievedIncident.IncidentId);
    }

    [Fact]
    public async Task Navigation_GetDonationsList()
    {
        // Act - Navigate to donations list
        var response = await _client.GetAsync("/api/donations");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var donations = await response.Content.ReadFromJsonAsync<List<Donation>>();
        Assert.NotNull(donations);
    }

    [Fact]
    public async Task Navigation_GetVolunteerById()
    {
        // Arrange - Create volunteer first
        var email = $"volnav{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Vol Nav", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        var volunteer = new Volunteer { FullName = "Nav Vol", Email = "nav@e.com", Skills = "Any" };
        var createResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await createResponse.Content.ReadFromJsonAsync<Volunteer>();

        // Act - Navigate to specific volunteer
        var response = await _client.GetAsync($"/api/volunteers/{createdVolunteer!.VolunteerId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var retrievedVolunteer = await response.Content.ReadFromJsonAsync<Volunteer>();
        Assert.NotNull(retrievedVolunteer);
        Assert.Equal(createdVolunteer.VolunteerId, retrievedVolunteer.VolunteerId);
    }

    [Fact]
    public async Task Navigation_ExportIncidentsCSV()
    {
        // Act - Navigate to export endpoint
        var response = await _client.GetAsync("/api/incidents/export");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        var csvContent = await response.Content.ReadAsStringAsync();
        Assert.NotNull(csvContent);
        Assert.Contains("incidentId", csvContent); // CSV header uses lowercase
    }

    #endregion

    #region Error Handling Mechanisms

    [Fact]
    public async Task ErrorHandling_UnauthorizedAccess_Returns401()
    {
        // Arrange - Don't provide authentication token
        var incident = new Incident { Type = "Test", Severity = "Low" };

        // Act - Attempt to access protected endpoint without auth
        var response = await _client.PostAsJsonAsync("/api/incidents", incident);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_InvalidLogin_Returns401()
    {
        // Arrange
        var loginDto = new AuthController.LoginDto("nonexistent@example.com", "wrongpassword");

        // Act - Attempt login with invalid credentials
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_DuplicateEmail_Returns409()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Test User", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Act - Attempt to register with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_InvalidIncidentId_Returns404()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act - Attempt to get non-existent incident
        var response = await _client.GetAsync($"/api/incidents/{invalidId}");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandling_InvalidAssignment_Returns400()
    {
        // Arrange - Get auth token
        var email = $"assignmenterror{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Error Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        // Arrange - Invalid assignment (non-existent volunteer/incident)
        var assignment = new Assignment
        {
            VolunteerId = Guid.NewGuid(), // Non-existent
            IncidentId = Guid.NewGuid(),  // Non-existent
            TaskDescription = "Test task",
            Status = "Assigned"
        };

        // Act - Attempt to create assignment with invalid IDs
        var response = await _client.PostAsJsonAsync($"/api/assignments?token={token}", assignment);

        // Assert - Should return 400 Bad Request or 404 Not Found
        Assert.True(
            response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
            response.StatusCode == System.Net.HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task ErrorHandling_MalformedRequest_Returns400()
    {
        // Arrange - Invalid JSON structure
        var invalidContent = new StringContent("{invalid json}", System.Text.Encoding.UTF8, "application/json");

        // Act - Send malformed request
        var response = await _client.PostAsync("/api/auth/register", invalidContent);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Complete User Workflows

    [Fact]
    public async Task CompleteWorkflow_Register_Login_CreateIncident_UpdateStatus()
    {
        // Step 1: Register
        var email = $"workflow{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Workflow User", email, "password123");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(System.Net.HttpStatusCode.OK, registerResponse.StatusCode);

        // Step 2: Login
        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        Assert.Equal(System.Net.HttpStatusCode.OK, loginResponse.StatusCode);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        // Step 3: Create Incident
        var incident = new Incident { Type = "Earthquake", Severity = "High", Status = "Open" };
        var createResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        Assert.Equal(System.Net.HttpStatusCode.Created, createResponse.StatusCode);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<Incident>();

        // Step 4: Update Status
        var statusDto = new IncidentsController.IncidentStatusDto("InProgress");
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/incidents/{createdIncident!.IncidentId}/status?token={token}",
            statusDto
        );
        Assert.Equal(System.Net.HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedIncident = await updateResponse.Content.ReadFromJsonAsync<Incident>();
        Assert.Equal("InProgress", updatedIncident!.Status);
    }

    [Fact]
    public async Task CompleteWorkflow_Volunteer_Assignment_Lifecycle()
    {
        // Arrange - Get auth token
        var email = $"assignmentflow{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Flow User", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        // Step 1: Create Volunteer
        var volunteer = new Volunteer { FullName = "Flow Vol", Email = "flow@e.com", Skills = "Any" };
        var volResponse = await _client.PostAsJsonAsync($"/api/volunteers?token={token}", volunteer);
        var createdVolunteer = await volResponse.Content.ReadFromJsonAsync<Volunteer>();

        // Step 2: Create Incident
        var incident = new Incident { Type = "Flood", Severity = "High" };
        var incResponse = await _client.PostAsJsonAsync($"/api/incidents?token={token}", incident);
        var createdIncident = await incResponse.Content.ReadFromJsonAsync<Incident>();

        // Step 3: Create Assignment
        var assignment = new Assignment
        {
            VolunteerId = createdVolunteer!.VolunteerId,
            IncidentId = createdIncident!.IncidentId,
            TaskDescription = "Distribute supplies",
            Status = "Assigned"
        };
        var assignResponse = await _client.PostAsJsonAsync($"/api/assignments?token={token}", assignment);
        Assert.Equal(System.Net.HttpStatusCode.Created, assignResponse.StatusCode);
        var createdAssignment = await assignResponse.Content.ReadFromJsonAsync<Assignment>();

        // Step 4: Complete Assignment
        var completeDto = new AssignmentsController.CompleteDto(true);
        var completeResponse = await _client.PutAsJsonAsync(
            $"/api/assignments/{createdAssignment!.AssignmentId}/complete?token={token}",
            completeDto
        );
        Assert.Equal(System.Net.HttpStatusCode.OK, completeResponse.StatusCode);
        var completedAssignment = await completeResponse.Content.ReadFromJsonAsync<Assignment>();
        Assert.Equal("Completed", completedAssignment!.Status);
        Assert.NotNull(completedAssignment.CompletedDate);
    }

    #endregion

    #region Query Parameters and Filtering

    [Fact]
    public async Task Navigation_FilterIncidentsByStatus()
    {
        // Arrange - Create incidents with different statuses
        var email = $"filter{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Filter Test", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        await _client.PostAsJsonAsync($"/api/incidents?token={token}", 
            new Incident { Type = "Fire", Status = "Open" });
        await _client.PostAsJsonAsync($"/api/incidents?token={token}", 
            new Incident { Type = "Earthquake", Status = "Closed" });

        // Act - Filter by status
        var response = await _client.GetAsync("/api/incidents?status=Open");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var incidents = await response.Content.ReadFromJsonAsync<List<Incident>>();
        Assert.NotNull(incidents);
        Assert.All(incidents, i => Assert.Equal("Open", i.Status));
    }

    [Fact]
    public async Task Navigation_FilterDonationsByStatus()
    {
        // Arrange - Create donations with different statuses
        var email = $"donfilter{Guid.NewGuid()}@example.com";
        var registerDto = new AuthController.RegisterDto("Don Filter", email, "password123");
        await _client.PostAsJsonAsync("/api/auth/register", registerDto);

        var loginDto = new AuthController.LoginDto(email, "password123");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>>();
        var token = loginResult!["token"].GetString()!;

        await _client.PostAsJsonAsync($"/api/donations?token={token}", 
            new Donation { DonorName = "D1", DonorEmail = "d1@e.com", ItemName = "Item1", Quantity = 10, Status = "Pledged" });
        await _client.PostAsJsonAsync($"/api/donations?token={token}", 
            new Donation { DonorName = "D2", DonorEmail = "d2@e.com", ItemName = "Item2", Quantity = 20, Status = "Delivered" });

        // Act - Filter by status
        var response = await _client.GetAsync("/api/donations?status=Pledged");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var donations = await response.Content.ReadFromJsonAsync<List<Donation>>();
        Assert.NotNull(donations);
        Assert.All(donations, d => Assert.Equal("Pledged", d.Status));
    }

    #endregion
}

