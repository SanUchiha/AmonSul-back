﻿using AS.Application.DTOs.Inscripcion;
using AS.Application.DTOs.Torneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class TorneoApplication : ITorneoApplication
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TorneoApplication(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
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
        var torneo = _mapper.Map<Torneo>(request);

        return await _unitOfWork.TorneoRepository.Register(torneo);
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
        var nombreTorneo = (await GetById(idTorneo)).NombreTorneo;

        if (nombreTorneo == null) throw new Exception();

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

    public async Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int IdUsuario)
    {
        var response = await _unitOfWork.TorneoRepository.GetTorneosCreadosUsuario(IdUsuario);

        var listaTorneos = _mapper.Map<List<TorneoCreadoUsuarioDTO>>(response);

        return listaTorneos;
    }
}
