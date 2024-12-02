using AS.Application.DTOs.Faccion;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class FaccionApplication : IFaccionApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FaccionApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<List<FaccionDTO>> GetFacciones()
    {
        var response = await _unitOfWork.FaccionRepository.GetFacciones();

        return _mapper.Map<List<FaccionDTO>>(response);
    }

    public Task<FaccionDTO> GetById(int Id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Register(RegistrarFaccionDTO registrarFaccionDTO)
    {
        Faccion faccion = new Faccion
        {
            NombreFaccion = registrarFaccionDTO.NombreFaccion,
        };

        var response = await _unitOfWork.FaccionRepository.Register(faccion);
        return response;    
    }

    public Task<bool> Edit(FaccionDTO faccionDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(FaccionDTO faccionDTO)
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetFaccionNameByIdUserAsync(int idUser)
    {
        string numbreFaccion = await _unitOfWork.UsuarioRepository.GetComunidadNameByIdUser(idUser);

        return numbreFaccion;
    }
}
