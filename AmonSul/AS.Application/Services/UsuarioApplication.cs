﻿using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Email;
using AS.Application.DTOs.Faccion;
using AS.Application.DTOs.Inscripcion;
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
using Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AS.Application.Services;

public class UsuarioApplication : IUsuarioApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly Utilidades _utilidades;
    private readonly IAccountRepository _accountRepository;
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication;
    private readonly IEloApplication _eloApplication;
    private readonly ITorneoApplication _torneoApplication;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailApplicacion _emailApplication;

    public UsuarioApplication(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        Utilidades utilidades, 
        IAccountRepository accountRepository, 
        IPartidaAmistosaApplication partidaAmistosaApplication, 
        IEloApplication eloApplication, 
        ITorneoApplication torneoApplication, 
        IServiceProvider serviceProvider, 
        IEmailApplicacion emailApplication)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _utilidades = utilidades;
        _accountRepository = accountRepository;
        _partidaAmistosaApplication = partidaAmistosaApplication;
        _eloApplication = eloApplication;
        _torneoApplication = torneoApplication;
        _serviceProvider = serviceProvider;
        _emailApplication = emailApplication;
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

        var listViewUsuarioPartidaDTO = new ConcurrentBag<ViewUsuarioPartidaDTO>();

        await Task.WhenAll(rawUsuario.Select(async item =>
        {
            // Crea un nuevo scope de DbContext para esta tarea
            using var scope = _serviceProvider.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var scopedPartidaAmistosaApplication = scope.ServiceProvider.GetRequiredService<IPartidaAmistosaApplication>();
            var scopedEloApplication = scope.ServiceProvider.GetRequiredService<IEloApplication>();

            ViewUsuarioPartidaDTO obj = new()
            {
                IdUsuario = item.IdUsuario,
                Email = item.Email,
                Nick = item.Nick,
                Ciudad = item.Ciudad,
                IdFaccion = item.IdFaccion
            };

            var partidasAmistosas = await scopedPartidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(obj.IdUsuario);
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

            int lastElo = await scopedEloApplication.GetLastElo(obj.IdUsuario);
            obj.PuntuacionElo = lastElo;

            listViewUsuarioPartidaDTO.Add(obj);
        }));

        return _mapper.Map<List<ViewUsuarioPartidaDTO>>(listViewUsuarioPartidaDTO.ToList());
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
            IdFaccion = rawUsuario.IdFaccion
        };

        var partidasAmistosas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(obj.IdUsuario);
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
        //var partidas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(168);
        //torneos
        //result.Partidas = partidas;
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

    public async Task<List<UsuarioNickDTO>> GetNicks()
    {
        List<Usuario> rawUsuario = await _unitOfWork.UsuarioRepository.GetAll();

        return _mapper.Map<List<UsuarioNickDTO>>(rawUsuario);
    }

    public async Task<UsuarioViewDTO> GetUsuario(string email)
    {
        var usuarioEncontrado = await _unitOfWork.UsuarioRepository.GetUsuario(email);
        return _mapper.Map<UsuarioViewDTO>(usuarioEncontrado);
    }

    public async Task<UsuarioDataDTO> GetUsuarioData(int idUsuario)
    {
        var usuario = await _unitOfWork.UsuarioRepository.GetById(idUsuario);
        if (usuario == null) return null!;

        var response = _mapper.Map<UsuarioDataDTO>(usuario);
        if(response == null) return null!;

        response.Faccion = 
            _mapper.Map<FaccionDTO>(usuario.IdFaccionNavigation);
        response.InscripcionesTorneo = 
            _mapper.Map<List<InscripcionUsuarioDTO>>(usuario.InscripcionTorneos);
        foreach (var item in response.InscripcionesTorneo)
        {
            item.NombreTorneo = (await _unitOfWork.TorneoRepository.GetById(item.IdTorneo)).NombreTorneo;
        }

        response.PartidasPendientes = 
            await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioPendientes(usuario.IdUsuario);
        response.PartidasValidadas =
            await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(usuario.IdUsuario);
        response.PuntuacionElo = response.Elos[response.Elos.Count - 1].PuntuacionElo;

        var listaElosUsuarios = await _eloApplication.GetEloUsuarios();
        List<EloUsuarioDTO> listaElosUsuariosFiltrados = listaElosUsuarios
            .GroupBy(u => u.IdUsuario)
            .Select(g => g.OrderByDescending(u => u.FechaElo).First())
            .OrderByDescending(e => e.PuntuacionElo)
            .ToList();

        response.ClasificacionElo = listaElosUsuariosFiltrados.FindIndex(u => u.IdUsuario == response.IdUsuario)+1;

        response.NumeroPartidasJugadas = response.PartidasValidadas.Count;

        int contadorVictorias = 0;
        int contadorEmpates = 0;
        int contadorDerrotas = 0;
        foreach (var partida in response.PartidasValidadas)
        {
            if (partida.GanadorPartida == response.IdUsuario) contadorVictorias++;
            else if (partida.GanadorPartida == 0) contadorEmpates++;
            else contadorDerrotas++;
        }
        response.PartidasGanadas = contadorVictorias;
        response.PartidasEmpatadas = contadorEmpates;
        response.PartidasPerdidas = contadorDerrotas;

        return response;
    }

    public async Task<List<UsuarioDTO>> GetUsuarios()
    {
        // Obtener la lista de usuarios y facciones
        var listaUsuarios = await _unitOfWork.UsuarioRepository.GetAll();
        var listaFacciones = await _unitOfWork.FaccionRepository.GetFacciones();

        // Crear un diccionario para acceder rápidamente a las facciones por IdFaccion
        var faccionesDictionary = listaFacciones.ToDictionary(f => f.IdFaccion);

        // Mapear los usuarios a DTOs
        var response = _mapper.Map<List<UsuarioDTO>>(listaUsuarios);

        // Asignar la facción correspondiente a cada usuario
        foreach (var usuarioDTO in response)
        {
            if (usuarioDTO.Faccion == null && usuarioDTO.IdFaccion != null)
            {
                var idFaccion = usuarioDTO.IdFaccion.Value;
                if (faccionesDictionary.TryGetValue(idFaccion, out var faccion))
                {
                    usuarioDTO.Faccion = _mapper.Map<FaccionDTO>(faccion);
                }
            }
        }
        return response;
    }

    public async Task<bool> ModificarFaccion(EditarFaccionUsuarioDTO editarFaccionUsuarioDTO)
    {
        var usuario = await _unitOfWork.UsuarioRepository.GetById(editarFaccionUsuarioDTO.IdUsuario);
        if (usuario == null) return false;

        usuario.IdFaccion = editarFaccionUsuarioDTO.IdFaccion;

        var result = await _unitOfWork.UsuarioRepository.Edit(usuario);

        // 5. Devolver el Resultado
        return result;
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

            var listaDestinatarios = new List<string> { registrarUsuarioDTO.Email };


            EmailRequestDTO request = new()
            {
                EmailTo = listaDestinatarios,
                Subject = ConstEmailMessage.MessageBienvenidaAsunto,
                Body = ConstEmailMessage.MessageBienvenidaBody
            };

            try
            {
                await _emailApplication.SendEmailRegister(request);
            }
            catch (EmailSendException emailEx)
            {
                throw new Exception(emailEx.Message);
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
