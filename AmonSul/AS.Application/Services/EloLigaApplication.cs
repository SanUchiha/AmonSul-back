using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Domain.Types.Torneo;
using AS.Infrastructure.Repositories.Interfaces;
using AS.Utils.Statics;
using Hangfire;

namespace AS.Application.Services;

/// <summary>
/// ELO de las ligas y copas que cuentan para el ELO: cada partida suma en cuanto la validan los dos jugadores, con el ELO que
/// tienen en ese momento. Si el organizador cambia luego el resultado se recalcula, y si deja de contar (se desvalida
/// o se retira a un jugador) se quita. Como cada fila guarda el ELO tras la partida, al corregir o quitar una partida
/// antigua el resto del historial de cada jugador se mueve en la misma diferencia
/// </summary>
public class EloLigaApplication(IUnitOfWork unitOfWork, IBackgroundJobClient trabajos) : IEloLigaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IBackgroundJobClient _trabajos = trabajos;

    public async Task SincronizarPartidaAsync(PartidaTorneo partida)
    {
        Torneo? torneo = await _unitOfWork.TorneoRepository.GetById(partida.IdTorneo);
        if (!FormatoTorneoType.EsPorJornadas(torneo?.FormatoTorneo) || !torneo!.CuentaElo)
            return;

        List<Elo> filas = await _unitOfWork.EloRepository.GetByPartidaAsync(partida.IdPartidaTorneo);
        bool cambia;
        if (!Cuenta(partida))
            cambia = filas.Count > 0 && await QuitarAsync(filas);
        else if (filas.Count == 0)
            cambia = await AplicarAsync(partida);
        else
            cambia = await CorregirAsync(partida, filas);

        if (cambia)
            RefrescarClasificacion();
    }

    /// <summary>
    /// Al retirar a un jugador sus partidas se borran: antes se quita el ELO de las que ya contaban, a él y a sus
    /// rivales
    /// </summary>
    public async Task QuitarDeJugadorAsync(int idTorneo, int idUsuario)
    {
        List<PartidaTorneo> partidas = await _unitOfWork.PartidaTorneoRepository.GetPartidasTorneo(idTorneo);
        bool cambia = false;
        foreach (PartidaTorneo partida in partidas.Where(p => p.IdUsuario1 == idUsuario || p.IdUsuario2 == idUsuario))
        {
            List<Elo> filas = await _unitOfWork.EloRepository.GetByPartidaAsync(partida.IdPartidaTorneo);
            if (filas.Count > 0)
                cambia |= await QuitarAsync(filas);
        }

        if (cambia)
            RefrescarClasificacion();
    }

    // Cuenta cuando la han validado los dos y tiene resultado, como en la clasificación
    private static bool Cuenta(PartidaTorneo partida) =>
        partida.PartidaValidadaUsuario1 == true
        && partida.PartidaValidadaUsuario2 == true
        && partida.ResultadoUsuario1 is not null
        && partida.ResultadoUsuario2 is not null;

    // Del jugador 1: victoria 1, empate 0,5 y derrota 0, según el resultado
    private static double PuntuacionJugador1(PartidaTorneo partida) =>
        partida.ResultadoUsuario1 > partida.ResultadoUsuario2 ? 1.0
        : partida.ResultadoUsuario1 < partida.ResultadoUsuario2 ? 0.0
        : 0.5;

    private async Task<bool> AplicarAsync(PartidaTorneo partida)
    {
        int elo1 = await _unitOfWork.EloRepository.GetUltimoEloAsync(partida.IdUsuario1);
        int elo2 = await _unitOfWork.EloRepository.GetUltimoEloAsync(partida.IdUsuario2);
        double puntuacion1 = PuntuacionJugador1(partida);
        DateTime ahora = DateTime.Now;

        _unitOfWork.EloRepository.Agregar(new Elo
        {
            IdUsuario = partida.IdUsuario1,
            PuntuacionElo = EloRating.CalculateNewRating(elo1, elo2, puntuacion1),
            FechaElo = ahora,
            IdPartidaTorneo = partida.IdPartidaTorneo,
        });
        _unitOfWork.EloRepository.Agregar(new Elo
        {
            IdUsuario = partida.IdUsuario2,
            PuntuacionElo = EloRating.CalculateNewRating(elo2, elo1, 1 - puntuacion1),
            FechaElo = ahora,
            IdPartidaTorneo = partida.IdPartidaTorneo,
        });
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Se recalcula con el ELO que tenían los dos justo antes de la partida
    /// </summary>
    private async Task<bool> CorregirAsync(PartidaTorneo partida, List<Elo> filas)
    {
        Elo? fila1 = filas.FirstOrDefault(f => f.IdUsuario == partida.IdUsuario1);
        Elo? fila2 = filas.FirstOrDefault(f => f.IdUsuario == partida.IdUsuario2);
        // Si las filas no son de estos dos jugadores, se rehace entera
        if (fila1 is null || fila2 is null || filas.Count != 2)
        {
            await QuitarAsync(filas);
            return await AplicarAsync(partida);
        }

        int antes1 = await _unitOfWork.EloRepository.GetEloAnteriorAsync(partida.IdUsuario1, fila1.IdElo);
        int antes2 = await _unitOfWork.EloRepository.GetEloAnteriorAsync(partida.IdUsuario2, fila2.IdElo);
        double puntuacion1 = PuntuacionJugador1(partida);
        int diferencia1 = EloRating.CalculateNewRating(antes1, antes2, puntuacion1) - (fila1.PuntuacionElo ?? antes1);
        int diferencia2 = EloRating.CalculateNewRating(antes2, antes1, 1 - puntuacion1) - (fila2.PuntuacionElo ?? antes2);
        if (diferencia1 == 0 && diferencia2 == 0)
            return false;

        await MoverAsync(fila1, diferencia1);
        await MoverAsync(fila2, diferencia2);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task<bool> QuitarAsync(List<Elo> filas)
    {
        foreach (Elo fila in filas.Where(f => f.IdUsuario is not null))
        {
            int antes = await _unitOfWork.EloRepository.GetEloAnteriorAsync(fila.IdUsuario!.Value, fila.IdElo);
            int variacion = (fila.PuntuacionElo ?? antes) - antes;
            await _unitOfWork.EloRepository.DesplazarPosterioresAsync(fila.IdUsuario.Value, fila.IdElo, -variacion);
        }
        foreach (Elo fila in filas)
            _unitOfWork.EloRepository.Quitar(fila);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task MoverAsync(Elo fila, int diferencia)
    {
        if (diferencia == 0)
            return;
        fila.PuntuacionElo += diferencia;
        await _unitOfWork.EloRepository.DesplazarPosterioresAsync(fila.IdUsuario!.Value, fila.IdElo, diferencia);
    }

    // La clasificación de ELO sale de una caché: se rehace en segundo plano
    private void RefrescarClasificacion() =>
        _trabajos.Enqueue<IEloApplication>(elo => elo.UpdateClasificacionEloCacheAsync());
}
