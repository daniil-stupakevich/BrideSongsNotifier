using BrideSongs.Notifier.App.Models;
using System.Windows;
using System.Windows.Controls;

namespace BrideSongs.Notifier.App.TemplateSelectors
{
    public class ActiveControlTemplateSelector : DataTemplateSelector
    {
        public DataTemplate OneSongPopup { get; set; }
        public DataTemplate ManySongsPopup { get; set; }
        public DataTemplate ShortManySongsPopup { get; set; }
        public DataTemplate LoadingPopup { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is PopupType popupType) 
            {
                switch (popupType) 
                {
                    case PopupType.OneSong:
                        return OneSongPopup;
                    case PopupType.ManySongs:
                        return ManySongsPopup;
                    case PopupType.ShortManySongs:
                        return ShortManySongsPopup;
                }
            }

            return LoadingPopup;
        }
    }
}