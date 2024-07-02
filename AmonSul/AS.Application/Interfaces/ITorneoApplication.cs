using AS.Application.DTOs.Torneo;

namespace AS.Application.Interfaces;

public interface ITorneoApplication
{
    Task<List<TorneoDTO>> GetTorneos();
    Task<TorneoDTO> GetById(int Id);
    Task<bool> Edit(TorneoDTO torneoDTO);
    Task<bool> Register(TorneoDTO torneoDTO);
    Task<bool> Delete(int id);
}
