﻿using AS.Application.DTOs.LigaTorneo;
using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;
using AutoMapper;

namespace AS.Application.Services;

public class LigaApplication(IUnitOfWork unitOfWork, IMapper mapper) : ILigaApplication
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<bool> AddTorneoToLigaAsync(LigaTorneoDTO ligaTorneoDTO) =>
        await _unitOfWork.LigaRepository.AddTorneoToLigaAsync(
            _mapper.Map<LigaTorneo>(ligaTorneoDTO));

    public async Task<List<Liga>> GetAllLigasAsync() =>
        await _unitOfWork.LigaRepository.GetAllLigasAsync();

    public async Task<Liga?> GetLigaByIdAsync(int idLiga) =>
        await _unitOfWork.LigaRepository.GetLigaByIdAsync(idLiga);

    public async Task<List<LigaTorneo>?> GetTorneosByIdLigaAsync(int idLiga) =>
        await _unitOfWork.LigaRepository.GetTorneosByIdLigaAsync(idLiga);
}
