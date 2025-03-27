using AS.Application.DTOs.Inscripcion;
using AS.Domain.DTOs.Equipo;
using AS.Domain.DTOs.Inscripcion;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IInscripcionApplication
{
    Task<List<InscripcionTorneo>> GetInscripciones();
    Task<List<InscripcionUsuarioIndividualDTO>> GetInscripcionesIndividualByUser(int idUsuario);
    Task<List<InscripcionUsuarioIndividualDTO>> GetInscripcionesByTorneo(int idTorneo);
    Task<InscripcionTorneoDTO> GetInscripcionById(int Id);
    Task<InscripcionTorneo> Delete(int id);
    Task<bool> Register(CrearInscripcionDTO inscripcionTorneo);

    Task<bool> CambiarEstadoPago(ActualizarEstadoPago actualizarEstadoPago);
    Task<bool> CambiarEstadoLista(ActualizarEstadoLista actualizarEstadoLista);

    Task<bool> CreaInscripcionEquipo(CreateEquipoDTO createEquipoDTO);
    Task<List<InscripcionUsuarioEquipoDTO>> GetInscripcionEquipoByIdAsync(int idUser);
    Task<InscripcionEquipoDTO> GetInscripcionEquipo(int idInscripcion);
    Task<List<EquipoDTO>> GetInscripcionesEquipoByTorneoAsync(int idTorneo);
    Task<bool> EstaApuntadoAsync(int idUsuario, int idTorneo);
    Task<bool> DeleteEquipo(int idEquipo);
    Task<ResultInscripcionTorneoDTO> RegisterMiembroAsync(CreateMiembroEquipoDTO createMiembroEquipoDTO);
    Task<bool> DeleteMiembroAsync(int idInscripcion);
    Task<bool> CambiarEstadoPagoEquipo(ActualizarEstadoPagoEquipo actualizarEstadoPagoEquipo);
}
