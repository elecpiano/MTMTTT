using System;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

using System.Runtime.InteropServices.WindowsRuntime;

namespace Shared.Utility
{
    public class ImageHelper
    {
        public static async void CaptureToMediaLibrary(FrameworkElement uiElement, string fileName)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(uiElement);//, (int)uiElement.ActualWidth, (int)uiElement.ActualHeight);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            // Encode the image to file
            StorageFolder folder = KnownFolders.SavedPictures;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }
        }

        public static async void CaptureScreen()
        {
            //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn642091.aspx
        }

    }
}
