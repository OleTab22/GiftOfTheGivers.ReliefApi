using GiftOfTheGivers.ReliefApi.Data;
using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssignmentsController(ReliefDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Assignment a)
    {
        if (!await db.Volunteers.AnyAsync(v => v.VolunteerId == a.VolunteerId) ||
            !await db.Incidents.AnyAsync(i => i.IncidentId == a.IncidentId))
            return BadRequest("Invalid volunteer or incident");

        a.AssignmentId = Guid.NewGuid();
        a.AssignedDate = DateTime.UtcNow;
        db.Assignments.Add(a);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = a.AssignmentId }, a);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var a = await db.Assignments.FindAsync(id);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("by-volunteer/{volunteerId}")]
    public async Task<IEnumerable<Assignment>> ByVolunteer(Guid volunteerId)
        => await db.Assignments.Where(x => x.VolunteerId == volunteerId).ToListAsync();

    public record CompleteDto(bool Completed);

    [Authorize]
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> Complete(Guid id, CompleteDto dto)
    {
        var a = await db.Assignments.FindAsync(id);
        if (a is null) return NotFound();
        if (dto.Completed)
        {
            a.Status = "Completed";
            a.CompletedDate = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
        return Ok(a);
    }
}


