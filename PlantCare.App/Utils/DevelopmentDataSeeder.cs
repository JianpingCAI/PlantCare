using PlantCare.Data.DbModels;
using PlantCare.Data.Repositories;
using System.Diagnostics;

namespace PlantCare.App.Utils;

#if DEBUG
/// <summary>
/// Development helper to seed test data for debugging.
/// Only active in DEBUG builds.
/// </summary>
public static class DevelopmentDataSeeder
{
    public static async Task SeedTestDataIfNeededAsync(ApplicationDbContext context)
    {
        try
        {
            // Check if database is empty
            int plantCount = context.Plants.Count();
            
            if (plantCount > 0)
            {
                Debug.WriteLine($"[DevSeeder] Database has {plantCount} plants - skipping seed");
                return;
            }

            Debug.WriteLine("[DevSeeder] Database is empty - seeding test data...");

            // Create test plants
            var testPlants = new List<PlantDbModel>
            {
                new PlantDbModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Rose",
                    Species = "Rosa",
                    Age = 2,
                    PhotoPath = "default_plant.png",
                    LastWatered = DateTime.Now.AddDays(-1),
                    WateringFrequencyInHours = 48,
                    LastFertilized = DateTime.Now.AddDays(-7),
                    FertilizeFrequencyInHours = 168,
                    Notes = "Beautiful red rose - test data"
                },
                new PlantDbModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Cactus",
                    Species = "Cactaceae",
                    Age = 1,
                    PhotoPath = "default_plant.png",
                    LastWatered = DateTime.Now.AddDays(-10),
                    WateringFrequencyInHours = 336, // 2 weeks
                    LastFertilized = DateTime.Now.AddDays(-30),
                    FertilizeFrequencyInHours = 720, // 30 days
                    Notes = "Desert cactus - test data"
                },
                new PlantDbModel
                {
                    Id = Guid.NewGuid(),
                    Name = "Fern",
                    Species = "Polypodiopsida",
                    Age = 3,
                    PhotoPath = "default_plant.png",
                    LastWatered = DateTime.Now.AddHours(-12),
                    WateringFrequencyInHours = 24, // Daily
                    LastFertilized = DateTime.Now.AddDays(-14),
                    FertilizeFrequencyInHours = 336, // 2 weeks
                    Notes = "Tropical fern - test data"
                }
            };

            context.Plants.AddRange(testPlants);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[DevSeeder] Successfully seeded {testPlants.Count} test plants");
            
            foreach (PlantDbModel plant in testPlants)
            {
                Debug.WriteLine($"[DevSeeder]   - {plant.Name} (ID: {plant.Id})");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DevSeeder] ERROR: {ex.Message}");
        }
    }
}
#endif
