using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Pricing;

namespace NigdeTerminal.App.Services;

public sealed class ExitRegistrationService(
    VehiclePlateService vehiclePlateService,
    PricingService pricingService,
    ExitRecordDataAccess exitRecordDataAccess)
{
    public async Task<ExitRecord> RegisterAsync(
        string? vehiclePlate,
        Company? company,
        PaymentMethod? paymentMethod,
        bool departedFromCenter,
        CancellationToken cancellationToken = default)
    {
        if (!vehiclePlateService.TryNormalize(vehiclePlate, out var normalizedPlate))
        {
            throw new ArgumentException("Geçerli bir plaka giriniz.");
        }

        if (company is null)
        {
            throw new ArgumentException("Firma seçiniz.");
        }

        if (paymentMethod is null || !Enum.IsDefined(paymentMethod.Value))
        {
            throw new ArgumentException("Ödeme yöntemi seçiniz.");
        }

        if (departedFromCenter && !company.CanDepartFromCenter)
        {
            throw new InvalidOperationException(
                "Seçilen firma Niğde çıkışlı kayıt desteklemiyor.");
        }

        var systemDateTime = DateTimeOffset.Now;
        var pricing = pricingService.Calculate(company, systemDateTime, departedFromCenter);
        var exitRecord = new ExitRecord
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            VehiclePlate = normalizedPlate,
            DepartureDate = DateOnly.FromDateTime(systemDateTime.DateTime),
            DepartureTime = TimeOnly.FromDateTime(systemDateTime.DateTime),
            PaymentMethod = paymentMethod.Value,
            TariffName = pricing.TariffName,
            TariffAmount = pricing.Amount,
            DepartedFromCenter = departedFromCenter
        };

        await exitRecordDataAccess.AddAsync(exitRecord, cancellationToken);
        return exitRecord;
    }
}
