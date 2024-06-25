using AS.Application.DTOs;
using AS.Application.DTOs.Usuario;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.DTOs;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services
{
    public class UsuarioApplication : IUsuarioApplication
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly Utilidades _utilidades;
        private readonly IAccountRepository _accountRepository;


        public UsuarioApplication(IUnitOfWork unitOfWork, IMapper mapper, Utilidades utilidades, IAccountRepository accountRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _utilidades = utilidades;
            _accountRepository = accountRepository;
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

        public async Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            try
            {
                var rawPass = registrarUsuarioDTO.Contraseña;
                registrarUsuarioDTO.Contraseña = _utilidades.encriptarSHA256(registrarUsuarioDTO.Contraseña);
                var usuario = _mapper.Map<Usuario>(registrarUsuarioDTO);
                var rawResponse = await _unitOfWork.UsuarioRepository.Register(usuario);

                var loginDTO = new LoginDTO();
                loginDTO.Email = registrarUsuarioDTO.Email;
                loginDTO.Password = rawPass;

                var login = await _accountRepository.Login(loginDTO);

                var response = new RegistrarUsuarioResponseDTO();
                response.Status = true;
                response.Message = "Usuario creado con existo";
                response.Token = login.Token;

                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }        }
    }
}
