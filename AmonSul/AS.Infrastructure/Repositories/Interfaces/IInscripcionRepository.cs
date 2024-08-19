using AS.Domain.Models;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IInscripcionRepository
{
    Task<List<InscripcionTorneo>> GetInscripciones();
    Task<List<InscripcionTorneo>> GetInscripcionesByUser(int idUsuario);
    Task<List<InscripcionTorneo>> GetInscripcionesByTorneo(int idTorneo);
    Task<InscripcionTorneo> GetInscripcionById(int Id);
    Task<InscripcionTorneo> Delete(int id);
    Task<bool> Register(InscripcionTorneo inscripcionTorneo);

    Task<bool> CambiarEstadoPago(InscripcionTorneo actualizarEstadoPago);
    Task<bool> CambiarEstadoLista(InscripcionTorneo actualizarEstadoLista);
    Task<bool> CambiarEstadoInscripcion(InscripcionTorneo actualizarEstadoInscripcion);

    Task<bool> Update(InscripcionTorneo actualizarEstadoInscripcion);
}
