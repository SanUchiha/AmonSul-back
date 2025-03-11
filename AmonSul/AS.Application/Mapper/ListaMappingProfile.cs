using AS.Application.DTOs.Lista;
using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class ListaMappingProfile : Profile
{
    public ListaMappingProfile()
    {
        CreateMap<CreateListaTorneoDTO, Lista>().ReverseMap();
        CreateMap<ListaViewDTO, Lista>().ReverseMap();
        CreateMap<ListaDTO, Lista>().ReverseMap();
    }
}
