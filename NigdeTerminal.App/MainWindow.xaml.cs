using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Models;
using NigdeTerminal.App.Pricing;
using NigdeTerminal.App.Services;

namespace NigdeTerminal.App;

public partial class MainWindow : Window
{
    private readonly CompanyDataAccess _companyDataAccess;
    private readonly PricingService _pricingService;
    private readonly VehiclePlateService _vehiclePlateService;
    private readonly ExitRegistrationService _exitRegistrationService;
    private readonly ExitRecordDataAccess _exitRecordDataAccess;
    private ICollectionView? _companyView;
    private RecordListWindow? _recordListWindow;

    public MainWindow(
        CompanyDataAccess companyDataAccess,
        PricingService pricingService,
        VehiclePlateService vehiclePlateService,
        ExitRegistrationService exitRegistrationService,
        ExitRecordDataAccess exitRecordDataAccess)
    {
        _companyDataAccess = companyDataAccess;
        _pricingService = pricingService;
        _vehiclePlateService = vehiclePlateService;
        _exitRegistrationService = exitRegistrationService;
        _exitRecordDataAccess = exitRecordDataAccess;
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void VehiclePlateTextBox_LostKeyboardFocus(
        object sender,
        KeyboardFocusChangedEventArgs e)
    {
        if (_vehiclePlateService.TryNormalize(
                VehiclePlateTextBox.Text,
                out var normalizedPlate))
        {
            VehiclePlateTextBox.Text = normalizedPlate;
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;

        try
        {
            var companies = await _companyDataAccess.GetAllAsync();
            _companyView = CollectionViewSource.GetDefaultView(companies);
            _companyView.Filter = FilterCompany;
            CompanyComboBox.ItemsSource = _companyView;
        }
        catch (Exception)
        {
            StatusTextBlock.Text = "Firmalar yüklenemedi. Lütfen tekrar deneyin.";
            StatusTextBlock.Foreground = Brushes.Firebrick;
        }
    }

    private bool FilterCompany(object item)
    {
        if (item is not Company company)
        {
            return false;
        }

        var searchText = CompanyComboBox.Text.Trim();
        return searchText.Length == 0
            || company.Name.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
    }

    private void CompanyComboBox_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Up or Key.Down or Key.Enter or Key.Escape or Key.Tab)
        {
            return;
        }

        var searchText = CompanyComboBox.Text;

        if (CompanyComboBox.SelectedItem is Company selectedCompany
            && !string.Equals(
                selectedCompany.Name,
                searchText,
                StringComparison.CurrentCulture))
        {
            CompanyComboBox.SelectedItem = null;
            CompanyComboBox.Text = searchText;
        }

        _companyView?.Refresh();
        CompanyComboBox.IsDropDownOpen = _companyView is not null;
    }

    private void CompanyComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (CompanyComboBox.SelectedItem is not Company)
        {
            _companyView?.MoveCurrentToFirst();
            CompanyComboBox.SelectedItem = _companyView?.CurrentItem;
        }

        if (CompanyComboBox.SelectedItem is Company)
        {
            CompanyComboBox.IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F2 when SaveButton.IsEnabled:
                SaveButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
                break;
            case Key.F3:
                CashRadioButton.IsChecked = true;
                e.Handled = true;
                break;
            case Key.F4:
                CreditCardRadioButton.IsChecked = true;
                e.Handled = true;
                break;
            case Key.Enter when !CompanyComboBox.IsKeyboardFocusWithin:
                e.Handled = true;
                break;
        }
    }

    private void CompanyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        DepartedFromCenterCheckBox.IsChecked = false;
        DepartedFromCenterCheckBox.Visibility =
            CompanyComboBox.SelectedItem is Company { CanDepartFromCenter: true }
                ? Visibility.Visible
                : Visibility.Collapsed;

        UpdatePricingPreview();
    }

    private void DepartedFromCenterCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        UpdatePricingPreview();
    }

    private void UpdatePricingPreview()
    {
        if (CompanyComboBox.SelectedItem is not Company company)
        {
            TariffTextBox.Clear();
            AmountTextBox.Clear();
            return;
        }

        var pricing = _pricingService.Calculate(
            company,
            DateTimeOffset.Now,
            DepartedFromCenterCheckBox.IsChecked == true);

        TariffTextBox.Text = pricing.TariffName;
        AmountTextBox.Text = $"{pricing.Amount:N0} TL";
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        PaymentMethod? paymentMethod = CashRadioButton.IsChecked == true
            ? PaymentMethod.Cash
            : CreditCardRadioButton.IsChecked == true
                ? PaymentMethod.CreditCard
                : null;

        SaveButton.IsEnabled = false;

        try
        {
            var record = await _exitRegistrationService.RegisterAsync(
                VehiclePlateTextBox.Text,
                CompanyComboBox.SelectedItem as Company,
                paymentMethod,
                DepartedFromCenterCheckBox.IsChecked == true);

            ResetForm();
            StatusTextBlock.Foreground = Brushes.ForestGreen;
            StatusTextBlock.Text = $"Kayıt başarıyla oluşturuldu — {record.TariffAmount:N0} TL";
        }
        catch (ArgumentException exception)
        {
            ShowValidationError(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            ShowValidationError(exception.Message);
        }
        catch (Exception)
        {
            StatusTextBlock.Foreground = Brushes.Firebrick;
            StatusTextBlock.Text = "Kayıt oluşturulamadı. Lütfen tekrar deneyin.";
        }
        finally
        {
            SaveButton.IsEnabled = true;
        }
    }

    private void ShowRecordsButton_Click(object sender, RoutedEventArgs e)
    {
        if (_recordListWindow is not null)
        {
            if (_recordListWindow.WindowState == WindowState.Minimized)
            {
                _recordListWindow.WindowState = WindowState.Normal;
            }

            _recordListWindow.Activate();
            return;
        }

        _recordListWindow = new RecordListWindow(_companyDataAccess, _exitRecordDataAccess)
        {
            Owner = this
        };
        _recordListWindow.Closed += (_, _) => _recordListWindow = null;
        _recordListWindow.Show();
    }

    private void ResetForm()
    {
        VehiclePlateTextBox.Clear();
        CompanyComboBox.SelectedItem = null;
        CompanyComboBox.Text = string.Empty;
        _companyView?.Refresh();
        DepartedFromCenterCheckBox.IsChecked = false;
        CashRadioButton.IsChecked = false;
        CreditCardRadioButton.IsChecked = false;
        TariffTextBox.Clear();
        AmountTextBox.Clear();
        VehiclePlateTextBox.Focus();
    }

    private void ShowValidationError(string message)
    {
        StatusTextBlock.Foreground = Brushes.Firebrick;
        StatusTextBlock.Text = message;
    }
}
