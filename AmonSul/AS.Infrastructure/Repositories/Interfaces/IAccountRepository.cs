using AS.Infrastructure.DTOs;

namespace AS.Infrastructure.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<LoginResponse> Login(LoginDTO loginDTO);
}
