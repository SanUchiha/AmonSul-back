﻿using AS.Application.DTOs.PartidaTorneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class PartidaTorneoMappingProfile : Profile
{
    public PartidaTorneoMappingProfile()
    {
        CreateMap<PartidaTorneo, PartidaTorneoDTO>()
             .ForMember(dest => dest.Nick1, opt => opt.MapFrom(src => src.IdUsuario1Navigation!.Nick))
             .ForMember(dest => dest.Nick2, opt => opt.MapFrom(src => src.IdUsuario2Navigation!.Nick))
             .ForMember(dest => dest.NombreEquipo1, opt => opt.MapFrom(src => src.IdEquipo1Navigation!.NombreEquipo))
             .ForMember(dest => dest.NombreEquipo2, opt => opt.MapFrom(src => src.IdEquipo2Navigation!.NombreEquipo))
             .ReverseMap();

        CreateMap<PartidaTorneo, PartidaTorneoMasDTO>()
             .ForMember(dest => dest.Nick1, opt => opt.MapFrom(src => src.IdUsuario1Navigation!.Nick))
             .ForMember(dest => dest.Nick2, opt => opt.MapFrom(src => src.IdUsuario2Navigation!.Nick))
             .ReverseMap();

        CreateMap<AddPairingTorneoDTO, PartidaTorneo>()
            .ForMember(dest => dest.NumeroRonda, opt => opt.MapFrom(src => src.IdRonda))
            .ReverseMap();
    }
}
