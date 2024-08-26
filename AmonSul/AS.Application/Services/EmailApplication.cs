using AS.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using AS.Application.DTOs.Email;
using AS.Application.Exceptions;
using Microsoft.Extensions.Options;
using Azure.Core;
using AS.Domain.Models;
using AS.Infrastructure.Repositories.Interfaces;

namespace AS.Application.Services;

public class EmailApplication(IOptions<EmailSettings> emailSettings, IUnitOfWork unitOfWork): IEmailApplicacion
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
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

    public async Task SendEmailModificacionInscripcion(EmailContactoDTO request)
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
                Subject = $"Modificacion estado inscripción",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <p>Hola,</p>
                    <p>Se ha modificado el estado de tu inscripción para el torneo <span style=""font-size: 18px; color: #2e6c80;""><strong>{request.Message}</strong></span>.</p>
                    <p>Ya puedes entrar al dashboard para visualizarla.</p>
                    <p>Gracias por utilizar Amon Súl.</p>
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

    public async Task SendEmailNuevoTorneo(string nombreTorneo)
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
                Subject = $"Nuevo Torneo Creado",
                Body = $@"
                <html>
                <body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333;"">
                    <p>Hola,</p>
                    <p>Nos complace informarte que se ha creado un nuevo torneo llamado <span style=""font-size: 18px; color: #2e6c80;""><strong>{nombreTorneo}</strong></span>.</p>      
                    <p>Puedes acceder a más detalles y gestionar tu inscripción a través del dashboard.</p>
                    <p>Gracias por formar parte de Amon Súl.</p>
                </body>
                </html>",
                IsBodyHtml = true
            };

            var listaDestinatarios = await _unitOfWork.UsuarioRepository.GetAll();

         

            foreach (var destinatario in listaDestinatarios)
            {
                mailMessage.To.Add(destinatario.Email);
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
