using AS.Application.DTOs.Ganador;
using AS.Application.DTOs.PartidaTorneo;
using AS.Domain.Models;
using Hangfire;

namespace AS.Application.Interfaces;

public interface IGanadorApplication
{
    Task<Ganador> GetById(int id);
    Task<List<GanadorDTO>> GetAll();
    Task<bool> SaveResultTournamentAsync(GuardarResultadosDTO guardarResultadosDTO);
    Task<bool> Delete(int id);
    Task<bool> IsSave(int idTorneo);
    Task<bool> PuedeGuardarResultadosAsync(int idTorneo, int idUsuarioSolicitante);
    Task ActualizarEloAsync(GenerarRondaDTO request);
    // Nunca dos a la vez: los dos aplicarían el ELO del mismo torneo
    [DisableConcurrentExecution(timeoutInSeconds: 60 * 60)]
    Task GuardarPosicionesTorneosTerminadosAsync();
    Task<bool?> PuedeBorrarGanadorAsync(int idGanador, int idUsuarioSolicitante);
}
