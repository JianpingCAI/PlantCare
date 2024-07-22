using AutoMapper;
using PlantCare.Data.DbModels;
using PlantCare.Data.Models;

namespace PlantCare.App.Tests.Common;

public class MappingProfile2 : Profile
{
    public MappingProfile2()
    {
        CreateMap<PlantDbModel, Plant>();
        CreateMap<Plant, PlantDbModel>();

        //CreateMap<PlantDto, Plant>()
        //    .ForMember(dest => dest.NextWateringDate, opt => opt.Ignore())
        //    .ForMember(dest => dest.DaysUntilNextWatering, opt => opt.Ignore())
        //    .ForMember(dest => dest.WateringProgress, opt => opt.Ignore());

        //CreateMap<Plant, PlantDto>();
    }
}