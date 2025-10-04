namespace GiftOfTheGivers.ReliefApi.Models;

public class Assignment
{
    public Guid AssignmentId { get; set; } = Guid.NewGuid();
    public Guid VolunteerId { get; set; }
    public Guid IncidentId { get; set; }
    public string TaskDescription { get; set; } = default!;
    public string Status { get; set; } = "Assigned"; // Assigned/InProgress/Completed
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedDate { get; set; }
}


