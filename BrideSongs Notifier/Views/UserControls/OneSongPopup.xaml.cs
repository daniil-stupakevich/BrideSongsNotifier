using BrideSongs.Notifier.App.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace BrideSongs.Notifier.App.Views.UserControls
{
    public partial class OneSongPopup : UserControl
    {
        public OneSongPopup()
        {
            InitializeComponent();
            DataContext = App.GetService<MainViewModel>();
        }
    }
}
