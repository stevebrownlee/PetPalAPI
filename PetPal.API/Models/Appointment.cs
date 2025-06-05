namespace PetPal.API.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public int VeterinarianId { get; set; }
    public Veterinarian Veterinarian { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; } // e.g., "Check-up", "Vaccination", "Surgery", etc.
    public string Notes { get; set; }
    public string Status { get; set; } // e.g., "Scheduled", "Completed", "Cancelled", etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}