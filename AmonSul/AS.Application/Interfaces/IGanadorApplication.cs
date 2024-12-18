using AS.Application.DTOs.Ganador;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IGanadorApplication
{
    Task<Ganador> GetById(int id);
    Task<List<Ganador>> GetAll();
    Task<bool> Register(List<GanadorDTO> ganadoresDTO);
    Task<bool> Delete(int id);

    Task<bool> IsSave(int idTorneo);
}
