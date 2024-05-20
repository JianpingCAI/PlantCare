namespace PlantCare.Data.Models;

public class Plant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Species { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? LastWatered { get; set; }
    public string? PhotoPath { get; set; }
}