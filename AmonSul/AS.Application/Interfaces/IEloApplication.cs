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

    Task<ViewEloDTO> GetElo(string email);
    Task<List<ViewEloDTO>> GetAllElos();
    Task<int> GetLastElo(int idUsuario);
}
