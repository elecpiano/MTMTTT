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
        private ProgressBar progressBarControl;
        private Action onCompleteAction;
        private StorageFile destinationFile;

        public async void Download(string url, string module, string file, ProgressBar progressBar, Action onComplete)
        {
            progressBarControl = progressBar;
            onCompleteAction = onComplete;

            if (cts == null)
            {
                cts = new CancellationTokenSource();
            }

            if (progressBar != null)
            {
                progressBar.Maximum = 100d;
            }

            try
            {
                Uri source = new Uri(url, UriKind.Absolute);
                destinationFile = await IsolatedStorageHelper.CreateFileAsync(module, file);
                BackgroundDownloader downloader = new BackgroundDownloader();
                DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                //download.Priority = BackgroundTransferPriority.High;

                // Attach progress and completion handlers.
                HandleDownloadAsync(download);
            }
            catch (Exception)
            {
            }
        }

        public void Cancel()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }

            destinationFile.DeleteAsync();
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
            if (progressBarControl != null)
            {
                progressBarControl.Value = percent;
            }

            /* it's strange that following code does not work,
             * if (download.Progress.Status == BackgroundTransferStatus.Completed)
             * so i had to use the byte number to determine the completion
             */
            if (download.Progress.BytesReceived == download.Progress.TotalBytesToReceive && onCompleteAction != null)
            {
                onCompleteAction();
            }
        }

    }
}
