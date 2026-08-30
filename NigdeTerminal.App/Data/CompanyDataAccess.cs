using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Data;

public sealed class CompanyDataAccess(NigdeTerminalDbContext dbContext)
{
    public Task<List<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .ToListAsync(cancellationToken);
    }
}
