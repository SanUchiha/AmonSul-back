using AS.Application.DTOs.Inscripcion;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IInscripcionApplication
{
    Task<List<InscripcionTorneo>> GetInscripciones();
    Task<List<InscripcionUsuarioDTO>> GetInscripcionesByUser(int idUsuario);
    Task<List<InscripcionUsuarioDTO>> GetInscripcionesByTorneo(int idTorneo);
    Task<InscripcionTorneoDTO> GetInscripcionById(int Id);
    Task<InscripcionTorneo> Delete(int id);
    Task<bool> Register(CrearInscripcionDTO inscripcionTorneo);

    Task<bool> CambiarEstadoPago(ActualizarEstadoPago actualizarEstadoPago);
    Task<bool> CambiarEstadoLista(ActualizarEstadoLista actualizarEstadoLista);
    Task<bool> CreaInsciprcionEquipo(CreateEquipoDTO createEquipoDTO);
    Task<InscripcionTorneoEquiposDTO> GetInscripcionEquipoByIdAsync(int idUsuario);
}
