using AS.Application.DTOs.PartidaTorneo;
using AS.Application.DTOs.Torneo;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IPartidaTorneoApplication
{
    Task<EstaJugandoDTO> GetPartidasTorneoPorFechaYUsuarioAsync(
        DateTime fecha,
        int idUsuario
    );
    Task<PartidaTorneo> GetById(int idPartida); // Partida por id de partida
    Task<List<PartidaTorneoDTO>> GetPartidasTorneo(int idTorneo); // Todas las partidas de un torneo
    Task<List<PartidaTorneoDTO>> GetPartidasTorneoByRonda(int idTorneo, int ronda); // Todas las partidas de una ronda de un torneo
    Task<List<ViewPartidaTorneoDTO>> GetPartidasTorneosByUsuario(int idUsuario); // Todas las partidas de todos los torneos de un jugador

    Task<PartidaTorneoDTO?> EditAsync(UpdatePartidaTorneoDTO request);
    Task<bool> Register(AddPairingTorneoDTO addPairingTorneoDTO);
    Task<bool> GenerateRound(GenerarRondaDTO generarRondaDTO);
    Task<PartidaTorneoDTO?> EdtarPairingAsync(UpdatePairingTorneoDTO request);
    Task<bool> EdtarPairingEquiposAsync(UpdatePairingTorneoDTO request);

    Task<bool> Delete(int idPartida);
    Task<List<PartidaTorneoMasDTO>> GetPartidasMasTorneoAsync(int idTorneo);
    Task<bool> GenerateRoundEquipos(GenerarRondaEquiposDTO request);
    Task<bool> GenerarOtraRondaEquiposAsync(GenerarOtraRondaEquiposRequestDTO request);
    Task<List<PartidaTorneoDTO>> GetPartidasTorneoAsync(int idTorneo);
    Task<bool> ModificarPairingEquiposAsync(ModificarPairingTorneoEquiposDTO request, int idTorneo);
    Task<List<ViewPartidaTorneoDTO>> GetPartidasTorneoByUsuarioAsync(int idTorneo, int idUsuario);
}
