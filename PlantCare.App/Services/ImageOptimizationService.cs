using System.Diagnostics;

namespace PlantCare.App.Services;

/// <summary>
/// Image optimization service for compressing and resizing plant photos
/// </summary>
public interface IImageOptimizationService
{
    /// <summary>
    /// Optimizes an image from a stream and saves it to the photos directory
    /// </summary>
    /// <param name="sourceStream">Source image stream</param>
    /// <param name="fileName">Desired filename</param>
    /// <returns>Path to the saved optimized image</returns>
    Task<string> OptimizeAndSaveImageAsync(Stream sourceStream, string fileName);

    /// <summary>
    /// Creates a thumbnail for a given image path
    /// </summary>
    /// <param name="imagePath">Path to the source image</param>
    /// <returns>Path to the created thumbnail</returns>
    Task<string> CreateThumbnailAsync(string imagePath);

    /// <summary>
    /// Gets the thumbnail path for a given image path
    /// </summary>
    /// <param name="imagePath">Path to the source image</param>
    /// <returns>Path where the thumbnail should be/is stored</returns>
    string GetThumbnailPath(string imagePath);

    /// <summary>
    /// Deletes both the image and its thumbnail
    /// </summary>
    /// <param name="imagePath">Path to the image to delete</param>
    Task DeleteImageAndThumbnailAsync(string imagePath);

    /// <summary>
    /// Cleans up orphaned images and thumbnails
    /// </summary>
    Task CleanupOrphanedImagesAsync(IEnumerable<string> validImagePaths);
    
    /// <summary>
    /// Regenerates missing thumbnails for all provided photo paths
    /// </summary>
    /// <param name="photoPaths">Collection of photo paths to check and regenerate thumbnails for</param>
    /// <returns>Number of thumbnails regenerated</returns>
    Task<int> RegenerateMissingThumbnailsAsync(IEnumerable<string> photoPaths);
}

public class ImageOptimizationService : IImageOptimizationService
{
    private const int MaxImageWidth = 800;
    private const int MaxImageHeight = 800;
    private const int ThumbnailSize = 300;
    private const int JpegQuality = 90;

    private readonly string _photosDirectory;
    private readonly string _thumbnailsDirectory;

    public ImageOptimizationService()
    {
        _photosDirectory = Path.Combine(FileSystem.AppDataDirectory, "photos");
        _thumbnailsDirectory = Path.Combine(FileSystem.AppDataDirectory, "thumbnails");

        Directory.CreateDirectory(_photosDirectory);
        Directory.CreateDirectory(_thumbnailsDirectory);
    }

    public async Task<string> OptimizeAndSaveImageAsync(Stream sourceStream, string fileName)
    {
        try
        {
            // Resize the image
            byte[] resizedImage = await ResizeImageAsync(sourceStream, MaxImageWidth, MaxImageHeight);

            // Save the optimized image
            string fullPath = Path.Combine(_photosDirectory, fileName);
            await File.WriteAllBytesAsync(fullPath, resizedImage);

            // Create thumbnail
            using var thumbnailStream = new MemoryStream(resizedImage);
            await CreateThumbnailInternalAsync(thumbnailStream, fileName);

            return fullPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error optimizing image: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CreateThumbnailAsync(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException("Source image not found", imagePath);
        }

        using var sourceStream = File.OpenRead(imagePath);
        string fileName = Path.GetFileName(imagePath);
        return await CreateThumbnailInternalAsync(sourceStream, fileName);
    }

    private async Task<string> CreateThumbnailInternalAsync(Stream sourceStream, string fileName)
    {
        byte[] thumbnailImage = await ResizeImageAsync(sourceStream, ThumbnailSize, ThumbnailSize);
        
        string thumbnailPath = Path.Combine(_thumbnailsDirectory, $"thumb_{fileName}");
        await File.WriteAllBytesAsync(thumbnailPath, thumbnailImage);

        return thumbnailPath;
    }

    public string GetThumbnailPath(string imagePath)
    {
        string fileName = Path.GetFileName(imagePath);
        return Path.Combine(_thumbnailsDirectory, $"thumb_{fileName}");
    }

    public async Task DeleteImageAndThumbnailAsync(string imagePath)
    {
        await Task.Run(() =>
        {
            // Delete main image
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }

            // Delete thumbnail
            string thumbnailPath = GetThumbnailPath(imagePath);
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
        });
    }

    public async Task CleanupOrphanedImagesAsync(IEnumerable<string> validImagePaths)
    {
        await Task.Run(() =>
        {
            var validPaths = new HashSet<string>(validImagePaths);

            // Clean up photos directory
            var photoFiles = Directory.GetFiles(_photosDirectory);
            foreach (var photoFile in photoFiles)
            {
                if (!validPaths.Contains(photoFile))
                {
                    File.Delete(photoFile);
                }
            }

            // Clean up thumbnails directory
            var thumbnailFiles = Directory.GetFiles(_thumbnailsDirectory);
            foreach (var thumbnailFile in thumbnailFiles)
            {
                string originalFileName = Path.GetFileName(thumbnailFile).Replace("thumb_", "");
                string originalPath = Path.Combine(_photosDirectory, originalFileName);
                
                if (!validPaths.Contains(originalPath))
                {
                    File.Delete(thumbnailFile);
                }
            }
        });
    }

    public async Task<int> RegenerateMissingThumbnailsAsync(IEnumerable<string> photoPaths)
    {
        int regeneratedCount = 0;

        foreach (var photoPath in photoPaths)
        {
            // Skip default or empty paths
            if (string.IsNullOrEmpty(photoPath) || photoPath.Contains("default_plant.png"))
                continue;

            // Check if photo file exists
            if (!File.Exists(photoPath))
                continue;

            string thumbnailPath = GetThumbnailPath(photoPath);

            // Regenerate thumbnail if missing
            if (!File.Exists(thumbnailPath))
            {
                try
                {
                    await CreateThumbnailAsync(photoPath);
                    regeneratedCount++;
                    Debug.WriteLine($"Regenerated thumbnail for: {Path.GetFileName(photoPath)}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to regenerate thumbnail for {photoPath}: {ex.Message}");
                }
            }
        }

        if (regeneratedCount > 0)
        {
            Debug.WriteLine($"Regenerated {regeneratedCount} missing thumbnails");
        }

        return regeneratedCount;
    }

    private static async Task<byte[]> ResizeImageAsync(Stream sourceStream, int maxWidth, int maxHeight)
    {
        return await Task.Run(() =>
        {
            using var sourceBitmap = SkiaSharp.SKBitmap.Decode(sourceStream);
            if (sourceBitmap == null)
            {
                throw new InvalidOperationException("Failed to decode image");
            }

            int width = sourceBitmap.Width;
            int height = sourceBitmap.Height;

            float aspectRatio = (float)width / height;

            // Calculate new dimensions maintaining aspect ratio
            if (width > maxWidth || height > maxHeight)
            {
                if (aspectRatio > 1)
                {
                    // Landscape
                    width = maxWidth;
                    height = (int)(maxWidth / aspectRatio);
                }
                else
                {
                    // Portrait
                    height = maxHeight;
                    width = (int)(maxHeight * aspectRatio);
                }
            }

            using var resizedBitmap = sourceBitmap.Resize(
                new SkiaSharp.SKImageInfo(width, height),
                SkiaSharp.SKFilterQuality.High);

            if (resizedBitmap == null)
            {
                throw new InvalidOperationException("Failed to resize image");
            }

            using var image = SkiaSharp.SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, JpegQuality);
            
            return data.ToArray();
        });
    }
}
