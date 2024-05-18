namespace PlantCare.Data.Models;

public class RootObject
{
    public User User { get; set; }
    public Plant Plant { get; set; }
    public Reminder Reminder { get; set; }
    public Log Log { get; set; }
}