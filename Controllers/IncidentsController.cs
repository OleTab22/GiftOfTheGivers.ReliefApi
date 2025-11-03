using System.Text;
using GiftOfTheGivers.ReliefApi.Data;
using GiftOfTheGivers.ReliefApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.ReliefApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController(ReliefDbContext db) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Incident inc)
    {
        inc.IncidentId = Guid.NewGuid();
        inc.CreatedAt = DateTime.UtcNow;
        db.Incidents.Add(inc);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = inc.IncidentId }, inc);
    }

    [HttpGet]
    public async Task<IEnumerable<Incident>> List([FromQuery] string? status, [FromQuery] string? severity)
    {
        var q = db.Incidents.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(i => i.Status == status);
        if (!string.IsNullOrWhiteSpace(severity)) q = q.Where(i => i.Severity == severity);
        return await q.OrderByDescending(i => i.CreatedAt).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var inc = await db.Incidents.FindAsync(id);
        return inc is null ? NotFound() : Ok(inc);
    }

    public record IncidentStatusDto(string Status);

    [Authorize]
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, IncidentStatusDto dto)
    {
        var inc = await db.Incidents.FindAsync(id);
        if (inc is null) return NotFound();
        inc.Status = dto.Status;
        await db.SaveChangesAsync();
        return Ok(inc);
    }

    [HttpGet("export")]
    public async Task<FileContentResult> ExportCsv([FromQuery] string? status)
    {
        var q = db.Incidents.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(i => i.Status == status);
        var list = await q.OrderByDescending(i => i.CreatedAt).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("incidentId,type,severity,status,latitude,longitude,needs,createdAt");
        foreach (var i in list)
            sb.AppendLine($"{i.IncidentId},{i.Type},{i.Severity},{i.Status},{i.Latitude},{i.Longitude},\"{i.Needs}\",{i.CreatedAt:o}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "incidents.csv");
    }
}


