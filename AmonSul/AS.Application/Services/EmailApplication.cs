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
}
