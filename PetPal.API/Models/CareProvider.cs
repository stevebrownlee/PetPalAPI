namespace PetPal.API.Models;

public class CareProvider
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } // e.g., "Veterinarian", "Groomer", "Trainer"
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Website { get; set; } // optional
    public string Notes { get; set; } // optional
    public string UserId { get; set; } // to associate with the user who added the provider
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}