using System;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage;

namespace Shared.Utility
{
    public partial class ImageHelper
    {
        public static async Task<WriteableBitmap> Resize(StorageFile file, double sizeMax)
        {
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

            double sourceWidth = (double)decoder.PixelWidth;
            double sourceHeight = (double)decoder.PixelHeight;
            double width = sourceWidth;
            double height = sourceHeight;

            bool needResize = false;
            double WtoH = sourceWidth / sourceHeight;
            if (WtoH > 1d)//width is longer
            {
                if (sourceWidth > sizeMax)
                {
                    width = sizeMax;
                    height = width * sourceHeight / sourceWidth;
                    needResize = true;
                }
            }
            else//height is longer
            {
                if (sourceHeight > sizeMax)
                {
                    height = sizeMax;
                    width = height * sourceWidth / sourceHeight;
                    needResize = true;
                }
            }

            WriteableBitmap wb = null;
            wb = new WriteableBitmap((int)width, (int)height);
            if (needResize)
            {
                BitmapTransform transform = new BitmapTransform()
                {
                    ScaledWidth = (uint)width,
                    ScaledHeight = (uint)height
                };

                PixelDataProvider pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    transform, ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage);
                byte[] pixelBuffer = pixelData.DetachPixelData();
                wb = wb.FromByteArray(pixelBuffer);
            }
            else
            {
                wb.SetSource(stream);
            }

            return wb;
        }

    }
}
