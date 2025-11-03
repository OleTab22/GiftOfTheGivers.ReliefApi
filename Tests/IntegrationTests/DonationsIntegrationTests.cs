using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.IntegrationTests;

public class DonationsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DonationsIntegrationTests(CustomWebApplicationFactory factory)
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
    public async Task CreateDonation_WithAuth_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var donation = new Donation
        {
            DonorName = "Generous Donor",
            DonorEmail = "donor@example.com",
            ItemName = "Blankets",
            Quantity = 100,
            Unit = "pieces",
            Location = "Warehouse A"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/donations?token={token}", donation);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdDonation = await response.Content.ReadFromJsonAsync<Donation>();
        Assert.NotNull(createdDonation);
        Assert.Equal("Blankets", createdDonation.ItemName);
    }

    [Fact]
    public async Task GetDonations_ReturnsListOfDonations()
    {
        // Act
        var response = await _client.GetAsync("/api/donations");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var donations = await response.Content.ReadFromJsonAsync<List<Donation>>();
        Assert.NotNull(donations);
    }

    [Fact]
    public async Task GetDonationById_WithValidId_ReturnsDonation()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var donation = new Donation 
        { 
            DonorName = "Test Donor", 
            DonorEmail = "test@example.com",
            ItemName = "Water",
            Quantity = 50
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/donations?token={token}", donation);
        var createdDonation = await createResponse.Content.ReadFromJsonAsync<Donation>();

        // Act
        var response = await _client.GetAsync($"/api/donations/{createdDonation!.DonationId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var returnedDonation = await response.Content.ReadFromJsonAsync<Donation>();
        Assert.NotNull(returnedDonation);
    }

    [Fact]
    public async Task UpdateDonationStatus_WithAuth_UpdatesSuccessfully()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        var donation = new Donation 
        { 
            DonorName = "Status Test", 
            DonorEmail = "status@example.com",
            ItemName = "Medical Supplies",
            Quantity = 200,
            Status = "Pledged"
        };
        var createResponse = await _client.PostAsJsonAsync($"/api/donations?token={token}", donation);
        var createdDonation = await createResponse.Content.ReadFromJsonAsync<Donation>();

        var statusDto = new DonationsController.DonationStatusDto("Received");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/donations/{createdDonation!.DonationId}/status?token={token}",
            statusDto
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedDonation = await response.Content.ReadFromJsonAsync<Donation>();
        Assert.Equal("Received", updatedDonation!.Status);
    }

    [Fact]
    public async Task ListDonations_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        await _client.PostAsJsonAsync($"/api/donations?token={token}", 
            new Donation { DonorName = "D1", DonorEmail = "d1@e.com", ItemName = "Item1", Quantity = 10, Status = "Pledged" });
        await _client.PostAsJsonAsync($"/api/donations?token={token}", 
            new Donation { DonorName = "D2", DonorEmail = "d2@e.com", ItemName = "Item2", Quantity = 20, Status = "Delivered" });

        // Act
        var response = await _client.GetAsync("/api/donations?status=Pledged");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var donations = await response.Content.ReadFromJsonAsync<List<Donation>>();
        Assert.NotNull(donations);
        Assert.Contains(donations, d => d.Status == "Pledged");
    }
}


