namespace PetPal.API.Models;

public class Veterinarian
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Specialty { get; set; }
    public string ClinicName { get; set; }
    public string Address { get; set; }
    public string LicenseNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}