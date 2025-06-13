namespace PetPal.API.DTOs;

public class ExportRequestDto
{
    public int PetId { get; set; }
    public string Format { get; set; } // "PDF", "CSV"
    public List<string> Sections { get; set; } = new List<string>(); // "BasicInfo", "HealthRecords", "Medications", "Appointments", "WeightRecords", "All"
    public DateTime? StartDate { get; set; } // Optional date range filter
    public DateTime? EndDate { get; set; } // Optional date range filter
}

public class PetExportDto
{
    // Basic pet information
    public int Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
    public decimal Weight { get; set; }
    public string Color { get; set; }
    public string MicrochipNumber { get; set; }

    // Owner information
    public List<OwnerExportDto> Owners { get; set; } = new List<OwnerExportDto>();

    // Records
    public List<HealthRecordExportDto> HealthRecords { get; set; } = new List<HealthRecordExportDto>();
    public List<MedicationExportDto> Medications { get; set; } = new List<MedicationExportDto>();
    public List<AppointmentExportDto> Appointments { get; set; } = new List<AppointmentExportDto>();
    public List<WeightExportDto> WeightRecords { get; set; } = new List<WeightExportDto>();
    public List<FeedingScheduleExportDto> FeedingSchedules { get; set; } = new List<FeedingScheduleExportDto>();

    // Metadata
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    public string ExportFormat { get; set; }
}

public class OwnerExportDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class HealthRecordExportDto
{
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string VeterinarianName { get; set; }
    public string Notes { get; set; }
}

public class MedicationExportDto
{
    public string Name { get; set; }
    public string Dosage { get; set; }
    public string Frequency { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Instructions { get; set; }
    public string Prescriber { get; set; }
    public bool IsActive { get; set; }
}

public class AppointmentExportDto
{
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; }
    public string VeterinarianName { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; }
}

public class WeightExportDto
{
    public decimal WeightValue { get; set; }
    public DateTime RecordDate { get; set; }
    public string Notes { get; set; }
}

public class FeedingScheduleExportDto
{
    public string FoodType { get; set; }
    public string Amount { get; set; }
    public string Frequency { get; set; }
    public string SpecialInstructions { get; set; }
}

public class ExportResultDto
{
    public bool Success { get; set; }
    public string FileUrl { get; set; }
    public string FileName { get; set; }
    public string Format { get; set; }
    public string ErrorMessage { get; set; }
}