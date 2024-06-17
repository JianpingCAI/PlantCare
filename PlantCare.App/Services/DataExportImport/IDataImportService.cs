namespace PlantCare.App.Services.DataExportImport
{
    public interface IDataImportService
    {
        Task<int> ImportDataAsync(string zipFilePath);
    }
}