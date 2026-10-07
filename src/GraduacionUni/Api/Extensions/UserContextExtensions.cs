using System.Security.Claims;
using GraduacionUni.Application.Services;

namespace GraduacionUni.Api.Extensions;

public static class UserContextExtensions
{
    /// <summary>Convierte los claims del JWT (sub, name, role) en un CurrentUser.</summary>
    public static CurrentUser? ToCurrentUser(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst("sub")?.Value;
        var email = principal.FindFirst("name")?.Value;
        var role = principal.FindFirst("role")?.Value;

        if (!int.TryParse(sub, out var id) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
            return null;

        return new CurrentUser(id, email, role);
    }
}
