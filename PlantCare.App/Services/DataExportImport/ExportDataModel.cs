using PlantCare.App.Services;
using PlantCare.Data.DbModels;

internal class ExportDataModel
{
    public List<PlantDbModel> Plants { get; set; } = [];
    public AppSettings AppSettings { get; internal set; }
}