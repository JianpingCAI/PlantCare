using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCare.Data.DbModels;

/*Notes:
  - SQLite does not support the DatabaseGenerated attribute for GUIDs.
*/

public abstract class CareHistoryBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid HistoryId { get; set; }

    [Required]
    public Guid PlantId { get; set; }

    [Required]
    public DateTime CareTime { get; set; }

    // Relationship
    [ForeignKey(nameof(PlantId))]
    public virtual PlantDbModel? Plant { get; set; }
}