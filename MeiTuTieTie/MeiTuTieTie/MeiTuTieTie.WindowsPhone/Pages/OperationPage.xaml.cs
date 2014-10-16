using System;
using MeiTuTieTie.Common;
using MeiTuTieTie.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        private List<SpriteControl> spriteList = new List<SpriteControl>();

        #endregion

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            ChoosePhotos();
            return;
            ThrowPhotos();
        }

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
        }

        private void ThrowPhotos()
        {
            SpriteControl sprite = null;

            for (int i = 0; i < 1; i++)
            {
                sprite = new SpriteControl();
                sprite.SetImage("ms-appx:///Assets/TestImages/TestImage001.jpg");
                spriteList.Add(sprite);

                sprite.SetContainer(stage);
            }
        }

        ObservableCollection<BitmapImage> photoList = new ObservableCollection<BitmapImage>();
        private async Task ChoosePhotos()
        {

            picList.ItemsSource = photoList;

            //FileOpenPicker picker = new FileOpenPicker();
            //picker.ViewMode = PickerViewMode.Thumbnail;
            //picker.PickSingleFileAndContinue();
            //if (picker.ContinuationData!=null && picker.ContinuationData.Values.Count==1)
            //{
            //    foreach (var value in picker.ContinuationData.Values)
            //    {
            //        BitmapImage bm = new BitmapImage();

            //    }
            //}

            StorageFolder PictureFolder = KnownFolders.CameraRoll;
            var files = await PictureFolder.GetFilesAsync();
            foreach (var file in files)
            {
                BitmapImage bm = new BitmapImage();
                var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
                bm.SetSource(thumbnail);
                photoList.Add(bm);
            };
        }
    }
}
