using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class InscripcionMappingProfile : Profile
{
    public InscripcionMappingProfile()
    {
        CreateMap<CrearInscripcionEquipoDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<CrearInscripcionDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<InscripcionDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionUsuarioIndividualDTO>()
            .ForMember(dest => dest.Nick, opt => opt.MapFrom(src => src.IdUsuarioNavigation!.Nick))
            .ForMember(dest => dest.NombreTorneo, opt => opt.MapFrom(src => src.IdTorneoNavigation!.NombreTorneo))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.IdTorneoNavigation))
            .ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionUsuarioEquipoDTO>()
            .ForMember(dest => dest.Nick, opt => opt.MapFrom(src => src.IdUsuarioNavigation!.Nick))
            .ForMember(dest => dest.NombreTorneo, opt => opt.MapFrom(src => src.IdTorneoNavigation!.NombreTorneo))
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.IdTorneoNavigation))
            .ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionTorneoEquiposDTO>().ReverseMap();
        CreateMap<InscripcionTorneoEmparejamientoDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionTorneoCreadoDTO>().ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionTorneoDTO>()
            .ForMember(dest => dest.Torneo, opt => opt.MapFrom(src => src.IdTorneoNavigation))
            .ReverseMap();
        CreateMap<InscripcionTorneo, InscripcionEquipoDTO>().ReverseMap();
        CreateMap<InscripcionTorneo, ComponentesEquipoDTO>()
            .ForMember(dest => dest.IdLista, opt => opt.MapFrom(src => src.Lista.FirstOrDefault()!.IdLista))
            .ForMember(dest => dest.ListaData, opt => opt.MapFrom(src => src.Lista.FirstOrDefault()!.ListaData))
            .ForMember(dest => dest.FechaEntregaLista, opt => opt.MapFrom(src => src.Lista.FirstOrDefault()!.FechaEntrega))
            .ForMember(dest => dest.Ejercito, opt => opt.MapFrom(src => src.Lista.FirstOrDefault()!.Ejercito))
            .ForMember(dest => dest.Nick, opt => opt.MapFrom(src => src.IdUsuarioNavigation!.Nick))
            .ReverseMap();

        CreateMap<InscripcionTorneo, InscripcionTorneoCreadoMasDTO>()
            .ForMember(dest => dest.Lista, opt => opt.MapFrom(src => src.Lista))
            .ReverseMap();

        CreateMap<InscripcionTorneoEmparejamientoDTO, InscripcionTorneoDTO>().ReverseMap();
        CreateMap<JugadoresEquipoParaCambioDTO, InscripcionEquipoDTO>().ReverseMap();
    }
}
