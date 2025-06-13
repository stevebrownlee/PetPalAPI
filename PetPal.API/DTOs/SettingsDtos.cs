namespace PetPal.API.DTOs;

public class NotificationSettingsDto
{
    public int Id { get; set; }
    public bool EmailNotificationsEnabled { get; set; }
    public bool PushNotificationsEnabled { get; set; }
    public bool AppointmentReminders { get; set; }
    public bool MedicationReminders { get; set; }
    public bool VaccinationReminders { get; set; }
    public bool WeightReminders { get; set; }
    public bool FeedingReminders { get; set; }
    public int ReminderLeadTime { get; set; }
}

public class NotificationSettingsUpdateDto
{
    public bool EmailNotificationsEnabled { get; set; }
    public bool PushNotificationsEnabled { get; set; }
    public bool AppointmentReminders { get; set; }
    public bool MedicationReminders { get; set; }
    public bool VaccinationReminders { get; set; }
    public bool WeightReminders { get; set; }
    public bool FeedingReminders { get; set; }
    public int ReminderLeadTime { get; set; }
}

public class ThemePreferenceDto
{
    public string ThemePreference { get; set; }
}

public class ThemePreferenceUpdateDto
{
    public string ThemePreference { get; set; }
}