using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Data;

public sealed record DailyReportRecord(
    Guid Id,
    string VehiclePlate,
    DateOnly DepartureDate,
    TimeOnly DepartureTime,
    PaymentMethod PaymentMethod,
    decimal TariffAmount,
    bool DepartedFromCenter,
    string CompanyName,
    CompanyType CompanyType,
    bool CompanyCanDepartFromCenter);
