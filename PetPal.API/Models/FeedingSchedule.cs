namespace PetPal.API.Models;

public class FeedingSchedule
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public TimeSpan FeedingTime { get; set; }
    public string FoodType { get; set; }
    public string Portion { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}