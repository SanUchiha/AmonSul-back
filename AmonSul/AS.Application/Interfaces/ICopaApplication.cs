namespace AS.Application.Interfaces;

public interface ICopaApplication
{
    Task<(bool Ok, string? Error)> RepartirGruposAsync(int idTorneo);
    Task<(bool Ok, string? Error)> CambiarGrupoAsync(int idTorneo, int idInscripcion, int grupo);
    Task<(bool Ok, string? Error)> GenerarGruposAsync(int idTorneo);
    Task<(bool Ok, string? Error)> EliminarJornadasAsync(int idTorneo);
    Task<(bool Ok, string? Error)> RetirarJugadorAsync(int idTorneo, int idInscripcion);
    Task<(bool Ok, string? Error)> GenerarEliminatoriaAsync(int idTorneo);
    Task<(bool Ok, string? Error)> EliminarEliminatoriaAsync(int idTorneo);
    Task<(bool Ok, string? Error)> ElegirQuienPasaAsync(int idTorneo, int idPartida, int idUsuario);
    Task SincronizarCuadroAsync(int idTorneo);
    Task<(bool Ok, string? Error)> CerrarAsync(int idTorneo);
}
