using BrideSongs.Notifier.App.ViewModels;
using System.Windows.Controls;

namespace BrideSongs.Notifier.App.Views.UserControls
{
    /// <summary>
    /// Interaction logic for SongListPopup.xaml
    /// </summary>
    public partial class ManySongsPopup : System.Windows.Controls.UserControl
    {
        public ManySongsPopup()
        {
            InitializeComponent();
            DataContext = App.GetService<MainViewModel>();
        }
    }
}
