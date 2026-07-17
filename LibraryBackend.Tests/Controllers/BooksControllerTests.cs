using LibraryBackend.Controllers;
using LibraryBackend.Models;
using LibraryBackend.Tests.TestHelpers;
using Xunit;

namespace LibraryBackend.Tests.Controllers;

public class BooksControllerTests
{
    [Fact]
    public async Task GetBooks_FiltersByTitleOrAuthor()
    {
        using var context = TestDbContextFactory.Create();
        context.AddRange(
            new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", IsAvailable = true },
            new Book { Id = 2, Title = "The Hobbit", Author = "J.R.R. Tolkien", IsAvailable = true },
            new Book { Id = 3, Title = "Refactoring", Author = "Martin Fowler", IsAvailable = true }
        );
        await context.SaveChangesAsync();

        var controller = new BooksController(context);
        controller.SetUser("someone@test.com");

        var byTitle = await controller.GetBooks(search: "Hobbit");
        Assert.Single(byTitle.Value!.Items);

        var byAuthor = await controller.GetBooks(search: "Martin");
        Assert.Equal(2, byAuthor.Value!.Items.Count());
    }

    [Fact]
    public async Task GetBooks_RespectsPagination()
    {
        using var context = TestDbContextFactory.Create();
        for (var i = 1; i <= 15; i++)
        {
            context.Add(new Book { Id = i, Title = $"Book {i}", Author = "Author", IsAvailable = true });
        }
        await context.SaveChangesAsync();

        var controller = new BooksController(context);
        controller.SetUser("someone@test.com");

        var page1 = await controller.GetBooks(search: null, page: 1, pageSize: 10);
        Assert.Equal(10, page1.Value!.Items.Count());
        Assert.Equal(15, page1.Value.TotalCount);

        var page2 = await controller.GetBooks(search: null, page: 2, pageSize: 10);
        Assert.Equal(5, page2.Value!.Items.Count());
    }

    [Fact]
    public async Task GetBooks_Admin_SeesBorrowerEmailForUnavailableBooks()
    {
        using var context = TestDbContextFactory.Create();
        var borrower = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", IsAvailable = false };
        var activeLoan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        context.AddRange(borrower, book, activeLoan);
        await context.SaveChangesAsync();

        var controller = new BooksController(context);
        controller.SetUser("admin@test.com", role: "Admin");

        var result = await controller.GetBooks(search: null);

        var returnedBook = Assert.Single(result.Value!.Items);
        Assert.Equal("alice@test.com", returnedBook.BorrowedByEmail);
    }

    [Fact]
    public async Task GetBooks_NonAdmin_DoesNotSeeBorrowerEmail()
    {
        using var context = TestDbContextFactory.Create();
        var borrower = new Client { Id = 1, Name = "Alice", Email = "alice@test.com", Role = "User" };
        var book = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", IsAvailable = false };
        var activeLoan = new Loan { Id = 1, BookId = 1, ClientId = 1, LoanDate = DateTime.UtcNow };
        context.AddRange(borrower, book, activeLoan);
        await context.SaveChangesAsync();

        var controller = new BooksController(context);
        controller.SetUser("bob@test.com", role: "User");

        var result = await controller.GetBooks(search: null);

        var returnedBook = Assert.Single(result.Value!.Items);
        Assert.Null(returnedBook.BorrowedByEmail);
    }
}
