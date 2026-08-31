using Microsoft.EntityFrameworkCore;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Pricing;
using NigdeTerminal.App.Services;

namespace NigdeTerminal.Tests;

public sealed class ExitRegistrationServiceTests : IDisposable
{
    private readonly NigdeTerminalDbContext _dbContext;
    private readonly ExitRegistrationService _service;

    public ExitRegistrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<NigdeTerminalDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused")
            .Options;

        _dbContext = new NigdeTerminalDbContext(options);
        _service = new ExitRegistrationService(
            new VehiclePlateService(),
            new PricingService(),
            new ExitRecordDataAccess(_dbContext));
    }

    [Fact]
    public async Task Registration_is_rejected_when_company_is_missing()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync("51 ABC 123", null, PaymentMethod.Cash, false));

        Assert.Equal("Firma seçiniz.", exception.Message);
    }

    [Fact]
    public async Task Registration_is_rejected_when_payment_method_is_missing()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync("51 ABC 123", CreateCompany(), null, false));

        Assert.Equal("Ödeme yöntemi seçiniz.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("82 ABC 123")]
    [InlineData("bozuk plaka")]
    public async Task Registration_is_rejected_when_plate_is_empty_or_invalid(string? plate)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RegisterAsync(plate, CreateCompany(), PaymentMethod.Cash, false));

        Assert.Equal("Geçerli bir plaka giriniz.", exception.Message);
    }

    [Fact]
    public async Task Center_departure_is_rejected_when_company_is_not_eligible()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RegisterAsync(
                "51 ABC 123",
                CreateCompany(canDepartFromCenter: false),
                PaymentMethod.Cash,
                departedFromCenter: true));

        Assert.Equal(
            "Seçilen firma Niğde çıkışlı kayıt desteklemiyor.",
            exception.Message);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private static Company CreateCompany(bool canDepartFromCenter = false)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = "TEST FİRMASI",
            CompanyType = CompanyType.Intercity,
            CanDepartFromCenter = canDepartFromCenter,
            IsActive = true
        };
    }
}
