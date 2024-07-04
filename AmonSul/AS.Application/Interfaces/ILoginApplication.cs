using AS.Infrastructure.DTOs.Login;

namespace AS.Application.Interfaces;

public interface ILoginApplication
{
    Task<LoginResponse> Login(LoginDTO loginDTO);
}
