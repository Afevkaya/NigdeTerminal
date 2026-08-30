namespace NigdeTerminal.App.Models;

public sealed class ExitRecord
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public required string VehiclePlate { get; set; }

    public DateTimeOffset DepartureDateTime { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public required string TariffName { get; set; }

    public decimal TariffAmount { get; set; }

    public bool DepartedFromCenter { get; set; }
}
