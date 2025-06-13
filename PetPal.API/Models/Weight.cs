namespace PetPal.API.Models;

public class Weight
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public decimal WeightValue { get; set; }
    public string WeightUnit { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}