using System.Diagnostics.CodeAnalysis;
using AINotesApp.Data;
using Microsoft.EntityFrameworkCore;

namespace AINotesApp.Tests.Integration.Database;

/// <summary>
/// Fixture for integration tests that provides an in-memory database context.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);

        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Dispose();
    }

    public ApplicationDbContext CreateNewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
