using AS.Application.DTOs.PartidaAmistosa;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class PartidaAmistosaMappingProfile : Profile
{
    public PartidaAmistosaMappingProfile()
    {
        CreateMap<PartidaAmistosa, PartidaAmistosaDTO>().ReverseMap();
    }
}
