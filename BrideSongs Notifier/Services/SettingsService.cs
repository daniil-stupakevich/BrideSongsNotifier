using BrideSongs.Notifier.App.Models;
using BrideSongs.Notifier.App.Services.Interfaces;
using System.IO;
using System.Text.Json;

namespace BrideSongs.Notifier.App.Services;
public class SettingsService : ISettingsService
{
    private const string settingsFilename = "settings.json";

    public SettingsConfiguration GetSettings()
    {
        if (File.Exists(settingsFilename))
        {
            string jsonString = File.ReadAllText(settingsFilename);
            return JsonSerializer.Deserialize<SettingsConfiguration>(jsonString)!;
        }

        return new SettingsConfiguration();
    }

    public async Task<SettingsConfiguration> GetSettingsAsync() 
    {
        if (File.Exists(settingsFilename))
        {
            string jsonString = await File.ReadAllTextAsync(settingsFilename);
            var sett = JsonSerializer.Deserialize<SettingsConfiguration>(jsonString)!;
            if (!Directory.Exists(sett.FavoritesFilePath)) 
            {
                sett.FavoritesFilePath = Directory.GetCurrentDirectory();
            }
            return sett;
        }

        return new SettingsConfiguration();
    }

    public async Task SaveSettingsAsync<T>(string sectionName, T value)
    {
        SettingsConfiguration settings = new SettingsConfiguration();
        if(File.Exists(settingsFilename))
        {
            settings = await GetSettingsAsync();
        }

        if (sectionName == nameof(settings.MonitorsForNotification) && value is List<DisplayModel> displays) 
        {
            settings.MonitorsForNotification = displays.Where(x => x.IsNotificationVisible).Select(x => x.DisplayName);
        }

        if (sectionName == nameof(settings.ShowShortPopupForFavorites) && value is bool result)
        {
            settings.ShowShortPopupForFavorites = result;
        }

        var jsonString = JsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(settingsFilename, jsonString);
    }

    public async Task SaveSettingsAsync(SettingsConfiguration settings) 
    {
        var jsonString = JsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(settingsFilename, jsonString);
    }
}
