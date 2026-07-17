using System.Security.Claims;
using LibraryBackend.Models;
using LibraryBackend.Services;
using LibraryBackend.Tests.TestHelpers;
using Xunit;

namespace LibraryBackend.Tests.Services;

public class RoleClaimsTransformationTests
{
    [Fact]
    public async Task TransformAsync_MatchingClient_AddsRoleClaimFromDatabase()
    {
        using var context = TestDbContextFactory.Create();
        context.Add(new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "Admin" });
        await context.SaveChangesAsync();

        var identity = new ClaimsIdentity(authenticationType: "TestAuth");
        identity.AddClaim(new Claim("upn", "alice@test.com"));
        var principal = new ClaimsPrincipal(identity);

        var sut = new RoleClaimsTransformation(context);
        var transformed = await sut.TransformAsync(principal);

        Assert.Equal("Admin", transformed.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task TransformAsync_NoMatchingClient_DoesNotAddRoleClaim()
    {
        using var context = TestDbContextFactory.Create();

        var identity = new ClaimsIdentity(authenticationType: "TestAuth");
        identity.AddClaim(new Claim("upn", "ghost@test.com"));
        var principal = new ClaimsPrincipal(identity);

        var sut = new RoleClaimsTransformation(context);
        var transformed = await sut.TransformAsync(principal);

        Assert.Null(transformed.FindFirst(ClaimTypes.Role));
    }

    [Fact]
    public async Task TransformAsync_AlreadyHasRoleClaim_DoesNotQueryOrDuplicate()
    {
        using var context = TestDbContextFactory.Create();
        context.Add(new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "Admin" });
        await context.SaveChangesAsync();

        var identity = new ClaimsIdentity(authenticationType: "TestAuth");
        identity.AddClaim(new Claim("upn", "alice@test.com"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
        var principal = new ClaimsPrincipal(identity);

        var sut = new RoleClaimsTransformation(context);
        var transformed = await sut.TransformAsync(principal);

        Assert.Single(transformed.FindAll(ClaimTypes.Role));
        Assert.Equal("User", transformed.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public async Task TransformAsync_UnauthenticatedPrincipal_ReturnsUnchanged()
    {
        using var context = TestDbContextFactory.Create();
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var sut = new RoleClaimsTransformation(context);
        var transformed = await sut.TransformAsync(principal);

        Assert.Null(transformed.FindFirst(ClaimTypes.Role));
    }

    [Fact]
    public async Task TransformAsync_NoEmailLikeClaim_DoesNotAddRoleClaim()
    {
        using var context = TestDbContextFactory.Create();
        context.Add(new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "Admin" });
        await context.SaveChangesAsync();

        var identity = new ClaimsIdentity(authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var sut = new RoleClaimsTransformation(context);
        var transformed = await sut.TransformAsync(principal);

        Assert.Null(transformed.FindFirst(ClaimTypes.Role));
    }
}
