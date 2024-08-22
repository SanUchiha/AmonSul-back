using AS.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using AS.Application.DTOs.Email;
using AS.Application.Exceptions;
using Microsoft.Extensions.Options;

namespace AS.Application.Services;

public class EmailApplication(IOptions<EmailSettings> emailSettings): IEmailApplicacion
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

    public async Task SendEmailRegister(EmailRequestDTO request)
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
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = true
            };

            foreach (var recipient in request.EmailTo)
            {
                mailMessage.To.Add(recipient);
            }
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
}
