using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace NigdeTerminal.App.Data;

public static class DatabaseBootstrap
{
    public static NigdeTerminalDbContext CreateDbContext(string? basePath = null)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath ?? AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("NigdeTerminal")
            ?? throw new InvalidOperationException(
                "Connection string 'NigdeTerminal' was not found in configuration.");

        var options = new DbContextOptionsBuilder<NigdeTerminalDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NigdeTerminalDbContext(options);
    }
}
