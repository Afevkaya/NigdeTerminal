using Microsoft.EntityFrameworkCore;

namespace NigdeTerminal.App.Data;

public sealed class NigdeTerminalDbContext(DbContextOptions<NigdeTerminalDbContext> options)
    : DbContext(options)
{
}
