using LibraryBackend.Controllers;
using LibraryBackend.Models;
using LibraryBackend.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LibraryBackend.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task GetCurrentUser_NoEmailClaim_ReturnsUnauthorized()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new AuthController(context);
        controller.SetUser(null);

        var result = await controller.GetCurrentUser();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrentUser_ExistingClient_ReturnsExistingRecordWithoutDuplicating()
    {
        using var context = TestDbContextFactory.Create();
        context.Add(new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "Admin" });
        await context.SaveChangesAsync();

        var controller = new AuthController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.GetCurrentUser();

        var client = Assert.IsType<Client>(result.Value);
        Assert.Equal(1, client.Id);
        Assert.Equal("Admin", client.Role);
        Assert.Single(context.Clients);
    }

    [Fact]
    public async Task GetCurrentUser_FirstLogin_CreatesClientWithNamePrefixedFromEmailAndUserRole()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new AuthController(context);
        controller.SetUser("new.person@test.com");

        var result = await controller.GetCurrentUser();

        var client = Assert.IsType<Client>(result.Value);
        Assert.Equal("new.person", client.Name);
        Assert.Equal("new.person@test.com", client.Email);
        Assert.Equal("User", client.Role);
        Assert.Single(context.Clients);
    }
}
