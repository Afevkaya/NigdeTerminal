using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Reporting;

namespace NigdeTerminal.App.Services;

public sealed class DailyReportService(ExitRecordDataAccess exitRecordDataAccess)
{
    public async Task<DailyReport> GetAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var records = await exitRecordDataAccess.GetDailyReportRecordsAsync(
            date,
            cancellationToken);

        return Create(records);
    }

    public DailyReport Create(IEnumerable<DailyReportRecord> records)
    {
        var sourceRecords = records.ToList();
        var rows = sourceRecords
            .Select((record, index) => CreateRow(record, index + 1))
            .ToList();

        var cashRecords = sourceRecords
            .Where(record => record.PaymentMethod == PaymentMethod.Cash)
            .ToList();
        var creditCardRecords = sourceRecords
            .Where(record => record.PaymentMethod == PaymentMethod.CreditCard)
            .ToList();

        var summary = new DailyReportSummary(
            cashRecords.Count,
            cashRecords.Sum(record => record.TariffAmount),
            creditCardRecords.Count,
            creditCardRecords.Sum(record => record.TariffAmount),
            sourceRecords.Count,
            sourceRecords.Sum(record => record.TariffAmount));

        return new DailyReport(rows, summary);
    }

    private static DailyReportRow CreateRow(DailyReportRecord record, int sequenceNumber)
    {
        var isNearDistance = record.CompanyType == CompanyType.LocalMinibus;
        var isLocalCompany = record.CompanyType == CompanyType.Intercity
            && record.CompanyCanDepartFromCenter;

        return new DailyReportRow(
            sequenceNumber,
            record.VehiclePlate,
            isLocalCompany ? record.CompanyName : string.Empty,
            !isNearDistance && !isLocalCompany ? record.CompanyName : string.Empty,
            isNearDistance ? record.CompanyName : string.Empty,
            record.DepartedFromCenter ? "EVET" : string.Empty,
            record.DepartureDate,
            record.DepartureTime,
            record.TariffAmount,
            record.PaymentMethod == PaymentMethod.Cash ? "NAKİT" : "KREDİ KARTI");
    }
}
