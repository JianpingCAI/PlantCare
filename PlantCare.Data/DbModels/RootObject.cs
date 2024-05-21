namespace PlantCare.Data.DbModels;

public class RootObject
{
    public User User { get; set; }
    public PlantDbModel Plant { get; set; }
    public Reminder Reminder { get; set; }
    public Log Log { get; set; }
}