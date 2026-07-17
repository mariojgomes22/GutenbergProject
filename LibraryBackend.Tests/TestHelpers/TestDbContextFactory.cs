using LibraryBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryBackend.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static LibraryContext Create()
    {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LibraryContext(options);
    }
}
