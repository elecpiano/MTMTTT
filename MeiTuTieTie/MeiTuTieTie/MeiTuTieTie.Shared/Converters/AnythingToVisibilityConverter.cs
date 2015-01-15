using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Shared.Converters
{
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class AnythingToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result = false;

            bool negate = false;
            if (parameter != null && parameter.ToString().ToLower() == "neg")
            {
                negate = true;
            }

            if (value is bool)
            {
                result = (bool)value;
            }
            else if (value is string)
            {
                result = !string.IsNullOrEmpty(value.ToString().Trim());
            }
            else if (value is int)
            {
                result = (int)value > 0;
            }
            else if (value is DateTime)
            {
                result = (DateTime)value > DateTime.Now;
            }
            else
            {
                result = value == null ? false : true;
            }

            if (negate)
            {
                result = !result;
            }
            return result ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)

        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }

    }
}
