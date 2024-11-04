using Microsoft.EntityFrameworkCore;
using PlantCare.App.Services.DBService;
using PlantCare.App.Utils;
using PlantCare.Data.Repositories;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.App.Tests.Common
{
    public class TestStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            services.AddTransient<PlantService>();
            services.AddScoped<IPlantRepository, PlantRepository>();
            services.AddScoped<IWateringHistoryRepository, WateringHistoryRepository>();
            services.AddScoped<IFertilizationHistoryRepository, FertilizationHistoryRepository>();

            services.AddAutoMapper(typeof(MappingProfile));
        }
    }

    public static class ServiceProviderFactory
    {
        public static ServiceProvider Create()
        {
            ServiceCollection serviceCollection = new();
            TestStartup.ConfigureServices(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }
    }
}