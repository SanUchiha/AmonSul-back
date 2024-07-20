using AS.Application.DTOs.Inscripcion;
using AS.Domain.Models;

namespace AS.Application.Interfaces;

public interface IInscripcionApplication
{
    Task<List<InscripcionTorneo>> GetInscripciones();
    Task<List<InscripcionUsuarioDTO>> GetInscripcionesByUser(int idUsuario);
    Task<List<InscripcionUsuarioDTO>> GetInscripcionesByTorneo(int idTorneo);
    Task<InscripcionTorneo> GetInscripcionById(int Id);
    Task<InscripcionTorneo> Delete(int id);
    Task<bool> Register(CrearInscripcionDTO inscripcionTorneo);
}
