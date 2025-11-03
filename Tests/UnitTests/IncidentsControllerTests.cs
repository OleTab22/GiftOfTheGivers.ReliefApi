using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class IncidentsControllerTests
{
    [Fact]
    public async Task Create_WithValidIncident_ReturnsCreatedAtAction()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        var incident = new Incident
        {
            Type = "Flood",
            Severity = "High",
            Latitude = -33.9249,
            Longitude = 18.4241,
            Needs = "Food, Water, Shelter",
            Status = "Open"
        };

        // Act
        var result = await controller.Create(incident);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        Assert.NotNull(createdResult);
        Assert.Equal(nameof(controller.Get), createdResult.ActionName);
        
        var returnedIncident = createdResult.Value as Incident;
        Assert.NotNull(returnedIncident);
        Assert.NotEqual(Guid.Empty, returnedIncident.IncidentId);
        Assert.Equal("Flood", returnedIncident.Type);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsIncident()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        // Act
        var result = await controller.Get(incident.IncidentId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedIncident = okResult?.Value as Incident;
        Assert.NotNull(returnedIncident);
        Assert.Equal(incident.IncidentId, returnedIncident.IncidentId);
    }

    [Fact]
    public async Task Get_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await controller.Get(nonExistingId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task List_WithNoFilters_ReturnsAllIncidents()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        db.Incidents.Add(new Incident { Type = "Flood", Severity = "High" });
        db.Incidents.Add(new Incident { Type = "Fire", Severity = "Low" });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.List(null, null);

        // Assert
        var incidents = result.ToList();
        Assert.Equal(2, incidents.Count);
    }

    [Fact]
    public async Task List_WithStatusFilter_ReturnsFilteredIncidents()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        db.Incidents.Add(new Incident { Type = "Flood", Status = "Open" });
        db.Incidents.Add(new Incident { Type = "Fire", Status = "Resolved" });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.List("Open", null);

        // Assert
        var incidents = result.ToList();
        Assert.Single(incidents);
        Assert.Equal("Open", incidents[0].Status);
    }

    [Fact]
    public async Task UpdateStatus_WithValidId_UpdatesStatus()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        var incident = new Incident { Type = "Earthquake", Status = "Open" };
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        // Act
        var statusDto = new IncidentsController.IncidentStatusDto("InProgress");
        var result = await controller.UpdateStatus(incident.IncidentId, statusDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var updatedIncident = okResult?.Value as Incident;
        Assert.NotNull(updatedIncident);
        Assert.Equal("InProgress", updatedIncident.Status);
    }

    [Fact]
    public async Task ExportCsv_ReturnsFileContentResult()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new IncidentsController(db);
        db.Incidents.Add(new Incident { Type = "Flood", Severity = "High", Needs = "Food" });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.ExportCsv(null);

        // Assert
        Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/csv", result.ContentType);
        Assert.Equal("incidents.csv", result.FileDownloadName);
    }
}


