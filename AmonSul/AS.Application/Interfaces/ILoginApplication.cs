using AS.Infrastructure.DTOs;

namespace AS.Application.Interfaces
{
    public interface ILoginApplication
    {
        Task<LoginResponse> Login(LoginDTO loginDTO);
    }
}
