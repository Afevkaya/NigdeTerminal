using ClosedXML.Excel;
using NigdeTerminal.App.Reporting;

namespace NigdeTerminal.Tests;

public sealed class DailyExcelReportWriterTests
{
    [Fact]
    public void Write_creates_reopenable_workbook_with_row_and_payment_summary()
    {
        var filePath = CreateTemporaryFilePath();
        var reportDate = new DateOnly(2035, 5, 10);
        var departureTime = new TimeOnly(14, 35);
        var report = new DailyReport(
            [
                new DailyReportRow(
                    1,
                    "51 ABC 123",
                    "NİĞDE İNAN TURİZM",
                    "TALİ TEST",
                    "DERİNKUYU",
                    "EVET",
                    reportDate,
                    departureTime,
                    700m,
                    "KREDİ KARTI")
            ],
            new DailyReportSummary(2, 500m, 1, 700m, 3, 1200m));

        try
        {
            new DailyExcelReportWriter().Write(filePath, reportDate, report);

            Assert.True(File.Exists(filePath));

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet("Günlük Rapor");
                Assert.Equal("Günlük Rapor", worksheet.Name);

                var expectedHeaders = new[]
                {
                    "SIRA",
                    "PLAKA",
                    "YERLİ FİRMA",
                    "TALİ FİRMA",
                    "YAKIN MESAFE",
                    "MERKEZ ÇIKIŞ",
                    "TARİH",
                    "SAAT",
                    "FİYAT",
                    "ÖDEME"
                };
                Assert.Equal(
                    expectedHeaders,
                    worksheet.Row(5).Cells(1, 10).Select(cell => cell.GetString()));

                Assert.Equal(1, worksheet.Cell("A6").GetValue<int>());
                Assert.Equal("51 ABC 123", worksheet.Cell("B6").GetString());
                Assert.Equal("NİĞDE İNAN TURİZM", worksheet.Cell("C6").GetString());
                Assert.Equal("TALİ TEST", worksheet.Cell("D6").GetString());
                Assert.Equal("DERİNKUYU", worksheet.Cell("E6").GetString());
                Assert.Equal("EVET", worksheet.Cell("F6").GetString());
                Assert.Equal(reportDate.ToDateTime(TimeOnly.MinValue), worksheet.Cell("G6").GetDateTime());
                Assert.Equal(departureTime.ToTimeSpan(), worksheet.Cell("H6").GetTimeSpan());
                Assert.Equal(700m, worksheet.Cell("I6").GetValue<decimal>());
                Assert.Equal("KREDİ KARTI", worksheet.Cell("J6").GetString());

                Assert.Equal(XLDataType.DateTime, worksheet.Cell("G6").DataType);
                Assert.Equal(XLDataType.TimeSpan, worksheet.Cell("H6").DataType);
                Assert.Equal(XLDataType.Number, worksheet.Cell("I6").DataType);

                Assert.Equal("NAKİT", worksheet.Cell("A11").GetString());
                Assert.Equal(2, worksheet.Cell("B11").GetValue<int>());
                Assert.Equal(500m, worksheet.Cell("C11").GetValue<decimal>());
                Assert.Equal("KREDİ KARTI", worksheet.Cell("A12").GetString());
                Assert.Equal(1, worksheet.Cell("B12").GetValue<int>());
                Assert.Equal(700m, worksheet.Cell("C12").GetValue<decimal>());
                Assert.Equal("GENEL TOPLAM", worksheet.Cell("A13").GetString());
                Assert.Equal(3, worksheet.Cell("B13").GetValue<int>());
                Assert.Equal(1200m, worksheet.Cell("C13").GetValue<decimal>());
            }

            using var reopenedWorkbook = new XLWorkbook(filePath);
            Assert.Equal("Günlük Rapor", reopenedWorkbook.Worksheet(1).Name);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Write_creates_valid_workbook_and_zero_summary_for_empty_report()
    {
        var filePath = CreateTemporaryFilePath();
        var report = new DailyReport(
            [],
            new DailyReportSummary(0, 0m, 0, 0m, 0, 0m));

        try
        {
            new DailyExcelReportWriter().Write(
                filePath,
                new DateOnly(2035, 5, 11),
                report);

            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet("Günlük Rapor");
                Assert.Equal("NAKİT", worksheet.Cell("A10").GetString());
                Assert.Equal(0, worksheet.Cell("B10").GetValue<int>());
                Assert.Equal(0m, worksheet.Cell("C10").GetValue<decimal>());
                Assert.Equal("KREDİ KARTI", worksheet.Cell("A11").GetString());
                Assert.Equal(0, worksheet.Cell("B11").GetValue<int>());
                Assert.Equal(0m, worksheet.Cell("C11").GetValue<decimal>());
                Assert.Equal("GENEL TOPLAM", worksheet.Cell("A12").GetString());
                Assert.Equal(0, worksheet.Cell("B12").GetValue<int>());
                Assert.Equal(0m, worksheet.Cell("C12").GetValue<decimal>());
            }

            Assert.True(File.Exists(filePath));
        }
        finally
        {
            File.Delete(filePath);
        }

        Assert.False(File.Exists(filePath));
    }

    private static string CreateTemporaryFilePath() =>
        Path.Combine(Path.GetTempPath(), $"NigdeTerminal-{Guid.NewGuid():N}.xlsx");
}
