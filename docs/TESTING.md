# Testing Guide

## Overview

PlantCare uses xUnit for unit testing. This guide covers testing strategy, running tests, and writing new tests.

## Test Project Structure

```
PlantCare.App.Tests/
├── Common/                          # Test infrastructure
│   ├── ServiceProviderFixture.cs    # DI container for tests
│   ├── ServiceProviderFactory.cs    # Service configuration
│   └── MappingProfile.cs            # AutoMapper configuration
└── Services/                        # Service tests
    └── DBService/
        └── PlantServiceTests.cs     # PlantService unit tests
```

## Testing Stack

| Component | Technology | Purpose |
|-----------|-----------|---------|
| Test Framework | xUnit | Unit testing framework |
| Test Runner | Microsoft.NET.Test.Sdk | Test execution |
| Database | SQLite (in-memory) | Isolated database for tests |
| DI Container | Microsoft.Extensions.DependencyInjection | Service injection in tests |
| Mocking | Manual mocks | Service mocking (can add Moq if needed) |

## Running Tests

### Visual Studio

1. **Open Test Explorer**
   - View → Test Explorer (Ctrl+E, T)

2. **Run All Tests**
   - Click "Run All" or press Ctrl+R, A

3. **Run Specific Test**
   - Right-click on test → Run

4. **Debug Test**
   - Right-click on test → Debug

### Command Line

```bash
# Navigate to solution directory
cd PlantCare

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~PlantServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~PlantServiceTests.DeletePlant_ShouldPerformCascadeDeletion"

# Run tests and generate coverage report (requires coverlet)
dotnet test /p:CollectCoverage=true
```

### Continuous Integration

Tests run automatically on:
- Every push to main branch
- Pull request creation
- Manual workflow trigger

## Test Organization

### Test Naming Convention

Follow the pattern: `MethodName_Scenario_ExpectedBehavior`

**Examples**:
```csharp
[Fact]
public async Task DeletePlant_ShouldPerformCascadeDeletion()

[Fact]
public async Task AddPlant_WithValidData_ReturnsPlantId()

[Fact]
public async Task GetPlantById_WithInvalidId_ReturnsNull()
```

### Test Structure (Arrange-Act-Assert)

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedResult()
{
    // Arrange - Set up test data and dependencies
    var plant = new PlantDbModel { Name = "Test Plant" };
    
    // Act - Execute the method being tested
    var result = await _plantService.CreatePlantAsync(plant);
    
    // Assert - Verify the expected outcome
    Assert.NotEqual(Guid.Empty, result);
}
```

## Test Fixtures

### ServiceProviderFixture

Provides a configured DI container for tests.

```csharp
public class ServiceProviderFixture
{
    public ServiceProvider ServiceProvider { get; }
    
    public ServiceProviderFixture()
    {
        ServiceProvider = ServiceProviderFactory.Create();
    }
}
```

**Usage in tests**:
```csharp
public class PlantServiceTests : IClassFixture<ServiceProviderFixture>
{
    private ServiceProvider _serviceProvider;

    public PlantServiceTests(ServiceProviderFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }
}
```

### ServiceProviderFactory

Configures services for testing environment.

**Key Configurations**:
- In-memory SQLite database
- All repositories and services
- AutoMapper with test profile
- Scoped DbContext per test

## Writing Tests

### Example: Testing PlantService

```csharp
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
        public async Task DeletePlant_ShouldPerformCascadeDeletion()
        {
            // Arrange
            var plantService = _serviceProvider.GetService<PlantService>();
            Assert.NotNull(plantService);

            await plantService.AddPlantsAsync([
                new() {
                    WateringHistories = [
                        new() { CareTime = DateTime.Now }
                    ],
                    FertilizationHistories = [
                        new() { CareTime = DateTime.Now }
                    ]
                }
            ]);

            var plants = await plantService.GetAllPlantsWithCareHistoryAsync();
            Assert.Single(plants);

            var waterHistoryRepository = _serviceProvider
                .GetService<IWateringHistoryRepository>();
            Assert.NotNull(waterHistoryRepository);

            // Act
            await plantService.DeletePlantAsync(plants.First().PlantId);

            // Assert
            var remainingPlants = await plantService
                .GetAllPlantsWithCareHistoryAsync();
            Assert.Empty(remainingPlants);

            var waterHistory = await waterHistoryRepository.GetAllAsync();
            Assert.Empty(waterHistory);
        }
    }
}
```

### Testing Repositories

```csharp
[Fact]
public async Task AddPlant_ShouldSaveToDatabaseAndReturnId()
{
    // Arrange
    var repository = _serviceProvider.GetService<IPlantRepository>();
    var plant = new PlantDbModel
    {
        Name = "Rose",
        Species = "Rosa",
        Age = 2,
        LastWatered = DateTime.Now,
        WateringFrequencyInHours = 48
    };

    // Act
    var savedPlant = await repository.AddAsync(plant);

    // Assert
    Assert.NotEqual(Guid.Empty, savedPlant.Id);
    Assert.Equal("Rose", savedPlant.Name);

    // Verify it's in database
    var retrieved = await repository.GetByIdAsync(savedPlant.Id);
    Assert.NotNull(retrieved);
    Assert.Equal(plant.Name, retrieved.Name);
}
```

### Testing ViewModels (Example)

```csharp
[Fact]
public async Task AddPlantCommand_WithValidData_AddsPlantToDatabase()
{
    // Arrange
    var viewModel = _serviceProvider.GetService<PlantAddEditViewModel>();
    viewModel.Name = "Cactus";
    viewModel.Species = "Cactaceae";
    viewModel.WateringFrequencyDays = 7;

    // Act
    await viewModel.SubmitCommand.ExecuteAsync(null);

    // Assert
    var plantService = _serviceProvider.GetService<IPlantService>();
    var plants = await plantService.GetAllPlantsAsync();
    Assert.Contains(plants, p => p.Name == "Cactus");
}
```

### Testing with Messages

```csharp
[Fact]
public async Task DeletePlant_ShouldSendDeletedMessage()
{
    // Arrange
    var viewModel = _serviceProvider.GetService<PlantDetailViewModel>();
    PlantDeletedMessage? receivedMessage = null;
    
    WeakReferenceMessenger.Default.Register<PlantDeletedMessage>(this, 
        (r, m) => receivedMessage = m);

    // Act
    await viewModel.DeletePlantCommand.ExecuteAsync(null);

    // Assert
    Assert.NotNull(receivedMessage);
    Assert.Equal(plantId, receivedMessage.PlantId);

    // Cleanup
    WeakReferenceMessenger.Default.UnregisterAll(this);
}
```

## Test Data Builders

Create helper methods for common test data:

```csharp
public static class TestDataBuilder
{
    public static PlantDbModel CreatePlant(string name = "Test Plant")
    {
        return new PlantDbModel
        {
            Name = name,
            Species = "Test Species",
            Age = 1,
            PhotoPath = "/default.png",
            LastWatered = DateTime.Now,
            WateringFrequencyInHours = 72,
            LastFertilized = DateTime.Now,
            FertilizeFrequencyInHours = 720,
            Notes = "Test notes"
        };
    }

    public static WateringHistory CreateWateringHistory(
        Guid plantId, 
        DateTime? careTime = null)
    {
        return new WateringHistory
        {
            PlantId = plantId,
            CareTime = careTime ?? DateTime.Now
        };
    }
}
```

## Test Coverage

### Current Coverage Areas

✅ **Covered**:
- PlantService CRUD operations
- Cascade deletion
- Repository operations

⚠️ **Partially Covered**:
- ViewModels
- Navigation
- Notification scheduling

❌ **Not Covered**:
- UI interactions
- Platform-specific code
- Image optimization

### Measuring Coverage

Install coverlet for code coverage:

```bash
dotnet add package coverlet.collector
```

Run tests with coverage:

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

View coverage report (install reportgenerator):

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport

# Open coveragereport/index.html in browser
```

## Best Practices

### DO

✅ Write tests for all business logic
✅ Use descriptive test names
✅ Follow Arrange-Act-Assert pattern
✅ Test edge cases and error conditions
✅ Keep tests independent and isolated
✅ Use test fixtures for shared setup
✅ Clean up resources (database, messages, etc.)
✅ Test one thing per test method

### DON'T

❌ Don't test framework code (.NET MAUI, EF Core)
❌ Don't write tests that depend on external systems
❌ Don't test private methods directly
❌ Don't share state between tests
❌ Don't use Thread.Sleep() - use async/await
❌ Don't ignore failing tests
❌ Don't test implementation details

## Common Testing Patterns

### Testing Async Methods

```csharp
[Fact]
public async Task GetAllPlantsAsync_ShouldReturnAllPlants()
{
    // Arrange
    var service = _serviceProvider.GetService<IPlantService>();
    
    // Act
    var plants = await service.GetAllPlantsAsync();
    
    // Assert
    Assert.NotNull(plants);
}
```

### Testing Exceptions

```csharp
[Fact]
public async Task UpdatePlant_WithNullPlant_ThrowsArgumentNullException()
{
    // Arrange
    var service = _serviceProvider.GetService<IPlantService>();
    
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => service.UpdatePlantAsync(null));
}
```

### Parameterized Tests

```csharp
[Theory]
[InlineData(24, 1)]    // 1 day
[InlineData(72, 3)]    // 3 days
[InlineData(168, 7)]   // 7 days
public void ConvertHoursToDays_ShouldReturnCorrectDays(
    int hours, 
    int expectedDays)
{
    // Act
    var days = hours / 24;
    
    // Assert
    Assert.Equal(expectedDays, days);
}
```

### Testing Collections

```csharp
[Fact]
public async Task GetAllPlants_ShouldReturnOrderedByName()
{
    // Arrange
    var service = _serviceProvider.GetService<IPlantService>();
    await service.AddPlantsAsync([
        new() { Name = "Zebra Plant" },
        new() { Name = "Aloe Vera" },
        new() { Name = "Monstera" }
    ]);
    
    // Act
    var plants = await service.GetAllPlantsAsync();
    
    // Assert
    Assert.Equal(3, plants.Count);
    Assert.Equal("Aloe Vera", plants[0].Name);
    Assert.Equal("Monstera", plants[1].Name);
    Assert.Equal("Zebra Plant", plants[2].Name);
}
```

## Debugging Tests

### Visual Studio

1. Set breakpoint in test method
2. Right-click test → Debug
3. Step through code with F10/F11

### Check Test Output

```csharp
[Fact]
public void MyTest()
{
    // Use output helper for debugging
    var output = new TestOutputHelper();
    output.WriteLine("Debug message");
    
    // Your test code
}
```

### Viewing Database During Tests

```csharp
[Fact]
public async Task DebugDatabaseState()
{
    var context = _serviceProvider.GetService<ApplicationDbContext>();
    
    // Add test data
    context.Plants.Add(new PlantDbModel { Name = "Test" });
    await context.SaveChangesAsync();
    
    // Inspect database state
    var plants = await context.Plants.ToListAsync();
    // Set breakpoint here and inspect 'plants'
}
```

## Continuous Testing

### File Watcher (dotnet watch)

Run tests automatically when code changes:

```bash
dotnet watch test
```

### VS Live Unit Testing

Enable in Visual Studio:
- Test → Live Unit Testing → Start

Tests run automatically as you type.

## Future Testing Improvements

1. **Add UI Testing**: Use .NET MAUI UI testing framework
2. **Integration Tests**: Test full application flows
3. **Performance Tests**: Benchmark critical operations
4. **Add Moq**: For more sophisticated mocking
5. **Increase Coverage**: Target 80%+ code coverage
6. **Add API Tests**: When backend is added

## Resources

- [xUnit Documentation](https://xunit.net/)
- [.NET Testing Best Practices](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices)
- [Coverlet Coverage](https://github.com/coverlet-coverage/coverlet)
- [Moq Mocking Library](https://github.com/moq/moq4)

## Troubleshooting

### Tests Not Discovered

**Solution**:
1. Rebuild solution
2. Clean test cache: Test → Clean Test Cache
3. Restart Visual Studio

### Database Locked Errors

**Solution**:
- Ensure each test uses a new DbContext
- Use scoped services from DI container
- Don't reuse DbContext across tests

### Flaky Tests

**Causes**:
- Timing issues (use async/await, not Thread.Sleep)
- Shared state between tests
- Order-dependent tests

**Solution**:
- Make tests independent
- Reset state in test setup
- Use proper async patterns

---

**Remember**: Good tests are maintainable, readable, and fast!
