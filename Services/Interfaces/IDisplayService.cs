using BrideSongs.Notifier.App.Models;

namespace BrideSongs.Notifier.App.Services.Interfaces;
public interface IDisplayService
{
    Task<List<DisplayModel>> GetDisplaysAsync();
    List<string> SelectedDisplays { get; set; }
}
