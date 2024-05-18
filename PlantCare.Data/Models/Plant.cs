namespace PlantCare.Data.Models;

public class Plant
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Species { get; set; }
    public string Nickname { get; set; }
    public string Age { get; set; }
    public string LastWatered { get; set; }
    public string PhotoPath { get; set; }
}