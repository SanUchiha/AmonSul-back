using AS.Application.DTOs.Faccion;

namespace AS.Application.Interfaces
{
    public interface IFaccionApplication
    {
        Task<List<FaccionDTO>> GetFacciones();
        Task<FaccionDTO> GetById(int Id);
        Task<bool> Edit(FaccionDTO faccionDTO);
        Task<bool> Register(RegistrarFaccionDTO registrarFaccionDTO);
        Task<bool> Delete(FaccionDTO faccionDTO);
    }
}
