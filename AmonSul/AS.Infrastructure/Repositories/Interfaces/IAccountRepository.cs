using AS.Infrastructure.DTOs.Login;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<LoginResponse> Login(LoginDTO loginDTO);
}
