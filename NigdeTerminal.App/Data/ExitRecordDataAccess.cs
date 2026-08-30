using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Data;

public sealed class ExitRecordDataAccess(NigdeTerminalDbContext dbContext)
{
    public async Task AddAsync(
        ExitRecord exitRecord,
        CancellationToken cancellationToken = default)
    {
        dbContext.ExitRecords.Add(exitRecord);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
