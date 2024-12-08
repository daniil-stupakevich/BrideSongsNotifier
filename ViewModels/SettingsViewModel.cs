using BrideSongs.Notifier.App.Models;
using BrideSongs.Notifier.App.Services;
using BrideSongs.Notifier.App.Services.Interfaces;
using BrideSongs.Notifier.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace BrideSongs.Notifier.App.ViewModels;
public partial class SettingsViewModel : ObservableObject
{
    private readonly IDisplayService _displayService;
    private readonly ISettingsService _settingsService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private bool _isShortFavoritesListShown;

    [ObservableProperty]
    private double _shortPopupForFavoritesTextSize;

    [ObservableProperty]
    private string _FavoritesPath;

    [ObservableProperty]
    private string _listenerAdress;

    [ObservableProperty]
    private string _listenerPort;

    public ObservableCollection<DisplayModel> Displays { get; set; }

    public SettingsViewModel()
    {
        _displayService = App.GetService<IDisplayService>();
        _settingsService = App.GetService<ISettingsService>();
        _mainViewModel = App.GetService<MainViewModel>();
    }

    [RelayCommand]
    public async Task SetFavoritesPath() 
    {
        OpenFolderDialog openFolderDialog = new OpenFolderDialog();
        var result = openFolderDialog.ShowDialog();

        if(result.HasValue && result.Value) 
        {
           var settings = await _settingsService.GetSettingsAsync();
           settings.FavoritesFilePath = openFolderDialog.FolderName;
           await _settingsService.SaveSettingsAsync(settings);
           FavoritesPath = openFolderDialog.FolderName;
        }
    
    }

    [RelayCommand]
    public async Task SettingsLoaded()
    {
        var settings = await _settingsService.GetSettingsAsync();
        IsShortFavoritesListShown = settings.ShowShortPopupForFavorites;
        ShortPopupForFavoritesTextSize = settings.ShortPopupForFavoritesTextSize == 0 ? 16 : settings.ShortPopupForFavoritesTextSize;
        FavoritesPath = !string.IsNullOrEmpty(settings.FavoritesFilePath) ? settings.FavoritesFilePath : "...";
        ListenerAdress = settings.ListenAdress;
        ListenerPort = settings.ListenPort;

        var dislpays = await _displayService.GetDisplaysAsync();
        Displays = new ObservableCollection<DisplayModel>(dislpays);
        OnPropertyChanged(nameof(Displays));
    }

    [RelayCommand]
    public async Task SaveSettings() 
    {
        try
        {
            if (Displays != null) 
            {
                SettingsConfiguration settings = new SettingsConfiguration
                {
                    MonitorsForNotification = Displays.Where(x => x.IsNotificationVisible).Select(x => x.DisplayName),
                    ShowShortPopupForFavorites = IsShortFavoritesListShown,
                    ShortPopupForFavoritesTextSize = ShortPopupForFavoritesTextSize,
                    FavoritesFilePath = FavoritesPath,
                    ListenAdress = ListenerAdress,
                    ListenPort = ListenerPort
                };

                await _settingsService.SaveSettingsAsync(settings);
            }           
        }
        catch (Exception ex) 
        {
            System.Windows.MessageBox.Show(ex.ToString());
        }       
    }

    [RelayCommand]
    public async Task ShowTestScreen(string displayName)
    {
        try
        {
            if (Displays != null)
            {
                var display = Displays.First(x => x.DisplayName.Contains(displayName)).WorkingArea;
                TestWindow testWindow = new TestWindow();
                testWindow.Left = display.Right - testWindow.Width;
                testWindow.Top = display.Bottom - testWindow.Height;
                testWindow.UpdateLayout();

                testWindow.Visibility = Visibility.Visible;
                testWindow.WindowState = WindowState.Normal;
                testWindow.lbl_DisplayName.Content = displayName;
                testWindow.Show();
                await Task.Delay(1500);
                testWindow.Close();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.ToString());
        }
    }

    [RelayCommand]
    public void IncreseShortFavoritesTextSize()
    {
        ShortPopupForFavoritesTextSize++;
        _mainViewModel.ChangeShortFavoriteTextSize(ShortPopupForFavoritesTextSize);
    }

    [RelayCommand]
    public void DecreaseShortFavoritesTextSize()
    {
        ShortPopupForFavoritesTextSize--;
        _mainViewModel.ChangeShortFavoriteTextSize(ShortPopupForFavoritesTextSize);
    }

    [RelayCommand]
    public async Task SelectionOfDisplaysChanged(DisplayModel args)
    {
        if (args.IsNotificationVisible)
        {
            _displayService.SelectedDisplays.Add(args.DisplayName);
        }
        else 
        {
            _displayService.SelectedDisplays.RemoveAll(x => string.Equals(x, args.DisplayName, comparisonType: StringComparison.OrdinalIgnoreCase));
        }
        
        await SaveSettings();
    }
}
