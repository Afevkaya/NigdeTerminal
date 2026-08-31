using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using Npgsql;

namespace NigdeTerminal.Tests;

public sealed class PostgreSqlPersistenceTests : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [PostgreSqlFact]
    public async Task Seeded_companies_can_be_read_with_expected_business_rules()
    {
        await using var dbContext = CreateDbContext();
        var dataAccess = new CompanyDataAccess(dbContext);

        var companies = await dataAccess.GetAllAsync();

        AssertCompany(companies, "AKSARAY BİRLİK", CompanyType.LocalMinibus, false);
        AssertCompany(companies, "DERİNKUYU", CompanyType.LocalMinibus, false);
        AssertCompany(companies, "KARACAERLER", CompanyType.LocalMinibus, false);
        AssertCompany(companies, "NİĞDE AYDOĞANLAR SEYAHAT", CompanyType.Intercity, true);
        AssertCompany(companies, "NİĞDE İNAN TURİZM", CompanyType.Intercity, true);
        AssertCompany(companies, "LÜKS EREĞLİ", CompanyType.Intercity, true);
        AssertCompany(companies, "NET TURİZM SEYAHAT", CompanyType.Intercity, true);
    }

    [PostgreSqlFact]
    public async Task Exit_record_without_company_is_rejected()
    {
        await AssertInvalidCompanyIsRejected(Guid.Empty);
    }

    [PostgreSqlFact]
    public async Task Exit_record_with_unknown_company_is_rejected_by_foreign_key()
    {
        await AssertInvalidCompanyIsRejected(Guid.NewGuid());
    }

    [PostgreSqlFact]
    public async Task Invalid_payment_method_is_rejected_by_check_constraint()
    {
        await using var dbContext = CreateDbContext();
        var companyId = await GetCompanyId(dbContext, "AKSARAY BİRLİK");

        await Assert.ThrowsAsync<PostgresException>(() =>
            dbContext.Database.ExecuteSqlInterpolatedAsync($$"""
                INSERT INTO cikis_kayitlari
                    (id, firma_id, plaka, cikis_tarihi, odeme_yontemi, tarife_adi, tarife_ucreti, merkezden_cikti_mi)
                VALUES
                    ({{Guid.NewGuid()}}, {{companyId}}, {{"51 TEST 03"}}, {{DateTimeOffset.UtcNow}}, {{"Havale"}}, {{"Test"}}, {{10m}}, {{false}})
                """));
    }

    [PostgreSqlFact]
    public async Task Negative_tariff_amount_is_rejected_by_check_constraint()
    {
        await using var dbContext = CreateDbContext();
        var companyId = await GetCompanyId(dbContext, "AKSARAY BİRLİK");
        var dataAccess = new ExitRecordDataAccess(dbContext);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            dataAccess.AddAsync(CreateExitRecord(companyId, "51 TEST 04", PaymentMethod.Cash, -1m)));
    }

    [PostgreSqlFact]
    public async Task Cash_credit_card_and_repeated_plate_can_be_persisted()
    {
        await using var dbContext = CreateDbContext();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var companyId = await GetCompanyId(dbContext, "AKSARAY BİRLİK");
        var dataAccess = new ExitRecordDataAccess(dbContext);
        const string plate = "51 TEST 05";

        var cashRecord = CreateExitRecord(companyId, plate, PaymentMethod.Cash, 10m);
        var creditCardRecord = CreateExitRecord(companyId, plate, PaymentMethod.CreditCard, 20m);

        await dataAccess.AddAsync(cashRecord);
        await dataAccess.AddAsync(creditCardRecord);

        var persistedRecords = await dbContext.ExitRecords
            .AsNoTracking()
            .Where(record => record.Id == cashRecord.Id || record.Id == creditCardRecord.Id)
            .ToListAsync();

        Assert.Equal(2, persistedRecords.Count);
        Assert.Contains(persistedRecords, record => record.PaymentMethod == PaymentMethod.Cash);
        Assert.Contains(persistedRecords, record => record.PaymentMethod == PaymentMethod.CreditCard);
        Assert.All(persistedRecords, record => Assert.Equal(plate, record.VehiclePlate));

        await transaction.RollbackAsync();
    }

    private static NigdeTerminalDbContext CreateDbContext()
    {
        return DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);
    }

    private static async Task<Guid> GetCompanyId(
        NigdeTerminalDbContext dbContext,
        string companyName)
    {
        return await dbContext.Companies
            .Where(company => company.Name == companyName)
            .Select(company => company.Id)
            .SingleAsync();
    }

    private static async Task AssertInvalidCompanyIsRejected(Guid companyId)
    {
        await using var dbContext = CreateDbContext();
        var dataAccess = new ExitRecordDataAccess(dbContext);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            dataAccess.AddAsync(CreateExitRecord(
                companyId,
                $"51 TEST {Guid.NewGuid():N}",
                PaymentMethod.Cash,
                10m)));
    }

    private static ExitRecord CreateExitRecord(
        Guid companyId,
        string plate,
        PaymentMethod paymentMethod,
        decimal tariffAmount)
    {
        return new ExitRecord
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            VehiclePlate = plate,
            DepartureDateTime = DateTimeOffset.UtcNow,
            PaymentMethod = paymentMethod,
            TariffName = "Test Tarifesi",
            TariffAmount = tariffAmount,
            DepartedFromCenter = false
        };
    }

    private static void AssertCompany(
        IEnumerable<Company> companies,
        string name,
        CompanyType companyType,
        bool canDepartFromCenter)
    {
        var company = Assert.Single(companies, company => company.Name == name);
        Assert.Equal(companyType, company.CompanyType);
        Assert.Equal(canDepartFromCenter, company.CanDepartFromCenter);
        Assert.True(company.IsActive);
    }
}
