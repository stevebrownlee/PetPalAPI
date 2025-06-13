namespace PetPal.API.Models;

public class Pet
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public DateTime DateOfBirth { get; set; }
    public decimal Weight { get; set; }
    public string Color { get; set; }
    public string? ImageUrl { get; set; }
    public string MicrochipNumber { get; set; }
    public List<PetOwner> Owners { get; set; } = new List<PetOwner>();
    public List<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    public List<Appointment> Appointments { get; set; } = new List<Appointment>();
    public List<Medication> Medications { get; set; } = new List<Medication>();
    public List<Weight> WeightRecords { get; set; } = new List<Weight>();
    public List<FeedingSchedule> FeedingSchedules { get; set; } = new List<FeedingSchedule>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}