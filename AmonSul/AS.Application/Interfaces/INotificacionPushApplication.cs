using AS.Application.DTOs.Notificaciones;

namespace AS.Application.Interfaces;

public interface INotificacionPushApplication
{
    Task RegistrarDispositivoAsync(int idUsuario, string token, string plataforma);
    Task QuitarDispositivoAsync(int idUsuario, string token);
    Task<ResultadoPushDTO> EnviarAUsuariosAsync(IEnumerable<int> idsUsuario, string titulo, string cuerpo, string? ruta = null);
}
