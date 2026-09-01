using System.Globalization;
using System.Windows;
using System.Windows.Media;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;

namespace NigdeTerminal.App;

public partial class RecordListWindow : Window
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");
    private readonly CompanyDataAccess _companyDataAccess;
    private readonly ExitRecordDataAccess _exitRecordDataAccess;

    public RecordListWindow(
        CompanyDataAccess companyDataAccess,
        ExitRecordDataAccess exitRecordDataAccess)
    {
        _companyDataAccess = companyDataAccess;
        _exitRecordDataAccess = exitRecordDataAccess;
        InitializeComponent();
        DatePicker.SelectedDate = DateTime.Today;
        Loaded += RecordListWindow_Loaded;
    }

    private async void RecordListWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= RecordListWindow_Loaded;
        await LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        SetLoadingState(true);

        try
        {
            var companies = await _companyDataAccess.GetAllAsync();
            var companyOptions = companies
                .Select(company => new CompanyFilterOption(company.Id, company.Name))
                .Prepend(CompanyFilterOption.All)
                .ToList();

            CompanyComboBox.ItemsSource = companyOptions;
            CompanyComboBox.SelectedIndex = 0;
            await LoadRecordsAsync();
        }
        catch (Exception)
        {
            ShowError("Kayıtlar yüklenemedi. Lütfen tekrar deneyin.");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void ListButton_Click(object sender, RoutedEventArgs e)
    {
        await ReloadRecordsAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ReloadRecordsAsync();
    }

    private async Task ReloadRecordsAsync()
    {
        SetLoadingState(true);

        try
        {
            await LoadRecordsAsync();
        }
        catch (Exception)
        {
            ShowError("Kayıtlar yüklenemedi. Lütfen tekrar deneyin.");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task LoadRecordsAsync()
    {
        var selectedDate = DateOnly.FromDateTime(DatePicker.SelectedDate ?? DateTime.Today);
        var companyId = (CompanyComboBox.SelectedItem as CompanyFilterOption)?.CompanyId;
        var records = await _exitRecordDataAccess.GetListAsync(
            selectedDate,
            companyId,
            VehiclePlateTextBox.Text);

        RecordsDataGrid.ItemsSource = records.Select(RecordListRow.FromDataItem).ToList();
        StatusTextBlock.Foreground = Brushes.DimGray;
        StatusTextBlock.Text = records.Count == 0
            ? "Seçilen kriterlere ait çıkış kaydı bulunamadı."
            : string.Empty;
    }

    private void SetLoadingState(bool isLoading)
    {
        ListButton.IsEnabled = !isLoading;
        RefreshButton.IsEnabled = !isLoading;

        if (isLoading)
        {
            StatusTextBlock.Foreground = Brushes.DimGray;
            StatusTextBlock.Text = "Kayıtlar yükleniyor...";
        }
    }

    private void ShowError(string message)
    {
        RecordsDataGrid.ItemsSource = null;
        StatusTextBlock.Foreground = Brushes.Firebrick;
        StatusTextBlock.Text = message;
    }

    private sealed record CompanyFilterOption(Guid? CompanyId, string Name)
    {
        public static CompanyFilterOption All { get; } = new(null, "Tüm Firmalar");
    }

    private sealed record RecordListRow(
        string DepartureTimeText,
        string CompanyName,
        string VehiclePlate,
        string PaymentMethodText,
        string TariffAmountText,
        string DepartedFromCenterText)
    {
        public static RecordListRow FromDataItem(ExitRecordListItem item) =>
            new(
                item.DepartureTime.ToString("HH:mm", TurkishCulture),
                item.CompanyName,
                item.VehiclePlate,
                item.PaymentMethod == PaymentMethod.Cash ? "Nakit" : "Kredi Kartı",
                $"{item.TariffAmount.ToString("N2", TurkishCulture)} ₺",
                item.DepartedFromCenter ? "Evet" : "Hayır");
    }
}
