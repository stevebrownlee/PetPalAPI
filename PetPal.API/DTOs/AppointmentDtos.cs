namespace PetPal.API.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public int VeterinarianId { get; set; }
    public string VeterinarianName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AppointmentCreateDto
{
    public int PetId { get; set; }
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; }
    public string Notes { get; set; }
}

public class AppointmentUpdateDto
{
    public int VeterinarianId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; }
    public string Notes { get; set; }
    public string Status { get; set; }
}