using AS.Application.DTOs.LigaTorneo;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class LigaMappingProfile : Profile
{
    public LigaMappingProfile()
    {
        CreateMap<LigaTorneo, LigaTorneoDTO>().ReverseMap();
    }
}
