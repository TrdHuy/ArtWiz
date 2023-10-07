using SPRNetTool.Utils;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SPRNetTool.View.Utils
{

    public enum InvisibleType
    {
        /// <summary>
        /// If false, view will be collapsed
        /// </summary>
        COLLAPSED,
        /// <summary>
        /// If false, view will be hidden
        /// </summary>
        HIDDEN,
        /// <summary>
        /// If true, view will be collapsed
        /// </summary>
        REVERSE_COLLAPSED,
        /// <summary>
        /// If true, view will be hidden
        /// </summary>
        REVERSE_HIDDEN
    }

    public class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
        {
            var invisibleType = InvisibleType.HIDDEN;
            parameter?.IfIsThenAlso<InvisibleType>(it => invisibleType = it);

            switch (invisibleType)
            {
                case InvisibleType.HIDDEN:
                    return System.Convert.ToBoolean(value) == false ? Visibility.Hidden : Visibility.Visible;
                case InvisibleType.COLLAPSED:
                    return System.Convert.ToBoolean(value) == false ? Visibility.Collapsed : Visibility.Visible;
                case InvisibleType.REVERSE_HIDDEN:
                    return System.Convert.ToBoolean(value) == true ? Visibility.Hidden : Visibility.Visible;
                case InvisibleType.REVERSE_COLLAPSED:
                    return System.Convert.ToBoolean(value) == true ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invisibleType = InvisibleType.COLLAPSED;
            parameter?.IfIsThenAlso<InvisibleType>(it => invisibleType = it);

            switch (invisibleType)
            {
                case InvisibleType.HIDDEN:
                    return value == null ? Visibility.Hidden : Visibility.Visible;
                case InvisibleType.COLLAPSED:
                    return value == null ? Visibility.Collapsed : Visibility.Visible;
                case InvisibleType.REVERSE_HIDDEN:
                    return value != null ? Visibility.Hidden : Visibility.Visible;
                case InvisibleType.REVERSE_COLLAPSED:
                    return value != null ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
