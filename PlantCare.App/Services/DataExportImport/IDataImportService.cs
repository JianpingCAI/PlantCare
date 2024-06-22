namespace PlantCare.App.Services.DataExportImport
{
    public interface IDataImportService
    {
        Task<ExportDataModel> ImportDataAsync(string zipFilePath, bool isRemoveExistingData);
    }
}