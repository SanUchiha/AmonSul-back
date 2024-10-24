using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Email;
using AS.Application.DTOs.Faccion;
using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.Torneo;
using AS.Application.DTOs.Usuario;
using AS.Application.Exceptions;
using AS.Application.Interfaces;
using AS.Application.Mapper;
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
using System.Security.Claims;

namespace AS.Application.Services;

public class UsuarioApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAccountRepository accountRepository,
    IPartidaAmistosaApplication partidaAmistosaApplication,
    IEloApplication eloApplication,
    ITorneoApplication torneoApplication,
    IServiceProvider serviceProvider,
    IEmailApplicacion emailApplication) : IUsuarioApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IPartidaAmistosaApplication _partidaAmistosaApplication = partidaAmistosaApplication;
    private readonly IEloApplication _eloApplication = eloApplication;
    private readonly ITorneoApplication _torneoApplication = torneoApplication;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly IEmailApplicacion _emailApplication = emailApplication;

    public async Task<bool> CambiarPass(CambiarPassDTO cambiarPassDTO)
    {
        //1. comprobar que el usuario existe
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(cambiarPassDTO.idUsuario);
        if (usuario == null) return false;

        //2. comprobar que la pass antigua es correcta
        string oldPassEnc = Utilidades.EncriptarSHA256(cambiarPassDTO.OldPass!);
        if (oldPassEnc != usuario.Contraseña)
            return false;

        //3. encriptar passs nueva
        string newPassEnc = Utilidades.EncriptarSHA256(cambiarPassDTO.NewPass!);

        //4. actualizar el usuario con la pass nueva
        usuario.Contraseña = newPassEnc;
        bool result = await _unitOfWork.UsuarioRepository.Edit(usuario);

        return result;

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
            usuario.Contraseña = Utilidades.EncriptarSHA256(usuario.Contraseña);
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
        cla = [.. cla.OrderByDescending(item => item.Elo)];

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
        clasificacionElo = [.. clasificacionElo.OrderByDescending(x => x.Elo)];
        result.ClasificacionElo = clasificacionElo.FindIndex(x=>x.Nick == result.Nick) +1;
        result.Elos = elos.Elos;
        //partidas
        //var partidas = await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(168);
        //torneos
        //result.Partidas = partidas;
        result.PuntuacionElo = elos.Elos[^1].PuntuacionElo;

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
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(idUsuario);
        if (usuario == null) return null!;

        UsuarioDataDTO response = _mapper.Map<UsuarioDataDTO>(usuario);
        if (response == null) return null!;

        response.Faccion =
            _mapper.Map<FaccionDTO>(usuario.IdFaccionNavigation);
        response.InscripcionesTorneo =
            _mapper.Map<List<InscripcionUsuarioDTO>>(usuario.InscripcionTorneos);
        foreach (var item in response.InscripcionesTorneo)
        {
            item.NombreTorneo = (await _unitOfWork.TorneoRepository.GetById(item.IdTorneo)).NombreTorneo;
        }

        response.PartidasTorneo = await PartidasTorneo(idUsuario);
        response.PartidasPendientes =
            await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioPendientes(usuario.IdUsuario);
        response.PartidasValidadas =
            await _partidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(usuario.IdUsuario);
        response.PuntuacionElo = response.Elos[^1].PuntuacionElo;

        List<EloUsuarioDTO> listaElosUsuarios = await _eloApplication.GetEloUsuarios();
        List<EloUsuarioDTO> listaElosUsuariosFiltrados = [.. listaElosUsuarios
                .GroupBy(u => u.IdUsuario)
                .Select(g => g.OrderByDescending(u => u.FechaElo).First())
                .OrderByDescending(e => e.PuntuacionElo)];

        response.ClasificacionElo = 
            listaElosUsuariosFiltrados.FindIndex(u => u.IdUsuario == response.IdUsuario) + 1;

        response.NumeroPartidasJugadas = 
            response.PartidasValidadas.Count + response.PartidasTorneo.Count;

        int contadorVictorias = 0;
        int contadorEmpates = 0;
        int contadorDerrotas = 0;
        foreach (var partida in response.PartidasValidadas)
        {
            if (partida.GanadorPartida == response.IdUsuario) contadorVictorias++;
            else if (partida.GanadorPartida == 0) contadorEmpates++;
            else contadorDerrotas++;
        }
        foreach (var partida in response.PartidasTorneo)
        {
            if (partida.GanadorPartidaTorneo == response.IdUsuario) contadorVictorias++;
            else if (partida.GanadorPartidaTorneo == 0) contadorEmpates++;
            else contadorDerrotas++;
        }
        response.PartidasGanadas = contadorVictorias;
        response.PartidasEmpatadas = contadorEmpates;
        response.PartidasPerdidas = contadorDerrotas;

        List<Ganador> listaResultadosRaw = await _unitOfWork.GanadorRepository.GetAllByUsuario(idUsuario);

        List<ClasificacionTorneosDTO> clasificacionTorneos = [];
        if(listaResultadosRaw.Count > 0)
        {
            foreach (var item in listaResultadosRaw)
            {
                Torneo torneo = await _unitOfWork.TorneoRepository.GetById(item.IdTorneo);
                ClasificacionTorneosDTO c = new()
                {
                    Resultado = item.Resultado,
                    NombreTorneo = torneo.NombreTorneo
                };
                clasificacionTorneos.Add( c );

            }
        }

        response.ClasificacionTorneos = clasificacionTorneos;

        return response;
    }

    /* public async Task<UsuarioDataDTO> GetUsuarioData(int idUsuario)
     {
         Usuario usuario = 
             await _unitOfWork.UsuarioRepository.GetById(idUsuario) ??
             throw new Exception("Usuario no encontrado");

         // Mapea la entidad Usuario al DTO UsuarioDataDTO
         UsuarioDataDTO usuarioData = _mapper.Map<UsuarioDataDTO>(usuario);

         usuarioData.ClasificacionElo = await CalcularClasificacionEloAsync(usuario);
         usuarioData.NumeroPartidasJugadas = CalcularNumeroPartidas(usuario);
         usuarioData.PartidasGanadas = CalcularPartidasGanadas(usuario);
         usuarioData.PartidasEmpatadas = CalcularPartidasEmpatadas(usuario);
         usuarioData.PartidasPerdidas = CalcularPartidasPerdidas(usuario);
         usuarioData.PartidasValidadas = ObtenerPartidasValidadas(usuario);
         usuarioData.PartidasPendientes = ObtenerPartidasPendientes(usuario);

         return usuarioData;
     }

     private List<ViewPartidaAmistosaDTO> ObtenerPartidasPendientes(Usuario usuario) =>
         usuario.PartidaAmistosaGanadorPartidaNavigations
             .Where(p => p.PartidaValidadaUsuario1 == false || p.PartidaValidadaUsuario2 == false)
             .Select(p => _mapper.Map<ViewPartidaAmistosaDTO>(p))
             .ToList();

     private List<ViewPartidaAmistosaDTO> ObtenerPartidasValidadas(Usuario usuario) =>
         usuario.PartidaAmistosaGanadorPartidaNavigations
             .Where(p => p.PartidaValidadaUsuario1 == true && p.PartidaValidadaUsuario2 == true)
             .Select(p => _mapper.Map<ViewPartidaAmistosaDTO>(p))
             .ToList();

     private int CalcularPartidasPerdidas(int partidasGanadas, int partidasEmpatadas, int numeroPartidasJugadas) =>
         numeroPartidasJugadas - (partidasEmpatadas + partidasGanadas);

     private int CalcularPartidasEmpatadas(Usuario usuario) =>
         usuario.PartidaAmistosaIdUsuario1Navigations
             .Where(p => p.GanadorPartida != null)
             .Count() +
         usuario.PartidaTorneoIdUsuario1Navigations
             .Where(p => p.GanadorPartidaTorneo != null)
             .Count();

     private int CalcularPartidasGanadas(Usuario usuario) =>
         usuario.PartidaAmistosaGanadorPartidaNavigations.Count +
         usuario.PartidaTorneoGanadorPartidaNavigations.Count;

     private int CalcularNumeroPartidas(Usuario usuario) =>
         usuario.

     private async Task<int> CalcularClasificacionEloAsync(Usuario usuario)
     {
         List<EloUsuarioDTO> listaElosUsuarios = await _eloApplication.GetEloUsuarios();
         List<EloUsuarioDTO> listaElosUsuariosFiltrados = [.. listaElosUsuarios
                 .GroupBy(u => u.IdUsuario)
                 .Select(g => g.OrderByDescending(u => u.FechaElo).First())
                 .OrderByDescending(e => e.PuntuacionElo)];

         return listaElosUsuariosFiltrados.FindIndex(u => u.IdUsuario == usuario.IdUsuario) + 1;
     }*/

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
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(editarFaccionUsuarioDTO.IdUsuario);
        if (usuario == null) return false;

        usuario.IdFaccion = editarFaccionUsuarioDTO.IdFaccion;

        bool result = await _unitOfWork.UsuarioRepository.Edit(usuario);

        return result;
    }

    public async Task<bool> RecordarPass(string email)
    {
        //1. conseguimos el usuario
        Usuario usuario  = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return false;

        //2. conseguimos la contraseña
        string pass = Utilidades.GenerarPassTemporal();
        string passEnc = Utilidades.EncriptarSHA256(pass);
        
        usuario.Contraseña = passEnc;

        bool actualizarUsuario = await _unitOfWork.UsuarioRepository.Edit(usuario);
        if (!actualizarUsuario) return false;

        //3. Se la enviamos por correo
        EmailContactoDTO emailContacto = new()
        {
            Email = email,
            Message = pass
        };

        await _emailApplication.SendEmailResetPass(emailContacto);
        
        return true;
    }

    public async Task<RegistrarUsuarioResponseDTO> Register(RegistrarUsuarioDTO registrarUsuarioDTO)
    {
        try
        {
            string rawPass = registrarUsuarioDTO.Contraseña;
            registrarUsuarioDTO.Contraseña = Utilidades.EncriptarSHA256(registrarUsuarioDTO.Contraseña);
            Usuario usuario = _mapper.Map<Usuario>(registrarUsuarioDTO);
            bool rawResponse = await _unitOfWork.UsuarioRepository.Register(usuario);

            LoginDTO loginDTO = new()
            {
                Email = registrarUsuarioDTO.Email,
                Password = rawPass
            };

            LoginResponse login = await _accountRepository.Login(loginDTO);

            RegistrarUsuarioResponseDTO response = new()
            {
                Status = true,
                Message = "Usuario creado con existo",
                Token = login.Token
            };

            List<string> listaDestinatarios = [registrarUsuarioDTO.Email];


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

            ViewUsuarioPartidaDTO usuarioRegistrado = await GetByEmail(registrarUsuarioDTO.Email);

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

    public async Task<bool> UpdateProteccionDatos(UpdateProteccionDatosDTO updateProteccionDatosDTO)
    {
        //1. Buscamos el usuario

        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(updateProteccionDatosDTO.IdUsuario);
        if (usuario == null) return false;

        //2. actualizamos el usuario
        usuario.ProteccionDatos = updateProteccionDatosDTO.ProteccionDatos;

        //3. lo guardamos en la base de datos
        bool result = await _unitOfWork.UsuarioRepository.Edit(usuario);

        return result;
    }

    private async Task<List<ViewPartidaTorneoDTO>> PartidasTorneo(int idUsuario)
    {
        try
        {
            List<PartidaTorneo> partidasTorneoRaw =
                await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneosByUsuario(idUsuario);

            return _mapper.Map<List<ViewPartidaTorneoDTO>>(partidasTorneoRaw);
        }
        catch(Exception ex)
        {
            throw ex;
        }

    }
}
