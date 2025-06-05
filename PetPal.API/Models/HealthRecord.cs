namespace PetPal.API.Models;

public class HealthRecord
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public string RecordType { get; set; } // e.g., "Vaccination", "Check-up", "Surgery", etc.
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public int? VeterinarianId { get; set; }
    public Veterinarian Veterinarian { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; } // Could be a URL or file path
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}