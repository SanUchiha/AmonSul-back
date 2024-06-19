using AS.Application.DTOs.Faccion;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper
{
    public class FaccionMappingProfile : Profile
    {
        public FaccionMappingProfile()
        {
            CreateMap<Faccion, FaccionDTO>().ReverseMap();
        }
    }
}
