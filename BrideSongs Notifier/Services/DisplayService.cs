using BrideSongs.Notifier.App.Models;
using BrideSongs.Notifier.App.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BrideSongs.Notifier.App.Services;
public class DisplayService : IDisplayService
{
    private readonly ISettingsService _configuration;
    private readonly ILogger<DisplayService> _logger;

    public DisplayService(ISettingsService configuration, ILogger<DisplayService> logger)
    { 
        _logger = logger;
        _configuration = configuration;
    }

    public List<string> SelectedDisplays { get; set; } = new();

    public async Task <List<DisplayModel>> GetDisplaysAsync()
    {
        var result = new List<DisplayModel>();
        try 
        {
            _logger?.LogInformation($"Founded {Screen.AllScreens.Count(x => !x.Primary)} additional displays");

            foreach (var screen in Screen.AllScreens.Where(x => !x.Primary).OrderByDescending(x => x.Bounds.Right))
            {
                result.Add(new DisplayModel
                {
                    DisplayName = screen.DeviceName,
                    IsPrimary = screen.Primary,
                    IsNotificationVisible = await IsDisplayReceivesNotification(screen.DeviceName),
                    WorkingArea = screen.WorkingArea
                });
            }
        }
        catch (Exception ex) 
        {
            _logger?.LogError(ex.Message);
        }
       
        return result;
    }

    private async Task<bool> IsDisplayReceivesNotification(string displayName) 
    {
        var savedDisplays = await _configuration.GetSettingsAsync();
        SelectedDisplays.AddRange(savedDisplays.MonitorsForNotification);
        return SelectedDisplays.Any(x => string.Equals(x, displayName, comparisonType: StringComparison.OrdinalIgnoreCase));
    }
}
