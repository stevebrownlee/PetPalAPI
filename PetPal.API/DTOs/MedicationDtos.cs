namespace PetPal.API.DTOs;

public class MedicationDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Instructions { get; set; }
    public string Prescriber { get; set; }
    public bool IsActive { get; set; }

    // Reminder settings
    public bool ReminderEnabled { get; set; }
    public string ReminderFrequency { get; set; }
    public TimeSpan? ReminderTime { get; set; }
    public DateTime? LastReminderSent { get; set; }
    public DateTime? NextReminderDue { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MedicationCreateDto
{
    public int PetId { get; set; }
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Instructions { get; set; }
    public string Prescriber { get; set; }

    // Reminder settings
    public bool ReminderEnabled { get; set; } = false;
    public string ReminderFrequency { get; set; }
    public TimeSpan? ReminderTime { get; set; }
}

public class MedicationUpdateDto
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Instructions { get; set; }
    public string Prescriber { get; set; }
    public bool IsActive { get; set; }

    // Reminder settings
    public bool ReminderEnabled { get; set; }
    public string ReminderFrequency { get; set; }
    public TimeSpan? ReminderTime { get; set; }
}

// DTOs for reminder management
public class MedicationReminderUpdateDto
{
    public bool ReminderEnabled { get; set; }
    public string ReminderFrequency { get; set; }
    public TimeSpan? ReminderTime { get; set; }
}

public class MedicationReminderDto
{
    public int MedicationId { get; set; }
    public string MedicationName { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public bool ReminderEnabled { get; set; }
    public string ReminderFrequency { get; set; }
    public TimeSpan? ReminderTime { get; set; }
    public DateTime? NextReminderDue { get; set; }
}