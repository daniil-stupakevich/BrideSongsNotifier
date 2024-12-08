using BrideSongs.Models.Notifications;
using BrideSongs.Notifier.App.Models;
using BrideSongs.Notifier.App.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Xml;

namespace BrideSongs.Notifier.App.Services;
public class FileWatcherService : BackgroundService
{
    private FileInfo _FavoriteFile;

    private readonly INotificationServer _notificationServer;
    private readonly ISettingsService _settingsService;
    private SettingsConfiguration _settingsConfiguration;

    public FileWatcherService(INotificationServer notificationServer, ISettingsService settingsService)
    {
        _notificationServer = notificationServer;
        _settingsService = settingsService;
        _settingsConfiguration = settingsService.GetSettings();
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            if (!string.IsNullOrEmpty(_settingsConfiguration.FavoritesFilePath))
            {
                var files = Directory.GetFiles(_settingsConfiguration.FavoritesFilePath, "*.xml", SearchOption.TopDirectoryOnly);

                if (files.Any())
                {
                    SendNotification(files);
                }
            }

            await Task.Delay(1000);
        }
    }

    private void SendNotification(string[] files)
    {
        var receivedFile = new FileInfo(files.FirstOrDefault()!);

        if (_FavoriteFile is null || _FavoriteFile.LastWriteTimeUtc != receivedFile.LastWriteTimeUtc)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(files.FirstOrDefault()!);

            XmlElement? xRoot = xDoc.DocumentElement;
            if (xRoot != null)
            {
                var songs = new List<SongNotificationModel>();
                foreach (XmlElement xnode in xRoot.ChildNodes)
                {
                    var song = new SongNotificationModel();
                    var text = xnode.InnerText.Split("|");
                    song.SongbookTitle = text[0];

                    var splittedArray = text[1].ToUpper().Split(" ").ToList();
                    song.Title = splittedArray.Count > 1 ? string.Join(' ', text[1].ToUpper().Split(" ").ToList().Skip(2)) : text[1];
                    songs.Add(song);
                }

                _FavoriteFile = receivedFile;
                _notificationServer.DisplayManySongsReceivedPopup(songs);
            }
            _FavoriteFile = receivedFile;
        }
        else
        {
            _FavoriteFile = receivedFile;
        }
    }

    public void CheckFavoritesFile()
    {
        var settings = _settingsService.GetSettings();
        _settingsConfiguration = settings;

        if (!string.IsNullOrEmpty(_settingsConfiguration.FavoritesFilePath))
        {
            var files = Directory.GetFiles(_settingsConfiguration.FavoritesFilePath, "*.xml", SearchOption.TopDirectoryOnly);
            if (files.Any())
            {
                SendNotification(files);
            }
        }
    }
}

