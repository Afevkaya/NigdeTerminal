namespace NigdeTerminal.App.Reporting;

public sealed record DailyReportRow(
    int SequenceNumber,
    string Plate,
    string LocalCompany,
    string SecondaryCompany,
    string NearDistanceCompany,
    string CenterDeparture,
    DateOnly Date,
    TimeOnly Time,
    decimal Price,
    string PaymentMethod);
