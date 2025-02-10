using AS.Application.DTOs.Ganador;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IGanadorApplication
{
    Task<Ganador> GetById(int id);
    Task<List<GanadorDTO>> GetAll();
    Task<bool> SaveResultTournamentAsync(GuardarResultadosDTO guardarResultadosDTO);
    Task<bool> Delete(int id);

    Task<bool> IsSave(int idTorneo);
}
