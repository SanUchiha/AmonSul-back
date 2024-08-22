using AS.Application.DTOs.Email;

namespace AS.Application.Interfaces;

public interface IEmailApplicacion
{
    Task SendEmailRegister(EmailRequestDTO request);
    Task SendEmailContacto(EmailContactoDTO request);
    Task SendEmailResetPass(EmailContactoDTO request);

}
