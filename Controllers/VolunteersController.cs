using GiftOfTheGivers.ReliefApi.Data;
using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VolunteersController(ReliefDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Volunteer v)
    {
        v.VolunteerId = Guid.NewGuid();
        db.Volunteers.Add(v);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = v.VolunteerId }, v);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await db.Volunteers.FindAsync(id));
}


