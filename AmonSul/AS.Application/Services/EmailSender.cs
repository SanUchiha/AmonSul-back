using AS.Application.Interfaces;
using System.Net.Mail;
using System.Net;
using AS.Application.DTOs.Email;
using AS.Application.Exceptions;
using Microsoft.Extensions.Options;

namespace AS.Application.Services;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public Task SendEmailRegister(EmailRegisterDTO request)
    {
        try {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.From, _emailSettings.Password)
            };

            var send = client.SendMailAsync(
                new MailMessage(
                    from: _emailSettings.From,
                    to: request.EmailTo,
                    request.Subject,
                    request.Body));

            return send;
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
