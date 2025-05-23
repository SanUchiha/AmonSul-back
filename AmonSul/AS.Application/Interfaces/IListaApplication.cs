﻿using AS.Application.DTOs.Lista;
using AS.Domain.DTOs.Lista;
using AS.Domain.Models;
using AS.Infrastructure.DTOs.Lista;

namespace AS.Application.Interfaces;

public interface IListaApplication
{
    Task<List<Lista>> GetListas();
    Task<List<Lista>> GetListasByUser(int idUsuario);
    Task<List<Lista>> GetListasByTorneo(int idTorneo);
    Task<ListaViewDTO> GetListaInscripcionById(int idInscripcion);
    Task<ListaDTO> GetListaById(int idLista);
    Task<Lista> Delete(int idLista);
    Task<ResultRegisterListarDTO> RegisterLista(CreateListaTorneoDTO createListaTorneoDTO);
    Task<bool> UpdateLista(UpdateListaDTO updateListaTorneoDTO);
    Task<ListaDTO> GetListaTorneo(ListaTorneoRequestDTO listaTorneoRequestDTO);
    Task<List<ListaViewDTO>> GetListasByInscripcionAsync(int idInscripcion);
    Task<bool> UpdateEstadoLista(UpdateEstadoListaDTO request);
}
