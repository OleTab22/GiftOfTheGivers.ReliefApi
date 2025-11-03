using GiftOfTheGivers.ReliefApi.Controllers;
using GiftOfTheGivers.ReliefApi.Models;
using GiftOfTheGivers.ReliefApi.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GiftOfTheGivers.ReliefApi.Tests.UnitTests;

public class AssignmentsControllerTests
{
    [Fact]
    public async Task Create_WithValidAssignment_ReturnsCreatedAtAction()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        // Create prerequisite volunteer and incident
        var volunteer = new Volunteer { FullName = "Test Volunteer", Email = "test@example.com" };
        var incident = new Incident { Type = "Flood", Severity = "High" };
        db.Volunteers.Add(volunteer);
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            VolunteerId = volunteer.VolunteerId,
            IncidentId = incident.IncidentId,
            TaskDescription = "Distribute food supplies",
            Status = "Assigned"
        };

        // Act
        var result = await controller.Create(assignment);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result);
        var createdResult = result as CreatedAtActionResult;
        var returnedAssignment = createdResult?.Value as Assignment;
        Assert.NotNull(returnedAssignment);
        Assert.NotEqual(Guid.Empty, returnedAssignment.AssignmentId);
    }

    [Fact]
    public async Task Create_WithInvalidVolunteer_ReturnsBadRequest()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        var incident = new Incident { Type = "Flood", Severity = "High" };
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            VolunteerId = Guid.NewGuid(), // Non-existent volunteer
            IncidentId = incident.IncidentId,
            TaskDescription = "Test task"
        };

        // Act
        var result = await controller.Create(assignment);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_WithInvalidIncident_ReturnsBadRequest()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        var volunteer = new Volunteer { FullName = "Test Volunteer", Email = "test@example.com" };
        db.Volunteers.Add(volunteer);
        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            VolunteerId = volunteer.VolunteerId,
            IncidentId = Guid.NewGuid(), // Non-existent incident
            TaskDescription = "Test task"
        };

        // Act
        var result = await controller.Create(assignment);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsAssignment()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        var volunteer = new Volunteer { FullName = "Test", Email = "test@example.com" };
        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        db.Volunteers.Add(volunteer);
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            VolunteerId = volunteer.VolunteerId,
            IncidentId = incident.IncidentId,
            TaskDescription = "Emergency response"
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        // Act
        var result = await controller.Get(assignment.AssignmentId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var returnedAssignment = okResult?.Value as Assignment;
        Assert.NotNull(returnedAssignment);
        Assert.Equal(assignment.AssignmentId, returnedAssignment.AssignmentId);
    }

    [Fact]
    public async Task ByVolunteer_ReturnsVolunteerAssignments()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        var volunteer1 = new Volunteer { FullName = "Volunteer 1", Email = "v1@example.com" };
        var volunteer2 = new Volunteer { FullName = "Volunteer 2", Email = "v2@example.com" };
        var incident = new Incident { Type = "Flood", Severity = "High" };
        db.Volunteers.AddRange(volunteer1, volunteer2);
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        db.Assignments.Add(new Assignment { VolunteerId = volunteer1.VolunteerId, IncidentId = incident.IncidentId, TaskDescription = "Task 1" });
        db.Assignments.Add(new Assignment { VolunteerId = volunteer1.VolunteerId, IncidentId = incident.IncidentId, TaskDescription = "Task 2" });
        db.Assignments.Add(new Assignment { VolunteerId = volunteer2.VolunteerId, IncidentId = incident.IncidentId, TaskDescription = "Task 3" });
        await db.SaveChangesAsync();

        // Act
        var result = await controller.ByVolunteer(volunteer1.VolunteerId);

        // Assert
        var assignments = result.ToList();
        Assert.Equal(2, assignments.Count);
        Assert.All(assignments, a => Assert.Equal(volunteer1.VolunteerId, a.VolunteerId));
    }

    [Fact]
    public async Task Complete_WithValidId_MarksAsCompleted()
    {
        // Arrange
        var db = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var controller = new AssignmentsController(db);
        
        var volunteer = new Volunteer { FullName = "Test", Email = "test@example.com" };
        var incident = new Incident { Type = "Fire", Severity = "Medium" };
        db.Volunteers.Add(volunteer);
        db.Incidents.Add(incident);
        await db.SaveChangesAsync();

        var assignment = new Assignment
        {
            VolunteerId = volunteer.VolunteerId,
            IncidentId = incident.IncidentId,
            TaskDescription = "Test task",
            Status = "Assigned"
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();

        // Act
        var dto = new AssignmentsController.CompleteDto(true);
        var result = await controller.Complete(assignment.AssignmentId, dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        var okResult = result as OkObjectResult;
        var completedAssignment = okResult?.Value as Assignment;
        Assert.NotNull(completedAssignment);
        Assert.Equal("Completed", completedAssignment.Status);
        Assert.NotNull(completedAssignment.CompletedDate);
    }
}


