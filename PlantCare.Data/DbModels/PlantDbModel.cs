using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCare.Data.DbModels;

[Table("Plants")]
public class PlantDbModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "Unknown";

    [MaxLength(100)]
    public string Species { get; set; } = string.Empty;

    [Range(1, 2400)]
    public int Age { get; set; }

    [MaxLength(200)]
    public string PhotoPath { get; set; } = string.Empty;

    [Required]
    public DateTime LastWatered { get; set; } = DateTime.Now;

    [Required]
    [Range(1, 8760)]
    public int WateringFrequencyInHours { get; set; }

    [Required]
    public DateTime LastFertilized { get; set; } = DateTime.Now;

    [Required]
    [Range(1, 8760)]
    public int FertilizeFrequencyInHours { get; set; }

    // Relationships
    public virtual ICollection<WateringHistory> WateringHistories { get; set; } = [];

    public virtual ICollection<FertilizationHistory> FertilizationHistories { get; set; } = [];
}

public abstract class EventHistoryBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    public Guid PlantId { get; set; }

    [Required]
    public DateTime CareTime { get; set; }

    // Relationship
    [ForeignKey(nameof(PlantId))]
    public virtual PlantDbModel Plant { get; set; }
}

[Table(nameof(WateringHistory))]
public class WateringHistory : EventHistoryBase
{
}

[Table(nameof(FertilizationHistory))]
public class FertilizationHistory : EventHistoryBase
{
}