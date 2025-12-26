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
    private static readonly string[] PlantNames = 
    {
        "Rose", "Tulip", "Daisy", "Sunflower", "Orchid", "Lily", "Iris", "Peony", "Violet", "Jasmine",
        "Cactus", "Succulent", "Aloe", "Jade Plant", "Snake Plant", "Spider Plant", "Fern", "Ivy", "Pothos", "Monstera",
        "Basil", "Mint", "Rosemary", "Thyme", "Lavender", "Sage", "Parsley", "Cilantro", "Dill", "Oregano",
        "Tomato", "Pepper", "Cucumber", "Lettuce", "Spinach", "Carrot", "Radish", "Beet", "Kale", "Broccoli",
        "Maple", "Oak", "Pine", "Birch", "Willow", "Cherry", "Apple", "Pear", "Plum", "Peach",
        "Bamboo", "Palm", "Rubber Tree", "Fiddle Leaf", "Money Tree", "Peace Lily", "Philodendron", "Dracaena", "ZZ Plant", "Calathea",
        "Azalea", "Hydrangea", "Camellia", "Gardenia", "Magnolia", "Hibiscus", "Begonia", "Geranium", "Marigold", "Zinnia",
        "Ficus", "Yucca", "Croton", "Dieffenbachia", "Anthurium", "Bromeliad", "Alocasia", "Aglaonema", "Cordyline", "Schefflera",
        "Bonsai", "Carnation", "Chrysanthemum", "Dahlia", "Gladiolus", "Poppy", "Ranunculus", "Anemone", "Cosmos", "Aster",
        "Coleus", "Impatiens", "Nasturtium", "Petunia", "Pansy", "Salvia", "Snapdragon", "Sweet Pea", "Verbena", "Vinca"
    };

    private static readonly string[] Species = 
    {
        "Rosa", "Tulipa", "Bellis", "Helianthus", "Orchidaceae", "Lilium", "Iris", "Paeonia", "Viola", "Jasminum",
        "Cactaceae", "Crassula", "Aloe", "Crassula ovata", "Sansevieria", "Chlorophytum", "Polypodiopsida", "Hedera", "Epipremnum", "Monstera deliciosa",
        "Ocimum basilicum", "Mentha", "Rosmarinus", "Thymus", "Lavandula", "Salvia officinalis", "Petroselinum", "Coriandrum", "Anethum", "Origanum",
        "Solanum lycopersicum", "Capsicum", "Cucumis", "Lactuca", "Spinacia", "Daucus carota", "Raphanus", "Beta vulgaris", "Brassica oleracea", "Brassica",
        "Acer", "Quercus", "Pinus", "Betula", "Salix", "Prunus", "Malus", "Pyrus", "Prunus domestica", "Prunus persica"
    };

    public static async Task SeedTestDataIfNeededAsync(ApplicationDbContext context, int seeds)
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

            Debug.WriteLine("[DevSeeder] Database is empty - seeding 100 test plants for performance testing...");

            var testPlants = new List<PlantDbModel>();
            var random = new Random(42); // Fixed seed for reproducibility

            // Generate 100 varied test plants
            for (int i = 0; i < seeds; i++)
            {
                // Vary watering frequency (6 hours to 30 days)
                int wateringHours = random.Next(1, 6) switch
                {
                    1 => 6,      // Every 6 hours (very thirsty)
                    2 => 24,     // Daily
                    3 => 48,     // Every 2 days
                    4 => 168,    // Weekly
                    _ => 720     // Monthly (30 days)
                };

                // Vary fertilization frequency (1 week to 3 months)
                int fertilizeHours = random.Next(1, 5) switch
                {
                    1 => 168,    // Weekly
                    2 => 336,    // Bi-weekly
                    3 => 720,    // Monthly
                    _ => 2160    // 90 days
                };

                // Vary last watered (overdue, due soon, or healthy)
                int wateringDaysAgo = random.Next(0, 40);
                
                // Vary last fertilized
                int fertilizingDaysAgo = random.Next(0, 100);

                // Vary age
                int age = random.Next(0, 10);

                // Select plant name and species
                string name = PlantNames[i % PlantNames.Length];
                if (i >= PlantNames.Length)
                {
                    name += $" {i / PlantNames.Length + 1}"; // Add number for duplicates
                }
                
                string species = Species[i % Species.Length];

                // Add some variety in notes
                string notes = (i % 5) switch
                {
                    0 => "Needs frequent watering",
                    1 => "Prefers indirect sunlight",
                    2 => "Low maintenance plant",
                    3 => "Fertilize monthly during growing season",
                    _ => $"Performance test plant #{i + 1}"
                };

                testPlants.Add(new PlantDbModel
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Species = species,
                    Age = age,
                    PhotoPath = "default_plant.png",
                    LastWatered = DateTime.Now.AddDays(-wateringDaysAgo),
                    WateringFrequencyInHours = wateringHours,
                    LastFertilized = DateTime.Now.AddDays(-fertilizingDaysAgo),
                    FertilizeFrequencyInHours = fertilizeHours,
                    Notes = notes
                });
            }

            context.Plants.AddRange(testPlants);
            await context.SaveChangesAsync();

            Debug.WriteLine($"[DevSeeder] Successfully seeded {testPlants.Count} test plants");
            Debug.WriteLine($"[DevSeeder] Plant care status distribution:");
            
            // Count plants by status
            int overdue = 0, dueSoon = 0, healthy = 0;
            foreach (PlantDbModel plant in testPlants)
            {
                double hoursSinceWatering = (DateTime.Now - plant.LastWatered).TotalHours;
                double nextWateringHours = plant.WateringFrequencyInHours - hoursSinceWatering;
                
                if (nextWateringHours < 0)
                    overdue++;
                else if (nextWateringHours < 24)
                    dueSoon++;
                else
                    healthy++;
            }
            
            Debug.WriteLine($"[DevSeeder]   - Overdue: {overdue}");
            Debug.WriteLine($"[DevSeeder]   - Due Soon: {dueSoon}");
            Debug.WriteLine($"[DevSeeder]   - Healthy: {healthy}");
            Debug.WriteLine($"[DevSeeder] Use this data to test:");
            Debug.WriteLine($"[DevSeeder]   - Scroll performance with 100+ plants");
            Debug.WriteLine($"[DevSeeder]   - Search/filter performance");
            Debug.WriteLine($"[DevSeeder]   - Collection virtualization");
            Debug.WriteLine($"[DevSeeder]   - Memory usage");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DevSeeder] ERROR: {ex.Message}");
        }
    }
}
#endif
