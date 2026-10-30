using AS.Application.DTOs.PartidaTorneo;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Application.Utils;

/// <summary>
/// Lo que hace falta del cuadro de una copa fuera de su gestión: cuántas rondas tiene, para que una partida de la
/// eliminatoria se pueda nombrar por su ronda (final, semifinales...) sin cargar el cuadro entero
/// </summary>
public static class CuadroCopa
{
    // Con el hueco más alto de la primera ronda: hasta 2 huecos, 1 ronda; hasta 4, 2; hasta 8, 3...
    public static int Rondas(int maximaPosicion)
    {
        int huecos = 1;
        int rondas = 0;
        while (huecos < maximaPosicion)
        {
            huecos *= 2;
            rondas++;
        }
        return rondas;
    }

    public static bool EsDeEliminatoria(string? fase) =>
        fase is FaseCopaType.ELIMINATORIA or FaseCopaType.TERCER_PUESTO;

    /// <summary>
    /// Pone las rondas del cuadro a las partidas de eliminatoria de una lista, con una sola consulta para todas sus copas
    /// </summary>
    public static async Task CompletarRondasAsync(IUnitOfWork unitOfWork, List<ViewPartidaTorneoDTO> partidas)
    {
        List<int> copas = [.. partidas.Where(p => EsDeEliminatoria(p.Fase)).Select(p => p.IdTorneo).Distinct()];
        if (copas.Count == 0)
            return;

        Dictionary<int, int> maximos = await unitOfWork.InscripcionRepository.GetMaxPosicionCuadroAsync(copas);
        foreach (ViewPartidaTorneoDTO partida in partidas.Where(p => EsDeEliminatoria(p.Fase)))
            if (maximos.TryGetValue(partida.IdTorneo, out int maximo))
                partida.RondasEliminatoria = Rondas(maximo);
    }
}
