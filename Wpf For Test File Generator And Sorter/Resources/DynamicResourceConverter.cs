using System.Windows;
using System.Windows.Data;

namespace Wpf_For_Test_File_Generator_And_Sorter.Resources;

public class DynamicResourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        var key = value as string;
        if (string.IsNullOrEmpty(key)) return string.Empty;

        return Application.Current.TryFindResource(key) ?? key;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}