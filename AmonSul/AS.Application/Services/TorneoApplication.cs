using AS.Application.DTOs.Email;
using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class TorneoApplication(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IEmailApplicacion emailApplicacion) : ITorneoApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

    public async Task<List<TorneoDTO>> GetTorneos()
    {
        var response = await _unitOfWork.TorneoRepository.GetTorneos();

        return _mapper.Map<List<TorneoDTO>>(response);
    }

    public async Task<TorneoDTO> GetById(int Id)
    {
        var response = await _unitOfWork.TorneoRepository.GetById(Id);

        return _mapper.Map<TorneoDTO>(response);
    }

    public async Task<bool> Register(CrearTorneoDTO request)
    {
        Torneo torneo = _mapper.Map<Torneo>(request);

        bool torneoCreado = await _unitOfWork.TorneoRepository.Register(torneo);

        try { await _emailApplicacion.SendEmailNuevoTorneo(request.NombreTorneo!); }
        catch { throw new Exception("No se ha podido enviar el correo"); }

        await _emailApplicacion.SendEmailNuevoTorneo(request.NombreTorneo!);

        return torneoCreado;
    }

    public Task<bool> Edit(TorneoDTO TorneoDTO)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<(byte[] FileBytes, string FileName)> GetBasesTorneo(int idTorneo)
    {
        string nombreTorneo = (await GetById(idTorneo)).NombreTorneo ?? throw new Exception();
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "Bases");
        string filePath = Path.Combine(folderPath, nombreTorneo + ".pdf");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("No se encontró el archivo de bases para este torneo.");
        }

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        var fileName = $"Bases_Torneo_{nombreTorneo}.pdf";

        return (fileBytes, fileName);
    }

    public async Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int IdUsuario) =>        
        await _unitOfWork.TorneoRepository.GetTorneosCreadosUsuario(IdUsuario);


    public async Task<TorneoGestionInfoDTO> GetInfoTorneoCreado(int IdTorneo)
    {
        List<InscripcionTorneo> inscripciones =
            await _unitOfWork.InscripcionRepository.GetInscripcionesByTorneo(IdTorneo);

        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(IdTorneo) ??
            throw new Exception("Torneo no encontrado");
        
        TorneoCreadoDTO torneoDTO = _mapper.Map<TorneoCreadoDTO>(torneo);

        List<InscripcionTorneoCreadoDTO> inscripcionesDTO =
            _mapper.Map<List<InscripcionTorneoCreadoDTO>>(inscripciones);

        for (int i = 0; i < inscripciones.Count; i++)
        {
            inscripcionesDTO[i].Nick = inscripciones[i].IdUsuarioNavigation?.Nick;
            inscripcionesDTO[i].FechaEntrega = inscripciones[i].FechaEntregaLista;

            if(inscripciones[i].Lista.Count > 0)
            {
                inscripcionesDTO[i].Bando = inscripciones[i].Lista.ToList()[0].Bando;
                inscripcionesDTO[i].IdLista = inscripciones[i].Lista.ToList()[0].IdLista;
                inscripcionesDTO[i].Ejercito = inscripciones[i].Lista.ToList()[0].Ejercito;
                inscripcionesDTO[i].FechaEntrega = inscripciones[i].Lista.ToList()[0].FechaEntrega;
                if (inscripciones[i].EstadoLista == "NO ENTREGADA") 
                {
                    inscripcionesDTO[i].EstadoLista = "ENTREGADA";
                    //TODO: actualizar inscipcion
                    await _unitOfWork.InscripcionRepository.CambiarEstadoLista(
                        inscripcionesDTO[i].IdInscripcion,
                        "ENTREGADA");
                }

            }
            if (inscripciones[i].EsPago == "NO") inscripcionesDTO[i].EsPago = "NO";
        }

        TorneoGestionInfoDTO result = new()
        {
            Torneo = torneoDTO,
            Inscripciones = inscripcionesDTO
        };

        return result;
    }

}
