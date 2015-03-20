using System;
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
using Shared.Control;
using Shared.Enum;
using Shared.Global;
using Windows.UI;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page, IFileOpenPickerPageBase
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private List<SpriteControl> sprites = new List<SpriteControl>();
        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_OperationPage_PickPhotos = "PickPhotos";
        private OperationPageType pageType = OperationPageType.Single;

        private bool SingleImageLocked
        {
            get
            {
                return !imgSingleMode.IsHitTestVisible;
            }
            set
            {
                imgSingleMode.IsHitTestVisible = !value;
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

            LoadSettings();

            if (e.NavigationMode == NavigationMode.New)
            {
                pageType = (OperationPageType)e.Parameter;
                InitializePage();
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (App.CurrentInstance.SelectedMaterial != null)
                {
                    MaterialToSprite();
                }
                //else if (!string.IsNullOrEmpty(App.CurrentInstance.SelectedFont) || App.CurrentInstance.SelectedTextColor != null)
                //{
                //    SetFont();
                //}
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
                    VisualStateManager.GoToState(this, "vsSingleModeButtons", false);
                    this.Frame.BackStack.RemoveAt(this.Frame.BackStack.Count - 1);
                    PreapreSingleModeImage();
                    break;
                case OperationPageType.Multi:
                    VisualStateManager.GoToState(this, "vsMultiModeButtons", false);
                    PickPhotos();
                    break;
                default:
                    break;
            }

            SpriteControl.Initialize(stage);
            SpriteControl.OnSelected += Sprite_OnSelected;

            InitColorFontList();

            VisualStateManager.GoToState(this, "vsLayerButtonShown", true);
            BuildBottomAppBar_Normal();
        }

        #endregion

        #region Image Processing

        double exportWidth = 1248d;

        private async void GenerateImage()
        {
            SpriteControl.DismissActiveSprite();

            //http://social.technet.microsoft.com/wiki/contents/articles/20648.using-the-rendertargetbitmap-in-windows-store-apps-with-xaml-and-c.aspx
            RectangleGeometry cropArea = new RectangleGeometry() { Rect = new Rect(0d, 0d, stagePanel.ActualWidth, stagePanel.ActualHeight) };

            /* IMPORTANT: in order to get the expected result, the clipped element and the captured element should NOT be the same one */
            stagePanelForClipping.Clip = cropArea;

            //string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            //ImageHelper.CaptureToMediaLibrary(this.stagePanel, fileName, exportWidth);
            var bitmap = await ImageHelper.Capture(this.stagePanel, exportWidth);

            Frame.Navigate(typeof(ExportPage), bitmap);
        }

        #endregion

        #region Manipulation

        private void PreapreSingleModeImage()
        {
            imgSingleMode.Source = App.CurrentInstance.wbForSingleMode;
            imgSingleMode.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.Rotate;
            imgSingleMode.ManipulationDelta += imgSingleMode_ManipulationDelta;
            imgSingleMode.PointerPressed += stageBackground_PointerPressed;
        }

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();

            VisualStateManager.GoToState(this, "vsColorFontHidden", true);
            colorListShown = fontListShown = false;

            if (pageType == OperationPageType.Single)
            {
                VisualStateManager.GoToState(this, "vsSingleModeButtons", false);
            }
            else if (pageType == OperationPageType.Multi)
            {
                VisualStateManager.GoToState(this, "vsMultiModeButtons", false);
            }
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
            picker.ContinuationData[Continuation_Key_Operation] = Continuation_OperationPage_PickPhotos;
            picker.PickMultipleFilesAndContinue();
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation)
                && args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_OperationPage_PickPhotos)
            {
                foreach (var file in args.Files)
                {
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(stream);

                    //sprite
                    SpriteControl sprite = new SpriteControl(SpriteType.Photo);
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
                    SpriteControl sprite = new SpriteControl(SpriteType.Material);
                    sprite.SetImage(bi);
                    sprites.Add(sprite);
                    sprite.AddToContainer();
                }
                else if (App.CurrentInstance.MaterialSelectedBy == WidgetPageType.BianKuang)
                {
                    imgBiankuang.Source = bi;
                }
                else if (App.CurrentInstance.MaterialSelectedBy == WidgetPageType.Beijing)
                {
                    imgBeijing.Source = bi;
                }

                App.CurrentInstance.SelectedMaterial = null;
            }
        }

        #endregion

        #region Layer Buttons

        private void spriteUp_Click(object sender, TappedRoutedEventArgs e)
        {
            if (SpriteControl.SelectedSprite != null)
            {
                SpriteControl.SelectedSprite.ChangeZIndex(true);
            }
        }

        private void spriteDown_Click(object sender, TappedRoutedEventArgs e)
        {
            if (SpriteControl.SelectedSprite != null)
            {
                SpriteControl.SelectedSprite.ChangeZIndex(false);
            }
        }

        private void spriteDelete_Click(object sender, TappedRoutedEventArgs e)
        {
            SpriteControl.RemoveSelectedSprite();
        }

        private void photoLock_Click(object sender, TappedRoutedEventArgs e)
        {
            SingleImageLocked = !SingleImageLocked;

            btnPhotoUnLock.Opacity = SingleImageLocked ? 1d : 0d;
            btnPhotoLock.Opacity = SingleImageLocked ? 0d : 1d;
        }

        private void spriteFuncOff_Click(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "vsLayerButtonHidden", true);
        }

        private void spriteFuncOn_Click(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "vsLayerButtonShown", true);
        }

        #endregion

        #region Test

        private void InsertSprites()
        {
            SpriteControl sprite = null;

            for (int i = 0; i < 1; i++)
            {
                sprite = new SpriteControl(SpriteType.Photo);
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

        private SpriteTextBox selectedSpriteText = null;
        private List<string> colorListData = null;
        private List<string> fontListData = null;

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            AddTextSprite();
        }

        private void AddTextSprite()
        {
            SpriteControl sprite = new SpriteControl(SpriteType.Text);
            sprite.EditingStarted += sprite_EditingStarted;
            sprite.EditingEnded += sprite_EditingEnded;

            sprites.Add(sprite);
            sprite.AddToContainer();
        }

        void Sprite_OnSelected(object sender, EventArgs e)
        {
            SpriteControl sprite = sender as SpriteControl;
            if (sprite == null)
            {
                BuildBottomAppBar_Normal();
            }
            else
            {
                if (sprite.SpriteType == SpriteType.Text)
                {
                    selectedSpriteText = (sender as SpriteControl).spriteText;
                    VisualStateManager.GoToState(this, "vsTextModeButtons", false);
                }
                else
                {
                    if (pageType == OperationPageType.Single)
                    {
                        VisualStateManager.GoToState(this, "vsSingleModeButtons", false);
                    }
                    else if (pageType == OperationPageType.Multi)
                    {
                        VisualStateManager.GoToState(this, "vsMultiModeButtons", false);
                    }
                }
            }
        }

        void sprite_EditingStarted(object sender, EventArgs e)
        {
            BuildBottomAppBar_TextEditor();
            DelayExecutor.Delay(20, () =>
                {
                    (sender as SpriteControl).SyncButtonsPosition();
                });
        }

        void sprite_EditingEnded(object sender, EventArgs e)
        {
            BuildBottomAppBar_Normal();
            DelayExecutor.Delay(20, () =>
            {
                (sender as SpriteControl).SyncButtonsPosition();
            });
        }

        private bool colorListShown = false;
        private void color_Click(object sender, RoutedEventArgs e)
        {
            if (colorListShown)
            {
                VisualStateManager.GoToState(this, "vsColorFontHidden", true);
                colorListShown = fontListShown = false;
            }
            else
            {
                VisualStateManager.GoToState(this, "vsColorListShown", true);
                colorListShown = true;
                fontListShown = false;
            }
        }

        private bool fontListShown = false;
        private void font_Click(object sender, RoutedEventArgs e)
        {
            if (fontListShown)
            {
                VisualStateManager.GoToState(this, "vsColorFontHidden", true);
                colorListShown = fontListShown = false;
            }
            else
            {
                VisualStateManager.GoToState(this, "vsFontListShown", true);
                fontListShown = true;
                colorListShown = false;
            }
        }

        private void InitColorFontList()
        {
            if (colorListData == null)
            {
                colorListData = new List<string>();
                colorListData.Add("#ff000000");
                colorListData.Add("#ffffffff");
                colorListData.Add("#ffc2393a");
                colorListData.Add("#fff09fb8");
                colorListData.Add("#ffeb6195");
                colorListData.Add("#fff7bfd0");
                colorListData.Add("#ffb985b6");
                colorListData.Add("#ff854a92");
                colorListBox.ItemsSource = colorListData;
            }

            if (fontListData == null)
            {
                fontListData = new List<string>();
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListData.Add(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
                fontListBox.ItemsSource = fontListData;
            }
        }

        private void colorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                string hex = e.AddedItems[0].ToString();
                selectedSpriteText.TextColor = ColorUtil.GetBrushFromHex(hex);
            }
        }

        private void fontListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count == 1)
            {
                string font = e.AddedItems[0].ToString();
                selectedSpriteText.Font = new FontFamily(font);
            }
        }

        //private void SetFont()
        //{
        //    if (!string.IsNullOrEmpty(App.CurrentInstance.SelectedFont))
        //    {
        //        //selectedSpriteText.Font = new FontFamily(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
        //        selectedSpriteText.Font = new FontFamily(App.CurrentInstance.SelectedFont);
        //        App.CurrentInstance.SelectedFont = string.Empty;
        //    }
        //    if (App.CurrentInstance.SelectedTextColor != null)
        //    {
        //        selectedSpriteText.TextColor = App.CurrentInstance.SelectedTextColor;
        //        App.CurrentInstance.SelectedTextColor = null;
        //    }
        //}

        #endregion

        #region AppBar

        AppBarButton appBarButton_ok = null;
        int appBarState = -1;//0-normal, 1-font, 

        private void BuildBottomAppBar_Normal()
        {
            if (appBarState == 0)
            {
                return;
            }

            this.bottomAppBar.PrimaryCommands.Clear();
            bottomAppBar.Visibility = Visibility.Collapsed;

            appBarState = 0;
        }

        private void BuildBottomAppBar_TextEditor()
        {
            if (appBarState == 1)
            {
                return;
            }

            this.bottomAppBar.PrimaryCommands.Clear();
            bottomAppBar.Visibility = Visibility.Visible;

            //ok
            if (appBarButton_ok == null)
            {
                appBarButton_ok = new AppBarButton();
                appBarButton_ok.Label = "确认";
                appBarButton_ok.Icon = new SymbolIcon(Symbol.Accept);
                appBarButton_ok.Click += AppbarButton_TextOK_Click;
            }
            this.bottomAppBar.PrimaryCommands.Add(appBarButton_ok);

            appBarState = 1;
        }

        private void AppbarButton_TextOK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AppbarButton_OK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GenerateImage();
        }

        //void AppbarButton_Font_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Frame.Navigate(typeof(FontPage));
        //}

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            SpriteControl.EdgeEnabled = App.CurrentInstance.GetSetting<bool>(Constants.KEY_EDGE, false);
            SpriteControl.ShadowEnabled = App.CurrentInstance.GetSetting<bool>(Constants.KEY_SHADOW, false);
            exportWidth = App.CurrentInstance.GetSetting<double>(Constants.KEY_EXPORT_WIDTH, 1248d);
        }

        #endregion




    }
}
