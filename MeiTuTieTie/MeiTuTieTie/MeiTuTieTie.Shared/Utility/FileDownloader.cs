using System;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using System.Threading;
using Windows.UI.Xaml.Controls;

namespace Shared.Utility
{
    public class FileDownloader
    {
        private DownloadOperation activeDownload;
        private CancellationTokenSource cts;
        private ProgressBar progressBar;

        public async void Download(string url, string module, string file, ProgressBar pBar, Action callback)
        {
            progressBar = pBar;
            if (progressBar != null)
            {
                progressBar.Maximum = 100d;
            }

            try
            {
                Uri source = new Uri(url, UriKind.Absolute);
                StorageFile destinationFile = await IsolatedStorageHelper.CreateFileAsync(module, file);
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                download.Priority = BackgroundTransferPriority.High;
                // Attach progress and completion handlers.
                HandleDownloadAsync(download);
            }
            catch (Exception)
            {
            }
        }

        private async void HandleDownloadAsync(DownloadOperation download)
        {
            try
            {
                // Store the download so we can pause/resume.
                activeDownload = download;

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                // Start the download and attach a progress handler.
                await download.StartAsync().AsTask(cts.Token, progressCallback);
            }
            catch (Exception)
            {
            }
        }

        private void DownloadProgress(DownloadOperation download)
        {
            double percent = 100;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            }
            if (progressBar != null)
            {
                progressBar.Value = percent;
            }
        }

    }
}
