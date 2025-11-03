using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class DonationsControllerTests
{
    [Fact]
    public async Task Create_WithValidDonation_ReturnsCreatedAtAction()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        var donation = new Donation
        {
            DonorName = "John Donor",
            DonorEmail = "donor@example.com",
            ItemName = "Blankets",
            Quantity = 50,
            Unit = "pieces",
            Location = "Warehouse A",
            Status = "Pledged"
        };

        // Act
        var result = await controller.Create(donation);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        var returnedDonation = createdResult?.Value as Donation;
        Assert.NotNull(returnedDonation);
        Assert.NotEqual(Guid.Empty, returnedDonation.DonationId);
        Assert.Equal("Blankets", returnedDonation.ItemName);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsDonation()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        var donation = new Donation
        {
            DonorName = "Jane Donor",
            DonorEmail = "jane@example.com",
            ItemName = "Food Parcels",
            Quantity = 100
        };
        db.Donations.Add(donation);
        await db.SaveChangesAsync();

        // Act
        var result = await controller.Get(donation.DonationId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedDonation = okResult?.Value as Donation;
        Assert.NotNull(returnedDonation);
        Assert.Equal(donation.DonationId, returnedDonation.DonationId);
    }

    [Fact]
    public async Task List_WithNoFilter_ReturnsAllDonations()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        db.Donations.Add(new Donation { DonorName = "Donor 1", DonorEmail = "d1@example.com", ItemName = "Water", Quantity = 10 });
        db.Donations.Add(new Donation { DonorName = "Donor 2", DonorEmail = "d2@example.com", ItemName = "Food", Quantity = 20 });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.List(null);

        // Assert
        var donations = result.ToList();
        Assert.Equal(2, donations.Count);
    }

    [Fact]
    public async Task List_WithStatusFilter_ReturnsFilteredDonations()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        db.Donations.Add(new Donation { DonorName = "Donor 1", DonorEmail = "d1@example.com", ItemName = "Water", Status = "Pledged", Quantity = 10 });
        db.Donations.Add(new Donation { DonorName = "Donor 2", DonorEmail = "d2@example.com", ItemName = "Food", Status = "Delivered", Quantity = 20 });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.List("Pledged");

        // Assert
        var donations = result.ToList();
        Assert.Single(donations);
        Assert.Equal("Pledged", donations[0].Status);
    }

    [Fact]
    public async Task UpdateStatus_WithValidId_UpdatesStatus()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        var donation = new Donation
        {
            DonorName = "Test Donor",
            DonorEmail = "test@example.com",
            ItemName = "Medical Supplies",
            Quantity = 100,
            Status = "Pledged"
        };
        db.Donations.Add(donation);
        await db.SaveChangesAsync();

        // Act
        var statusDto = new DonationsController.DonationStatusDto("Received");
        var result = await controller.UpdateStatus(donation.DonationId, statusDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var updatedDonation = okResult?.Value as Donation;
        Assert.NotNull(updatedDonation);
        Assert.Equal("Received", updatedDonation.Status);
    }

    [Fact]
    public async Task UpdateStatus_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new DonationsController(db);
        var statusDto = new DonationsController.DonationStatusDto("Delivered");

        // Act
        var result = await controller.UpdateStatus(Guid.NewGuid(), statusDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}


