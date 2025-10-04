using GiftOfTheGivers.ReliefApi.Data;
using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DonationsController(ReliefDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Donation d)
    {
        d.DonationId = Guid.NewGuid();
        d.CreatedAt = DateTime.UtcNow;
        db.Donations.Add(d);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = d.DonationId }, d);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
        => Ok(await db.Donations.FindAsync(id));

    [HttpGet]
    public async Task<IEnumerable<Donation>> List([FromQuery] string? status)
    {
        var q = db.Donations.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(d => d.Status == status);
        return await q.OrderByDescending(d => d.CreatedAt).ToListAsync();
    }

    public record DonationStatusDto(string Status);

    [Authorize]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, DonationStatusDto dto)
    {
        var d = await db.Donations.FindAsync(id);
        if (d is null) return NotFound();
        d.Status = dto.Status;
        await db.SaveChangesAsync();
        return Ok(d);
    }
}


