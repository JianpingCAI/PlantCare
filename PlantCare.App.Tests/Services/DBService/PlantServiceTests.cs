using PlantCare.App.Tests.Common;
using PlantCare.Data.Repositories.interfaces;

namespace PlantCare.App.Services.DBService.Tests
{
    public class PlantServiceTests : IClassFixture<ServiceProviderFixture>
    {
        private ServiceProvider _serviceProvider;

        public PlantServiceTests(ServiceProviderFixture fixture)
        {
            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        public async Task DeletePlantAsyncTest1()
        {
            PlantService? plantService = _serviceProvider.GetService<PlantService>();

            Assert.NotNull(plantService);

            await plantService.AddPlantsAsync(
            [
                new() {
                    WateringHistories =
                    [
                        new() {
                            CareTime = DateTime.Now,
                        }
                    ],

                    FertilizationHistories = [
                        new() {
                            CareTime= DateTime.Now,
                        }
                    ]
                }
            ]);

            List<ViewModels.PlantCareHistory> plants = await plantService.GetAllPlantsWithCareHistoryAsync();
            Assert.Single(plants);

            IWateringHistoryRepository? waterHistoryRepository = _serviceProvider.GetService<IWateringHistoryRepository>();
            Assert.NotNull(waterHistoryRepository);

            List<Data.DbModels.WateringHistory> waterHistory = await waterHistoryRepository.GetWateringHistoryByPlantIdAsync(plants.First().PlantId);
            Assert.NotNull(waterHistory);
            Assert.Single(waterHistory);

            IFertilizationHistoryRepository? fertilizationHistoryRepository = _serviceProvider.GetService<IFertilizationHistoryRepository>();
            Assert.NotNull(fertilizationHistoryRepository);

            List<Data.DbModels.FertilizationHistory> fertilizationHistory = await fertilizationHistoryRepository.GetFertilizationHistoryByPlantIdAsync(plants.First().PlantId);
            Assert.NotNull(fertilizationHistory);
            Assert.Single(fertilizationHistory);

            await plantService.DeletePlantAsync(plants.First().PlantId);
            List<ViewModels.PlantCareHistory> remainPlants = await plantService.GetAllPlantsWithCareHistoryAsync();
            Assert.Empty(remainPlants);

            List<Data.DbModels.WateringHistory> waterHistory2 = await waterHistoryRepository.GetAllAsync();
            Assert.NotNull(waterHistory2);
            Assert.Empty(waterHistory2);

            List<Data.DbModels.FertilizationHistory> fertilizationHistory2 = await fertilizationHistoryRepository.GetAllAsync();
            Assert.NotNull(fertilizationHistory2);
            Assert.Empty(fertilizationHistory2);
        }
    }
}