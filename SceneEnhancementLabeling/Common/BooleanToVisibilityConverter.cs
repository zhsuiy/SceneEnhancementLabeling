using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SceneEnhancementLabeling.Common
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                return value is bool && (bool)value ? Visibility.Collapsed : Visibility.Visible;
            }
            return value is bool && (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
