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
    Task<InscripcionTorneo> Delete(int id, int idSolicitante);
    Task<(bool Ok, string? Error)> PuedeBorrarInscripcionAsync(int idInscripcion, int idSolicitante);
    Task<bool> TorneoCerradoAsync(int idTorneo);
    Task<bool> TorneoDeInscripcionCerradoAsync(int idInscripcion);
    Task<bool> TorneoDeEquipoCerradoAsync(int idEquipo);
    Task<bool> EsOrganizadorDelTorneoAsync(int idTorneo, int idSolicitante);
    Task<bool> EsOrganizadorDeInscripcionAsync(int idInscripcion, int idSolicitante);
    Task<bool> PuedeTocarInscripcionAsync(int idInscripcion, int idSolicitante, bool companeroDeListaConjunta = false);
    Task<bool> EsOrganizadorDeEquipoAsync(int idEquipo, int idSolicitante);
    Task<bool> EsCapitanUOrganizadorDeEquipoAsync(int idEquipo, int idSolicitante);
    Task<bool> Register(CrearInscripcionDTO inscripcionTorneo);
    Task<(bool Ok, string? Error)> CambiarEjercitoElegidoAsync(CambiarEjercitoElegidoDTO request, int idSolicitante);
    Task<bool> CambiarCompartirEmailAsync(CambiarCompartirEmailDTO request, int idSolicitante);

    Task<bool> CambiarEstadoPago(ActualizarEstadoPago actualizarEstadoPago);
    Task<bool> CambiarEstadoLista(ActualizarEstadoLista actualizarEstadoLista);

    Task<bool> CreaInscripcionEquipo(CreateEquipoDTO createEquipoDTO);
    Task<List<InscripcionUsuarioEquipoDTO>> GetInscripcionEquipoByIdAsync(int idUser);
    Task<InscripcionEquipoDTO> GetInscripcionEquipo(int idInscripcion);
    Task<List<EquipoDTO>> GetInscripcionesEquipoByTorneoAsync(int idTorneo);
    Task<bool> EstaApuntadoAsync(int idUsuario, int idTorneo);
    Task<bool> DeleteEquipo(int idEquipo, int idSolicitante);
    Task<ResultInscripcionTorneoDTO> RegisterMiembroAsync(CreateMiembroEquipoDTO createMiembroEquipoDTO);
    Task<bool> DeleteMiembroAsync(int idInscripcion, int idSolicitante);
    Task<bool> CambiarEstadoPagoEquipo(ActualizarEstadoPagoEquipo actualizarEstadoPagoEquipo);
    Task<List<JugadoresEquipoParaCambioDTO>> GetJugadoresEquipoParaCambioPairingAsync(int idEquipo1, int idEquipo2);
    Task<(bool Ok, string? Error)> UpdatePuntosExtraAsync(UpdatePuntosExtraDTO request, int idSolicitante);
}
