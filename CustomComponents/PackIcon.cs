using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Control = System.Windows.Controls.Control;

namespace BrideSongs.Notifier.App.CustomComponents;

public class CustomIcon : Control
{
    private static readonly Lazy<IDictionary<CustomIconKind, string>> _dataIndex
        = new Lazy<IDictionary<CustomIconKind, string>>(CustomIconDataFactory.Create);

    static CustomIcon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomIcon), new FrameworkPropertyMetadata(typeof(CustomIcon)));
    }

    public static readonly DependencyProperty KindProperty
        = DependencyProperty.Register(nameof(Kind), typeof(CustomIconKind), typeof(CustomIcon), new PropertyMetadata(default(CustomIconKind), KindPropertyChangedCallback));

    private static void KindPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        => ((CustomIcon)dependencyObject).UpdateData();

    /// <summary>
    /// Gets or sets the icon to display.
    /// </summary>
    public CustomIconKind Kind
    {
        get => (CustomIconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private static readonly DependencyPropertyKey DataPropertyKey
        = DependencyProperty.RegisterReadOnly(nameof(Data), typeof(string), typeof(CustomIcon), new PropertyMetadata(""));

    // ReSharper disable once StaticMemberInGenericType
    public static readonly DependencyProperty DataProperty = DataPropertyKey.DependencyProperty;

    /// <summary>
    /// Gets the icon path data for the current <see cref="Kind"/>.
    /// </summary>
    [TypeConverter(typeof(GeometryConverter))]
    public string? Data
    {
        get => (string?)GetValue(DataProperty);
        private set => SetValue(DataPropertyKey, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateData();
    }

    private void UpdateData()
    {
        string? data = null;
        _dataIndex.Value?.TryGetValue(Kind, out data);
        Data = data;
    }
}
