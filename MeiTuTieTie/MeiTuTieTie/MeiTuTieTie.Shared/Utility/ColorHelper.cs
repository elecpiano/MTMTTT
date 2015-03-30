using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utility
{
    public class ColorUtil  
    {
        private static Regex _hexColorMatchRegex = new Regex("^#?(?<a>[a-z0-9][a-z0-9])?(?<r>[a-z0-9][a-z0-9])(?<g>[a-z0-9][a-z0-9])(?<b>[a-z0-9][a-z0-9])$", RegexOptions.IgnoreCase);
        public static Brush GetBrushFromHex(string hexColorString)
        {
            if (string.IsNullOrEmpty(hexColorString))
                return null;

            // Regex match the string
            var match = _hexColorMatchRegex.Match(hexColorString);

            if (!match.Success)
                return null;

            // a value is optional
            byte a = 255, r = 0, b = 0, g = 0;
            if (match.Groups["a"].Success)
                a = System.Convert.ToByte(match.Groups["a"].Value, 16);
            // r,b,g values are not optional
            r = System.Convert.ToByte(match.Groups["r"].Value, 16);
            b = System.Convert.ToByte(match.Groups["b"].Value, 16);
            g = System.Convert.ToByte(match.Groups["g"].Value, 16);
            Color color = Color.FromArgb(a, r, g, b);
            SolidColorBrush brush = new SolidColorBrush(color);
            return brush;
        }

        public static string GetHexFromBrush(SolidColorBrush brush)
        {
            if (brush == null)
            {
                return string.Empty;
            }
            string hex = string.Empty;
            hex = brush.Color.ToString();
            return hex;
        }
    }
}
