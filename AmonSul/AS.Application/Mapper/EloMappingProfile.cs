using AS.Application.DTOs.Elo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class EloMappingProfile : Profile
{
    public EloMappingProfile()
    {
        CreateMap<Elo, CreateEloDTO>().ReverseMap();
        CreateMap<Elo, EloDTO>().ReverseMap();
        CreateMap<ClasificacionElo, ViewEloDTO>().ReverseMap();
    }
}
