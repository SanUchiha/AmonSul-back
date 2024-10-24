using AS.Application.DTOs.Partida;
using AS.Application.DTOs.PartidaAmistosa;
using AutoMapper;

namespace AS.Application.Mapper;

public class MatchMappingProfile : Profile
{
    public MatchMappingProfile()
    {
        CreateMap<ViewPartidaAmistosaDTO, MatchDTO>()
            .ForMember(dest => dest.GanadorPartidaNick, opt => opt.MapFrom(src => src.GanadorPartidaNick))
            .ForMember(dest => dest.EsTorneo, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.GanadorPartida, opt => opt.MapFrom(src => src.GanadorPartida));

        CreateMap<ViewPartidaTorneoDTO, MatchDTO>()
            .ForMember(dest => dest.GanadorPartidaNick, opt => opt.MapFrom(src => src.GanadorPartidaNick))
            .ForMember(dest => dest.EsTorneo, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.GanadorPartida, opt => opt.MapFrom(src => src.GanadorPartidaTorneo));

    }
}
