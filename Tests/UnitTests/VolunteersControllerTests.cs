using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class VolunteersControllerTests
{
    [Fact]
    public async Task Create_WithValidVolunteer_ReturnsCreatedAtAction()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new VolunteersController(db);
        var volunteer = new Volunteer
        {
            FullName = "Jane Smith",
            Email = "jane@example.com",
            Phone = "0123456789",
            Skills = "Medical, First Aid",
            HomeBase = "Cape Town",
            Availability = "Available"
        };

        // Act
        var result = await controller.Create(volunteer);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.NotNull(createdResult);
        
        var returnedVolunteer = createdResult.Value as Volunteer;
        Assert.NotNull(returnedVolunteer);
        Assert.NotEqual(Guid.Empty, returnedVolunteer.VolunteerId);
        Assert.Equal("Jane Smith", returnedVolunteer.FullName);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsVolunteer()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new VolunteersController(db);
        var volunteer = new Volunteer 
        { 
            FullName = "John Doe", 
            Email = "john@example.com",
            Skills = "Engineering"
        };
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync();

        // Act
        var result = await controller.Get(volunteer.VolunteerId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedVolunteer = okResult?.Value as Volunteer;
        Assert.NotNull(returnedVolunteer);
        Assert.Equal(volunteer.VolunteerId, returnedVolunteer.VolunteerId);
        Assert.Equal("John Doe", returnedVolunteer.FullName);
    }

    [Fact]
    public async Task Get_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new VolunteersController(db);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await controller.Get(nonExistingId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        Assert.Null(okResult?.Value); // Controller returns Ok(null) when not found
    }
}


