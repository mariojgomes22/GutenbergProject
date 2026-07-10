using System.Security.Claims;
using LibraryBackend.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace LibraryBackend.Services;

/// <summary>
/// Maps the authenticated Entra ID user to their Client.Role in the database
/// and adds it as a standard role claim, so [Authorize(Roles = "...")] works.
/// </summary>
public class RoleClaimsTransformation : IClaimsTransformation
{
    private readonly LibraryContext _context;

    public RoleClaimsTransformation(LibraryContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identity
            || identity.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return principal;
        }

        var email = principal.FindFirst("preferred_username")?.Value
                    ?? principal.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal.FindFirst("upn")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return principal;
        }

        var role = await _context.Clients
            .AsNoTracking()
            .Where(c => c.Email == email)
            .Select(c => c.Role)
            .FirstOrDefaultAsync();

        if (role != null)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        return principal;
    }
}
