using System;
using Shared.Common;
using MeiTuTieTie.Controls;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Shared.Utility;
using Windows.UI.Xaml.Media;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private List<SpriteControl> spriteList = new List<SpriteControl>();
        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_Operation_PickPhotos = "PickPhotos";

        #endregion

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();

            //http://social.technet.microsoft.com/wiki/contents/articles/20648.using-the-rendertargetbitmap-in-windows-store-apps-with-xaml-and-c.aspx
            RectangleGeometry cropArea = new RectangleGeometry() { Rect = new Rect(0d, 0d, stagePanel.ActualWidth, stagePanel.ActualHeight) };
            stage.Clip = cropArea;

            string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            ImageHelper.CaptureToMediaLibrary(this.stagePanel, fileName, 640);
        }

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
        }

        private void InsertSprites()
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


        private async void PickPhotos()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.ContinuationData[Continuation_Key_Operation] = Continuation_Operation_PickPhotos;
            picker.PickMultipleFilesAndContinue();
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation)
                && args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_Operation_PickPhotos)
            {
                foreach (var file in args.Files)
                {
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(stream);

                    //sprite
                    SpriteControl sprite = new SpriteControl();
                    sprite.SetImage(bi);
                    spriteList.Add(sprite);
                    sprite.SetContainer(stage);
                }
            }
        }

        ObservableCollection<BitmapImage> photoList = new ObservableCollection<BitmapImage>();
        private async void LoadPhotos()
        {
            picListBox.ItemsSource = photoList;
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

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            PickPhotos();
        }

        private void Widget_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage));
        }

    }
}
