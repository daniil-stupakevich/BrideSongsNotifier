namespace BrideSongs.Notifier.App.Models;
public class SettingsConfiguration
{
    public bool ShowShortPopupForFavorites { get; set; }
    public double ShortPopupForFavoritesTextSize { get; set; }
    public string ListenAdress { get; set; }
    public string ListenPort { get; set; }
    public IEnumerable<string> MonitorsForNotification { get; set; }
    public string FavoritesFilePath { get; set; }
}
