using LibraryBackend.Controllers;
using LibraryBackend.Models;
using LibraryBackend.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace LibraryBackend.Tests.Controllers;

public class LoansControllerTests
{
    [Fact]
    public async Task ReturnLoan_OwnActiveLoan_MarksReturnedAndFreesBook()
    {
        using var context = TestDbContextFactory.Create();
        var user = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        var loan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow.AddDays(-1) };
        context.AddRange(user, book, loan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.ReturnLoan(1);

        Assert.IsType<NoContentResult>(result);
        var updatedLoan = await context.Loans.FindAsync(1);
        Assert.NotNull(updatedLoan!.ReturnDate);
        var updatedBook = await context.Books.FindAsync(1);
        Assert.True(updatedBook!.IsAvailable);
    }

    [Fact]
    public async Task ReturnLoan_OtherUsersLoan_NonAdmin_ReturnsForbid()
    {
        using var context = TestDbContextFactory.Create();
        var owner = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var intruder = new Client { Id = 2, Name = "Bob", Email = "bob@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        var loan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        context.AddRange(owner, intruder, book, loan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("bob@test.com");

        var result = await controller.ReturnLoan(1);

        Assert.IsType<ForbidResult>(result);
        var untouchedLoan = await context.Loans.FindAsync(1);
        Assert.Null(untouchedLoan!.ReturnDate);
    }

    [Fact]
    public async Task ReturnLoan_OtherUsersLoan_Admin_Succeeds()
    {
        using var context = TestDbContextFactory.Create();
        var owner = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var admin = new Client { Id = 2, Name = "Root", Email = "admin@test.com", Role = "Admin" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        var loan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        context.AddRange(owner, admin, book, loan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("admin@test.com");

        var result = await controller.ReturnLoan(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ReturnLoan_AlreadyReturned_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();
        var user = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = true };
        var loan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow.AddDays(-2), ReturnDate = DateTime.UtcNow.AddDays(-1) };
        context.AddRange(user, book, loan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.ReturnLoan(1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ReturnLoan_UnknownLoan_ReturnsNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var user = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        context.Add(user);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.ReturnLoan(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ReturnLoan_NoAuthenticatedUser_ReturnsUnauthorized()
    {
        using var context = TestDbContextFactory.Create();
        var controller = new LoansController(context);
        controller.SetUser(null);

        var result = await controller.ReturnLoan(1);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task GetLoans_NonAdmin_OnlySeesOwnLoansRegardlessOfClientIdFilter()
    {
        using var context = TestDbContextFactory.Create();
        var alice = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var bob = new Client { Id = 2, Name = "Bob", Email = "bob@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        var aliceLoan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        var bobLoan = new Loan { Id = 2, BookId = 1, ClientId = 2, LoanDate = DateTime.UtcNow };
        context.AddRange(alice, bob, book, aliceLoan, bobLoan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        // Even asking explicitly for Bob's loans, a non-admin should only get their own back.
        var result = await controller.GetLoans(clientId: 2);

        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(result.Value);
        var loanList = loans.ToList();
        Assert.Single(loanList);
        Assert.Equal(1, loanList[0].ClientId);
    }

    [Fact]
    public async Task GetLoans_Admin_CanFilterByClientId()
    {
        using var context = TestDbContextFactory.Create();
        var alice = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var admin = new Client { Id = 2, Name = "Root", Email = "admin@test.com", Role = "Admin" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        var aliceLoan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        var adminLoan = new Loan { Id = 2, BookId = 1, ClientId = 2, LoanDate = DateTime.UtcNow };
        context.AddRange(alice, admin, book, aliceLoan, adminLoan);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("admin@test.com");

        var result = await controller.GetLoans(clientId: 1);

        var loans = Assert.IsAssignableFrom<IEnumerable<Loan>>(result.Value);
        var loanList = loans.ToList();
        Assert.Single(loanList);
        Assert.Equal(1, loanList[0].ClientId);
    }

    [Fact]
    public async Task PostLoan_NonAdmin_ForcesLoanOntoOwnAccount()
    {
        using var context = TestDbContextFactory.Create();
        var alice = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var bob = new Client { Id = 2, Name = "Bob", Email = "bob@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = true };
        context.AddRange(alice, bob, book);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        // Alice tries to request the book on Bob's behalf; the controller must override this.
        var result = await controller.PostLoan(new Loan { BookId = 1, ClientId = 2 });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdLoan = Assert.IsType<Loan>(created.Value);
        Assert.Equal(1, createdLoan.ClientId);
    }

    [Fact]
    public async Task PostLoan_SetsDueDate30DaysAfterLoanDate()
    {
        using var context = TestDbContextFactory.Create();
        var alice = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = true };
        context.AddRange(alice, book);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.PostLoan(new Loan { BookId = 1, ClientId = 1 });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdLoan = Assert.IsType<Loan>(created.Value);
        Assert.NotNull(createdLoan.DueDate);
        Assert.Equal(createdLoan.LoanDate.AddDays(30), createdLoan.DueDate);

        var bookAfter = await context.Books.FindAsync(1);
        Assert.False(bookAfter!.IsAvailable);
    }

    [Fact]
    public async Task PostLoan_BookNotAvailable_ReturnsBadRequest()
    {
        using var context = TestDbContextFactory.Create();
        var alice = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Book A", Author = "Author A", IsAvailable = false };
        context.AddRange(alice, book);
        await context.SaveChangesAsync();

        var controller = new LoansController(context);
        controller.SetUser("alice@test.com");

        var result = await controller.PostLoan(new Loan { BookId = 1, ClientId = 1 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
