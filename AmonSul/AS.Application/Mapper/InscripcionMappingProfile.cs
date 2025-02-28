using AS.Application.DTOs.Inscripcion;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class InscripcionMappingProfile : Profile
{
    public InscripcionMappingProfile()
    {
        CreateMap<CrearInscripcionEquipoDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<CrearInscripcionDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionUsuarioDTO>()
            .ForMember(dest => dest.Nick, opt => opt.MapFrom(src => src.IdUsuarioNavigation!.Nick))
            .ForMember(dest => dest.NombreTorneo, opt => opt.MapFrom(src => src.IdTorneoNavigation!.NombreTorneo))
            .ForMember(dest => dest.IdEquipo, opt => opt.MapFrom(src => src.Equipo!.IdEquipo))
            .ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionTorneoEquiposDTO>().ReverseMap();

        CreateMap<InscripcionTorneo, InscripcionTorneoCreadoDTO>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionTorneoDTO>().ReverseMap();
    }
}
