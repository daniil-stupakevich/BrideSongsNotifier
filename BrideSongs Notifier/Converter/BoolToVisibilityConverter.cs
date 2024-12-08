using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace BrideSongs.Notifier.App.Converter
{
    public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public bool Invert { get; set; } = true;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Invert)
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }

            return (bool)value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}