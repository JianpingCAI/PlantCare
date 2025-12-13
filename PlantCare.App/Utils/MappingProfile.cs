using AutoMapper;
using PlantCare.Data;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Utils;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PlantDbModel, Plant>()
            .ForMember(dest => dest.ThumbnailPath, opt => opt.MapFrom(src => GetThumbnailPath(src.PhotoPath)));
        
        CreateMap<Plant, PlantDbModel>();
    }

    /// <summary>
    /// Gets the thumbnail path for a given photo path using naming convention.
    /// Falls back to the original photo path if thumbnail doesn't exist.
    /// Convention: thumbnail filename = "thumb_{originalFileName}"
    /// </summary>
    /// <param name="photoPath">Path to the main photo</param>
    /// <returns>Path to thumbnail if exists, otherwise path to main photo</returns>
    private static string GetThumbnailPath(string photoPath)
    {
        // Return as-is for default or empty paths
        if (string.IsNullOrEmpty(photoPath) || photoPath.Contains(ConstStrings.DefaultPhotoPath))
        {
            return photoPath;
        }

        // Construct expected thumbnail path based on convention
        string fileName = Path.GetFileName(photoPath);
        string thumbnailsDirectory = Path.Combine(FileSystem.AppDataDirectory, "thumbnails");
        string thumbnailPath = Path.Combine(thumbnailsDirectory, $"thumb_{fileName}");
        
        // ✅ Validate thumbnail exists, fallback to main photo if missing
        if (!File.Exists(thumbnailPath))
        {
            // Thumbnail missing - fallback to main photo
            // Note: Consider calling RegenerateMissingThumbnailsAsync() to recreate it
            return photoPath;
        }
        
        return thumbnailPath;
    }
}
