using AS.Domain.DTOs.Elos;
using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IEloRepository
{
    Task<List<Elo>> GetElos();
    Task<List<Elo>> GetElosByIdUser(int idUsuario);
    Task<Elo> GetEloById(int idElo);
    Task<bool> Edit(Elo elo);
    Task<bool> RegisterElo(Elo elo);
    Task<bool> Delete(int idElo);
    Task<bool> CheckEloByUser(int idUsuario);
    Task<List<Usuario>> GetUsuariosWithElos();
    Task<List<ClasificacionEloDataDTO>> GetClasificacionEloAsync();
    Task<List<EloVariacionPartidaDTO>> GetVariacionesByPartidas(List<int> idsPartidaTorneo);
    Task<List<Elo>> GetByPartidaAsync(int idPartidaTorneo);
    Task<int> GetUltimoEloAsync(int idUsuario);
    Task<int> GetEloAnteriorAsync(int idUsuario, int idElo);
    Task DesplazarPosterioresAsync(int idUsuario, int idElo, int diferencia);
    void Agregar(Elo elo);
    void Quitar(Elo elo);
}
