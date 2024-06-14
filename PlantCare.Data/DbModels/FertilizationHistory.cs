using System.ComponentModel.DataAnnotations.Schema;

namespace PlantCare.Data.DbModels;

[Table(nameof(FertilizationHistory))]
public class FertilizationHistory : EventHistoryBase
{
}