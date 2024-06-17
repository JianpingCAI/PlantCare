
namespace PlantCare.App.Services.DataExportImport
{
    public interface IDataImportService
    {
        Task ImportDataAsync(string zipFilePath);
    }
}