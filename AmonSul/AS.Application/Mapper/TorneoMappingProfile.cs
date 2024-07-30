using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.Torneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class TorneoMappingProfile : Profile
{
    public TorneoMappingProfile()
    {
        CreateMap<Torneo, TorneoDTO>().ReverseMap();
        CreateMap<Torneo, CrearTorneoDTO>().ReverseMap();
        CreateMap<Torneo, TorneoCreadoUsuarioDTO>()
            .ForMember(dest => dest.InscripcionTorneos, opt => opt.MapFrom(src => src.InscripcionTorneos))
            .ReverseMap();

        CreateMap<InscripcionTorneoCreadoDTO, InscripcionTorneo>().ReverseMap();

    }
}
