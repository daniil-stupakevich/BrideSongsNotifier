using BrideSongs.Notifier.App.ViewModels;

namespace BrideSongs.Notifier.App.Views.UserControls;
/// <summary>
/// Interaction logic for ShortManySongPopup.xaml
/// </summary>
public partial class ShortManySongPopup
{
    public ShortManySongPopup()
    {
        InitializeComponent();
        DataContext = App.GetService<MainViewModel>();
    }
}
