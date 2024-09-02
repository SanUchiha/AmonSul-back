using AS.Application.DTOs.Elo;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Services;

public class EloApplication : IEloApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IServiceProvider _serviceProvider;

    public EloApplication(IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> RegisterElo(CreateEloDTO requestElo) => 
        await _unitOfWork.EloRepository.RegisterElo(_mapper.Map<Elo>(requestElo));
    public async Task<Elo> GetEloById(int idElo) => 
        await _unitOfWork.EloRepository.GetEloById(idElo);
    public async Task<List<Elo>> GetElos() => 
        await _unitOfWork.EloRepository.GetElos();
    public async Task<List<Elo>> GetElosByIdUser(int idUsuario) => 
        await _unitOfWork.EloRepository.GetElosByIdUser(idUsuario);
    public async Task<bool> Delete(int idElo) => 
        await _unitOfWork.EloRepository.Delete(idElo);
    public async Task<bool> Edit(Elo elo) => 
        await _unitOfWork.EloRepository.Edit(elo);

    public async Task<ViewEloDTO> GetElo(string email)
    {
        // Conseguir el nick
        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);

        //Conseguimos todos los elos del jugador
        var elos = await GetElosByIdUser(usuario.IdUsuario);
        var elosMapper = _mapper.Map<List<EloDTO>>(elos);

        ViewEloDTO view = new()
        {
            Email = email,
            Nick = usuario.Nick, 
            IdUsuario = usuario.IdUsuario,
            Elos = elosMapper
        };

        return view;
    }

    public async Task<List<ViewEloDTO>> GetAllElos()
    {
        // Me quedo con todos los email en una lista
        var usuarios = await _unitOfWork.UsuarioRepository.GetAll();

        if (usuarios == null) return [];

        List<string> listaEmails = [];

        foreach (var usuario in usuarios)
        {
            listaEmails.Add(usuario.Email);
        }
        
        List<ViewEloDTO> listaViewElos = [];

        foreach (var item in listaEmails)
        {
            var view = await GetElo(item);
            listaViewElos.Add(view);
        }

        return listaViewElos;

    }

    public async Task<int> GetLastElo(int idUsuario)
    {
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(idUsuario);

        if(usuario.Email == null) throw new Exception("Usuario no encontrado");

        ViewEloDTO userElo = await GetElo(usuario.Email);

        if (userElo.Elos == null || userElo.Elos.Count == 0)
        {
            return 800;
        }
        EloDTO lastElo = userElo.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()!;

        return lastElo?.PuntuacionElo ?? throw new Exception("No se pudo encontrar el Elo más reciente");
    }

    public async Task<List<EloUsuarioDTO>> GetEloUsuarios()
    {
        var response = await _unitOfWork.EloRepository.GetElos();

        var lista = _mapper.Map<List<EloUsuarioDTO>>(response);
        
        return lista;

        /*// Obtener todos los usuarios
        var usuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (usuarios == null) return new List<ClasificacionEloDTO>();

        // Crear una lista de tareas para obtener la información de clasificación de cada usuario
        var tasks = usuarios.Select(usuario => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var scopedPartidaAmistosaApplication = scope.ServiceProvider.GetRequiredService<IPartidaAmistosaApplication>();
            var scopedEloApplication = scope.ServiceProvider.GetRequiredService<IEloApplication>();

            var view = await scopedEloApplication.GetElo(usuario.Email);
            var obj = scopedMapper.Map<ClasificacionEloDTO>(view);

            var partidas = await scopedPartidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(view.Email);
            if (partidas.Any())
            {
                obj.Partidas = partidas.Count;
                obj.Ganadas = partidas.Count(x => x.GanadorPartida == view.IdUsuario);
                obj.Empatadas = partidas.Count(x => x.GanadorPartida == 0);
                obj.Perdidas = obj.Partidas - obj.Ganadas - obj.Empatadas;
                obj.Elo = view.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()?.PuntuacionElo ?? 800;
            }
            else
            {
                obj.Elo = 800;
            }

            return obj;
        }));

        // Esperar todas las tareas y retornar la clasificación
        var clasificacion = await Task.WhenAll(tasks);
        return clasificacion.ToList();*/
    }

    private async Task <List<PartidaAmistosa>> PartidasValidadas(string email)
    {
        var rawPartidas = await _unitOfWork.PartidaAmistosaRepository.GetPartidasAmistosas();
        if (rawPartidas == null) return [];

        var usuario = await _unitOfWork.UsuarioRepository.GetByEmail(email);
        if (usuario == null) return [];

        rawPartidas = rawPartidas
            .FindAll(p => (p.IdUsuario1 == usuario.IdUsuario || p.IdUsuario2 == usuario.IdUsuario)
                      && (p.PartidaValidadaUsuario1 ?? false)
                      && (p.PartidaValidadaUsuario2 ?? false));

        return rawPartidas;
    }

    public async Task<List<ClasificacionEloDTO>> GetClasificacion()
    {
        // Obtener todos los usuarios
        var listaUsuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (listaUsuarios == null) return new List<ClasificacionEloDTO>();

        //var response = _mapper.Map

        // Crear una lista de tareas para obtener la información de clasificación de cada usuario
        var tasks = listaUsuarios.Select(usuario => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var scopedPartidaAmistosaApplication = scope.ServiceProvider.GetRequiredService<IPartidaAmistosaApplication>();
            var scopedEloApplication = scope.ServiceProvider.GetRequiredService<IEloApplication>();

            var view = await scopedEloApplication.GetElo(usuario.Email);
            var obj = scopedMapper.Map<ClasificacionEloDTO>(view);

            obj.IdFaccion = usuario.IdFaccion;
            var partidas = await scopedPartidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(view.IdUsuario);
            if (partidas.Any())
            {
                obj.Partidas = partidas.Count;
                obj.Ganadas = partidas.Count(x => x.GanadorPartida == view.IdUsuario);
                obj.Empatadas = partidas.Count(x => x.GanadorPartida == 0);
                obj.Perdidas = obj.Partidas - obj.Ganadas - obj.Empatadas;
                obj.Elo = view.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()?.PuntuacionElo ?? 800;
                obj.NumeroPartidasJugadas = partidas.Count;
            }
            else
            {
                obj.Elo = 800;
                obj.NumeroPartidasJugadas = 0;
            }

            return obj;
        }));

        // Esperar todas las tareas y retornar la clasificación
        var clasificacion = await Task.WhenAll(tasks);
        return clasificacion.ToList();
    }

    public async Task<List<ClasificacionEloDTO>> GetClasificacionMensual()
    {
        // Obtener todos los usuarios
        List<Usuario> listaUsuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (listaUsuarios == null) return new List<ClasificacionEloDTO>();

        int mesActual = DateTime.Now.Month;
        int anoActual = DateTime.Now.Year;

        // Crear una lista de tareas para obtener la información de clasificación de cada usuario
        IEnumerable<Task<ClasificacionEloDTO>> tasks = listaUsuarios.Select(usuario => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            IUnitOfWork scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            IMapper scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            IPartidaAmistosaApplication scopedPartidaAmistosaApplication = scope.ServiceProvider.GetRequiredService<IPartidaAmistosaApplication>();
            IEloApplication scopedEloApplication = scope.ServiceProvider.GetRequiredService<IEloApplication>();

            ViewEloDTO view = await scopedEloApplication.GetElo(usuario.Email);
            ClasificacionEloDTO obj = scopedMapper.Map<ClasificacionEloDTO>(view);

            obj.IdFaccion = usuario.IdFaccion;
            List<ViewPartidaAmistosaDTO> partidas = await scopedPartidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(view.IdUsuario);

            List<ViewPartidaAmistosaDTO> partidasMensuales = partidas
                .Where(x => x.FechaPartida.HasValue &&
                            x.FechaPartida.Value.Month == mesActual &&
                            x.FechaPartida.Value.Year == anoActual)
                .ToList();

            if (partidasMensuales.Any())
            {
                obj.Partidas = partidas.Count;
                obj.Ganadas = partidas.Count(x => x.GanadorPartida == view.IdUsuario);
                obj.Empatadas = partidas.Count(x => x.GanadorPartida == 0);
                obj.Perdidas = obj.Partidas - obj.Ganadas - obj.Empatadas;
                obj.Elo = view.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()?.PuntuacionElo ?? 800;
                obj.NumeroPartidasJugadas = partidas.Count;
            }
            else
            {
                obj.Elo = 800;
                obj.NumeroPartidasJugadas = 0;
            }

            return obj;
        }));

        // Esperar todas las tareas y retornar la clasificación
        ClasificacionEloDTO[] clasificacion = await Task.WhenAll(tasks);

        List<ClasificacionEloDTO> clasificacionMensual = new();

        foreach (var item in clasificacion)
        {
            if(item.Partidas>0) clasificacionMensual.Add(item);
        }

        return clasificacionMensual;
    }
}
