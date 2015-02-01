﻿using System;
using Shared.Common;
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
using Windows.UI.Xaml.Navigation;
using Shared.Model;
using Shared.Control;
using Shared.Enum;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page, IFileOpenPickerPageBase
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private List<SpriteControl> sprites = new List<SpriteControl>();
        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_Operation_PickPhotos = "PickPhotos";
        private OperationPageType pageType = OperationPageType.Single;
        private bool SingleImageLocked
        {
            get
            {
                return imgSingleMode.IsHitTestVisible;
            }
            set
            {
                imgSingleMode.IsHitTestVisible = value;
            }
        }
        public WidgetPageType MaterialSelectedBy { get; set; }

        #endregion

        #region Lifecycle

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                pageType = (OperationPageType)e.Parameter;
                InitializePage();
            }
            else if (e.NavigationMode == NavigationMode.Back && App.CurrentInstance.SelectedMaterial != null)
            {
                MaterialToSprite();
            }

        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                SpriteControl.DetachContainer();
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            }
            else
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            }

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Page Initialization

        private void InitializePage()
        {
            switch (pageType)
            {
                case OperationPageType.Single:
                    btnBiankuang.Visibility = Visibility.Visible;
                    this.Frame.BackStack.RemoveAt(this.Frame.BackStack.Count - 1);
                    PreapreSingleModeImage();
                    break;
                case OperationPageType.Multi:
                    btnPhoto.Visibility = Visibility.Visible;
                    btnBeijing.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }

            SpriteControl.Initialize(stage);
        }

        #endregion

        #region Image Processing

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();

            //http://social.technet.microsoft.com/wiki/contents/articles/20648.using-the-rendertargetbitmap-in-windows-store-apps-with-xaml-and-c.aspx
            RectangleGeometry cropArea = new RectangleGeometry() { Rect = new Rect(0d, 0d, stagePanel.ActualWidth, stagePanel.ActualHeight) };

            /* IMPORTANT: in order to get the expected result, the clipped element and the captured element should NOT be the same one */
            stagePanelForClipping.Clip = cropArea;

            string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            ImageHelper.CaptureToMediaLibrary(this.stagePanel, fileName, 640);
        }

        #endregion

        #region Manipulation

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
        }

        private void PreapreSingleModeImage()
        {
            imgSingleMode.Source = App.CurrentInstance.wbForSingleMode;
            imgSingleMode.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.Rotate;
            imgSingleMode.ManipulationDelta += imgSingleMode_ManipulationDelta;
            imgSingleMode.PointerPressed += stageBackground_PointerPressed;
        }

        void imgSingleMode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            bool isDrag = e.Delta.Rotation == 0 && e.Delta.Expansion == 0;
            if (isDrag)
            {
                transformSingleModeImage.TranslateX += e.Delta.Translation.X;
                transformSingleModeImage.TranslateY += e.Delta.Translation.Y;
            }
            else
            {
                transformSingleModeImage.Rotation += e.Delta.Rotation;
                transformSingleModeImage.ScaleX *= e.Delta.Scale;
                transformSingleModeImage.ScaleY *= e.Delta.Scale;
            }
        }

        #endregion

        #region Load Photo

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            PickPhotos();
        }

        private void PickPhotos()
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
                    SpriteControl sprite = new SpriteControl(SpriteType.Image);
                    sprite.SetImage(bi);
                    sprites.Add(sprite);
                    sprite.AddToContainer();
                }
            }
        }

        #endregion

        #region Shipin, Biankuang, Beijing

        private void Shipin_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.Shipin);
        }

        private void Biankuang_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.BianKuang);
        }

        private void Beijing_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.Beijing);
        }

        private async void MaterialToSprite()
        {
            if (App.CurrentInstance.SelectedMaterial != null)
            {
                BitmapImage bi = await ImageHelper.ReadImage(App.CurrentInstance.SelectedMaterial.image);

                if (App.CurrentInstance.MaterialSelectedBy == WidgetPageType.Shipin)
                {
                    SpriteControl sprite = new SpriteControl(SpriteType.Image);
                    sprite.SetImage(bi);
                    sprites.Add(sprite);
                    sprite.AddToContainer();
                }
                else if (App.CurrentInstance.MaterialSelectedBy == WidgetPageType.BianKuang || App.CurrentInstance.MaterialSelectedBy == WidgetPageType.Beijing)
                {
                    imgBiankuangOrBeijing.Source = bi;
                }

                App.CurrentInstance.SelectedMaterial = null;
            }
        }

        #endregion

        #region Layer Buttons

        private void spriteUp_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteControl.SelectedSprite != null)
            {
                SpriteControl.SelectedSprite.ChangeZIndex(true);
            }
        }

        private void spriteDown_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteControl.SelectedSprite != null)
            {
                SpriteControl.SelectedSprite.ChangeZIndex(false);
            }
        }

        private void spriteDelete_Click(object sender, RoutedEventArgs e)
        {
            SpriteControl.RemoveSelectedSprite();
        }

        private void photoLock_Click(object sender, RoutedEventArgs e)
        {
            SingleImageLocked = !SingleImageLocked;
        }

        #endregion

        #region Test

        private void InsertSprites()
        {
            SpriteControl sprite = null;

            for (int i = 0; i < 1; i++)
            {
                sprite = new SpriteControl(SpriteType.Image);
                sprite.SetImage("ms-appx:///Assets/TestImages/TestImage001.jpg");
                sprites.Add(sprite);

                SpriteControl.Initialize(stage);
            }
        }

        ObservableCollection<BitmapImage> photoList = new ObservableCollection<BitmapImage>();
        private async void LoadPhotos()
        {
            //picListBox.ItemsSource = photoList;
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

        #endregion

        #region Text

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            AddTextSprite();
        }

        private void AddTextSprite()
        {
            SpriteControl sprite = new SpriteControl(SpriteType.Text);
            sprites.Add(sprite);
            sprite.AddToContainer();
        }

        #endregion

    }
}
