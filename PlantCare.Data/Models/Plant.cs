namespace PlantCare.Data.Models
{
    public class Plant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public int Age { get; set; }
        public string PhotoPath { get; set; } = string.Empty;

        public DateTime LastWatered { get; set; } = DateTime.Now;
        public int WateringFrequencyInHours { get; set; }

        public DateTime NextWateringDate => LastWatered.AddHours(WateringFrequencyInHours);
        public int HoursUntilNextWatering => (int)(NextWateringDate - DateTime.Now).TotalHours;
        public double WateringProgress => Math.Max(0, 1 - (double)HoursUntilNextWatering / (double)WateringFrequencyInHours);
    }
}