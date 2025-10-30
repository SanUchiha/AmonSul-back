using AS.Application.DTOs.Elo;
using AS.Application.DTOs.Partida;
using AS.Application.DTOs.PartidaAmistosa;
using AS.Application.DTOs.PartidaTorneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Elos;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace AS.Application.Services;

public class EloApplication(
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IServiceProvider serviceProvider) : IEloApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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

    public async Task<ViewEloDTO> GetEloByIdUsuarioAsync(int idUsuario)
    {
        Usuario usuario = await _unitOfWork.UsuarioRepository.GetByIdFast(idUsuario);

        List<Elo> elos = await GetElosByIdUser(usuario.IdUsuario);
        List<EloDTO> elosMapper = _mapper.Map<List<EloDTO>>(elos);

        ViewEloDTO view = new()
        {
            Email = usuario.Email,
            Nick = usuario.Nick,
            IdUsuario = usuario.IdUsuario,
            Elos = elosMapper
        };

        return view;
    }

    public async Task<List<ViewEloDTO>> GetAllElos()
    {
        // Me quedo con todos los email en una lista
        List<Usuario> usuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (usuarios == null) return [];

        List<int> listaIds = [];

        foreach (var usuario in usuarios)
        {
            listaIds.Add(usuario.IdUsuario);
        }

        List<ViewEloDTO> listaViewElos = [];

        foreach (var item in listaIds)
        {
            ViewEloDTO view = await GetEloByIdUsuarioAsync(item);
            listaViewElos.Add(view);
        }

        return listaViewElos;
    }

    public async Task<int> GetLastElo(int idUsuario)
    {
        ViewEloDTO userElo = await GetEloByIdUsuarioAsync(idUsuario);

        if (userElo.Elos == null || userElo.Elos.Count == 0)
            return 800;

        EloDTO lastElo = userElo.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()!;

        return lastElo?.PuntuacionElo ?? throw new Exception("No se pudo encontrar el Elo más reciente");
    }

    public async Task<List<EloUsuarioDTO>> GetEloUsuarios()
    {
        var response = await _unitOfWork.EloRepository.GetElos();

        List<EloUsuarioDTO> lista = _mapper.Map<List<EloUsuarioDTO>>(response);

        lista = lista.Where(l => l.PuntuacionElo != 800).ToList();

        return lista;
    }

    public async Task<List<ClasificacionEloDTO>> GetEloClasificacionAsync()
    {
        // Obtener todos los usuarios
        List<Usuario> listaUsuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (listaUsuarios == null) return [];

        int currentYear = DateTime.UtcNow.Year;

        // Crear una lista de tareas para obtener la información de clasificación de cada usuario
        IEnumerable<Task<ClasificacionEloDTO>> tasks =
            listaUsuarios.Select(usuario => Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var scopedMapper = scope.ServiceProvider.GetRequiredService<IMapper>();
            var scopedPartidaAmistosaApplication = scope.ServiceProvider.GetRequiredService<IPartidaAmistosaApplication>();
            var scopedPartidaTorneoApplication = scope.ServiceProvider.GetRequiredService<IPartidaTorneoApplication>();
            var scopedEloApplication = scope.ServiceProvider.GetRequiredService<IEloApplication>();

            ViewEloDTO view = await scopedEloApplication.GetEloByIdUsuarioAsync(usuario.IdUsuario);

            List<ViewPartidaTorneoDTO> partidasTorneo =
                await scopedPartidaTorneoApplication.GetPartidasTorneosByUsuario(view.IdUsuario);

            List<MatchDTO> partidasTorneoMapped =
                _mapper.Map<List<MatchDTO>>(partidasTorneo);

            List<MatchDTO> partidas = [.. partidasTorneoMapped];

            partidas = [.. partidas.Where(p => 
                p.FechaPartida is DateOnly fecha && fecha.Year == currentYear)];

            ClasificacionEloDTO obj = scopedMapper.Map<ClasificacionEloDTO>(view);

                obj.IdFaccion = usuario.IdFaccion;
            if (partidas.Count != 0)
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
                obj = null!;
            }
            return obj;

        }));

        ClasificacionEloDTO[] clasificacion = await Task.WhenAll(tasks);

        return [.. clasificacion.Where(c => c != null && c.IdUsuario != 568).ToList()];
    }

    public async Task<List<ClasificacionEloDTO>> GetClasificacionMensual()
    {
        // Obtener todos los usuarios
        List<Usuario> listaUsuarios = await _unitOfWork.UsuarioRepository.GetAll();
        if (listaUsuarios == null) return [];

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

            ViewEloDTO view = await scopedEloApplication.GetEloByIdUsuarioAsync(usuario.IdUsuario);
            ClasificacionEloDTO obj = scopedMapper.Map<ClasificacionEloDTO>(view);

            obj.IdFaccion = usuario.IdFaccion;
            List<ViewPartidaAmistosaDTO> partidas = await scopedPartidaAmistosaApplication.GetPartidaAmistosasByUsuarioValidadas(view.IdUsuario);

            List<ViewPartidaAmistosaDTO> partidasMensuales = partidas
                .Where(x => x.FechaPartida.HasValue &&
                            x.FechaPartida.Value.Month == mesActual &&
                            x.FechaPartida.Value.Year == anoActual)
                .ToList();

            if (partidasMensuales.Count != 0)
            {
                obj.Partidas = partidasMensuales.Count;
                obj.Ganadas = partidasMensuales.Count(x => x.GanadorPartida == view.IdUsuario);
                obj.Empatadas = partidasMensuales.Count(x => x.GanadorPartida == 0);
                obj.Perdidas = obj.Partidas - obj.Ganadas - obj.Empatadas;
                obj.Elo = view.Elos.OrderByDescending(e => e.FechaElo).FirstOrDefault()?.PuntuacionElo ?? 800;
                obj.NumeroPartidasJugadas = partidasMensuales.Count;
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

        List<ClasificacionEloDTO> clasificacionMensual = [];

        foreach (var item in clasificacion)
        {
            if (item.Partidas > 0) clasificacionMensual.Add(item);
        }

        return clasificacionMensual;
    }

    public async Task<int?> GetRanking(int idUsuario)
    {
        List<UsersElosDTO> usersElosDTO = 
            await _unitOfWork.UsuarioRepository.GetAllUserWithElo();

        List<UsersElosDTO> users = [];

        foreach (var item in usersElosDTO)
        {
            if (item.Elos.Count > 1 && item.IdUsuario != 568) users.Add(item);
        }

        var lastEloForUser = users
            .Where(user => user.Elos.Count != 0)
            .Select(user => new
            {
                user.IdUsuario,
                LastElo = user.Elos
                    .OrderByDescending(e => e.FechaRegistroElo)
                    .FirstOrDefault(),
            })
            .Where(result => result.LastElo != null)
            .OrderByDescending(u => u.LastElo!.PuntuacionElo)
            .ToList();

        int rank = 1;

        List<RankingEloDTO> ranking = [];

        foreach (var item in lastEloForUser)
        {
            RankingEloDTO rankingIndividual = new()
            {
                IdUsuario = item.IdUsuario,
                Elo = item.LastElo!.PuntuacionElo!.Value,
                Ranking = rank
            };
            rank++;
            ranking.Add(rankingIndividual);
        }

        RankingEloDTO? posicionRanking = ranking.FirstOrDefault(x => x.IdUsuario == idUsuario);

        if (posicionRanking != null) return posicionRanking.Ranking;

        return null;
    }

    /// <summary>
    /// Verificar si ya existe un elo del usuario
    /// </summary>
    /// <param name="idUsuario"></param>
    /// <returns></returns>
    public async Task<bool> CheckEloByUser(int idUsuario) => 
        await _unitOfWork.EloRepository.CheckEloByUser(idUsuario);

    /// <summary>
    /// Obtener la clasificación ELO desde la tabla de caché
    /// </summary>
    /// <returns>Lista de clasificación ELO desde caché</returns>
    public async Task<List<ClasificacionEloDTO>> GetClasificacionEloCacheAsync()
    {
        try
        {
            // Obtener datos del caché
            List<ClasificacionEloCache> cacheData =
                await _unitOfWork.ClasificacionEloCacheRepository.GetClasificacionCacheAsync();
            
            // Convertir entidades de caché a DTOs
            List<ClasificacionEloDTO> result = [.. cacheData.Select(c => new ClasificacionEloDTO
            {
                IdUsuario = c.IdUsuario,
                Nick = c.Nick,
                IdFaccion = c.IdFaccion == 0 ? null : c.IdFaccion, // Convertir 0 de vuelta a null
                Elo = c.Elo,
                Partidas = c.Partidas,
                Ganadas = c.Ganadas,
                Empatadas = c.Empatadas,
                Perdidas = c.Perdidas,
                NumeroPartidasJugadas = c.NumeroPartidasJugadas
            })];
            
            return result;
        }
        catch
        {
            return [];
        }
    }
 
    /// <summary>
    /// Actualizar la tabla de caché de clasificación ELO de manera síncrona con resultado
    /// </summary>
    /// <returns>True si se actualizó correctamente</returns>
    public async Task<bool> UpdateClasificacionEloCacheAsync()
    {
        try
        {
            // Obtener la clasificación actual
            List<ClasificacionEloDTO> clasificacion = await GetEloClasificacionAsync();
            
            // Convertir DTOs a entidades de caché
            List<ClasificacionEloCache> cacheEntities = [.. clasificacion.Select(c => new ClasificacionEloCache
            {
                IdUsuario = c.IdUsuario,
                Nick = c.Nick,
                IdFaccion = c.IdFaccion ?? 0, // Default 0 si no tiene facción
                Elo = c.Elo,
                Partidas = c.Partidas,
                Ganadas = c.Ganadas,
                Empatadas = c.Empatadas,
                Perdidas = c.Perdidas,
                NumeroPartidasJugadas = c.NumeroPartidasJugadas
            })];

            // Limpiar caché existente
            bool clearResult = await _unitOfWork.ClasificacionEloCacheRepository.ClearCacheAsync();
            if (!clearResult) return false;
            
            // Insertar nuevos datos
            bool insertResult = await _unitOfWork.ClasificacionEloCacheRepository.InsertCacheBatchAsync(cacheEntities);
            
            return insertResult;
        }
        catch
        {
            return false;
        }
    }
}
