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

    [Required]
    public DateTime LastWatered { get; set; } = DateTime.Now;

    [MaxLength(200)]
    public string PhotoPath { get; set; } = string.Empty;

    [Required]
    [Range(1, 2400)]
    public int WateringFrequencyInHours { get; set; }
}