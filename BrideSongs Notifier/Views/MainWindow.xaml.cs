using BrideSongs.Notifier.App.Services.Interfaces;
using BrideSongs.Notifier.App.ViewModels;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace BrideSongs.Notifier.App.Views;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel _vm;
    private NotifyIcon nIcon = new NotifyIcon();

    public MainWindow(MainViewModel vm)
    {
        try 
        {
            InitializeComponent();
            _vm = vm;

            nIcon.Icon = new Icon(@"ic_launcher_round.ico");
            nIcon.Visible = true;
            nIcon.MouseClick += NotificationIconClick;
            nIcon.ContextMenuStrip = new ContextMenuStrip();
            //nIcon.ContextMenuStrip.Items.Add("Check Favorites", null, CheckFavoritesCommand);
            nIcon.ContextMenuStrip.Items.Add("Settings", null, ShowSettingsCommand);
            nIcon.ContextMenuStrip.Items.Add("Exit", null, ExitCommand);

            DataContext = vm;
            vm.Initialize(this).ConfigureAwait(false);
        }

        catch (Exception ex) 
        {
            System.Windows.MessageBox.Show(ex.Message);
        }
    }

    private async void NotificationIconClick(object? sender, MouseEventArgs e)
    {
        if(e.Button == MouseButtons.Left && _vm.ActivePopup != Models.PopupType.Loading) 
        {
            await _vm.ShowWindowCommandAsync();
        }
    }

    private void ShowSettingsCommand(object? sender, EventArgs e)
    {
        _vm.ShowSettings();
    }

    private void CheckFavoritesCommand(object? sender, EventArgs e)
    {
        _vm.CheckFavorites();
    }

    private void ExitCommand(object? sender, EventArgs e)
    {
        _vm.ExitMenu().ConfigureAwait(false);
    }
}