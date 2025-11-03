namespace GiftOfTheGivers.ReliefApi.Models;

public class Donation
{
    public Guid DonationId { get; set; } = Guid.NewGuid();
    public string DonorName { get; set; } = default!;
    public string DonorEmail { get; set; } = default!;
    public string ItemName { get; set; } = default!;
    public int Quantity { get; set; }
    public string Unit { get; set; } = "units";
    public string Location { get; set; } = "";
    public string Status { get; set; } = "Pledged"; // Pledged/Received/Dispatched/Delivered
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


