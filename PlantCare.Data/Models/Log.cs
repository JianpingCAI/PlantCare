namespace PlantCare.Data.Models;

public class Log
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string PlantId { get; set; }
    public string Note { get; set; }
    public string PhotoPath { get; set; }
    public string CreatedAt { get; set; }
}