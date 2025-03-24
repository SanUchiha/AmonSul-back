using AS.Application.DTOs.PartidaTorneo;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IPartidaTorneoApplication
{
    //GET
    Task<List<PartidaTorneo>> GetPartidasTorneos(); //Todas las partidas del torneo
    Task<PartidaTorneo> GetById(int idPartida); // Partida por id de partida
    Task<List<PartidaTorneoDTO>> GetPartidasTorneo(int idTorneo); // Todas las partidas de un torneo
    Task<List<PartidaTorneoDTO>> GetPartidasTorneoByRonda(int idTorneo, int ronda); // Todas las partidas de una ronda de un torneo
    Task<List<ViewPartidaTorneoDTO>> GetPartidasTorneosByUsuario(int idUsuario); // Todas las partidas de todos los torneos de un jugador
    Task<List<PartidaTorneo>> GetPartidasTorneoByUsuario(int idTorneo, int idUsuario); // Todas las partidas de un torneo de un jugador

    Task<bool> Edit(UpdatePartidaTorneoDTO request);
    Task<bool> Register(AddPairingTorneoDTO addPairingTorneoDTO);
    Task<bool> GenerateRound(GenerarRondaDTO generarRondaDTO);
    Task<bool> EdtarPairing(UpdatePairingTorneoDTO request);

    Task<bool> Delete(int idPartida);
}
