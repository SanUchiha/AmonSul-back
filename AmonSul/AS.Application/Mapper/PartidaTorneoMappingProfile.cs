using AS.Application.DTOs.PartidaTorneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class PartidaTorneoMappingProfile : Profile
{
    public PartidaTorneoMappingProfile()
    {
        CreateMap<PartidaTorneo, PartidaTorneoDTO>().ReverseMap();
    }
}
