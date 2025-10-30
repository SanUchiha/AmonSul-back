using AS.Application.DTOs.Elo;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IEloApplication
{
    Task<List<Elo>> GetElos();
    Task<List<Elo>> GetElosByIdUser(int idUsuario);
    Task<Elo> GetEloById(int idElo);
    Task<bool> Edit(Elo elo);
    Task<bool> RegisterElo(CreateEloDTO requestElo);
    Task<bool> Delete(int idElo);

    Task<ViewEloDTO> GetEloByIdUsuarioAsync(int idUsuario);
    Task<List<ViewEloDTO>> GetAllElos();
    Task<int> GetLastElo(int idUsuario);
    Task<List<EloUsuarioDTO>> GetEloUsuarios();
    Task<List<ClasificacionEloDTO>> GetEloClasificacionAsync();
    Task<List<ClasificacionEloDTO>> GetClasificacionMensual();
    Task<int?> GetRanking(int idUsuario);
    Task <bool> CheckEloByUser(int idUsuario);
}
