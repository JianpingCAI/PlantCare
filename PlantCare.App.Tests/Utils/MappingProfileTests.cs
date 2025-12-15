using AutoMapper;
using PlantCare.App.Utils;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;
using Xunit;

namespace PlantCare.App.Tests.Utils;

public class MappingProfileTests
{
    private readonly IMapper _mapper;

    public MappingProfileTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void MapPlantDbModelToPlant_WithValidPhotoPath_GeneratesThumbnailPath()
    {
        // Arrange
        var plantDb = new PlantDbModel
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            PhotoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", "test123.jpg")
        };

        // Act
        Plant plant = _mapper.Map<Plant>(plantDb);

        // Assert
        Assert.NotNull(plant);
        Assert.Equal(plantDb.Id, plant.Id);
        Assert.Equal(plantDb.Name, plant.Name);
        Assert.NotNull(plant.ThumbnailPath);
        Assert.Contains("thumb_test123.jpg", plant.ThumbnailPath);
    }

    [Fact]
    public void MapPlantDbModelToPlant_WithDefaultPhotoPath_UsesSamePathForThumbnail()
    {
        // Arrange
        var plantDb = new PlantDbModel
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            PhotoPath = ConstStrings.DefaultPhotoPath
        };

        // Act
        Plant plant = _mapper.Map<Plant>(plantDb);

        // Assert
        Assert.NotNull(plant);
        Assert.Equal(ConstStrings.DefaultPhotoPath, plant.PhotoPath);
        Assert.Equal(ConstStrings.DefaultPhotoPath, plant.ThumbnailPath);
    }

    [Fact]
    public void MapPlantDbModelToPlant_WithEmptyPhotoPath_ReturnsEmptyThumbnailPath()
    {
        // Arrange
        var plantDb = new PlantDbModel
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            PhotoPath = string.Empty
        };

        // Act
        Plant plant = _mapper.Map<Plant>(plantDb);

        // Assert
        Assert.NotNull(plant);
        Assert.Equal(string.Empty, plant.PhotoPath);
        Assert.Equal(string.Empty, plant.ThumbnailPath);
    }

    [Fact]
    public void MapPlantToPlantDbModel_PreservesThumbnailPath()
    {
        // Arrange
        var plant = new Plant
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            PhotoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", "test123.jpg"),
            ThumbnailPath = Path.Combine(FileSystem.AppDataDirectory, "thumbnails", "thumb_test123.jpg")
        };

        // Act
        PlantDbModel plantDb = _mapper.Map<PlantDbModel>(plant);

        // Assert
        Assert.NotNull(plantDb);
        Assert.Equal(plant.Id, plantDb.Id);
        Assert.Equal(plant.Name, plantDb.Name);
        Assert.Equal(plant.PhotoPath, plantDb.PhotoPath);
        // Note: PlantDbModel doesn't have ThumbnailPath property
    }

    [Theory]
    [InlineData("photo1.jpg", "thumb_photo1.jpg")]
    [InlineData("my-plant.png", "thumb_my-plant.png")]
    [InlineData("IMG_20240101_123456.jpeg", "thumb_IMG_20240101_123456.jpeg")]
    public void ThumbnailPathNamingConvention_FollowsExpectedPattern(string photoFileName, string expectedThumbFileName)
    {
        // Arrange
        var plantDb = new PlantDbModel
        {
            Id = Guid.NewGuid(),
            Name = "Test Plant",
            PhotoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", photoFileName)
        };

        // Act
        Plant plant = _mapper.Map<Plant>(plantDb);

        // Assert
        Assert.Contains(expectedThumbFileName, plant.ThumbnailPath);
    }

    [Fact]
    public void MapMultiplePlants_AllGetCorrectThumbnailPaths()
    {
        // Arrange
        var plantsDb = new List<PlantDbModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Plant 1", PhotoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", "p1.jpg") },
            new() { Id = Guid.NewGuid(), Name = "Plant 2", PhotoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", "p2.jpg") },
            new() { Id = Guid.NewGuid(), Name = "Plant 3", PhotoPath = ConstStrings.DefaultPhotoPath }
        };

        // Act
        List<Plant> plants = _mapper.Map<List<Plant>>(plantsDb);

        // Assert
        Assert.Equal(3, plants.Count);
        Assert.Contains("thumb_p1.jpg", plants[0].ThumbnailPath);
        Assert.Contains("thumb_p2.jpg", plants[1].ThumbnailPath);
        Assert.Equal(ConstStrings.DefaultPhotoPath, plants[2].ThumbnailPath);
    }
}
