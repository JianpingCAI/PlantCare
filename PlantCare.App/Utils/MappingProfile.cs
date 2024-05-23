using AutoMapper;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Utils;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PlantDbModel, Plant>()
            .ForMember(dest => dest.NextWateringDate, opt => opt.Ignore())
            .ForMember(dest => dest.WateringFrequencyInHours, opt => opt.Ignore())
            .ForMember(dest => dest.WateringProgress, opt => opt.Ignore());

        CreateMap<Plant, PlantDbModel>();

        //CreateMap<PlantDto, Plant>()
        //    .ForMember(dest => dest.NextWateringDate, opt => opt.Ignore())
        //    .ForMember(dest => dest.DaysUntilNextWatering, opt => opt.Ignore())
        //    .ForMember(dest => dest.WateringProgress, opt => opt.Ignore());

        //CreateMap<Plant, PlantDto>();
    }
}