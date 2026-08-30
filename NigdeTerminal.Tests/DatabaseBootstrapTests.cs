using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;

namespace NigdeTerminal.Tests;

public sealed class DatabaseBootstrapTests
{
    [Fact]
    public void DbContext_contains_company_entity()
    {
        using var dbContext = DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);

        var company = dbContext.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(Company));

        Assert.NotNull(company);
        Assert.Equal("firmalar", company.GetTableName());
    }

    [Fact]
    public void Company_seed_data_matches_business_rules()
    {
        using var dbContext = DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);
        var company = dbContext.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(Company));

        Assert.NotNull(company);

        var seedCompanies = company.GetSeedData()
            .Select(values => new
            {
                Name = (string)values[nameof(Company.Name)]!,
                CompanyType = (CompanyType)values[nameof(Company.CompanyType)]!,
                CanDepartFromCenter = (bool)values[nameof(Company.CanDepartFromCenter)]!,
                IsActive = (bool)values[nameof(Company.IsActive)]!
            })
            .ToArray();

        Assert.Equal(6, seedCompanies.Length);
        Assert.All(seedCompanies, companyData => Assert.True(companyData.IsActive));

        Assert.True(
            new HashSet<string>(["Aksaray", "Derinkuyu", "Karacaerler"])
                .SetEquals(seedCompanies
                .Where(companyData => companyData.CompanyType == CompanyType.LocalMinibus)
                .Select(companyData => companyData.Name)));

        Assert.True(
            new HashSet<string>(["Aydoğanlar", "İnan", "Lüks Ereğli"])
                .SetEquals(seedCompanies
                .Where(companyData => companyData.CanDepartFromCenter)
                .Select(companyData => companyData.Name)));
    }

    [Fact]
    public void Exit_record_schema_contains_required_relationship_and_indexes()
    {
        using var dbContext = DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);
        var model = dbContext.GetService<IDesignTimeModel>().Model;
        var exitRecord = model.FindEntityType(typeof(ExitRecord));

        Assert.NotNull(exitRecord);
        Assert.Equal("cikis_kayitlari", exitRecord.GetTableName());

        var foreignKey = Assert.Single(exitRecord.GetForeignKeys());
        Assert.Equal(nameof(ExitRecord.CompanyId), Assert.Single(foreignKey.Properties).Name);
        Assert.Equal(typeof(Company), foreignKey.PrincipalEntityType.ClrType);

        var indexedProperties = exitRecord.GetIndexes()
            .Select(index => Assert.Single(index.Properties).Name)
            .ToHashSet();

        Assert.True(
            new HashSet<string>(
                    [nameof(ExitRecord.CompanyId), nameof(ExitRecord.DepartureDateTime)])
                .SetEquals(indexedProperties));
    }

    [Fact]
    public void Exit_record_payment_method_and_tariff_constraints_match_business_rules()
    {
        using var dbContext = DatabaseBootstrap.CreateDbContext(AppContext.BaseDirectory);
        var exitRecord = dbContext.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(ExitRecord));

        Assert.NotNull(exitRecord);

        var paymentMethod = exitRecord.FindProperty(nameof(ExitRecord.PaymentMethod));
        Assert.NotNull(paymentMethod);

        var converter = paymentMethod.GetValueConverter();
        Assert.NotNull(converter);
        Assert.Equal("Nakit", converter.ConvertToProvider(PaymentMethod.Cash));
        Assert.Equal("KrediKarti", converter.ConvertToProvider(PaymentMethod.CreditCard));

        var constraints = exitRecord.GetCheckConstraints()
            .ToDictionary(constraint => constraint.Name!, constraint => constraint.Sql);

        Assert.Equal(
            "odeme_yontemi IN ('Nakit', 'KrediKarti')",
            constraints["ck_cikis_kayitlari_odeme_yontemi"]);
        Assert.Equal(
            "tarife_ucreti >= 0",
            constraints["ck_cikis_kayitlari_tarife_ucreti"]);
    }
}
