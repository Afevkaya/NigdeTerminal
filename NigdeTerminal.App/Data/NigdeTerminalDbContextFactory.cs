using Microsoft.EntityFrameworkCore.Design;

namespace NigdeTerminal.App.Data;

public sealed class NigdeTerminalDbContextFactory : IDesignTimeDbContextFactory<NigdeTerminalDbContext>
{
    public NigdeTerminalDbContext CreateDbContext(string[] args)
    {
        return DatabaseBootstrap.CreateDbContext();
    }
}
