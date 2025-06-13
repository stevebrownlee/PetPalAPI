namespace PetPal.API.DTOs;

public class WeightDto
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; }
    public decimal WeightValue { get; set; }
    public string WeightUnit { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WeightCreateDto
{
    public int PetId { get; set; }
    public decimal WeightValue { get; set; }
    public string WeightUnit { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; }
}

public class WeightUpdateDto
{
    public decimal WeightValue { get; set; }
    public string WeightUnit { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; }
}

// DTO for weight history data suitable for graphing
public class WeightHistoryDto
{
    public DateTime Date { get; set; }
    public decimal WeightValue { get; set; }
    public string WeightUnit { get; set; }
}