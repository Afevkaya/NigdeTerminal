using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Services;

namespace NigdeTerminal.Tests;

public sealed class DailyReportServiceTests : IDisposable
{
    private readonly NigdeTerminalDbContext _dbContext;
    private readonly DailyReportService _service;

    public DailyReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<NigdeTerminalDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;
        _dbContext = new NigdeTerminalDbContext(options);
        _service = new DailyReportService(new ExitRecordDataAccess(_dbContext));
    }

    [Theory]
    [InlineData("DERİNKUYU")]
    [InlineData("AKSARAY BİRLİK")]
    [InlineData("KARACAERLER")]
    public void Local_minibus_is_put_in_near_distance_column(string companyName)
    {
        var row = Assert.Single(_service.Create([
            CreateRecord(companyName, CompanyType.LocalMinibus)
        ]).Rows);

        Assert.Equal(companyName, row.NearDistanceCompany);
        Assert.Empty(row.LocalCompany);
        Assert.Empty(row.SecondaryCompany);
    }

    [Theory]
    [InlineData("NİĞDE İNAN TURİZM")]
    [InlineData("NİĞDE AYDOĞANLAR SEYAHAT")]
    [InlineData("LÜKS EREĞLİ")]
    [InlineData("NET TURİZM SEYAHAT")]
    public void Eligible_intercity_company_is_put_in_local_company_column(string companyName)
    {
        var row = Assert.Single(_service.Create([
            CreateRecord(companyName, CompanyType.Intercity, canDepartFromCenter: true)
        ]).Rows);

        Assert.Equal(companyName, row.LocalCompany);
        Assert.Empty(row.SecondaryCompany);
        Assert.Empty(row.NearDistanceCompany);
    }

    [Fact]
    public void Other_intercity_company_is_put_in_secondary_company_column()
    {
        var row = Assert.Single(_service.Create([
            CreateRecord("METRO TURİZM", CompanyType.Intercity)
        ]).Rows);

        Assert.Equal("METRO TURİZM", row.SecondaryCompany);
        Assert.Empty(row.LocalCompany);
        Assert.Empty(row.NearDistanceCompany);
    }

    [Theory]
    [InlineData(true, "EVET")]
    [InlineData(false, "")]
    public void Center_departure_text_matches_record_value(bool departedFromCenter, string expected)
    {
        var row = Assert.Single(_service.Create([
            CreateRecord("TEST", departedFromCenter: departedFromCenter)
        ]).Rows);

        Assert.Equal(expected, row.CenterDeparture);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash, "NAKİT")]
    [InlineData(PaymentMethod.CreditCard, "KREDİ KARTI")]
    public void Payment_method_text_is_created(PaymentMethod paymentMethod, string expected)
    {
        var row = Assert.Single(_service.Create([
            CreateRecord("TEST", paymentMethod: paymentMethod)
        ]).Rows);

        Assert.Equal(expected, row.PaymentMethod);
    }

    [Fact]
    public void Credit_card_price_is_preserved()
    {
        var row = Assert.Single(_service.Create([
            CreateRecord("TEST", paymentMethod: PaymentMethod.CreditCard, amount: 700m)
        ]).Rows);

        Assert.Equal(700m, row.Price);
    }

    [Fact]
    public void Sequence_numbers_and_payment_summary_are_calculated()
    {
        var report = _service.Create([
            CreateRecord("NAKİT 1", amount: 250m),
            CreateRecord("KART 1", paymentMethod: PaymentMethod.CreditCard, amount: 300m),
            CreateRecord("NAKİT 2", amount: 500m),
            CreateRecord("KART 2", paymentMethod: PaymentMethod.CreditCard, amount: 700m)
        ]);

        Assert.Equal(new[] { 1, 2, 3, 4 }, report.Rows.Select(row => row.SequenceNumber));
        Assert.Equal(2, report.Summary.CashCount);
        Assert.Equal(750m, report.Summary.CashTotal);
        Assert.Equal(2, report.Summary.CreditCardCount);
        Assert.Equal(1000m, report.Summary.CreditCardTotal);
        Assert.Equal(4, report.Summary.TotalCount);
        Assert.Equal(1750m, report.Summary.GrandTotal);
    }

    [Fact]
    public void Empty_day_creates_empty_rows_and_zero_summary()
    {
        var report = _service.Create([]);

        Assert.Empty(report.Rows);
        Assert.Equal(new(0, 0m, 0, 0m, 0, 0m), report.Summary);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static DailyReportRecord CreateRecord(
        string companyName,
        CompanyType companyType = CompanyType.Intercity,
        bool canDepartFromCenter = false,
        bool departedFromCenter = false,
        PaymentMethod paymentMethod = PaymentMethod.Cash,
        decimal amount = 100m) =>
        new(
            Guid.NewGuid(),
            "51 ABC 123",
            new DateOnly(2035, 5, 10),
            new TimeOnly(12, 30),
            paymentMethod,
            amount,
            departedFromCenter,
            companyName,
            companyType,
            canDepartFromCenter);
}
