using AS.Application.DTOs.Email;
using AS.Application.Exceptions;
using AS.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AS.Application.Services;

public class EmailApplication(
    IOptions<EmailSettings> emailSettings): IEmailApplicacion
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;

    public async Task SendEmailContacto(EmailContactoDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Consulta para Amon Sùl de: {request.Email}",
                Body = request.Message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(_emailSettings.From);

            await client.SendMailAsync(mailMessage);
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public async Task SendEmailModificacionLista(EmailListaDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Actualización del estado de tu lista para el torneo {request.NombreTorneo}",
                Body = $@"
                    <html>
                    <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; padding: 20px;"">
                        <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                            <h2 style=""font-size: 24px; color: #2e6c80; margin-bottom: 20px;"">Estado de la lista actualizado</h2>
                            <p>Hola,</p>
                            <p>El estado de tu lista para el torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.NombreTorneo}</strong></span> pasa a ser <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.EstadoLista}</strong></span>.</p>
                            <p>Puedes acceder a tu dashboard para revisar los detalles.</p>
                            <p>Gracias por ser parte de Amon Súl.</p>
                            <hr style=""margin-top: 30px;"" />
                            <p style=""font-size: 12px; color: #888;"">Este es un mensaje automático, por favor no respondas a este correo.</p>
                        </div>
                    </body>
                    </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.EmailTo!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public async Task SendEmailModificacionPago(EmailPagoDTO request)
    {
        try
        {
            SmtpClient client = new(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            string pago = "";
            if (request.EstadoPago == "SI") pago = "PAGADA";
            else pago = "NO PAGADA";

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Confirmación de pago para el torneo {request.NombreTorneo}",
                Body = $@"
                        <html>
                        <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; padding: 20px;"">
                            <div style=""max-width: 600px; margin: auto; background-color: #ffffff; padding: 20px; border-radius: 10px; box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);"">
                                <h2 style=""font-size: 24px; color: #2e6c80; margin-bottom: 20px;"">Estado de Pago Actualizado</h2>
                                <p>Hola,</p>
                                <p>El estado de tu inscripción para el torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.NombreTorneo}</strong></span> ha sido actualizado a <span style=""font-size: 18px; color: #2e6c80;""><strong>{pago}</strong></span>.</p>
                                <p>Puedes acceder a tu dashboard para revisar los detalles.</p>
                                <p>Gracias por ser parte de Amon Súl, y esperamos que disfrutes del torneo.</p>
                                <hr style=""margin-top: 30px;"" />
                                <p style=""font-size: 12px; color: #888;"">Este es un mensaje automático, por favor no respondas a este correo.</p>
                            </div>
                        </body>
                        </html>",
                IsBodyHtml = true

            };

            mailMessage.To.Add(request.EmailTo!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx) 
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public void SendEmailNuevoTorneo(string nombreTorneo, List<string> destinatarios)
    {
        string templateBody = $@"
        <html>
        <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
            <p>Hola,</p>
            <p>Nos complace informarte que se ha creado un nuevo torneo llamado <span style=""font-size: 18px; color: #2e6c80;""><strong>{nombreTorneo}</strong></span>.</p>      
            <p>Puedes acceder a más detalles y gestionar tu inscripción desde Amon Sûl.</p>
            <p>Gracias por formar parte de AS.</p>
        </body>
        </html>";

        List<string> emailsExcluidos = _emailSettings.EmailsExcluidos;

        if(emailsExcluidos.Count > 0)
            destinatarios = destinatarios
                .Where(email => !emailsExcluidos.Contains(email))
                .ToList();

        List<List<string>> destinatarioGroups = destinatarios
          .Select((item, index) => new { item, index })
          .GroupBy(x => x.index / 5)
          .Select(g => g.Select(x => x.item).ToList())
          .ToList();

        foreach (List<string> items in destinatarioGroups)
        {
            BackgroundJob.Enqueue(() =>
            EnviarCorreoAsync(
                items, 
                templateBody,
                Utils.Constantes.ConstEmailMessage.MESSAGE_NUEVO_TORNEO_ASUNTO));
        }
    }

    public void SendEmailRonda(string nombreTorneo, int ronda, List<string> destinatarios)
    {
        string templateBody = $@"
        <html>
        <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
            <p>Hola,</p>
            <p>La ronda número <span style=""font-size: 18px; color: #2e6c80;""><strong>{ronda}</strong></span> para el torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{nombreTorneo}</strong></span> ya está generada.</p>      
            <p>Puedes acceder a Amon Sûl y ver los detalles.</p>
            <p>Gracias por formar parte de AS.</p>
        </body>
        </html>";

        List<List<string>> destinatarioGroups = destinatarios
          .Select((item, index) => new { item, index })
          .GroupBy(x => x.index / 5)
          .Select(g => g.Select(x => x.item).ToList())
          .ToList();

        foreach (List<string> items in destinatarioGroups)
        {
            BackgroundJob.Enqueue(() =>
            EnviarCorreoAsync(
                items, 
                templateBody, 
                Utils.Constantes.ConstEmailMessage.MESSAGE_NUEVA_RONDA_ASUNTO));
        }
    }

    public async Task EnviarCorreoAsync(List<string> destinatarios, string templateBody, string asunto)
    {
        using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
        {
            EnableSsl = _emailSettings.EnableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_emailSettings.From),
            Subject = asunto,
            Body = templateBody,
            IsBodyHtml = true,
        };
        foreach (var item in destinatarios)
        {
            mailMessage.To.Add(item);
        }

        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (SmtpException smtpEx)
        {
            Console.WriteLine(smtpEx.Message);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("Error al enviar email.", ex);
        }
    }

    public async Task SendEmailOrganizadorEnvioListaTorneo(EmailContactoDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Actualización lista",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <h2 style=""color: #2e6c80;"">Un usuario ha subido la lista</h2>
                    <p>Hola,</p>
                    <p>Un jugador ha subido su lista del torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.Message}</strong></span>
                    .</p>
                    </p>
                    <p>Gracias por utilizar Amon Súl.</p>
                    <hr />
                </body>
                </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.Email!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public async Task SendEmailOrganizadorNuevoRegistro(EmailContactoDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Inscripción torneo",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <h2 style=""color: #2e6c80;"">Un nuevo jugador se ha apuntado</h2>
                    <p>Hola,</p>
                    <p>Se acaba de apuntar un nuevo jugador al torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.Message}</strong></span>
                    .</p>
                    </p>
                    <p>Gracias por utilizar Amon Súl.</p>
                    <hr />
                </body>
                </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.Email!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public void SendEmailNuevaPartida(List<string> destinatarios)
    {
        string templateBody = Utils.Constantes.ConstEmailMessage.MESSAGE_CREACION_PARTIDA_BODY;

        List<List<string>> destinatarioGroups = destinatarios
          .Select((item, index) => new { item, index })
          .GroupBy(x => x.index / 5)
          .Select(g => g.Select(x => x.item).ToList())
          .ToList();

        foreach (List<string> items in destinatarioGroups)
        {
            BackgroundJob.Enqueue(() =>
            EnviarCorreoAsync(
                items, 
                templateBody,
                Utils.Constantes.ConstEmailMessage.MESSAGE_CREACION_PARTIDA_ASUNTO));
        }
    }

    public async Task SendEmailRegistroTorneo(EmailContactoDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Inscripción torneo",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <h2 style=""color: #2e6c80;"">Te has inscrito correctamente</h2>
                    <p>Hola,</p>
                    <p>Hemos recibido la solicitud para la inscripción al torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.Message}</strong></span>
                    .</p>
                    </p>
                    <p>Gracias por utilizar Amon Súl.</p>
                    <hr />
                </body>
                </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.Email!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public async Task SendEmailResetPass(EmailContactoDTO request)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = $"Reseteo de contraseña",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <h2 style=""color: #2e6c80;"">Solicitud de restablecimiento de contraseña</h2>
                    <p>Hola,</p>
                    <p>Hemos recibido una solicitud para restablecer la contraseña asociada a tu cuenta.</p>
                    <p>
                        Tu nueva <strong>contraseña temporal</strong> es:
                        <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.Message}</strong></span>
                    </p>
                    <p>
                        Te recomendamos que inicies sesión lo antes posible y cambies esta contraseña por una
                        que solo tú conozcas.
                    </p>
                    <p>
                        Si no solicitaste este cambio, por favor, contacta a nuestro equipo de soporte
                        inmediatamente.
                    </p>
                    <p>Gracias,</p>
                    <p>El equipo de Amon Sûl</p>
                    <hr />
                </body>
                </html>",
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.Email!);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine(smtpEx.Message);
            }
        }
        catch (SmtpException smtpEx)
        {
            throw new EmailSendException("Error occurred while sending email.", smtpEx);
        }
        catch (Exception ex)
        {
            throw new EmailSendException("An unexpected error occurred while sending email.", ex);
        }
    }

    public void SendEmailNuevoUsuario(List<string> destinatarios)
    {
        string templateBody = Utils.Constantes.ConstEmailMessage.MESSAGE_BIENVENIDA_BODY;

        destinatarios.Add(_emailSettings.From);

        List<List<string>> destinatarioGroups = destinatarios
          .Select((item, index) => new { item, index })
          .GroupBy(x => x.index / 5)
          .Select(g => g.Select(x => x.item).ToList())
          .ToList();

        foreach (List<string> items in destinatarioGroups)
        {
            BackgroundJob.Enqueue(() =>
            EnviarCorreoAsync(
                items, 
                templateBody,
                Utils.Constantes.ConstEmailMessage.MESSAGE_BIENVENIDA_ASUNTO));
        }
    }
}
