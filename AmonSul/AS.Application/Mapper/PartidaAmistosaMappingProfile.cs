using AS.Application.DTOs.PartidaAmistosa;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class PartidaAmistosaMappingProfile : Profile
{
    public PartidaAmistosaMappingProfile()
    {
        CreateMap<PartidaAmistosa, ViewPartidaAmistosaDTO>().ReverseMap();
        CreateMap<PartidaAmistosa, CreatePartidaAmistosaDTO>().ReverseMap();
        CreateMap<PartidaAmistosa, UpdatePartidaAmistosaDTO>().ReverseMap();
        CreateMap<UpdatePartidaAmistosaDTO, ViewPartidaAmistosaDTO>().ReverseMap();
    }
}
