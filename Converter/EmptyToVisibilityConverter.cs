using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace BrideSongs.Notifier.App.Converter
{
    public class EmptyToVisibilityConverter : MarkupExtension, IValueConverter
    {
        private bool _isVisbile;

        public bool Invert { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) 
            {
                if (Invert)
                {
                    return _isVisbile ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return _isVisbile ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            if (value is string text)
            {
                _isVisbile = !string.IsNullOrEmpty(text);
            }

            Type type = value.GetType();
           
            if (type.GetInterface(nameof(IEnumerable<Type>)) != null)
            {
                var collection = value as IEnumerable<object>;
                _isVisbile = collection?.Count() > 0;
            }

            if (Invert)
            {
                return _isVisbile ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return _isVisbile ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
