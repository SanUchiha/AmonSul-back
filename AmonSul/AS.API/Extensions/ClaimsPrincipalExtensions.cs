using System.Security.Claims;

namespace AS.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Id del usuario autenticado, guardado en el claim NameIdentifier del JWT
    /// </summary>
    public static bool TryGetIdUsuario(this ClaimsPrincipal user, out int idUsuario)
    {
        idUsuario = 0;
        Claim? claim = user.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && int.TryParse(claim.Value, out idUsuario);
    }
}
