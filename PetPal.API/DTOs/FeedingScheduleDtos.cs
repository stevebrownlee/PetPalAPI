namespace PetPal.API.DTOs;

public class FeedingScheduleDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public TimeSpan FeedingTime { get; set; }
    public string FoodType { get; set; }
    public string Portion { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class FeedingScheduleCreateDto
{
    public int PetId { get; set; }
    public TimeSpan FeedingTime { get; set; }
    public string FoodType { get; set; }
    public string Portion { get; set; }
    public string Notes { get; set; }
}

public class FeedingScheduleUpdateDto
{
    public TimeSpan FeedingTime { get; set; }
    public string FoodType { get; set; }
    public string Portion { get; set; }
    public string Notes { get; set; }
    public bool IsActive { get; set; }
}