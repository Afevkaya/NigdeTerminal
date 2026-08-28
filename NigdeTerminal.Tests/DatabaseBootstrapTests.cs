using NigdeTerminal.App.Data;

namespace NigdeTerminal.Tests;

public sealed class DatabaseBootstrapTests
{
    [Fact]
    public void DbContext_starts_without_domain_entities()
    {
        using var dbContext = DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);

        Assert.Empty(dbContext.Model.GetEntityTypes());
    }
}
