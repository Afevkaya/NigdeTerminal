using System.Configuration;
using System.Windows;
using NigdeTerminal.App.Data;
using NigdeTerminal.App.Pricing;
using NigdeTerminal.App.Services;

namespace NigdeTerminal.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private NigdeTerminalDbContext? _dbContext;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _dbContext = DatabaseBootstrap.CreateDbContext();
        var companyDataAccess = new CompanyDataAccess(_dbContext);
        var pricingService = new PricingService();
        var vehiclePlateService = new VehiclePlateService();
        var exitRecordDataAccess = new ExitRecordDataAccess(_dbContext);
        var exitRegistrationService = new ExitRegistrationService(
            vehiclePlateService,
            pricingService,
            exitRecordDataAccess);
        var mainWindow = new MainWindow(
            companyDataAccess,
            pricingService,
            vehiclePlateService,
            exitRegistrationService);
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _dbContext?.Dispose();
        base.OnExit(e);
    }
}

