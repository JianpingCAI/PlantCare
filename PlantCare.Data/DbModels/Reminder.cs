namespace PlantCare.Data.DbModels;

public class Reminder
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string PlantId { get; set; }
    public string Type { get; set; }
    public string ScheduledAt { get; set; }
    public string IsRecurring { get; set; }
}