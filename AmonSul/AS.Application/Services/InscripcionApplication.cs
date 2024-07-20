using AS.Application.DTOs.Inscripcion;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class InscripcionApplication: IInscripcionApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InscripcionApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InscripcionTorneo> Delete(int id)
    {
        return await _unitOfWork.InscripcionRepository.Delete(id);
    }

    public async Task<InscripcionTorneo> GetInscripcionById(int Id)
    {
        return await _unitOfWork.InscripcionRepository.GetInscripcionById(Id);
    }

    public async Task<List<InscripcionTorneo>> GetInscripciones()
    {
        return await _unitOfWork.InscripcionRepository.GetInscripciones();
    }

    public async Task<List<InscripcionUsuarioDTO>> GetInscripcionesByTorneo(int idTorneo)
    {
        List<InscripcionTorneo> insc = await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(idTorneo);

        return _mapper.Map<List<InscripcionUsuarioDTO>>(insc);
    }

    public async Task<List<InscripcionUsuarioDTO>> GetInscripcionesByUser(int idUsuario)
    {
        List<InscripcionTorneo> insc = await _unitOfWork.InscripcionRepository.GetInscripcionesByUser(idUsuario);

        var result = _mapper.Map<List<InscripcionUsuarioDTO>>(insc);
        if (result.Count > 0)
        {
            foreach (var item in result)
            {
                item.NombreTorneo = (await _unitOfWork.TorneoRepository.GetById(item.IdTorneo)).NombreTorneo;
            }
        }
        
        return result;
    }

    public async Task<bool> Register(CrearInscripcionDTO inscripcionTorneo)
    {
        return await _unitOfWork.InscripcionRepository.Register(
            _mapper.Map<InscripcionTorneo>(inscripcionTorneo));
    }
}
