using AS.Application.DTOs;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper
{
    public class UsuarioMappingProfile : Profile
    {
        public UsuarioMappingProfile()
        {
            CreateMap<Usuario, RegistrarUsuarioDTO>().ReverseMap();
        }
    }
}
