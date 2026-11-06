using AS.Application.Interfaces;
using Hangfire;

namespace AS.Application.Services;

/// <summary>
/// Encola las notificaciones: quien avisa no espera a Firebase ni se entera si falla
/// </summary>
public static class AvisoPush
{
    public static string RutaEvento(int idTorneo) => $"/detalle-evento/{idTorneo}";

    public static void AEmails(IEnumerable<string?> emails, string titulo, string cuerpo, string? ruta = null)
    {
        List<string> destinatarios = [.. emails.Where(email => !string.IsNullOrWhiteSpace(email)).Select(email => email!)];
        if (destinatarios.Count == 0) return;
        Encolar(() => BackgroundJob.Enqueue<INotificacionPushApplication>(push =>
            push.EnviarAEmailsAsync(destinatarios, titulo, cuerpo, ruta)));
    }

    public static void ATodos(string titulo, string cuerpo, string? ruta = null) =>
        Encolar(() => BackgroundJob.Enqueue<INotificacionPushApplication>(push =>
            push.EnviarATodosAsync(titulo, cuerpo, ruta)));

    public static void RondaGenerada(int idTorneo, int numeroRonda) =>
        Encolar(() => BackgroundJob.Enqueue<INotificacionPushApplication>(push =>
            push.AvisarRondaAsync(idTorneo, numeroRonda)));

    public static void CambioEnRonda(int idTorneo, int numeroRonda, List<int> idsUsuario, List<int> idsEquipo) =>
        Encolar(() => BackgroundJob.Enqueue<INotificacionPushApplication>(push =>
            push.AvisarCambioEnRondaAsync(idTorneo, numeroRonda, idsUsuario, idsEquipo)));

    public static void EventoTerminado(int idTorneo) =>
        Encolar(() => BackgroundJob.Enqueue<INotificacionPushApplication>(push =>
            push.AvisarEventoTerminadoAsync(idTorneo)));

    public static string TextoPago(string? esPago) =>
        esPago == "SI" ? "Tu inscripción ya está pagada." : "Tu inscripción figura como no pagada.";

    public static string TextoLista(string? estado) => estado switch
    {
        "OK" => "Tu lista está aprobada.",
        "ILEGAL" => "Tu lista no es válida: revísala.",
        _ => $"Estado de tu lista: {estado}.",
    };

    private static void Encolar(Func<string> encolar)
    {
        try
        {
            encolar();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo encolar la notificación: {ex.Message}");
        }
    }
}
