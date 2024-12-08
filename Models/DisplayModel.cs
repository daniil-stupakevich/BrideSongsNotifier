using CommunityToolkit.Mvvm.ComponentModel;

namespace BrideSongs.Notifier.App.Models;
public partial class DisplayModel : ObservableObject
{
    [ObservableProperty]
    public bool _isNotificationVisible;

    public bool IsPrimary { get; set; }
    public string DisplayName { get; set; }     
    public Rectangle WorkingArea { get; set; }
}
