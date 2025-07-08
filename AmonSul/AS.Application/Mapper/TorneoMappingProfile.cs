using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class TorneoMappingProfile : Profile
{
    public TorneoMappingProfile()
    {
        CreateMap<Torneo, UpdateTorneoDTO>().ReverseMap();
        CreateMap<Torneo, TorneoDTO>().ReverseMap();
        CreateMap<Torneo, TorneoCreadoDTO>().ReverseMap();
        CreateMap<Torneo, TorneoViewDTO>().ReverseMap();

        CreateMap<InscripcionTorneoCreadoDTO, InscripcionTorneo>().ReverseMap();
        CreateMap<Equipo, EquipoDisponibleDTO>().ReverseMap();

        CreateMap<CrearTorneoDTO, Torneo>()
            .ForMember(dest => dest.BasesTorneo, opt =>
                opt.MapFrom(src => string.IsNullOrEmpty(src.BasesTorneo)
                    ? null
                    : Convert.FromBase64String(src.BasesTorneo)));
    }
}
