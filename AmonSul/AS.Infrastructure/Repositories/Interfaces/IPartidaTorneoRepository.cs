using AS.Domain.DTOs.Elos;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IPartidaTorneoRepository
{
    //GET
    Task<List<PartidaTorneo>> GetPartidasTorneos(); //Todas las partidas del torneo
    Task<PartidaTorneo> GetById(int idPartida); // Partida por id de partida
    Task<List<PartidaTorneo>> GetPartidasTorneo(int idTorneo); // Todas las partidas de un torneo
    Task<List<PartidaTorneo>> GetPartidasTorneoByRonda(int idTorneo, int ronda); // Todas las partidas de una ronda de un torneo
    Task<List<PartidaTorneo>> GetPartidasTorneosByUsuario(int idUsuario); // Todas las partidas de todos los torneos de un jugador
    Task<List<PartidaTorneo>> GetPartidasTorneoByUsuario(int idTorneo, int idUsuario); // Todas las partidas de un torneo de un jugador

    Task<bool> Edit(PartidaTorneo partidaTorneo);
    Task<bool> Register(PartidaTorneo partidaTorneo);
    Task<bool> RegisterMany(List<PartidaTorneo> partidaTorneos);
    Task<bool> GenerateRound(List<PartidaTorneo> partidasRonda);

    Task<bool> Delete(int idPartida);
    Task<List<UpdateEloPartidaDTO>> GetPartidasTorneoByRondaForEloAsync(int idTorneo, int idRonda);
    Task<List<PartidaTorneoDTO>> GetPartidasTorneoAsync(int idTorneo);
    Task<List<PartidaTorneo>> GetPartidasTorneoEquiposParaModificarAsync(int idEquipo1Old, int idEquipo2Old, int idTorneo, int numeroRonda);
}
