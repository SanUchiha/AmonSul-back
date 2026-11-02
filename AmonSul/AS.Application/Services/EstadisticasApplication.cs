using AS.Application.DTOs.Estadisticas;
using AS.Application.Interfaces;
using AS.Application.Utils;
using AS.Domain.DTOs.Estadisticas;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AS.Application.Services;

public class EstadisticasApplication(IUnitOfWork unitOfWork, IMemoryCache cache) : IEstadisticasApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMemoryCache _cache = cache;

    // Las partidas cambian poco: se preparan una vez cada 10 minutos para todas las consultas
    private const string CLAVE_CACHE = "estadisticas-ejercitos";
    private static readonly TimeSpan DURACION_CACHE = TimeSpan.FromMinutes(10);
    // Mínimos para que un dato salga: por debajo, una o dos partidas lo deciden todo
    private const int MINIMO_RIVAL = 5;
    private const int MINIMO_JUGADOR = 5;
    private const int MINIMO_DESEQUILIBRADO = 10;
    private const int MINIMO_RIVAL_DIFICIL = 3;
    private const int DESTACADOS = 10;
    private const int RIVALES_DIFICILES = 5;
    private const int MAXIMO_PARTIDAS_CRUCE = 50;

    private record Datos(
        List<EstadisticasEjercitos.Partida> Partidas,
        List<EstadisticasEjercitos.Participacion> Participaciones,
        EstadisticasEjercitos.Catalogo Catalogo,
        Dictionary<int, string> Nicks
    );

    private async Task<Datos> DatosAsync() =>
        (await _cache.GetOrCreateAsync(CLAVE_CACHE, async entrada =>
        {
            entrada.AbsoluteExpirationRelativeToNow = DURACION_CACHE;
            List<PartidaEstadisticaDTO> partidas =
            [
                .. await _unitOfWork.PartidaTorneoRepository.GetParaEstadisticasAsync(),
                .. await _unitOfWork.PartidaAmistosaRepository.GetParaEstadisticasAsync(),
            ];
            EstadisticasEjercitos.Catalogo catalogo = new(
                await _unitOfWork.ListaRepository.GetBandosPorEjercitoAsync(),
                partidas.SelectMany(p => new[] { p.Ejercito1, p.Ejercito2 })
            );
            List<EloFechaDTO> historial = await _unitOfWork.EloRepository.GetHistorialAsync();
            Dictionary<int, string> nicks = (await _unitOfWork.UsuarioRepository.GetAllSoloNicks())
                .ToDictionary(u => u.IdUsuario, u => u.Nick);
            List<EstadisticasEjercitos.Partida> preparadas = EstadisticasEjercitos.Preparar(partidas, catalogo, historial);
            List<EstadisticasEjercitos.Participacion> participaciones = EstadisticasEjercitos.Participaciones(
                await _unitOfWork.GanadorRepository.GetPosicionesIndividualesAsync(), preparadas);
            return new Datos(preparadas, participaciones, catalogo, nicks);
        }))!;

    public async Task<List<EjercitoBalanceDTO>> GetRankingEjercitosAsync(FiltroEstadisticasDTO filtro, int minimo)
    {
        Datos datos = await DatosAsync();
        return EstadisticasEjercitos.Ranking(
            EstadisticasEjercitos.Filtrar(datos.Partidas, filtro),
            EstadisticasEjercitos.Filtrar(datos.Participaciones, filtro),
            datos.Catalogo,
            filtro.Bando,
            minimo
        );
    }

    public async Task<FichaEjercitoDTO?> GetFichaEjercitoAsync(string ejercito, FiltroEstadisticasDTO filtro)
    {
        Datos datos = await DatosAsync();
        string? nombre = datos.Catalogo.Nombre(ejercito);
        if (nombre is null)
            return null;
        return EstadisticasEjercitos.Ficha(
            nombre,
            EstadisticasEjercitos.Filtrar(datos.Partidas, filtro),
            EstadisticasEjercitos.Filtrar(datos.Participaciones, filtro),
            datos.Catalogo,
            datos.Nicks,
            MINIMO_RIVAL,
            MINIMO_JUGADOR
        );
    }

    public async Task<CruceEjercitosDTO?> GetCruceAsync(
        string ejercito,
        string? rival,
        string? bandoRival,
        FiltroEstadisticasDTO filtro
    )
    {
        Datos datos = await DatosAsync();
        string? nombre = datos.Catalogo.Nombre(ejercito);
        string? nombreRival = rival is null ? null : datos.Catalogo.Nombre(rival);
        if (nombre is null || (rival is not null && nombreRival is null))
            return null;
        return EstadisticasEjercitos.Cruce(
            nombre,
            nombreRival,
            bandoRival,
            EstadisticasEjercitos.Filtrar(datos.Partidas, filtro),
            datos.Catalogo,
            datos.Nicks,
            MAXIMO_PARTIDAS_CRUCE
        );
    }

    public async Task<CrucesDestacadosDTO> GetCrucesDestacadosAsync(FiltroEstadisticasDTO filtro)
    {
        Datos datos = await DatosAsync();
        return EstadisticasEjercitos.Destacados(
            EstadisticasEjercitos.Filtrar(datos.Partidas, filtro), DESTACADOS, MINIMO_DESEQUILIBRADO);
    }

    public async Task<EjercitosJugadorDTO> GetEjercitosJugadorAsync(int idUsuario, FiltroEstadisticasDTO filtro)
    {
        Datos datos = await DatosAsync();
        return EstadisticasEjercitos.Jugador(
            idUsuario,
            EstadisticasEjercitos.Filtrar(datos.Partidas, filtro),
            datos.Catalogo,
            datos.Nicks,
            MINIMO_RIVAL_DIFICIL,
            RIVALES_DIFICILES
        );
    }
}
