using AS.Application.DTOs.Notificaciones;

namespace AS.Application.Interfaces;

public interface INotificacionPushApplication
{
    Task RegistrarDispositivoAsync(int idUsuario, string token, string plataforma);
    Task QuitarDispositivoAsync(int idUsuario, string token);
    Task<ResultadoPushDTO> EnviarAUsuariosAsync(IEnumerable<int> idsUsuario, string titulo, string cuerpo, string? ruta = null);
    Task EnviarAEmailsAsync(List<string> emails, string titulo, string cuerpo, string? ruta);
    Task EnviarATodosAsync(string titulo, string cuerpo, string? ruta);
    Task AvisarRondaAsync(int idTorneo, int numeroRonda);
    Task AvisarCambioEnRondaAsync(int idTorneo, int numeroRonda, List<int> idsUsuario, List<int> idsEquipo);
    Task AvisarPartidaValidadaAsync(int idPartidaTorneo);
    Task AvisarEventoTerminadoAsync(int idTorneo);
    Task AvisarEventosQueEmpiezanAsync();
}
