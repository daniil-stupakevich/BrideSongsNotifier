using BrideSongs.Notifier.App.Models;

namespace BrideSongs.Notifier.App.Services.Interfaces;
public interface ISettingsService
{
    Task<SettingsConfiguration> GetSettingsAsync();
    SettingsConfiguration GetSettings();
    Task SaveSettingsAsync<T>(string sectionName, T value);
    Task SaveSettingsAsync(SettingsConfiguration settings);
}
