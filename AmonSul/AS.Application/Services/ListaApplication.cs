using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class ListaApplication: IListaApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ListaApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Lista> Delete(int idLista) => 
        await _unitOfWork.ListaRepository.Delete(idLista);

    public async Task<Lista> GetListaById(int idLista) => 
        await _unitOfWork.ListaRepository.GetListaById(idLista);

    public async Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion)
    {
        var lista = await _unitOfWork.ListaRepository.GetListaInscripcionById(idInscripcion);

        return _mapper.Map<ListaViewDTO>(lista);
    }
     

    public async Task<List<Lista>> GetListas() => 
        await _unitOfWork.ListaRepository.GetListas();

public async Task<List<Lista>> GetListasByTorneo(int idTorneo) => 
        await _unitOfWork.ListaRepository.GetListasByTorneo(idTorneo);

    public async Task<List<Lista>> GetListasByUser(int idUsuario) =>      
        await _unitOfWork.ListaRepository.GetListasByUser(idUsuario);



    public async Task<bool> RegisterLista(CreateListaTorneoDTO createListaTorneoDTO) 
    {
        var lista = _mapper.Map<Lista>(createListaTorneoDTO);
        var result = await _unitOfWork.ListaRepository.RegisterLista(lista);

        return result;
    }

    public async Task<Lista> UpdateLista(Lista lista) 
    {
        var updatedLista = await _unitOfWork.ListaRepository.UpdateLista(lista);
        return updatedLista;
    }


}
