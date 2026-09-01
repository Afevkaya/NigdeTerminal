using NigdeTerminal.App.Models;

namespace NigdeTerminal.App.Data;

public sealed record ExitRecordListItem(
    Guid Id,
    TimeOnly DepartureTime,
    string CompanyName,
    string VehiclePlate,
    PaymentMethod PaymentMethod,
    decimal TariffAmount,
    bool DepartedFromCenter);
