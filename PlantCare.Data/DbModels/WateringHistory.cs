using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCare.Data.DbModels;

[Table(nameof(WateringHistory))]
public class WateringHistory : EventHistoryBase
{
}
