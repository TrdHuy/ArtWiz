using SPRNetTool.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SPRNetTool.View.Utils
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public enum InvisibleType
    {
        COLLAPSED, HIDDEN
    }

    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            var invisibleType = InvisibleType.HIDDEN;
            parameter?.IfIsThenAlso<InvisibleType>(it => invisibleType = it);

            return System.Convert.ToBoolean(value) == false ?
                (invisibleType == InvisibleType.HIDDEN ? Visibility.Hidden : Visibility.Collapsed)
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
