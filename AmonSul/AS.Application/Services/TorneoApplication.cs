﻿using AS.Application.DTOs.Inscripcion;
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

    public async Task<TorneoDTO> GetById(int id)
    {
        var response = await _unitOfWork.TorneoRepository.GetById(id);

        var torneoDTO = _mapper.Map<TorneoDTO>(response);

        bool tieneBases = TieneBases(id, torneoDTO.NombreTorneo);
        torneoDTO.tieneBases = tieneBases;

        return torneoDTO;
    }

    private bool TieneBases(int idTorneo, string nombreTorneo)
    {
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Bases");
        string filePath = Path.Combine(folderPath, nombreTorneo + ".pdf");

        if (!File.Exists(filePath))
        {
            string filePathWithoutCaracters = filePath.Replace("\"", "");
            if (File.Exists(filePathWithoutCaracters))
            {
                return true;
            }

            return false;
        }
        else return true;
    }

    public async Task<bool> Register(CrearTorneoDTO request)
    {
        Torneo torneo = _mapper.Map<Torneo>(request);
        torneo.BasesTorneo = null;
        torneo.DescripcionTorneo ??= "";
        torneo.MetodosPago ??= "";

        bool torneoCreado = 
            await _unitOfWork.TorneoRepository.Register(torneo);
        
        if(!torneoCreado) return false;

        if(!string.IsNullOrEmpty(request.BasesTorneo))
            await GuardarBasesEnPDFAsync(request.BasesTorneo, request.NombreTorneo!);

        List<string> listaDestinatarios =
           await _unitOfWork.UsuarioRepository.GetAllEmail();

        if (listaDestinatarios.Count > 0)
            _ = Task.Run(() => _emailApplicacion.SendEmailNuevoTorneo(
                request.NombreTorneo!,
                listaDestinatarios));

        return torneoCreado;
    }

    private static async Task GuardarBasesEnPDFAsync(string basesTorneo, string nombreTorneo)
    {
        byte[] basesBytes = Convert.FromBase64String(basesTorneo);

        string filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "Bases",
            nombreTorneo + 
            ".pdf");
        
        await File.WriteAllBytesAsync(filePath, basesBytes);
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

        byte[] fileBytes;
        string fileName;

        if (!File.Exists(filePath))
        {
            string filePathWithoutCaracters = filePath.Replace("\"", "");
            if (File.Exists(filePathWithoutCaracters))
            {
                fileBytes = await File.ReadAllBytesAsync(filePathWithoutCaracters);
                fileName = $"Bases_Torneo_{nombreTorneo}.pdf";

                return (fileBytes, fileName);
            }

            throw new FileNotFoundException(
                "No se encontró el archivo de bases para este torneo.");
        }

        fileBytes = await File.ReadAllBytesAsync(filePath);
        fileName = $"Bases_Torneo_{nombreTorneo}.pdf";

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

    public async Task<bool> UpdateTorneoAsync(UpdateTorneoDTO request)
    {
        var torneo = await _unitOfWork.TorneoRepository.GetById(request.IdTorneo);
        if (torneo == null) return false;

        if (request.LimiteParticipantes.HasValue)
            torneo.LimiteParticipantes = request.LimiteParticipantes.Value;

        if (request.FechaInicioTorneo.HasValue)
            torneo.FechaInicioTorneo = request.FechaInicioTorneo.Value;

        if (request.FechaFinTorneo.HasValue)
            torneo.FechaFinTorneo = request.FechaFinTorneo.Value;

        if (request.PrecioTorneo.HasValue)
            torneo.PrecioTorneo = request.PrecioTorneo.Value;

        if (request.NumeroPartidas.HasValue)
            torneo.NumeroPartidas = request.NumeroPartidas.Value;

        if (request.PuntosTorneo.HasValue)
            torneo.PuntosTorneo = request.PuntosTorneo.Value;

        if (!string.IsNullOrWhiteSpace(request.LugarTorneo))
            torneo.LugarTorneo = request.LugarTorneo;

        if (request.FechaEntregaListas.HasValue)
            torneo.FechaEntregaListas = request.FechaEntregaListas.Value;

        if (request.FechaFinInscripcion.HasValue)
            torneo.FechaFinInscripcion = request.FechaFinInscripcion.Value;

        if (!string.IsNullOrWhiteSpace(request.CartelTorneo))
            torneo.CartelTorneo = request.CartelTorneo;

        if (!string.IsNullOrWhiteSpace(request.MetodosPago))
            torneo.MetodosPago = request.MetodosPago;

        if (request.HoraInicioTorneo.HasValue)
            torneo.HoraInicioTorneo = request.HoraInicioTorneo.Value;

        if (request.HoraFinTorneo.HasValue)
            torneo.HoraFinTorneo = request.HoraFinTorneo.Value;

        torneo.BasesTorneo = null;
        torneo.DescripcionTorneo ??= "";
        torneo.MetodosPago ??= "";

        bool torneoEditado = await _unitOfWork.TorneoRepository.Edit(torneo);

        if (!torneoEditado) return false;

        if (string.IsNullOrWhiteSpace(request.BasesTorneo)) return true;

        await GuardarBasesEnPDFAsync(request.BasesTorneo, torneo.NombreTorneo);

        return torneoEditado;
    }

    public async Task<bool> UpdateBasesTorneoAsync(UpdateBasesDTO request)
    {
        var torneo = await _unitOfWork.TorneoRepository.GetById(request.IdTorneo);
        if (torneo == null) return false;

        if (string.IsNullOrWhiteSpace(request.BasesTorneo)) return false;

        await GuardarBasesEnPDFAsync(request.BasesTorneo, torneo.NombreTorneo);

        return true;
    }
}
