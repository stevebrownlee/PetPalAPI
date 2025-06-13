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

    // Reminder settings
    public bool ReminderEnabled { get; set; } = false;
    public string ReminderFrequency { get; set; } // e.g., "Daily", "Every 8 hours", etc.
    public TimeSpan? ReminderTime { get; set; } // Time of day for the reminder
    public DateTime? LastReminderSent { get; set; }
    public DateTime? NextReminderDue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}