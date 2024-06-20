using AS.Application.DTOs.PartidaAmistosa;

namespace AS.Application.Interfaces
{
    public interface IPartidaAmistosaApplication
    {
        Task<List<PartidaAmistosaDTO>> GetPartidasAmistosas();
        Task<PartidaAmistosaDTO> GetById(int Id);
        Task<bool> Edit(PartidaAmistosaDTO partidaAmistosa);
        Task<bool> Register(PartidaAmistosaDTO partidaAmistosa);
        Task<bool> Delete(int id);
    }
}
