namespace NigdeTerminal.App.Reporting;

public sealed record DailyReportSummary(
    int CashCount,
    decimal CashTotal,
    int CreditCardCount,
    decimal CreditCardTotal,
    int TotalCount,
    decimal GrandTotal);
