using AS.Application.DTOs.Email;
using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class ListaApplication: IListaApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IEmailApplicacion _emailApplicacion;

    public ListaApplication(IUnitOfWork unitOfWork, IMapper mapper, IEmailApplicacion emailApplicacion)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _emailApplicacion = emailApplicacion;
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
        Lista lista = _mapper.Map<Lista>(createListaTorneoDTO);
        bool result = await _unitOfWork.ListaRepository.RegisterLista(lista);

        if (result)
        {
            InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(createListaTorneoDTO.IdInscripcion);
            Torneo torneo = await _unitOfWork.TorneoRepository.GetById(inscripcion.IdTorneo);
            Usuario organizador = await _unitOfWork.UsuarioRepository.GetById(torneo.IdUsuario);
            Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(inscripcion.IdUsuario);

            EmailContactoDTO emailContactoDTO = new()
            {
                Email = organizador.Email,
                Message = usuario.Nick,
            };

            await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);
        }

        return result;
    }

    public async Task<bool> UpdateLista(Lista lista) 
    {
        Lista updatedLista = await _unitOfWork.ListaRepository.UpdateLista(lista);
        if (updatedLista == null) return false;

        if (updatedLista != null)
        {
            InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(lista.IdInscripcion!.Value);
            Torneo torneo = await _unitOfWork.TorneoRepository.GetById(inscripcion.IdTorneo);
            Usuario organizador = await _unitOfWork.UsuarioRepository.GetById(torneo.IdUsuario);
            Usuario usuario = await _unitOfWork.UsuarioRepository.GetById(inscripcion.IdUsuario);

            EmailContactoDTO emailContactoDTO = new()
            {
                Email = organizador.Email,
                Message = usuario.Nick,
            };

            await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);
        }

        return true;
    }


}
