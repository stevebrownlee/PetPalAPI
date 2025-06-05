namespace PetPal.API.Models;

public class Medication
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; } // e.g., "Once daily", "Twice daily", etc.
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Instructions { get; set; }
    public string Prescriber { get; set; } // Name of the veterinarian who prescribed the medication
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}