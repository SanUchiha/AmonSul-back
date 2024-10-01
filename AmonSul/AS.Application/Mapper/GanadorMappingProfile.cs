using AS.Application.DTOs.Ganador;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class GanadorMappingProfile : Profile
{
    public GanadorMappingProfile()
    {
        CreateMap<Ganador, GanadorDTO>().ReverseMap();
    }
}
