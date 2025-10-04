namespace GiftOfTheGivers.ReliefApi.Models;

public class Volunteer
{
    public Guid VolunteerId { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Phone { get; set; } = "";
    public string Skills { get; set; } = "";
    public string HomeBase { get; set; } = "";
    public string Availability { get; set; } = "Available";
}


