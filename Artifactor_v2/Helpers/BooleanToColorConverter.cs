using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace Artifactor_v2.Helpers;

public class BooleanToColorConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        return (value is bool && (bool)value) ? (Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"] as SolidColorBrush) : Application.Current.Resources["CardBackgroundFillColorDefaultBrush"] as SolidColorBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new Exception("Not Implemented");
    }
}
