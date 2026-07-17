using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryBackend.Tests.TestHelpers;

public static class ControllerTestExtensions
{
    /// <summary>
    /// Attaches a ClaimsPrincipal (built from "upn", plus an optional role claim as added
    /// by RoleClaimsTransformation) to the controller, mirroring an authenticated request.
    /// </summary>
    public static void SetUser(this ControllerBase controller, string? upn, string? role = null)
    {
        var identity = new ClaimsIdentity(authenticationType: upn != null ? "TestAuth" : null);
        if (upn != null)
        {
            identity.AddClaim(new Claim("upn", upn));
        }
        if (role != null)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
