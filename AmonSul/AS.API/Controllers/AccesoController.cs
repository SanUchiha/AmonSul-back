using AS.Application.Interfaces;
using AS.Domain.Models;
using AS.Infrastructure;
using AS.Infrastructure.DTOs;
using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AS.API.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AccesoController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly DbamonsulContext _dbamonsulContext;
        private readonly Utilidades _utilidades;
        private readonly ILoginApplication _loginApplication;

        public AccesoController(IAccountRepository accountRepository, DbamonsulContext dbamonsulContext, Utilidades utilidades, ILoginApplication loginApplication)
        {
            _accountRepository = accountRepository;
            _dbamonsulContext = dbamonsulContext;
            _utilidades = utilidades;
            _loginApplication = loginApplication;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            if (loginDTO == null || loginDTO.Email == null || loginDTO.Password == null) return BadRequest();

            var response = await _loginApplication.Login(loginDTO);

            return Ok(response);
        }
    }
}
