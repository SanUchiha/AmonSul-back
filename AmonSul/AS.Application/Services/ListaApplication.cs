using System.IO.Compression;
using System.Net.Mail;
using AS.Application.DTOs.Email;
using AS.Application.DTOs.Lista;
using AS.Application.Interfaces;
using AS.Domain.DTOs.Lista;
using AS.Domain.DTOs.Usuario;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class ListaApplication(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IEmailApplicacion emailApplicacion
) : IListaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IEmailApplicacion _emailApplicacion = emailApplicacion;

    public async Task<Lista> Delete(int idLista) =>
        await _unitOfWork.ListaRepository.Delete(idLista);

    public async Task<ListaDTO> GetListaById(int idLista)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaById(idLista);

        return _mapper.Map<ListaDTO>(lista);
    }

    public async Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaInscripcionById(idInscripcion);

        return _mapper.Map<ListaViewDTO>(lista);
    }

    public async Task<List<Lista>> GetListas() => await _unitOfWork.ListaRepository.GetListas();

    public async Task<List<ListaViewDTO>> GetListasByInscripcionAsync(int idInscripcion)
    {
        List<Lista> listas = await _unitOfWork.ListaRepository.GetListasByInscripcion(
            idInscripcion
        );

        return _mapper.Map<List<ListaViewDTO>>(listas);
    }

    public async Task<List<Lista>> GetListasByTorneo(int idTorneo) =>
        await _unitOfWork.ListaRepository.GetListasByTorneo(idTorneo);

    public async Task<List<Lista>> GetListasByUser(int idUsuario) =>
        await _unitOfWork.ListaRepository.GetListasByUser(idUsuario);

    public async Task<ListaDTO> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO)
    {
        Lista lista = await _unitOfWork.ListaRepository.GetListaTorneo(
            listaTorneoRequestDTO.IdTorneo,
            listaTorneoRequestDTO.IdUsuario
        );
        if (lista is null)
            return null!;

        ListaDTO listaDTO = _mapper.Map<ListaDTO>(lista);

        return listaDTO;
    }

    public async Task<ResultRegisterListarDTO> RegisterLista(
        CreateListaTorneoDTO createListaTorneoDTO
    )
    {
        Lista lista = _mapper.Map<Lista>(createListaTorneoDTO);
        lista.Bando = createListaTorneoDTO.Ejercito.Band;
        lista.Ejercito = createListaTorneoDTO.Ejercito.Name;

        ResultRegisterListarDTO result = await _unitOfWork.ListaRepository.RegisterLista(lista);
        if (!result.Result)
            return result;

        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(
            createListaTorneoDTO.IdInscripcion
        );
        if (inscripcion == null)
            return result;

        inscripcion.EstadoLista = "ENTREGADA";
        inscripcion.FechaEntregaLista = createListaTorneoDTO.FechaEntrega;

        await _unitOfWork.InscripcionRepository.Update(inscripcion);

        string? emailOrganizador = createListaTorneoDTO.EmailOrganizador;
        if (emailOrganizador is null)
        {
            int idOrganizador = await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(
                createListaTorneoDTO.IdTorneo
            );
            emailOrganizador = (
                await _unitOfWork.UsuarioRepository.GetEmailNickById(idOrganizador)
            ).Email;
        }

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = emailOrganizador,
            Message = createListaTorneoDTO.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);

        return result;
    }

    public async Task<bool> UpdateEstadoLista(UpdateEstadoListaDTO request)
    {
        bool result = await _unitOfWork.ListaRepository.UpdateEstadoLista(request);
        if (!result)
            return false;

        Torneo torneo = await _unitOfWork.TorneoRepository.GetById(request.IdTorneo);
        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            request.IdUsuario
        );

        EmailListaDTO emailListaDTO = new()
        {
            EmailTo = usuario.Email,
            NombreTorneo = torneo.NombreTorneo,
            EstadoLista = request.Estado.ToUpper().Trim(),
        };
        try
        {
            await _emailApplicacion.SendEmailModificacionLista(emailListaDTO);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }

        return true;
    }

    public async Task<bool> UpdateLista(UpdateListaDTO updateListaTorneoDTO)
    {
        Lista lista = await _unitOfWork.ListaRepository.UpdateLista(updateListaTorneoDTO);
        if (lista == null)
            return false;

        InscripcionTorneo inscripcion = await _unitOfWork.InscripcionRepository.GetInscripcionById(
            lista.IdInscripcion
        );
        if (inscripcion == null)
            return false;

        inscripcion.EstadoLista = "ENTREGADA";
        inscripcion.FechaEntregaLista = updateListaTorneoDTO.FechaEntrega;

        await _unitOfWork.InscripcionRepository.Update(inscripcion);

        int idOrganizador = await _unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(
            inscripcion.IdTorneo
        );
        UsuarioEmailDto organizador = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            idOrganizador
        );
        UsuarioEmailDto usuario = await _unitOfWork.UsuarioRepository.GetEmailNickById(
            inscripcion.IdUsuario
        );

        EmailContactoDTO emailContactoDTO = new()
        {
            Email = organizador.Email,
            Message = usuario.Nick,
        };

        await _emailApplicacion.SendEmailOrganizadorEnvioListaTorneo(emailContactoDTO);

        return true;
    }

    public async Task<(byte[] FileBytes, string FileName)> GetListasPdfByTorneo(int idTorneo)
    {
        List<Lista> listas = await GetListasByTorneo(idTorneo);

        Torneo torneo = 
            await _unitOfWork.TorneoRepository.GetById(idTorneo) 
            ?? throw new Exception("Torneo no encontrado");

        string torneoNombre = torneo.NombreTorneo;

        const int listasPorPdf = 10;
        int totalPdfs = (int)Math.Ceiling(listas.Count / (double)listasPorPdf);

        // Si hay 10 o menos listas, devolver un solo PDF
        if (totalPdfs == 1)
        {
            byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listas, torneoNombre);
            string fileName = $"Listas_{torneoNombre}.pdf";
            return (pdfBytes, fileName);
        }

        // Generar múltiples PDFs en un ZIP
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            for (int i = 0; i < totalPdfs; i++)
            {
                var listasSubset = listas.Skip(i * listasPorPdf).Take(listasPorPdf).ToList();
                byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listasSubset, torneoNombre);
                
                var entry = archive.CreateEntry($"Listas_{torneoNombre}_Parte{i + 1}de{totalPdfs}.pdf");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes);
            }
        }

        string zipFileName = $"Listas_{torneoNombre}.zip";
        return (memoryStream.ToArray(), zipFileName);
    }

    public async Task<(Stream Stream, string FileName)> GetListasPdfStreamByTorneo(int idTorneo)
    {
        List<Lista> listas = await GetListasByTorneo(idTorneo);

        Torneo torneo = 
            await _unitOfWork.TorneoRepository.GetById(idTorneo) 
            ?? throw new Exception("Torneo no encontrado");

        string torneoNombre = torneo.NombreTorneo;

        const int listasPorPdf = 40;
        int totalPdfs = (int)Math.Ceiling(listas.Count / (double)listasPorPdf);

        // Si hay 40 o menos listas, devolver un solo PDF
        if (totalPdfs == 1)
        {
            byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listas, torneoNombre);
            var stream = new MemoryStream(pdfBytes);
            string fileName = $"Listas_{torneoNombre}.pdf";
            return (stream, fileName);
        }

        // Generar múltiples PDFs en un ZIP con streaming
        var memoryStream = new MemoryStream();
        
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < totalPdfs; i++)
            {
                var listasSubset = listas.Skip(i * listasPorPdf).Take(listasPorPdf).ToList();
                byte[] pdfBytes = Utils.PdfListaGenerator.GenerateListasPdf(listasSubset, torneoNombre);
                
                var entry = archive.CreateEntry($"Listas_{torneoNombre}_Parte{i + 1}de{totalPdfs}.pdf");
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(pdfBytes);
            }
        }

        memoryStream.Position = 0;
        string zipFileName = $"Listas_{torneoNombre}.zip";
        return (memoryStream, zipFileName);
    }

    public async Task<List<ListaCompletaDTO>> GetListasCompletasByTorneo(int idTorneo)
    {
        List<Lista> listas = await _unitOfWork.ListaRepository.GetListasByTorneo(idTorneo);

        bool isTeams = false;
        if (listas.Count != 0)
        {
            if (listas[0].IdInscripcionNavigation?.IdEquipo is not null)
                isTeams = true;

            if (isTeams)
                listas = [.. listas
                    .OrderBy(l => l.IdInscripcionNavigation?.IdEquipo)
                    .ThenBy(l => l.IdInscripcionNavigation?.IdUsuarioNavigation?.Nick)];
            else
                listas = [.. listas
                    .OrderBy(l => l.IdInscripcionNavigation?.IdUsuarioNavigation?.Nick)];
        }
        
        return _mapper.Map<List<ListaCompletaDTO>>(listas);
    }
}
