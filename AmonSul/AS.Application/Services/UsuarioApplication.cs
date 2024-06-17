using AS.Application.DTOs;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services
{
    public class UsuarioApplication : IUsuarioApplication
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UsuarioApplication(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var usuario = _mapper.Map<Usuario>(registrarUsuarioDTO);
            var response = await _unitOfWork.UsuarioRepository.Register(usuario);

            return response;
        }
    }
}
