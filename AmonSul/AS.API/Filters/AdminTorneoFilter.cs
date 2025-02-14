using AS.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace AS.API.Filters;

public class AdminTorneoFilter(IServiceProvider serviceProvider) : ActionFilterAttribute
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.ActionArguments.TryGetValue("idTorneo", out var idTorneoObj) || idTorneoObj is not int idTorneo)
        {
            context.Result = new BadRequestObjectResult("El ID del torneo es inválido.");
            return;
        }

        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        var userEncontrado = await unitOfWork.TorneoRepository.GetIdOrganizadorByIdTorneo(idTorneo);
        if (userId != userEncontrado)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
