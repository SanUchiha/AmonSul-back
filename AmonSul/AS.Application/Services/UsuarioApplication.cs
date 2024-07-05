using AS.Application.DTOs.Email;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Usuario;
using AS.Application.Exceptions;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.DTOs.Login;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Constantes;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace AS.Application.Services;

public class UsuarioApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    Utilidades utilidades,
    IAccountRepository accountRepository,
    ILogger<UsuarioApplication> logger,
    IEmailSender emailSender,
    IPartidaAmistosaApplication partidaAmistosaApplication) : IUsuarioApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly Utilidades _utilidades = utilidades;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly ILogger<UsuarioApplication> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication = partidaAmistosaApplication;

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
            usuario.Contraseña = _utilidades.encriptarSHA256(usuario.Contraseña);
        }

        if (usuario.NuevoEmail != null) usuario.Email = usuario.NuevoEmail;

        foreach (var prop in usuario.GetType().GetProperties())
        {
            var newValue = prop.GetValue(usuario);
            if (newValue != null)
            {
                var rawProp = rawUsuario.GetType().GetProperty(prop.Name);
                rawProp?.SetValue(rawUsuario, newValue);
            }
        }

        var result = await _unitOfWork.UsuarioRepository.Edit(rawUsuario);
        return result;
    }

    public async Task<List<ViewUsuarioPartidaDTO>> GetAll()
    {
        List<Usuario> rawUsuario = await _unitOfWork.UsuarioRepository.GetAll();

        List<ViewUsuarioPartidaDTO> listViewUsuarioPartidaDTO = [];

        foreach (var item in rawUsuario)
        {
            ViewUsuarioPartidaDTO obj = new();
            obj.IdUsuario = item.IdUsuario;
            obj.Email = item.Email;
            obj.Nick = item.Nick;
            obj.Ciudad = item.Ciudad;
            obj.FechaRegistro = item.FechaRegistro;
            obj.IdFaccion = item.IdFaccion;

            var partidasAmistosas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(obj.Email);
            obj.NumeroPartidasJugadas = partidasAmistosas.Count;
            int contadorVictorias = 0;
            int contadorEmpates = 0;
            int contadorDerrotas = 0;
            foreach (var partida in partidasAmistosas)
            {
                if (partida.GanadorPartida == obj.IdUsuario) contadorVictorias++;
                else if (partida.GanadorPartida == 0) contadorEmpates++;
                else contadorDerrotas++;
            }
            obj.PartidasGanadas = contadorVictorias;
            obj.PartidasEmpatadas = contadorEmpates;
            obj.PartidasPerdidas = contadorDerrotas;

            //TODO
            //elo
            //clasi elo


            listViewUsuarioPartidaDTO.Add(obj);
        }

        return _mapper.Map<List<ViewUsuarioPartidaDTO>>(listViewUsuarioPartidaDTO);
    }

    public async Task<ViewUsuarioPartidaDTO> GetByEmail(string email)
    {
        Usuario rawUsuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (rawUsuario == null) return new();

        ViewUsuarioPartidaDTO obj = new();
        obj.IdUsuario = rawUsuario.IdUsuario;
        obj.Email = rawUsuario.Email;
        obj.Nick = rawUsuario.Nick;
        obj.Ciudad = rawUsuario.Ciudad;
        obj.FechaRegistro = rawUsuario.FechaRegistro;
        obj.IdFaccion = rawUsuario.IdFaccion;

        var partidasAmistosas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(obj.Email);
        obj.NumeroPartidasJugadas = partidasAmistosas.Count;
        int contadorVictorias = 0;
        int contadorEmpates = 0;
        int contadorDerrotas = 0;
        foreach (var partida in partidasAmistosas)
        {
            if (partida.GanadorPartida == obj.IdUsuario) contadorVictorias++;
            else if (partida.GanadorPartida == 0) contadorEmpates++;
            else contadorDerrotas++;
        }
        obj.PartidasGanadas = contadorVictorias;
        obj.PartidasEmpatadas = contadorEmpates;
        obj.PartidasPerdidas = contadorDerrotas;

        //TODO ELO

        return obj;
    }

    public async Task<UsuarioDTO> GetByNick(string nick)
    {
        var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetByNick(nick);
        return _mapper.Map<UsuarioDTO>(usuarioEncontrado);
    }

    public async Task<string> GetNickById(int idUsuario)
    {
        var usuario = await _unitOfWork.UsuarioRepository.GetById(idUsuario);
        if (usuario == null) return "";

        return usuario.Nick;
    }

    public async Task<UsuarioViewDTO> GetUsuario(string email)
    {
        var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetUsuario(email);
        return _mapper.Map<UsuarioViewDTO>(usuarioEncontrado);
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

            EmailRegisterDTO emailRegisterDTO = new()
            {
                EmailTo = registrarUsuarioDTO.Email,
                Subject = ConstEmailMessage.MessageBienvenidaAsunto,
                Body = ConstEmailMessage.MessageBienvenidaBody
            };

            try
            {
                await _emailSender.SendEmailRegister(emailRegisterDTO);
            }
            catch (EmailSendException emailEx)
            {
                _logger.LogError(emailEx, "Error al enviar el correo de bienvenida. ${emailEx.Message}", emailEx.Message);
            }

            return response;

        }
        catch
        {
            throw;
        }
    }
}
