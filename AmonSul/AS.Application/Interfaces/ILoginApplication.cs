using AS.Infrastructure.DTOs.Login;

namespace AS.Application.Interfaces;

public interface ILoginApplication
{
    Task<LoginResponse> Login(LoginDTO loginDTO);
    Task<LoginResponse> Refresh(string refreshToken);
    Task CerrarSesion(string refreshToken);
}
