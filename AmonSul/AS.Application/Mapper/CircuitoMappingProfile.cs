using AS.Application.DTOs.CircuitoTorneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class CircuitoMappingProfile : Profile
{
    public CircuitoMappingProfile()
    {
        CreateMap<CircuitoTorneo, CircuitoTorneoDTO>().ReverseMap();
    }
}
