using AS.Application.DTOs.Usuario;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper
{
    public class UsuarioMappingProfile : Profile
    {
        public UsuarioMappingProfile()
        {
            CreateMap<Usuario, RegistrarUsuarioDTO>().ReverseMap();
            CreateMap<Usuario, UsuarioDTO>().ReverseMap();
            CreateMap<Usuario, EditarUsuarioDTO>().ReverseMap();

        }
    }
}
