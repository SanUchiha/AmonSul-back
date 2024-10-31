using AS.Application.DTOs.Email;

namespace AS.Application.Interfaces;

public interface IEmailApplicacion
{
    void SendEmailNuevaPartida(List<string> destinatarios);
    void SendEmailNuevoUsuario(List<string> destinatarios);
    Task SendEmailContacto(EmailContactoDTO request);
    Task SendEmailResetPass(EmailContactoDTO request);
    Task SendEmailRegistroTorneo(EmailContactoDTO request);
    Task SendEmailModificacionLista(EmailListaDTO request);
    void SendEmailNuevoTorneo(string nombreTorneo, List<string> destinatarios);
    void SendEmailRonda(string nombreTorneo, int ronda, List<string> destinatarios);
    Task SendEmailOrganizadorNuevoRegistro(EmailContactoDTO request);
    Task SendEmailOrganizadorEnvioListaTorneo(EmailContactoDTO request);
    Task SendEmailModificacionPago(EmailPagoDTO emailPagoDTO);
}
