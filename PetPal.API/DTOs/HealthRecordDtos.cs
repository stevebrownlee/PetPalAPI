using Microsoft.AspNetCore.Http;

namespace PetPal.API.DTOs;

public class HealthRecordDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string VeterinarianName { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// Specialized DTOs for Vaccination Management
public class VaccinationDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string Description { get; set; } // Vaccine name/type
    public DateTime RecordDate { get; set; } // Date administered
    public DateTime? DueDate { get; set; } // Next due date
    public int? VeterinarianId { get; set; }
    public string VeterinarianName { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class VaccinationCreateDto
{
    public int PetId { get; set; }
    public string Description { get; set; } // Vaccine name/type
    public DateTime RecordDate { get; set; } // Date administered
    public DateTime? DueDate { get; set; } // Next due date
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}

public class VaccinationUpdateDto
{
    public string Description { get; set; } // Vaccine name/type
    public DateTime RecordDate { get; set; } // Date administered
    public DateTime? DueDate { get; set; } // Next due date
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}

public class HealthRecordCreateDto
{
    public int PetId { get; set; }
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}

public class HealthRecordUpdateDto
{
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}

// DTO for document upload
public class HealthRecordDocumentUploadDto
{
    public int HealthRecordId { get; set; }
    public IFormFile Document { get; set; }
}