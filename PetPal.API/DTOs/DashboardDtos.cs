namespace PetPal.API.DTOs;

public class PetDashboardDto
{
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
    public decimal CurrentWeight { get; set; }
    public string ImageUrl { get; set; }

    // Summary counts
    public int UpcomingAppointmentsCount { get; set; }
    public int ActiveMedicationsCount { get; set; }
    public int UpcomingVaccinationsCount { get; set; }

    // Recent records
    public List<HealthRecordSummaryDto> RecentHealthRecords { get; set; } = new List<HealthRecordSummaryDto>();
    public List<MedicationSummaryDto> ActiveMedications { get; set; } = new List<MedicationSummaryDto>();
    public List<AppointmentSummaryDto> UpcomingAppointments { get; set; } = new List<AppointmentSummaryDto>();
    public List<WeightSummaryDto> RecentWeightRecords { get; set; } = new List<WeightSummaryDto>();
}

public class UserDashboardDto
{
    public int UserProfileId { get; set; }
    public string UserName { get; set; }
    public List<PetDashboardSummaryDto> Pets { get; set; } = new List<PetDashboardSummaryDto>();
    public List<CalendarEventDto> UpcomingEvents { get; set; } = new List<CalendarEventDto>();
    public int TotalPets { get; set; }
    public int TotalUpcomingAppointments { get; set; }
    public int TotalActiveMedications { get; set; }
    public int TotalUpcomingVaccinations { get; set; }
}

public class PetDashboardSummaryDto
{
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public string ImageUrl { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int ActiveMedicationsCount { get; set; }
    public int UpcomingVaccinationsCount { get; set; }
}

public class HealthRecordSummaryDto
{
    public int Id { get; set; }
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string VeterinarianName { get; set; }
}

public class MedicationSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool ReminderEnabled { get; set; }
    public DateTime? NextReminderDue { get; set; }
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; }
    public string VeterinarianName { get; set; }
    public string Status { get; set; }
}

public class WeightSummaryDto
{
    public int Id { get; set; }
    public decimal WeightValue { get; set; }
    public DateTime RecordDate { get; set; }
}

public class CalendarEventDto
{
    public int Id { get; set; }
    public string EventType { get; set; } // "Appointment", "Medication", "Vaccination"
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan? EventTime { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string Color { get; set; } // For UI display purposes
}