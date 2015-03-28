using System;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Windows.Storage.Streams;
using Shared.Global;

namespace Shared.Utility
{
    public partial class ImageHelper
    {
        public static async Task<RenderTargetBitmap> Capture(FrameworkElement uiElement, double expectedWidth)
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

            return renderTargetBitmap;
        }

        public static async Task SaveBitmapToMediaLibrary(RenderTargetBitmap renderTargetBitmap, string fileName)
        {
            DisplayInformation di = DisplayInformation.GetForCurrentView();
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

        public static async Task<Stream> SaveBitmapToLocal(RenderTargetBitmap renderTargetBitmap, string fileName, bool returnStream = false)
        {
            DisplayInformation di = DisplayInformation.GetForCurrentView();
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            // Encode the image to file
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
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

            if (returnStream)
            {
                var resultStream = await folder.OpenStreamForReadAsync(fileName);
                return resultStream;
            }
            return null;
        }

        public static async Task<IRandomAccessStream> BitmapToStream(RenderTargetBitmap renderTargetBitmap)
        {
            DisplayInformation di = DisplayInformation.GetForCurrentView();
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            // Encode the image to file
            IRandomAccessStream stream = new MemoryStream().AsRandomAccessStream();
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

            return stream;
        }

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

        public static async Task<BitmapImage> Download(string uri, string folder, string file)
        {
            BitmapImage bitmapImage = null;
            try
            {
                //download
                HttpWebRequest request = HttpWebRequest.CreateHttp(new Uri(uri));
                request.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

                //save
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                var dataFolder = await local.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);

                using (Stream stream = response.GetResponseStream())
                {
                    byte[] fileBytes = new byte[response.ContentLength];
                    await stream.ReadAsync(fileBytes, 0, (int)response.ContentLength);

                    var newFile = await dataFolder.CreateFileAsync(file, CreationCollisionOption.ReplaceExisting);

                    // Write the data
                    using (var s = await newFile.OpenStreamForWriteAsync())
                    {
                        await s.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(stream.AsRandomAccessStream());
                }
            }
            catch (Exception e)
            {
            }

            return bitmapImage;
        }

        public static async Task<BitmapImage> ReadImage(string path)
        {
            BitmapImage bitmapImage = null;

            try
            {
                // Get the local folder.
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                if (local != null)
                {
                    // Get the DataFolder folder.
                    var storageFile = await local.GetFileAsync(path);

                    // Ensure the stream is disposed once the image is loaded
                    using (IRandomAccessStream fileStream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        // Set the image source to the selected bitmap
                        bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return bitmapImage;
        }

        public static async Task<BitmapImage> ReadImage(string folder, string file)
        {
            BitmapImage bitmapImage = null;

            try
            {
                // Get the local folder.
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                if (local != null)
                {
                    // Get the DataFolder folder.
                    var storageFolder = await local.GetFolderAsync(folder);
                    var storageFile = await storageFolder.GetFileAsync(file);

                    // Ensure the stream is disposed once the image is loaded
                    using (IRandomAccessStream fileStream = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        // Set the image source to the selected bitmap
                        bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return bitmapImage;
        }

        public static async Task<string> DownloadThumbnail(string uri, string extension)
        {
            string fileName = Guid.NewGuid().ToString() + extension;
            //string folderName = IsolatedStorageHelper.EnsureUserDataRoot(Constants.THUMBNAIL_FOLDER);
            await Download(uri, Constants.THUMBNAIL_FOLDER, fileName);
            string fullPath = Path.Combine(Constants.THUMBNAIL_FOLDER, fileName);
            return fullPath;
        }

        public static async Task<StorageFile> MakeResizedImage(StorageFile oldFile, string newFileName, double sizeMax)
        {
            StorageFile newFile = null;
            using (var sourceStream = await oldFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream);

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

                    //create new file
                    StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                    newFile = await local.CreateFileAsync(newFileName, CreationCollisionOption.ReplaceExisting);
                    DisplayInformation di = DisplayInformation.GetForCurrentView();

                    using (var destinationStream = await newFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, destinationStream);
                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, di.LogicalDpi, di.LogicalDpi, pixelBuffer);
                        await encoder.FlushAsync();
                    }
                }
                else
                {
                    newFile = oldFile;
                }
            }
            return newFile;
        }

        public static async Task<BitmapImage> GetThumbnail(StorageFile file)
        {
            BitmapImage bi = null;
            using (var sourceStream = await file.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream);
                var stream = await decoder.GetThumbnailAsync();
                bi = new BitmapImage();
                bi.SetSource(stream);
            }
            return bi;
        }

    }
}
