using AS.Application.DTOs.Inscripcion;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class InscripcionMappingProfile : Profile
{
    public InscripcionMappingProfile()
    {
        CreateMap<CrearInscripcionDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionUsuarioDTO>()
            .ForMember(dest => dest.Nick, opt => opt.MapFrom(src => src.IdUsuarioNavigation!.Nick))
            .ReverseMap();
    }
}
