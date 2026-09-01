using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Services;

namespace NigdeTerminal.App.Data;

public sealed class ExitRecordDataAccess(
    NigdeTerminalDbContext dbContext,
    VehiclePlateService? vehiclePlateService = null)
{
    private readonly VehiclePlateService _vehiclePlateService =
        vehiclePlateService ?? new VehiclePlateService();

    public async Task AddAsync(
        ExitRecord exitRecord,
        CancellationToken cancellationToken = default)
    {
        dbContext.ExitRecords.Add(exitRecord);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<List<ExitRecordListItem>> GetListAsync(
        DateOnly date,
        Guid? companyId = null,
        string? vehiclePlateSearch = null,
        CancellationToken cancellationToken = default)
    {
        var nextDate = date.AddDays(1);
        var normalizedPlateSearch = _vehiclePlateService.NormalizeSearchTerm(vehiclePlateSearch);

        var query =
            from exitRecord in dbContext.ExitRecords.AsNoTracking()
            join company in dbContext.Companies.AsNoTracking()
                on exitRecord.CompanyId equals company.Id
            where exitRecord.DepartureDate >= date
                && exitRecord.DepartureDate < nextDate
            select new { ExitRecord = exitRecord, CompanyName = company.Name };

        if (companyId.HasValue)
        {
            query = query.Where(item => item.ExitRecord.CompanyId == companyId.Value);
        }

        if (normalizedPlateSearch.Length > 0)
        {
            query = query.Where(item =>
                item.ExitRecord.VehiclePlate.Replace(" ", "")
                    .Contains(normalizedPlateSearch));
        }

        return query
            .OrderByDescending(item => item.ExitRecord.DepartureDate)
            .ThenByDescending(item => item.ExitRecord.DepartureTime)
            .ThenByDescending(item => item.ExitRecord.Id)
            .Select(item => new ExitRecordListItem(
                item.ExitRecord.Id,
                item.ExitRecord.DepartureTime,
                item.CompanyName,
                item.ExitRecord.VehiclePlate,
                item.ExitRecord.PaymentMethod,
                item.ExitRecord.TariffAmount,
                item.ExitRecord.DepartedFromCenter))
            .ToListAsync(cancellationToken);
    }
}
