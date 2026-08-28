using System.Configuration;
using System.Windows;
using NigdeTerminal.App.Data;

namespace NigdeTerminal.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private NigdeTerminalDbContext? _dbContext;

    protected override void OnStartup(StartupEventArgs e)
    {
        _dbContext = DatabaseBootstrap.CreateDbContext();
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _dbContext?.Dispose();
        base.OnExit(e);
    }
}

