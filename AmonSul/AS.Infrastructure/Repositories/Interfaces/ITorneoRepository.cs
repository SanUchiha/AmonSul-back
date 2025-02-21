﻿using AS.Domain.DTOs.Ganador;
using AS.Domain.DTOs.Torneo;
using AS.Domain.Models;
using AS.Infrastructure.DTOs;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface ITorneoRepository
{
    Task<List<Torneo>> GetTorneos();
    Task<List<TorneoCreadoUsuarioDTO>> GetTorneosCreadosUsuario(int idUsuario);
    Task<Torneo> GetById(int Id);
    Task<int> GetIdOrganizadorByIdTorneo(int IdTorneo);
    Task<bool> Edit(Torneo torneo);
    Task<ResultTorneoCreadoDTO> Register(Torneo torneo);
    Task<bool> Delete(int id);

    Task<TorneoUsuarioDto> GetNombreById(int idTorneo);
    Task<List<GanadorTorneoDTO>> GetAllSoloNames();
}
