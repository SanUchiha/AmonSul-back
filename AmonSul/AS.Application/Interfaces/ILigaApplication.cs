namespace AS.Application.Interfaces;

public interface ILigaApplication
{
    Task<(bool Ok, string? Error)> GenerarAsync(int idTorneo);
    Task<(bool Ok, string? Error)> EliminarJornadasAsync(int idTorneo);
    Task<(bool Ok, string? Error)> RetirarJugadorAsync(int idTorneo, int idInscripcion);
}
