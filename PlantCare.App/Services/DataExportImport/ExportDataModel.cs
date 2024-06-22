using PlantCare.App.Services;
using PlantCare.Data.DbModels;

public class ExportDataModel
{
    public List<PlantDbModel> Plants { get; set; } = [];
    public AppSettings AppSettings { get; set; }
}