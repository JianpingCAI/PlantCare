﻿using System.IO.Compression;
using System.Text;
using System.Text.Json;
using PlantCare.Data.DbModels;
using System.Text.Json.Serialization;
using PlantCare.App.Services.DBService;

namespace PlantCare.App.Services.DataExportImport;

public class DataImportService : IDataImportService
{
    private readonly IPlantService _plantService;
    private readonly IAppSettingsService _appSettingsService;

    public DataImportService(
        IPlantService plantService,
        IAppSettingsService appSettingsService)
    {
        _plantService = plantService;
        _appSettingsService = appSettingsService;
    }

    public Task<int> ImportDataAsync(string zipFilePath, bool isRemoveExistingData)
    {
        return Task.Run(async () =>
        {
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

            ResetIds(importData.Plants);

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
                await ClearTablesAsync();
            }

            // Add new data
            await _plantService.AddPlantsAsync(importData.Plants);

            await _appSettingsService.SaveAppSettingsAsync(importData.AppSettings);

            return importData.Plants.Count;
        });
    }

    private async Task ClearTablesAsync()
    {
        // Cascading deletes are not configured/enabled

        // Remove existing data from foreign tables first
        await _plantService.DeleteAllPhotosAsync();
        await _plantService.ClearAllAsync();

        //_context.WateringHistories.RemoveRange(_context.WateringHistories.ToList());
        //_context.FertilizationHistories.RemoveRange(_context.FertilizationHistories.ToList());
        //_context.Plants.RemoveRange(_context.Plants.ToList());
        //return _context.SaveChangesAsync();
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