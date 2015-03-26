using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using Shared.Global;
using Shared.Animation;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Control;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Storage;
using System;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.Storage.Search;
using Windows.Storage.FileProperties;

namespace MeiTuTieTie.Pages
{
    public sealed partial class PhotoPickerPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public PhotoPickerPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.CanGobackAsked += navigationHelper_CanGobackAsked;
        }

        void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {

                LoadPhoto();
            }
        }

        #endregion

        #region Load Photo

        ObservableCollection<BitmapImage> photoList = new ObservableCollection<BitmapImage>();

        private async void LoadPhoto1()
        {
            //photoListBox.ItemsSource = photoList;

            StorageFolder PictureFolder = KnownFolders.CameraRoll;
            var files = await PictureFolder.GetFilesAsync();
            foreach (var file in files)
            {
                BitmapImage bm = new BitmapImage();
                var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                thumbnailList.Add(thumbnail);
                //bm.SetSource(thumbnail);
                //photoList.Add(bm);
                debugText.Text = thumbnailList.Count.ToString();
            };
        }

        List<StorageItemThumbnail> thumbnailList = new List<StorageItemThumbnail>();
        private async void LoadPhoto()
        {
            var queryOptions = new Windows.Storage.Search.QueryOptions(CommonFileQuery.OrderByDate, null);
            queryOptions.SetThumbnailPrefetch(ThumbnailMode.PicturesView, 100,
                    ThumbnailOptions.ReturnOnlyIfCached);
            //queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.ImageProperties, new string[] { "System.Size" });
            queryOptions.SetThumbnailPrefetch(ThumbnailMode.ListView, 80, ThumbnailOptions.ReturnOnlyIfCached);

            StorageFileQueryResult queryResults = KnownFolders.PicturesLibrary.CreateFileQueryWithOptions(queryOptions);
            IReadOnlyList<StorageFile> files = await queryResults.GetFilesAsync();

            foreach (var file in files)
            {
                var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.ListView, 40, ThumbnailOptions.ReturnOnlyIfCached);
                thumbnailList.Add(thumbnail);
                debugText.Text = thumbnailList.Count.ToString();
                //BitmapImage bm = new BitmapImage();
                //bm.SetSource(thumbnail);
                //photoList.Add(bm);

                //ImageProperties imageProperties = await file.Properties.GetImagePropertiesAsync();

                //// Do something with the date the image was taken.
                //DateTimeOffset dateTaken = imageProperties.DateTaken;

                //// Performance gains increase with the number of properties that are accessed.
                //IDictionary<String, object> propertyResults =
                //    await file.Properties.RetrievePropertiesAsync(
                //          new string[] { "System.Size" });

                //// Get/Set extra properties here
                //var systemSize = propertyResults["System.Size"];
            }
        }

        #endregion

    }
}
