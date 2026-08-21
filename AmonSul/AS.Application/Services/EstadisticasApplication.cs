using AS.Application.DTOs.Estadisticas;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Application.Services;

public class EstadisticasApplication(IUnitOfWork unitOfWork) : IEstadisticasApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private static readonly DateOnly FechaMinima = new(2025, 1, 1);

    // ──────────────────────────────────────────────────────────────────────────
    // EP1: Top / bottom ejércitos por bando
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<TopBandosResponseDTO> GetTopEjercitosPorBandoAsync(int top = 3)
    {
        var (statsMap, _) = await BuildStatsMapAsync();

        var statsPorBando = statsMap.Values
            .Where(s => s.TotalPartidas >= 20 && !string.IsNullOrEmpty(s.Bando))
            .GroupBy(s => s.Bando!.ToLowerInvariant());

        var response = new TopBandosResponseDTO();

        foreach (var grupo in statsPorBando)
        {
            var ordenados = grupo
                .OrderByDescending(s => s.WinRate)
                .ThenByDescending(s => s.MediaPuntosFavor)
                .ToList();

            var mejores = ordenados.Take(top).ToList();
            // Los peores son los de menor win rate (desde el final), sin repetir con mejores
            var peores = ordenados
                .OrderBy(s => s.WinRate)
                .ThenBy(s => s.MediaPuntosFavor)
                .Take(top)
                .ToList();

            var bandoTop = new BandoTopDTO
            {
                Mejores = mejores,
                Peores = peores,
            };

            if (grupo.Key == "good")
                response.Bien = bandoTop;
            else if (grupo.Key == "evil")
                response.Oscuridad = bandoTop;
        }

        return response;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // EP2: Rating de un ejército con filtros
    // ──────────────────────────────────────────────────────────────────────────

    public async Task<RatingEjercitoResponseDTO> GetRatingEjercitoAsync(
        RatingEjercitoRequestDTO request
    )
    {
        var (_, bandoMap) = await BuildStatsMapAsync();

        var todasLasPartidas = await _unitOfWork.PartidaTorneoRepository.GetAllAsync();

        // Solo partidas con resultado y desde 2025
        var partidas = todasLasPartidas
            .Where(p =>
                p.ResultadoUsuario1.HasValue
                && p.ResultadoUsuario2.HasValue
                && p.FechaPartida.HasValue
                && p.FechaPartida.Value >= FechaMinima
                && (
                    EjercitosIguales(p.EjercitoUsuario1, request.Ejercito)
                    || EjercitosIguales(p.EjercitoUsuario2, request.Ejercito)
                )
            )
            .ToList();

        // Filtro por fecha adicional del request
        if (request.FechaDesde.HasValue)
            partidas = partidas
                .Where(p => p.FechaPartida.HasValue && p.FechaPartida.Value >= request.FechaDesde.Value)
                .ToList();

        if (request.FechaHasta.HasValue)
            partidas = partidas
                .Where(p => p.FechaPartida.HasValue && p.FechaPartida.Value <= request.FechaHasta.Value)
                .ToList();

        bool tieneFiltroRivales =
            request.EjercitosRivales is { Count: > 0 };
        bool tieneFiltroBandoRival = !string.IsNullOrEmpty(request.BandoRival);

        // Filtro por ejércitos rivales o bando rival
        if (tieneFiltroRivales)
        {
            var rivalesNorm = request.EjercitosRivales!
                .Select(e => e.Trim().ToLowerInvariant())
                .ToHashSet();

            partidas = partidas
                .Where(p =>
                {
                    string? rival = ObtenerEjercitoRival(p, request.Ejercito);
                    return rival != null && rivalesNorm.Contains(rival.Trim().ToLowerInvariant());
                })
                .ToList();
        }
        else if (tieneFiltroBandoRival)
        {
            string bandoRivalNorm = request.BandoRival!.Trim().ToLowerInvariant();
            partidas = partidas
                .Where(p =>
                {
                    string? rival = ObtenerEjercitoRival(p, request.Ejercito);
                    if (rival == null) return false;
                    string? bando = bandoMap.GetValueOrDefault(rival.Trim().ToLowerInvariant());
                    return bando?.ToLowerInvariant() == bandoRivalNorm;
                })
                .ToList();
        }

        // Filtro por rango de puntos de partida
        if (request.PuntosPartidaMin.HasValue)
            partidas = partidas
                .Where(p => p.PuntosPartida.HasValue && p.PuntosPartida.Value >= request.PuntosPartidaMin.Value)
                .ToList();

        if (request.PuntosPartidaMax.HasValue)
            partidas = partidas
                .Where(p => p.PuntosPartida.HasValue && p.PuntosPartida.Value <= request.PuntosPartidaMax.Value)
                .ToList();

        // Calcular stats
        var stats = CalcularStats(request.Ejercito, partidas, bandoMap);

        var descripcion = BuildDescripcionFiltro(request);

        return new RatingEjercitoResponseDTO
        {
            Ejercito = stats.Ejercito,
            Bando = stats.Bando,
            TotalPartidas = stats.TotalPartidas,
            Victorias = stats.Victorias,
            Derrotas = stats.Derrotas,
            Empates = stats.Empates,
            WinRate = stats.WinRate,
            MediaPuntosFavor = stats.MediaPuntosFavor,
            MediaPuntosContra = stats.MediaPuntosContra,
            EsFiltrado = tieneFiltroRivales || tieneFiltroBandoRival || request.FechaDesde.HasValue || request.FechaHasta.HasValue || request.PuntosPartidaMin.HasValue || request.PuntosPartidaMax.HasValue,
            DescripcionFiltro = descripcion,
        };
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers privados
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Construye el mapa de estadísticas por ejército y el mapa ejército→bando.
    /// </summary>
    private async Task<(Dictionary<string, EjercitoStatsDTO> statsMap, Dictionary<string, string> bandoMap)> BuildStatsMapAsync()
    {
        var listas = await _unitOfWork.ListaRepository.GetListas();
        // Mapa normalizado: ejercito (lower) → bando
        var bandoMap = listas
            .Where(l => !string.IsNullOrWhiteSpace(l.Ejercito) && !string.IsNullOrWhiteSpace(l.Bando))
            .GroupBy(l => l.Ejercito!.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Bando!);

        var todasLasPartidas = await _unitOfWork.PartidaTorneoRepository.GetAllAsync();
        // EP1 no filtra por fecha: cuenta todas las partidas con resultado
        var partidas = todasLasPartidas
            .Where(p =>
                p.ResultadoUsuario1.HasValue
                && p.ResultadoUsuario2.HasValue
                && p.FechaPartida.HasValue
                && p.FechaPartida.Value >= FechaMinima
                && (
                    !string.IsNullOrWhiteSpace(p.EjercitoUsuario1)
                    || !string.IsNullOrWhiteSpace(p.EjercitoUsuario2)
                )
            )
            .ToList();

        var statsMap = new Dictionary<string, EjercitoStatsDTO>(StringComparer.OrdinalIgnoreCase);

        foreach (var partida in partidas)
        {
            if (!string.IsNullOrWhiteSpace(partida.EjercitoUsuario1))
                AgregarResultadoAStats(
                    statsMap,
                    bandoMap,
                    partida.EjercitoUsuario1,
                    esGanador: partida.GanadorPartidaTorneo == partida.IdUsuario1,
                    esEmpate: partida.GanadorPartidaTorneo == null,
                    puntosFavor: partida.ResultadoUsuario1!.Value,
                    puntosContra: partida.ResultadoUsuario2!.Value
                );

            if (!string.IsNullOrWhiteSpace(partida.EjercitoUsuario2))
                AgregarResultadoAStats(
                    statsMap,
                    bandoMap,
                    partida.EjercitoUsuario2,
                    esGanador: partida.GanadorPartidaTorneo == partida.IdUsuario2,
                    esEmpate: partida.GanadorPartidaTorneo == null,
                    puntosFavor: partida.ResultadoUsuario2!.Value,
                    puntosContra: partida.ResultadoUsuario1!.Value
                );
        }

        // Calcular medias y win rate
        foreach (var stats in statsMap.Values)
        {
            if (stats.TotalPartidas == 0) continue;
            stats.WinRate = Math.Round((double)stats.Victorias / stats.TotalPartidas * 100, 2);
            stats.MediaPuntosFavor = Math.Round(stats.MediaPuntosFavor / stats.TotalPartidas, 2);
            stats.MediaPuntosContra = Math.Round(stats.MediaPuntosContra / stats.TotalPartidas, 2);
        }

        return (statsMap, bandoMap);
    }

    private static void AgregarResultadoAStats(
        Dictionary<string, EjercitoStatsDTO> statsMap,
        Dictionary<string, string> bandoMap,
        string ejercito,
        bool esGanador,
        bool esEmpate,
        int puntosFavor,
        int puntosContra
    )
    {
        string clave = ejercito.Trim();
        if (!statsMap.TryGetValue(clave, out var stats))
        {
            stats = new EjercitoStatsDTO
            {
                Ejercito = clave,
                Bando = bandoMap.GetValueOrDefault(clave.ToLowerInvariant()),
            };
            statsMap[clave] = stats;
        }

        stats.TotalPartidas++;
        if (esEmpate) stats.Empates++;
        else if (esGanador) stats.Victorias++;
        else stats.Derrotas++;

        // Acumulamos para calcular la media después
        stats.MediaPuntosFavor += puntosFavor;
        stats.MediaPuntosContra += puntosContra;
    }

    private EjercitoStatsDTO CalcularStats(
        string ejercito,
        List<PartidaTorneo> partidas,
        Dictionary<string, string> bandoMap
    )
    {
        var stats = new EjercitoStatsDTO
        {
            Ejercito = ejercito,
            Bando = bandoMap.GetValueOrDefault(ejercito.Trim().ToLowerInvariant()),
        };

        foreach (var partida in partidas)
        {
            bool esUsuario1 = EjercitosIguales(partida.EjercitoUsuario1, ejercito);
            bool esEmpate = partida.GanadorPartidaTorneo == null;
            bool esGanador = esUsuario1
                ? partida.GanadorPartidaTorneo == partida.IdUsuario1
                : partida.GanadorPartidaTorneo == partida.IdUsuario2;
            int favor = esUsuario1 ? partida.ResultadoUsuario1!.Value : partida.ResultadoUsuario2!.Value;
            int contra = esUsuario1 ? partida.ResultadoUsuario2!.Value : partida.ResultadoUsuario1!.Value;

            stats.TotalPartidas++;
            if (esEmpate) stats.Empates++;
            else if (esGanador) stats.Victorias++;
            else stats.Derrotas++;

            stats.MediaPuntosFavor += favor;
            stats.MediaPuntosContra += contra;
        }

        if (stats.TotalPartidas > 0)
        {
            stats.WinRate = Math.Round((double)stats.Victorias / stats.TotalPartidas * 100, 2);
            stats.MediaPuntosFavor = Math.Round(stats.MediaPuntosFavor / stats.TotalPartidas, 2);
            stats.MediaPuntosContra = Math.Round(stats.MediaPuntosContra / stats.TotalPartidas, 2);
        }

        return stats;
    }

    private static string? ObtenerEjercitoRival(PartidaTorneo partida, string ejercitoBuscado)
    {
        if (EjercitosIguales(partida.EjercitoUsuario1, ejercitoBuscado))
            return partida.EjercitoUsuario2;
        if (EjercitosIguales(partida.EjercitoUsuario2, ejercitoBuscado))
            return partida.EjercitoUsuario1;
        return null;
    }

    private static bool EjercitosIguales(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a)
        && !string.IsNullOrWhiteSpace(b)
        && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string? BuildDescripcionFiltro(RatingEjercitoRequestDTO request)
    {
        var partes = new List<string>();

        if (request.FechaDesde.HasValue || request.FechaHasta.HasValue)
        {
            string desde = request.FechaDesde?.ToString("dd/MM/yyyy") ?? "inicio";
            string hasta = request.FechaHasta?.ToString("dd/MM/yyyy") ?? "hoy";
            partes.Add($"fecha: {desde} - {hasta}");
        }

        if (request.EjercitosRivales is { Count: > 0 })
            partes.Add($"rivales: {string.Join(", ", request.EjercitosRivales)}");
        else if (!string.IsNullOrEmpty(request.BandoRival))
            partes.Add($"bando rival: {request.BandoRival}");

        if (request.PuntosPartidaMin.HasValue || request.PuntosPartidaMax.HasValue)
        {
            string min = request.PuntosPartidaMin?.ToString() ?? "*";
            string max = request.PuntosPartidaMax?.ToString() ?? "*";
            partes.Add($"puntos: {min}-{max}");
        }

        return partes.Count > 0 ? string.Join(" | ", partes) : null;
    }
}
