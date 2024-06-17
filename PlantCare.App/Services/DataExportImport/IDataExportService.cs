
namespace PlantCare.App.Services.DataExportImport
{
    public interface IDataExportService
    {
        Task<string> ExportDataAsync(string exportDirectory);
    }
}