using AS.Application.DTOs.Usuario;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.DTOs;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services
{
    public class UsuarioApplication(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        Utilidades utilidades, 
        IAccountRepository accountRepository) : IUsuarioApplication
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly Utilidades _utilidades = utilidades;
        private readonly IAccountRepository _accountRepository = accountRepository;

        public Task<bool> Delete(string email)
        {
            return _unitOfWork.UsuarioRepository.Delete(email);
        }

        public async Task<bool> Edit(EditarUsuarioDTO usuario)
        {
            var usuarioEncontrado = await GetByEmail(usuario.Email);
            if (usuarioEncontrado == null) return false;

            var rawUsuario = await _unitOfWork.UsuarioRepository.GetUsuario(usuario.Email);
            if (rawUsuario == null) return false;

            if (usuario.Contraseña != null)
            {
                var rawPass = usuario.Contraseña;
                usuario.Contraseña = _utilidades.encriptarSHA256(usuario.Contraseña);
            }

            if(usuario.NuevoEmail != null) usuario.Email = usuario.NuevoEmail;

            foreach (var prop in usuario.GetType().GetProperties())
            {
                var newValue = prop.GetValue(usuario);
                if (newValue != null)
                {
                    var rawProp = rawUsuario.GetType().GetProperty(prop.Name);
                    if (rawProp != null)
                    {
                        rawProp.SetValue(rawUsuario, newValue);
                    }
                }
            }

            var result = await _unitOfWork.UsuarioRepository.Edit(rawUsuario);
            return result;
        }

        public async Task<List<UsuarioDTO>> GetAll()
        {
            var response = await _unitOfWork.UsuarioRepository.GetAll();
            return _mapper.Map<List<UsuarioDTO>>(response);
        }

        public async Task<UsuarioDTO> GetByEmail(string email)
        {
            var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetByEmail(email);
            return _mapper.Map<UsuarioDTO>(usuarioEncontrado);
        }

        public async Task<UsuarioDTO> GetByNick(string nick)
        {
            var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetByNick(nick);
            return _mapper.Map<UsuarioDTO>(usuarioEncontrado);
        }

        public async Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO registrarUsuarioDTO)
        {
            try
            {
                var rawPass = registrarUsuarioDTO.Contraseña;
                registrarUsuarioDTO.Contraseña = _utilidades.encriptarSHA256(registrarUsuarioDTO.Contraseña);
                var usuario = _mapper.Map<Usuario>(registrarUsuarioDTO);
                var rawResponse = await _unitOfWork.UsuarioRepository.Register(usuario);

                var loginDTO = new LoginDTO
                {
                    Email = registrarUsuarioDTO.Email,
                    Password = rawPass
                };

                var login = await _accountRepository.Login(loginDTO);

                var response = new RegistrarUsuarioResponseDTO
                {
                    Status = true,
                    Message = "Usuario creado con existo",
                    Token = login.Token
                };

                return response;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }        
        }
    }
}
