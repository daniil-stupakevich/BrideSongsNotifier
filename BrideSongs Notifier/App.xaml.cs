using BrideSongs.Notifier.App.Services;
using BrideSongs.Notifier.App.Services.Interfaces;
using BrideSongs.Notifier.App.ViewModels;
using BrideSongs.Notifier.App.Views;
using BrideSongs.Notifier.App.Views.UserControls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BrideSongs.Notifier.App;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    private static IServiceProvider _serviceProvider;
    public static TService GetService<TService>()
            => _serviceProvider.GetService<TService>();


    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public App()
    {
        if (Process.GetProcessesByName("BrideSongs.Notifier.App").Length > 1)
        {
            return;
        }

        ILogger log = null;

        string[] args = Environment.GetCommandLineArgs();

        if (args.Any(x => x == "-addLogs")) 
        {
             log = new LoggerConfiguration()
               .Enrich.FromLogContext()
               .WriteTo.File(path: Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, $"Logs.txt"), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
               .CreateLogger();
        }

       

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((services) =>
            {
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<OneSongPopup>();
                services.AddSingleton<ManySongsPopup>();
                services.AddSingleton<FileWatcherService>();

                services.AddHostedService<FileWatcherService>();

                if (args.Any(x => x == "-addLogs"))
                {
                    services.AddLogging(l => l.AddSerilog(log));
                }

                try
                {
                    services.AddSingleton<INotificationServer, NotificationServer>();
                    services.AddSingleton<IDisplayService, DisplayService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }

            })
        .Build();

        _serviceProvider = _host.Services;
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        _host.Start();

        try 
        {
            MainWindow = _host.Services.GetRequiredService<MainWindow>();
            MainWindow.Show();
            base.OnStartup(e);
        }
        catch(Exception ex) 
        {
            System.Windows.MessageBox.Show(ex.Message);
        } 
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
