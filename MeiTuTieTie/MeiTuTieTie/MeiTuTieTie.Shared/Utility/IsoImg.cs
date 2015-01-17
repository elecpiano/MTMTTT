using Shared.Utility;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared.Utility
{
    public class IsoImg : DependencyObject
    {
        public static void SetPath(UIElement element, string value)
        {
            element.SetValue(PathProperty, value);
        }
        public static string GetPath(UIElement element)
        {
            return (string)element.GetValue(PathProperty);
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.RegisterAttached("Path", typeof(string), typeof(IsoImg), new PropertyMetadata("", Changed));

        private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image img = d as Image;

            if (img != null)
            {
                var path = e.NewValue as string;
                ReadImage(img, path);
                //SynchronizationContext uiThread = SynchronizationContext.Current;

                //Task.Factory.StartNew(() =>
                //{
                //    uiThread.Post(_ =>
                //    {
                //        var _img = new BitmapImage();
                //        img.Source = _img;
                //    }, null);
                //});
            }
        }

        private static async void ReadImage(Image img, string path)
        {
            var bi = await ImageHelper.ReadImage(path);
            img.Source = bi;
        }
    }
}
