using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PlantCare.Data.Repositories;
using System.Text.Json;
using PlantCare.Data.DbModels;
using System.Text.Json.Serialization;

namespace PlantCare.App.Services.DataExportImport;

public class DataExportService : IDataExportService
{
    private readonly ApplicationDbContext _context;
    private readonly IAppSettingsService _appSettingsService;

    public DataExportService(
        ApplicationDbContext context,
        IAppSettingsService appSettingsService)
    {
        _context = context;
        _appSettingsService = appSettingsService;
    }

    public async Task<string> ExportDataAsync(string exportDirectory)
    {
        ExportDataModel exportData = new()
        {
            Plants = await _context.Plants
           .Include(p => p.WateringHistories)
           .Include(p => p.FertilizationHistories)
           .ToListAsync(),
            AppSettings = await _appSettingsService.GetAppSettingsAsync()
        };

        // Define paths
        string tempExportDirectory = Path.Combine(exportDirectory, "TempExport");
        string jsonFilePath = Path.Combine(tempExportDirectory, "exportedData.json");
        string zipFilePath = Path.Combine(exportDirectory, "PlantCareExport.zip");

        try
        {
            // Ensure directory exists
            if (Directory.Exists(tempExportDirectory))
            {
                Directory.Delete(tempExportDirectory, true);
            }
            Directory.CreateDirectory(tempExportDirectory);

            // Save JSON data
            string json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions()
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            });
            await File.WriteAllTextAsync(jsonFilePath, json, Encoding.UTF8);

            // Copy photo files
            foreach (PlantDbModel plant in exportData.Plants)
            {
                if (!string.IsNullOrEmpty(plant.PhotoPath) && File.Exists(plant.PhotoPath))
                {
                    string photoFileName = Path.GetFileName(plant.PhotoPath);
                    string destPath = Path.Combine(tempExportDirectory, photoFileName);
                    File.Copy(plant.PhotoPath, destPath, true);
                }
            }

            // Create zip archive
            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }
            
            // Create ZIP file synchronously to ensure it's complete
            ZipFile.CreateFromDirectory(tempExportDirectory, zipFilePath);

            // Verify ZIP file was created successfully
            if (!File.Exists(zipFilePath))
            {
                throw new IOException("Failed to create ZIP file");
            }

            return zipFilePath;
        }
        finally
        {
            // Clean up temporary export directory
            try
            {
                if (Directory.Exists(tempExportDirectory))
                {
                    Directory.Delete(tempExportDirectory, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
