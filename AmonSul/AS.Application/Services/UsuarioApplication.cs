using AS.Application.DTOs;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services
{
    public class UsuarioApplication : IUsuarioApplication
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Utilidades _utilidades;

        public UsuarioApplication(IUnitOfWork unitOfWork, IMapper mapper, Utilidades utilidades)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _utilidades = utilidades;
        }

        public Task<bool> Delete(EliminarUsuarioDTO usuario)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Edit(EditarUsuarioDTO usuario)
        {
            throw new NotImplementedException();
        }

        public Task<UsuarioDTO> GetById(int IdUsuario)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Register(RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            registrarUsuarioDTO.Contraseña = _utilidades.encriptarSHA256(registrarUsuarioDTO.Contraseña);
            var usuario = _mapper.Map<Usuario>(registrarUsuarioDTO);
            var response = await _unitOfWork.UsuarioRepository.Register(usuario);

            return response;
        }
    }
}
