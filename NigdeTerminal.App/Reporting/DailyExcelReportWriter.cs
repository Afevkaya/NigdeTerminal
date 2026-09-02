using ClosedXML.Excel;

namespace NigdeTerminal.App.Reporting;

public sealed class DailyExcelReportWriter
{
    public void Write(string filePath, DateOnly reportDate, DailyReport report)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Günlük Rapor");

        worksheet.Cell(1, 1).Value = "NİĞDE ŞEHİRLERARASI OTOBÜS TERMİNALİ";
        worksheet.Cell(2, 1).Value = "GÜNLÜK ÇIKIŞ RAPORU";
        worksheet.Cell(3, 1).Value = reportDate.ToDateTime(TimeOnly.MinValue);
        worksheet.Cell(3, 1).Style.DateFormat.Format = "dd.MM.yyyy";

        foreach (var rowNumber in new[] { 1, 2, 3 })
        {
            var titleRange = worksheet.Range(rowNumber, 1, rowNumber, 10);
            titleRange.Merge();
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        worksheet.Range(1, 1, 2, 10).Style.Font.Bold = true;
        worksheet.Row(1).Height = 24;
        worksheet.Row(2).Height = 20;

        var columnHeaders = new[]
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

        for (var columnNumber = 1; columnNumber <= columnHeaders.Length; columnNumber++)
        {
            worksheet.Cell(5, columnNumber).Value = columnHeaders[columnNumber - 1];
        }

        var headerRange = worksheet.Range(5, 1, 5, columnHeaders.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        const int firstDataRow = 6;
        for (var rowIndex = 0; rowIndex < report.Rows.Count; rowIndex++)
        {
            var reportRow = report.Rows[rowIndex];
            var excelRow = firstDataRow + rowIndex;

            worksheet.Cell(excelRow, 1).Value = reportRow.SequenceNumber;
            worksheet.Cell(excelRow, 2).Value = reportRow.Plate;
            worksheet.Cell(excelRow, 3).Value = reportRow.LocalCompany;
            worksheet.Cell(excelRow, 4).Value = reportRow.SecondaryCompany;
            worksheet.Cell(excelRow, 5).Value = reportRow.NearDistanceCompany;
            worksheet.Cell(excelRow, 6).Value = reportRow.CenterDeparture;
            worksheet.Cell(excelRow, 7).Value = reportRow.Date.ToDateTime(TimeOnly.MinValue);
            worksheet.Cell(excelRow, 8).Value = reportRow.Time.ToTimeSpan();
            worksheet.Cell(excelRow, 9).Value = reportRow.Price;
            worksheet.Cell(excelRow, 10).Value = reportRow.PaymentMethod;

            worksheet.Cell(excelRow, 7).Style.DateFormat.Format = "dd.MM.yyyy";
            worksheet.Cell(excelRow, 8).Style.DateFormat.Format = "HH:mm";
            worksheet.Cell(excelRow, 9).Style.NumberFormat.Format = "#,##0.00 \"TL\"";
        }

        if (report.Rows.Count > 0)
        {
            var lastDataRow = firstDataRow + report.Rows.Count - 1;
            var dataRange = worksheet.Range(firstDataRow, 1, lastDataRow, 10);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range(firstDataRow, 1, lastDataRow, 1)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(firstDataRow, 6, lastDataRow, 6)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range(firstDataRow, 8, lastDataRow, 8)
                .Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var lastMainTableRow = report.Rows.Count == 0
            ? 5
            : firstDataRow + report.Rows.Count - 1;
        var summaryTitleRow = lastMainTableRow + 3;
        var summaryHeaderRow = summaryTitleRow + 1;
        var cashRow = summaryHeaderRow + 1;
        var creditCardRow = cashRow + 1;
        var grandTotalRow = creditCardRow + 1;

        var summaryTitleRange = worksheet.Range(summaryTitleRow, 1, summaryTitleRow, 3);
        summaryTitleRange.Merge();
        summaryTitleRange.Value = "ÖDEME ÖZETİ";
        summaryTitleRange.Style.Font.Bold = true;
        summaryTitleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Cell(summaryHeaderRow, 1).Value = "ÖDEME TÜRÜ";
        worksheet.Cell(summaryHeaderRow, 2).Value = "İŞLEM ADEDİ";
        worksheet.Cell(summaryHeaderRow, 3).Value = "TOPLAM";
        worksheet.Range(summaryHeaderRow, 1, summaryHeaderRow, 3).Style.Font.Bold = true;

        worksheet.Cell(cashRow, 1).Value = "NAKİT";
        worksheet.Cell(cashRow, 2).Value = report.Summary.CashCount;
        worksheet.Cell(cashRow, 3).Value = report.Summary.CashTotal;

        worksheet.Cell(creditCardRow, 1).Value = "KREDİ KARTI";
        worksheet.Cell(creditCardRow, 2).Value = report.Summary.CreditCardCount;
        worksheet.Cell(creditCardRow, 3).Value = report.Summary.CreditCardTotal;

        worksheet.Cell(grandTotalRow, 1).Value = "GENEL TOPLAM";
        worksheet.Cell(grandTotalRow, 2).Value = report.Summary.TotalCount;
        worksheet.Cell(grandTotalRow, 3).Value = report.Summary.GrandTotal;
        worksheet.Range(grandTotalRow, 1, grandTotalRow, 3).Style.Font.Bold = true;

        var summaryRange = worksheet.Range(summaryTitleRow, 1, grandTotalRow, 3);
        summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(cashRow, 3, grandTotalRow, 3)
            .Style.NumberFormat.Format = "#,##0.00 \"TL\"";

        worksheet.Column(1).Width = 18;
        worksheet.Column(2).Width = 15;
        worksheet.Column(3).Width = 28;
        worksheet.Column(4).Width = 24;
        worksheet.Column(5).Width = 20;
        worksheet.Column(6).Width = 18;
        worksheet.Column(7).Width = 13;
        worksheet.Column(8).Width = 10;
        worksheet.Column(9).Width = 12;
        worksheet.Column(10).Width = 18;

        workbook.SaveAs(filePath);
    }
}
