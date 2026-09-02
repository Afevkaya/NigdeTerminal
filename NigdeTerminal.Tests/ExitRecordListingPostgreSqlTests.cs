using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;

namespace NigdeTerminal.Tests;

public sealed class ExitRecordListingPostgreSqlTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [PostgreSqlFact]
    public async Task Selected_day_uses_inclusive_start_and_exclusive_next_day_boundary()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var company = await GetCompany(dbContext, "AKSARAY BİRLİK");
        var selectedDate = new DateOnly(2035, 4, 15);
        var previousDay = CreateRecord(company.Id, selectedDate.AddDays(-1), new TimeOnly(23, 59, 59), "51 SIN 01");
        var startOfDay = CreateRecord(company.Id, selectedDate, TimeOnly.MinValue, "51 SIN 02");
        var endOfDay = CreateRecord(company.Id, selectedDate, TimeOnly.MaxValue, "51 SIN 03");
        var nextDay = CreateRecord(company.Id, selectedDate.AddDays(1), TimeOnly.MinValue, "51 SIN 04");
        dbContext.ExitRecords.AddRange(previousDay, startOfDay, endOfDay, nextDay);
        await dbContext.SaveChangesAsync();

        var results = await new ExitRecordDataAccess(dbContext).GetListAsync(selectedDate);

        Assert.Equal(new[] { endOfDay.Id, startOfDay.Id }, results.Select(item => item.Id));
        Assert.DoesNotContain(results, item => item.Id == previousDay.Id || item.Id == nextDay.Id);
        await transaction.RollbackAsync();
    }

    [PostgreSqlFact]
    public async Task Listing_returns_company_and_ui_fields_with_newest_record_first()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var company = await GetCompany(dbContext, "NİĞDE İNAN TURİZM");
        var selectedDate = new DateOnly(2035, 4, 16);
        var older = CreateRecord(company.Id, selectedDate, new TimeOnly(8, 15), "51 LST 01");
        var newer = CreateRecord(company.Id, selectedDate, new TimeOnly(18, 45), "51 LST 02", PaymentMethod.CreditCard, 250m, true);
        dbContext.ExitRecords.AddRange(older, newer);
        await dbContext.SaveChangesAsync();

        var results = await new ExitRecordDataAccess(dbContext).GetListAsync(selectedDate);

        Assert.Equal(new[] { newer.Id, older.Id }, results.Select(item => item.Id));
        var item = Assert.Single(results, result => result.Id == newer.Id);
        Assert.Equal(newer.DepartureTime, item.DepartureTime);
        Assert.Equal(company.Name, item.CompanyName);
        Assert.Equal(newer.VehiclePlate, item.VehiclePlate);
        Assert.Equal(PaymentMethod.CreditCard, item.PaymentMethod);
        Assert.Equal(250m, item.TariffAmount);
        Assert.True(item.DepartedFromCenter);
        await transaction.RollbackAsync();
    }

    [PostgreSqlFact]
    public async Task Optional_company_filter_returns_only_matching_company()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var firstCompany = await GetCompany(dbContext, "AKSARAY BİRLİK");
        var secondCompany = await GetCompany(dbContext, "DERİNKUYU");
        var selectedDate = new DateOnly(2035, 4, 17);
        var matching = CreateRecord(firstCompany.Id, selectedDate, new TimeOnly(10, 0), "51 FRM 01");
        var other = CreateRecord(secondCompany.Id, selectedDate, new TimeOnly(11, 0), "51 FRM 02");
        dbContext.ExitRecords.AddRange(matching, other);
        await dbContext.SaveChangesAsync();

        var results = await new ExitRecordDataAccess(dbContext).GetListAsync(selectedDate, firstCompany.Id);

        var item = Assert.Single(results);
        Assert.Equal(matching.Id, item.Id);
        Assert.Equal(firstCompany.Name, item.CompanyName);
        await transaction.RollbackAsync();
    }

    [PostgreSqlFact]
    public async Task Plate_filter_is_case_and_whitespace_normalized_and_supports_partial_search()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var company = await GetCompany(dbContext, "AKSARAY BİRLİK");
        var selectedDate = new DateOnly(2035, 4, 18);
        var matching = CreateRecord(company.Id, selectedDate, new TimeOnly(12, 0), "51 ABC 123");
        var other = CreateRecord(company.Id, selectedDate, new TimeOnly(13, 0), "51 XYZ 987");
        dbContext.ExitRecords.AddRange(matching, other);
        await dbContext.SaveChangesAsync();

        var fullPlateResults = await new ExitRecordDataAccess(dbContext)
            .GetListAsync(selectedDate, vehiclePlateSearch: "51abc123");
        var partialResults = await new ExitRecordDataAccess(dbContext)
            .GetListAsync(selectedDate, vehiclePlateSearch: " abc ");

        Assert.Equal(matching.Id, Assert.Single(fullPlateResults).Id);
        Assert.Equal(matching.Id, Assert.Single(partialResults).Id);
        await transaction.RollbackAsync();
    }

    [PostgreSqlFact]
    public async Task Missing_filters_return_all_daily_records_and_empty_day_returns_empty_list()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var firstCompany = await GetCompany(dbContext, "AKSARAY BİRLİK");
        var secondCompany = await GetCompany(dbContext, "DERİNKUYU");
        var selectedDate = new DateOnly(2035, 4, 19);
        dbContext.ExitRecords.AddRange(
            CreateRecord(firstCompany.Id, selectedDate, new TimeOnly(9, 0), "51 ALL 01"),
            CreateRecord(secondCompany.Id, selectedDate, new TimeOnly(10, 0), "51 ALL 02"));
        await dbContext.SaveChangesAsync();

        var dataAccess = new ExitRecordDataAccess(dbContext);
        var allResults = await dataAccess.GetListAsync(selectedDate, null, "   ");
        var emptyResults = await dataAccess.GetListAsync(selectedDate.AddDays(10));

        Assert.Equal(2, allResults.Count);
        Assert.Empty(emptyResults);
        await transaction.RollbackAsync();
    }

    [PostgreSqlFact]
    public async Task Daily_report_query_returns_only_selected_day_with_company_details()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var company = await GetCompany(dbContext, "NİĞDE İNAN TURİZM");
        var selectedDate = new DateOnly(2035, 4, 20);
        var previousDay = CreateRecord(
            company.Id,
            selectedDate.AddDays(-1),
            TimeOnly.MaxValue,
            "51 RPR 01");
        var startOfDay = CreateRecord(
            company.Id,
            selectedDate,
            TimeOnly.MinValue,
            "51 RPR 02");
        var endOfDay = CreateRecord(
            company.Id,
            selectedDate,
            TimeOnly.MaxValue,
            "51 RPR 03");
        var nextDay = CreateRecord(
            company.Id,
            selectedDate.AddDays(1),
            TimeOnly.MinValue,
            "51 RPR 04");
        dbContext.ExitRecords.AddRange(previousDay, startOfDay, endOfDay, nextDay);
        await dbContext.SaveChangesAsync();

        var results = await new ExitRecordDataAccess(dbContext)
            .GetDailyReportRecordsAsync(selectedDate);

        Assert.Equal(new[] { startOfDay.Id, endOfDay.Id }, results.Select(item => item.Id));
        Assert.All(results, item =>
        {
            Assert.Equal(company.Name, item.CompanyName);
            Assert.Equal(company.CompanyType, item.CompanyType);
            Assert.Equal(company.CanDepartFromCenter, item.CompanyCanDepartFromCenter);
        });
        Assert.DoesNotContain(results, item => item.Id == previousDay.Id || item.Id == nextDay.Id);
        await transaction.RollbackAsync();
    }

    private static NigdeTerminalDbContext CreateDbContext() =>
        DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);

    private static Task<Company> GetCompany(NigdeTerminalDbContext dbContext, string name) =>
        dbContext.Companies.SingleAsync(company => company.Name == name);

    private static ExitRecord CreateRecord(
        Guid companyId,
        DateOnly date,
        TimeOnly time,
        string plate,
        PaymentMethod paymentMethod = PaymentMethod.Cash,
        decimal amount = 100m,
        bool departedFromCenter = false) =>
        new()
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            VehiclePlate = plate,
            DepartureDate = date,
            DepartureTime = time,
            PaymentMethod = paymentMethod,
            TariffName = "Test Tarifesi",
            TariffAmount = amount,
            DepartedFromCenter = departedFromCenter
        };
}
