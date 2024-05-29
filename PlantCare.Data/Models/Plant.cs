namespace PlantCare.Data.Models;

public class Plant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public int Age { get; set; }
    public string PhotoPath { get; set; } = string.Empty;

    public DateTime LastWatered { get; set; } = DateTime.Now;
    public int WateringFrequencyInHours { get; set; }

    public DateTime LastFertilized { get; set; } = DateTime.Now;
    public int FertilizeFrequencyInHours { get; set; }
}