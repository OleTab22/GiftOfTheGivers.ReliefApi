namespace GiftOfTheGivers.ReliefApi.Models;

public class Incident
{
    public Guid IncidentId { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = default!;
    public string Severity { get; set; } = "Low";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Needs { get; set; } = "";
    public string Status { get; set; } = "Open"; // Open/InProgress/Resolved
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


