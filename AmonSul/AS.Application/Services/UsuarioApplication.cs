using AS.Application.DTOs.Elo;
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

public class UsuarioApplication : IUsuarioApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly Utilidades _utilidades;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UsuarioApplication> _logger;
    private readonly IEmailSender _emailSender;
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication;
    private readonly IEloApplication _eloApplication;
    private readonly ITorneoApplication _torneoApplication;

    public UsuarioApplication(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        Utilidades utilidades,
        IAccountRepository accountRepository,
        ILogger<UsuarioApplication> logger,
        IEmailSender emailSender,
        IPartidaAmistosaApplication partidaAmistosaApplication,
        IEloApplication eloApplication,
        ITorneoApplication torneoApplication)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _utilidades = utilidades;
        _accountRepository = accountRepository;
        _logger = logger;
        _emailSender = emailSender;
        _partidaAmistosaApplication = partidaAmistosaApplication;
        _eloApplication = eloApplication;
        _torneoApplication = torneoApplication;
    }

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
            ViewUsuarioPartidaDTO obj = new()
            {
                IdUsuario = item.IdUsuario,
                Email = item.Email,
                Nick = item.Nick,
                Ciudad = item.Ciudad,
                FechaRegistro = item.FechaRegistro,
                IdFaccion = item.IdFaccion
            };

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
            int lastElo = await _eloApplication.GetLastElo(obj.IdUsuario);
            obj.PuntuacionElo = lastElo;


            listViewUsuarioPartidaDTO.Add(obj);
        }

        return _mapper.Map<List<ViewUsuarioPartidaDTO>>(listViewUsuarioPartidaDTO);
    }

    public async Task<ViewUsuarioPartidaDTO> GetByEmail(string email)
    {
        Usuario rawUsuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (rawUsuario == null) return new();

        ViewUsuarioPartidaDTO obj = new()
        {
            IdUsuario = rawUsuario.IdUsuario,
            Email = rawUsuario.Email,
            Nick = rawUsuario.Nick,
            Ciudad = rawUsuario.Ciudad,
            FechaRegistro = rawUsuario.FechaRegistro,
            IdFaccion = rawUsuario.IdFaccion
        };

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
        int lastElo = await _eloApplication.GetLastElo(obj.IdUsuario);
        obj.PuntuacionElo = lastElo;

        //obj.ClasificacionElo
        List<ClasificacionEloDTO> cla = await _eloApplication.GetClasificacion();
        cla = cla.OrderByDescending(item => item.Elo).ToList();

        obj.ClasificacionElo = cla.FindIndex(item => item.Nick == obj.Nick)+1;

        return obj;
    }

    public async Task<UsuarioDTO> GetById(int IdUsuario)
    {
        var usuario = await _unitOfWork.UsuarioRepository.GetById(IdUsuario);
        return _mapper.Map<UsuarioDTO>(usuario);
    }

    public async Task<UsuarioDTO> GetByNick(string nick)
    {
        var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetByNick(nick);
        return _mapper.Map<UsuarioDTO>(usuarioEncontrado);
    }

    public async Task<ViewDetalleUsuarioDTO> GetDetalleUsuarioByEmail(string email)
    {
        var userMail = await GetByEmail(email);

        var result = _mapper.Map<ViewDetalleUsuarioDTO>(userMail);

        //elo
        var elos = await _eloApplication.GetElo(email);
        var clasificacionElo = await _eloApplication.GetClasificacion();
        clasificacionElo = clasificacionElo.OrderByDescending(x => x.Elo).ToList();
        result.ClasificacionElo = clasificacionElo.FindIndex(x=>x.Nick == result.Nick) +1;
        result.Elos = elos.Elos;
        //partidas
        var partidas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(email);
        //torneos
        result.Partidas = partidas;
        result.PuntuacionElo = elos.Elos[elos.Elos.Count - 1].PuntuacionElo;

        var torneos = await _torneoApplication.GetTorneos();
        
        return result;
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


            var usuarioRegistrado = await GetByEmail(registrarUsuarioDTO.Email);

            CreateEloDTO createElo = new() 
            { 
                IdUsuario = usuarioRegistrado.IdUsuario, 
                PuntuacionElo = 800 
            };

            await _eloApplication.RegisterElo(createElo);

            return response;

        }
        catch
        {
            throw;
        }
    }
}
