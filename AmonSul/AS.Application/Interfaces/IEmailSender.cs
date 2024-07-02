using AS.Application.DTOs.Email;

namespace AS.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailRegister(EmailRegisterDTO request);
}
