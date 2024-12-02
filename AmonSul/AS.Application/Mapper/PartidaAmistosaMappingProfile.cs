using AS.Application.DTOs.PartidaAmistosa;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class PartidaAmistosaMappingProfile : Profile
{
    public PartidaAmistosaMappingProfile()
    {
        CreateMap<PartidaAmistosa, ViewPartidaAmistosaDTO>()
             .ForMember(dest => dest.NickUsuario1, opt => opt.MapFrom(src => src.IdUsuario1Navigation!.Nick))
             .ForMember(dest => dest.NickUsuario2, opt => opt.MapFrom(src => src.IdUsuario2Navigation!.Nick))
            .ReverseMap();
        CreateMap<PartidaAmistosa, CreatePartidaAmistosaDTO>().ReverseMap();
        CreateMap<PartidaAmistosa, UpdatePartidaAmistosaDTO>().ReverseMap();
        CreateMap<UpdatePartidaAmistosaDTO, ViewPartidaAmistosaDTO>().ReverseMap();
    }
}
