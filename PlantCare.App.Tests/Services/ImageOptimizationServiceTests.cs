using PlantCare.App.Services;
using PlantCare.Data;
using Xunit;

namespace PlantCare.App.Tests.Services;

public class ImageOptimizationServiceTests
{
    [Fact]
    public void GetThumbnailPath_WithValidPhotoPath_ReturnsCorrectThumbnailPath()
    {
        // Arrange
        var service = new ImageOptimizationService();
        string photoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", "abc123.jpg");
        string expectedThumbnailPath = Path.Combine(FileSystem.AppDataDirectory, "thumbnails", "thumb_abc123.jpg");

        // Act
        string actualThumbnailPath = service.GetThumbnailPath(photoPath);

        // Assert
        Assert.Equal(expectedThumbnailPath, actualThumbnailPath);
    }

    [Fact]
    public void GetThumbnailPath_WithDefaultPhotoPath_ReturnsOriginalPath()
    {
        // Arrange
        var service = new ImageOptimizationService();
        string photoPath = ConstStrings.DefaultPhotoPath;

        // Act
        string actualThumbnailPath = service.GetThumbnailPath(photoPath);

        // Assert
        Assert.Equal(photoPath, actualThumbnailPath);
    }

    [Fact]
    public void GetThumbnailPath_WithEmptyPath_ReturnsEmptyPath()
    {
        // Arrange
        var service = new ImageOptimizationService();
        string photoPath = string.Empty;

        // Act
        string actualThumbnailPath = service.GetThumbnailPath(photoPath);

        // Assert
        Assert.Equal(string.Empty, actualThumbnailPath);
    }

    [Fact]
    public void GetThumbnailPath_WithNullPath_ReturnsNull()
    {
        // Arrange
        var service = new ImageOptimizationService();
        string? photoPath = null;

        // Act
        string? actualThumbnailPath = service.GetThumbnailPath(photoPath!);

        // Assert
        Assert.Null(actualThumbnailPath);
    }

    [Theory]
    [InlineData("photo.jpg", "thumb_photo.jpg")]
    [InlineData("my-plant-123.png", "thumb_my-plant-123.png")]
    [InlineData("a1b2c3d4-e5f6.jpeg", "thumb_a1b2c3d4-e5f6.jpeg")]
    public void GetThumbnailPath_WithVariousFileNames_FollowsNamingConvention(string fileName, string expectedThumbName)
    {
        // Arrange
        var service = new ImageOptimizationService();
        string photoPath = Path.Combine(FileSystem.AppDataDirectory, "photos", fileName);
        string expectedThumbnailPath = Path.Combine(FileSystem.AppDataDirectory, "thumbnails", expectedThumbName);

        // Act
        string actualThumbnailPath = service.GetThumbnailPath(photoPath);

        // Assert
        Assert.Equal(expectedThumbnailPath, actualThumbnailPath);
    }

    [Fact]
    public async Task RegenerateMissingThumbnailsAsync_WithEmptyCollection_ReturnsZero()
    {
        // Arrange
        var service = new ImageOptimizationService();
        var emptyPaths = new List<string>();

        // Act
        int count = await service.RegenerateMissingThumbnailsAsync(emptyPaths);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RegenerateMissingThumbnailsAsync_WithDefaultPaths_ReturnsZero()
    {
        // Arrange
        var service = new ImageOptimizationService();
        var defaultPaths = new List<string> { ConstStrings.DefaultPhotoPath, string.Empty };

        // Act
        int count = await service.RegenerateMissingThumbnailsAsync(defaultPaths);

        // Assert
        Assert.Equal(0, count);
    }

    [Fact]
    public async Task RegenerateMissingThumbnailsAsync_WithNonExistentPhotos_ReturnsZero()
    {
        // Arrange
        var service = new ImageOptimizationService();
        var nonExistentPaths = new List<string> 
        { 
            Path.Combine(FileSystem.AppDataDirectory, "photos", "nonexistent1.jpg"),
            Path.Combine(FileSystem.AppDataDirectory, "photos", "nonexistent2.jpg")
        };

        // Act
        int count = await service.RegenerateMissingThumbnailsAsync(nonExistentPaths);

        // Assert
        Assert.Equal(0, count);
    }
}
