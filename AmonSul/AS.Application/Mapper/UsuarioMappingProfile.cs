using AS.Application.DTOs.Faccion;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Domain.Models;
using AutoMapper;

namespace AS.Application.Mapper;

public class UsuarioMappingProfile : Profile
{
    public UsuarioMappingProfile()
    {
        CreateMap<Usuario, RegistrarUsuarioDTO>().ReverseMap();
        CreateMap<Usuario, UsuarioDTO>().ReverseMap();
        CreateMap<Usuario, UsuarioInscripcionTorneoDTO>().ReverseMap();
        CreateMap<Usuario, EditarUsuarioDTO>().ReverseMap();
        CreateMap<Usuario, UsuarioViewDTO>().ReverseMap();
        CreateMap<Usuario, ViewUsuarioPartidaDTO>().ReverseMap();
        CreateMap<ViewUsuarioPartidaDTO, ViewDetalleUsuarioDTO>().ReverseMap();
        CreateMap<Usuario, UsuarioDataDTO>()
                   .ForMember(dest => dest.PartidasValidadas, opt => opt.Ignore()) // Se calcularán después
                   .ForMember(dest => dest.PartidasPendientes, opt => opt.Ignore()) // Se calcularán después
                   .ForMember(dest => dest.ClasificacionElo, opt => opt.Ignore()) // Se calculará después
                   .ForMember(dest => dest.NumeroPartidasJugadas, opt => opt.Ignore()) // Se calculará después
                   .ForMember(dest => dest.PartidasGanadas, opt => opt.Ignore()) // Se calculará después
                   .ForMember(dest => dest.PartidasEmpatadas, opt => opt.Ignore()) // Se calculará después
                   .ForMember(dest => dest.PartidasPerdidas, opt => opt.Ignore()); // Se calculará después;
        CreateMap<UsuarioNickDTO, Usuario>().ReverseMap();
        CreateMap<Faccion, FaccionDTO>().ReverseMap(); // Mapeo entre Faccion y FaccionDTO
        CreateMap<PartidaTorneo, ViewPartidaTorneoDTO>()
            .ForMember(dest => dest.NombreTorneo, opt => opt.MapFrom(src => src.IdTorneoNavigation!.NombreTorneo))
            .ReverseMap(); 
        
    }
}
