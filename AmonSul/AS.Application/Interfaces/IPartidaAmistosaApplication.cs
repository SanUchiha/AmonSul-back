using AS.Application.DTOs.PartidaAmistosa;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IPartidaAmistosaApplication
{
    Task<List<ViewPartidaAmistosaDTO>> GetPartidasAmistosas();
    Task<ViewPartidaAmistosaDTO> GetById(int Id);
    Task<List<ViewPartidaAmistosaDTO>> GetPartidaAmistosasByUsuario(string email);
    Task<bool> Edit(UpdatePartidaAmistosaDTO partidaAmistosa);
    Task<bool> Register(CreatePartidaAmistosaDTO partidaAmistosa);
    Task<bool> Delete(int id);
    Task<bool> ValidarPartidaAmistosa(ValidarPartidaDTO validarPartidaDTO);
}
