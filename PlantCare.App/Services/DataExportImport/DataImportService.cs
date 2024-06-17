using System.IO.Compression;
using System.Text;
using PlantCare.Data.Repositories;
using System.Text.Json;
using PlantCare.Data.DbModels;
using System.Text.Json.Serialization;

namespace PlantCare.App.Services.DataExportImport;

public class DataImportService : IDataImportService
{
    private readonly ApplicationDbContext _context;
    private readonly IAppSettingsService _appSettingsService;

    public DataImportService(
        ApplicationDbContext context,
        IAppSettingsService appSettingsService)
    {
        _context = context;
        _appSettingsService = appSettingsService;
    }

    public async Task<int> ImportDataAsync(string zipFilePath, bool isRemoveExistingData)
    {
        //string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string localAppDirectory = FileSystem.AppDataDirectory;
        string extractDirectory = Path.Combine(localAppDirectory, "PlantCareImport");
        string photoDirectory = Path.Combine(localAppDirectory, "photos");

        // Ensure directory exists
        if (Directory.Exists(extractDirectory))
        {
            Directory.Delete(extractDirectory, true);
        }
        Directory.CreateDirectory(extractDirectory);

        if (!Directory.Exists(photoDirectory))
        {
            Directory.CreateDirectory(photoDirectory);
        }

        // Extract zip archive
        ZipFile.ExtractToDirectory(zipFilePath, extractDirectory);

        string jsonFilePath = Path.Combine(extractDirectory, "exportedData.json");
        string json = await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8);
        //var importData = JsonConvert.DeserializeObject<ExportDataModel>(json);

        // Configure JsonSerializerOptions to handle reference loops
        JsonSerializerOptions jsonOptions = new()
        {
            ReferenceHandler = ReferenceHandler.Preserve
        };
        ExportDataModel? importData = JsonSerializer.Deserialize<ExportDataModel>(json, jsonOptions);
        if (importData is null)
        {
            throw new ArgumentNullException("Failed to convert the imported data");
        }
        DataImportService.ResetIds(importData.Plants);

        // Restore photo files
        foreach (PlantDbModel plant in importData.Plants)
        {
            if (!string.IsNullOrEmpty(plant.PhotoPath))
            {
                string fileName = Path.GetFileName(plant.PhotoPath);
                string originalPhotoPath = Path.Combine(extractDirectory, fileName);
                plant.PhotoPath = Path.Combine(photoDirectory, fileName);

                if (File.Exists(originalPhotoPath))
                {
                    File.Copy(originalPhotoPath, plant.PhotoPath, true);
                }
            }
        }

        if (isRemoveExistingData)
        {
            // Remove existing data
            _context.Plants.RemoveRange(_context.Plants);

            _context.WateringHistories.RemoveRange(_context.WateringHistories);

            _context.FertilizationHistories.RemoveRange(_context.FertilizationHistories);

            await _context.SaveChangesAsync();
        }

        // Add new data
        await _context.Plants.AddRangeAsync(importData.Plants);
        await _context.SaveChangesAsync();

        //SavePreferences(importData.Preferences);
        await _appSettingsService.SaveAppSettingsAsync(importData.AppSettings);

        return importData.Plants.Count;
    }

    private static void ResetIds(IEnumerable<PlantDbModel> plants)
    {
        foreach (var plant in plants)
        {
            plant.Id = Guid.Empty;

            foreach (var wateringHistory in plant.WateringHistories)
            {
                wateringHistory.Id = Guid.Empty;
                wateringHistory.PlantId = Guid.Empty; // Reset PlantId to prevent foreign key conflicts
            }

            foreach (var fertilizationHistory in plant.FertilizationHistories)
            {
                fertilizationHistory.Id = Guid.Empty;
                fertilizationHistory.PlantId = Guid.Empty; // Reset PlantId to prevent foreign key conflicts
            }
        }
    }
}