using System;

namespace PetPal.API.Models;

public class NotificationSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserProfile UserProfile { get; set; }
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool AppointmentReminders { get; set; } = true;
    public bool MedicationReminders { get; set; } = true;
    public bool VaccinationReminders { get; set; } = true;
    public bool WeightReminders { get; set; } = false;
    public bool FeedingReminders { get; set; } = false;
    public int ReminderLeadTime { get; set; } = 24; // hours before event to send reminder
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}