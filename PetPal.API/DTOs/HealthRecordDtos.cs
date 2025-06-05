namespace PetPal.API.DTOs;

public class HealthRecordDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string VeterinarianName { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HealthRecordCreateDto
{
    public int PetId { get; set; }
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}

public class HealthRecordUpdateDto
{
    public string RecordType { get; set; }
    public string Description { get; set; }
    public DateTime RecordDate { get; set; }
    public int? VeterinarianId { get; set; }
    public string Notes { get; set; }
    public string Attachments { get; set; }
}