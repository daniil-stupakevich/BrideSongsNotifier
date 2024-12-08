using BrideSongs.Models.Notifications;
using BrideSongs.Models.Notifications;
using BrideSongs.Notifier.App.Models;
using BrideSongs.Notifier.App.Services;
using BrideSongs.Notifier.App.Services.Interfaces;
using BrideSongs.Notifier.App.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Application = System.Windows.Application;

namespace BrideSongs.Notifier.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private MainWindow _mainWindow;
        private readonly INotificationServer _notificationListener;
        private readonly FileWatcherService _fileWatcherService;
        private readonly IDisplayService _displayService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<MainViewModel> _logger;
        private List<DuplicateWindow> _duplicateWindows;

        public MainViewModel(INotificationServer listener, IDisplayService displayService, ILogger<MainViewModel> logger, ISettingsService settingsService, FileWatcherService fileWatcherService)
        {
            _logger = logger;
            _notificationListener = listener;
            _displayService = displayService;
            _settingsService = settingsService;
            _fileWatcherService = fileWatcherService;
            
            _notificationListener.OneSongRecieved += _notificationListener_OneSongRecieved;
            _notificationListener.ManySongsRecieved += _notificationListener_ManySongsRecieved;
            _notificationListener.ErrorHappend += _notificationListener_ErrorHappend;

            _duplicateWindows = new List<DuplicateWindow>();

            for (int i = 0; i < 5; i++)
            {
                _duplicateWindows.Add(new DuplicateWindow());
                _duplicateWindows[i].DataContext = this;
            } 
        }

        public async Task Initialize(Window window)
        {
            _mainWindow = (window as MainWindow)!;
            var settings = _settingsService.GetSettings();
            await Task.Delay(2000);
            _mainWindow.Hide();
            _duplicateWindows.ForEach(x => x.Show());
            _duplicateWindows.ForEach(x => x.Hide());

            ActivePopup = PopupType.OneSong;
            try
            {
                await _notificationListener.StartServer(settings.ListenAdress, settings.ListenPort);
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show(ex.Message);
                _logger?.LogError(ex, ex.Message);
            }
        }

        #region Properties
        public ObservableCollection<SongNotificationModel> Songs { get; set; } = new();

        [ObservableProperty]
        private SongNotificationModel _notificationData = new SongNotificationModel();

        [ObservableProperty]
        private string _icoPath = @"pack://application:,,,/BrideSongs.Notifier.App;component/Resources/ic_launcher_round.ico";

        [ObservableProperty]
        private PopupType _activePopup = PopupType.Loading;

        [ObservableProperty]
        private PopupType _activePopupForDuplicates = PopupType.Loading;

        [ObservableProperty]
        private double _shortPopupForFavoritesTextSize;
        #endregion

        #region UI Commads
        public void ShowSettings()
        {
            SettingsWindow v = new SettingsWindow();
            v.DataContext = App.GetService<SettingsViewModel>();
            v.ShowDialog();
        }

        public void CheckFavorites() 
        {
            _fileWatcherService.CheckFavoritesFile();
        }

        [RelayCommand]
        public void AppLoaded()
        {
            _logger?.LogInformation("ApplicationStarted");
            _mainWindow.Hide();
        }

        [RelayCommand]
        public void SetWindowCordinates()
        {
            SetWindowPosition(_mainWindow);
        }

        [RelayCommand]
        public async Task ExitMenu()
        {
            await _notificationListener.StopServer();
            Application.Current.Shutdown();
        }

        [RelayCommand]
        public async Task DragWindowAsync(MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ChangedButton == MouseButton.Left && eventArgs.ButtonState == MouseButtonState.Pressed)
            {
                _mainWindow.DragMove();
            }
        }

        [RelayCommand]
        public void CopyToClipboard(object obj)
        {
            if (obj is string text)
            {
                System.Windows.Clipboard.SetText(text);
                return;
            }
        }

        [RelayCommand]
        public void ClosePopup()
        {
            _mainWindow.Hide();
            _duplicateWindows.ForEach(x => x.Hide());
        }

        [RelayCommand]
        public void WindowDeactivated()
        {
            _mainWindow.Topmost = false;
            _mainWindow.Topmost = true;

            _duplicateWindows.ForEach(x => x.Topmost = false);
            _duplicateWindows.ForEach(x => x.Topmost = true);
        }

        public async Task ShowWindowCommandAsync()
        {
            SetWindowCordinates();
            _mainWindow.Visibility = Visibility.Visible;
            _mainWindow.WindowState = WindowState.Normal;

            await ShowDuplicateWindowsAsync();
        }

        public void ChangeShortFavoriteTextSize(double size)
        {
            ShortPopupForFavoritesTextSize = size;

            if (Songs.Count == 0) 
            {
                for (int i = 0; i <= 7; i++) 
                {
                    var song = new SongNotificationModel
                    {
                        KeyChord = "E",
                        Number = Random.Shared.Next(0, 777).ToString(),
                        Title = $"Test Song Caption {i}"
                    };
                    Songs.Add(song);
                }
            }

            ActivePopup = PopupType.ShortManySongs;
            ActivePopupForDuplicates = PopupType.ShortManySongs;
            OnPropertyChanged(nameof(Songs));
            _mainWindow.Visibility = Visibility.Visible;
            _mainWindow.WindowState = WindowState.Normal;
            SetWindowPosition(_mainWindow);
            ShowDuplicateWindowsAsync().ConfigureAwait(false);
        }
        #endregion

        #region Events
        private void _notificationListener_ErrorHappend(Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _logger?.LogError(ex, ex.Message);
        }

        private void _notificationListener_ManySongsRecieved(IEnumerable<SongNotificationModel> obj)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var settings = _settingsService.GetSettings();
                ShortPopupForFavoritesTextSize = (settings.ShortPopupForFavoritesTextSize == 0) ? 18 : settings.ShortPopupForFavoritesTextSize;
                ActivePopup = settings.ShowShortPopupForFavorites ? PopupType.ShortManySongs : PopupType.ManySongs;
                Songs = new ObservableCollection<SongNotificationModel>(obj);
                OnPropertyChanged(nameof(Songs));
                _mainWindow.Visibility = Visibility.Visible;
                _mainWindow.WindowState = WindowState.Normal;
                SetWindowPosition(_mainWindow);

                ShowDuplicateWindowsAsync().ConfigureAwait(false);
            }));
        }

        private void _notificationListener_OneSongRecieved(SongNotificationModel notification)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ActivePopup = PopupType.OneSong;
                NotificationData = notification;
                _mainWindow.Visibility = Visibility.Visible;
                _mainWindow.WindowState = WindowState.Normal;
                SetWindowPosition(_mainWindow);

                ShowDuplicateWindowsAsync().ConfigureAwait(false);
            }));
        }
        #endregion

        #region Helpers
        private void SetWindowPosition(Window window)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            for (int i = 0; i < 2; i++)
            {
                window.Left = desktopWorkingArea.Right - window.Width;
                window.Top = desktopWorkingArea.Bottom - window.Height;
                window.UpdateLayout();
            }
        }

        private async Task ShowDuplicateWindowsAsync()
        {
            var screens = await _displayService.GetDisplaysAsync();

            for (int i = 0; i < screens.Count; i++)
            {
                if (screens[i].IsPrimary)
                {    
                    continue;
                }

                if (!screens[i].IsNotificationVisible) 
                {
                    _duplicateWindows[i].Hide();
                    continue;
                }

                var desktopWorkingArea = screens[i].WorkingArea;

                ActivePopupForDuplicates = ActivePopup == PopupType.OneSong ? PopupType.OneSong : PopupType.ShortManySongs;

                for (int j = 0; j < 2; j++)
                {
                    _duplicateWindows[i].Left = desktopWorkingArea.Right - _duplicateWindows[i].Width;
                    _duplicateWindows[i].Top = desktopWorkingArea.Bottom - _duplicateWindows[i].Height;
                    _duplicateWindows[i].UpdateLayout();
                }

                _duplicateWindows[i].Visibility = Visibility.Visible;
                _mainWindow.WindowState = WindowState.Normal;

            }
        }
        #endregion
    }
}
