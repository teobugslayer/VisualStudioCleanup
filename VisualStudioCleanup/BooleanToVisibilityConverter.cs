using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisualStudioCleanup
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInverted { get; set; }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);

            if (this.IsInverted)
            {
                val = !val;
            }

            return val ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
