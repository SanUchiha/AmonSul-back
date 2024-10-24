﻿using AS.Application.DTOs.Email;

namespace AS.Application.Interfaces;

public interface IEmailApplicacion
{
    Task SendEmailRegister(EmailRequestDTO request);
    Task SendEmailContacto(EmailContactoDTO request);
    Task SendEmailResetPass(EmailContactoDTO request);
    Task SendEmailRegistroTorneo(EmailContactoDTO request);
    Task SendEmailModificacionLista(EmailListaDTO request);
    Task SendEmailNuevoTorneo(string nombreTorneo);
    Task SendEmailOrganizadorNuevoRegistro(EmailContactoDTO request);
    Task SendEmailOrganizadorEnvioListaTorneo(EmailContactoDTO request);
    Task SendEmailModificacionPago(EmailPagoDTO emailPagoDTO);
}
