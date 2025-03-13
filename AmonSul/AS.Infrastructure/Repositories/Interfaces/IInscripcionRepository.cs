using AS.Domain.DTOs.Equipo;
using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IInscripcionRepository
{
    Task<List<InscripcionTorneo>> GetInscripciones();
    Task<List<InscripcionTorneo>> GetInscripcionesIndividualByUser(int idUsuario);
    Task<List<InscripcionTorneo>> GetInscripcionesByTorneo(int idTorneo);
    Task<InscripcionTorneo> GetInscripcionById(int Id);
    Task<InscripcionTorneo> Delete(int id);
    Task<bool> Register(InscripcionTorneo inscripcionTorneo);

    Task<bool> CambiarEstadoPago(InscripcionTorneo actualizarEstadoPago);
    Task<bool> CambiarEstadoLista(int idInscripcion, string estadoLista);
    Task<bool> CambiarEstadoInscripcion(InscripcionTorneo actualizarEstadoInscripcion);

    Task<bool> Update(InscripcionTorneo actualizarEstadoInscripcion);

    Task<List<EquipoDTO>> GetAllEquiposByTorneoAsync(int Id_Torneo);
    Task<Equipo?> GetEquipoByIdAsync(int id);
    Task<Equipo> CreateEquipoAsync(Equipo equipo);
    Task<bool> AddUsuarioToEquipoAsync(EquipoUsuario equipoUsurio);
    Task<List<InscripcionTorneo>> GetInscripcionesEquipoByUser(int idUser);
    Task<List<InscripcionTorneo>> GetAllInscripcionesByEquipoAsync(int value);
    Task<bool> EstaApuntadoAsync(int idUsuario, int idTorneo);
    Task<bool> DeleteEquipoAsync(int idEquipo);
}
