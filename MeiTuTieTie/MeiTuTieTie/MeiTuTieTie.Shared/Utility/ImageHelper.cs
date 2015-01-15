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
        public static async void CaptureToMediaLibrary(FrameworkElement uiElement, string fileName, double expectedWidth)
        {
            double scale = 1.0d;
            DisplayInformation di = DisplayInformation.GetForCurrentView();
            
#if WINDOWS_PHONE_APP
            scale = di.RawPixelsPerViewPixel;
#else
            scale = (double)di.ResolutionScale * 0.01;
#endif
            double expectedHeight = expectedWidth * uiElement.ActualHeight / uiElement.ActualWidth;

            double renderWidth = expectedWidth / scale;
            double renderHeight = expectedHeight / scale;

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(uiElement, (int)renderWidth, (int)renderHeight);//, (int)uiElement.ActualWidth, (int)uiElement.ActualHeight);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            // Encode the image to file
            StorageFolder folder = KnownFolders.SavedPictures;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegXREncoderId, stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    di.LogicalDpi,
                    di.LogicalDpi,
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
