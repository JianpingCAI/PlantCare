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

    private static string GetThumbnailPath(string photoPath)
    {
        if (string.IsNullOrEmpty(photoPath) || photoPath.Contains(ConstStrings.DefaultPhotoPath))
        {
            return photoPath;
        }

        string fileName = Path.GetFileName(photoPath);
        string thumbnailsDirectory = Path.Combine(FileSystem.AppDataDirectory, "thumbnails");
        return Path.Combine(thumbnailsDirectory, $"thumb_{fileName}");
    }
}
