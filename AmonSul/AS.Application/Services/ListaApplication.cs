using AS.Application.DTOs.Email;
using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class ListaApplication(IUnitOfWork unitOfWork, IMapper mapper, IEmailApplicacion emailApplicacion) : IListaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

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

    public async Task<string> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO )
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaTorneo(listaTorneoRequestDTO.IdTorneo, listaTorneoRequestDTO.IdUsuario);

        if (lista is null) return "";
        
        return lista.ListaData!;
    }

    public async Task<bool> RegisterLista(CreateListaTorneoDTO createListaTorneoDTO) 
    {
        Lista lista = _mapper.Map<Lista>(createListaTorneoDTO);
        lista.Bando = createListaTorneoDTO.Ejercito.Band;
        lista.Ejercito = createListaTorneoDTO.Ejercito.Name;
        bool result = await _unitOfWork.ListaRepository.RegisterLista(lista);

        if (!result) return false;

        int idOrganizador =
            await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(createListaTorneoDTO.IdTorneo);
        UsuarioEmailDto organizador =
            await _unitOfWork.UsuarioRepository.GetEmailNickById(idOrganizador);
        UsuarioEmailDto usuario =
            await _unitOfWork.UsuarioRepository.GetEmailNickById(createListaTorneoDTO.IdUsuario);

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = organizador.Email,
            Message = usuario.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);

        return result;
    }

    public async Task<bool> UpdateLista(UpdateListaDTO updateListaTorneoDTO) 
    {
        Lista result = await _unitOfWork.ListaRepository.UpdateLista(updateListaTorneoDTO);
        if (result == null) return false;

        int idOrganizador = 
            await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(updateListaTorneoDTO.IdTorneo);
        UsuarioEmailDto organizador =
            await _unitOfWork.UsuarioRepository.GetEmailNickById(idOrganizador);
        UsuarioEmailDto usuario =
            await _unitOfWork.UsuarioRepository.GetEmailNickById(updateListaTorneoDTO.IdUsuario);

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = organizador.Email,
            Message = usuario.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);
        

        return true;
    }
}
