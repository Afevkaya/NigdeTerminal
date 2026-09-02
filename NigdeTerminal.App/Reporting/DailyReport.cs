namespace NigdeTerminal.App.Reporting;

public sealed record DailyReport(
    IReadOnlyList<DailyReportRow> Rows,
    DailyReportSummary Summary);
