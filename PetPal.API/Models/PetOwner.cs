namespace PetPal.API.Models;

public class PetOwner
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; }
    public int UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; }
    public bool IsPrimaryOwner { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}