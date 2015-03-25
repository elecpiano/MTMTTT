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
using Windows.Media.Playback;

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
                if (App.CurrentInstance.SelectedDIYBackground != null)
                {
                    DIYBackground();
                }
                else if (App.CurrentInstance.SelectedMaterial != null)
                {
                    MaterialToSprite();
                }
            }

        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //PreventStrangeSFXBehavior();
            base.OnNavigatingFrom(e);
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
                    btnPhotoLock.Visibility = btnPhotoUnLock.Visibility = Visibility.Collapsed;
                    PickPhotos();
                    break;
                default:
                    break;
            }

            SpriteControl.Initialize(stage);
            SpriteControl.OnSelected += Sprite_OnSelected;
            SpriteControl.OnRemoved += Sprite_OnRemoved;
            SpriteControl.OnSpriteChanged += Sprite_OnSpriteChanged;

            InitColorFontList();

            VisualStateManager.GoToState(this, "vsLayerButtonShown", true);
            BuildBottomAppBar_Normal();

            App.CurrentInstance.OpertationPageChanged = true;
        }

        #endregion

        #region Image Processing

        double exportWidth = 1248d;

        private async void GenerateImage()
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

            App.CurrentInstance.OpertationPageChanged = true;
        }

        #endregion

        #region Load Photo

        private bool initialPick = true;

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
            VisualStateManager.GoToState(this, "vsColorFontHidden", true);
            colorListShown = fontListShown = false;

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
                if (initialPick && args.Files.Count == 0)
                {
                    navigationHelper.GoBack();
                }
                else
                {
                    initialPick = false;
                    //AddPhotosToStage(args);
                    DelayExecutor.Delay(200d, () => AddPhotosToStage(args));
                }
            }
        }

        private async void AddPhotosToStage(FileOpenPickerContinuationEventArgs args)
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

                App.CurrentInstance.OpertationPageChanged = true;
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
                    //imgBeijing.Source = bi;
                    imgBeijingBrush.ImageSource = bi;
                }

                App.CurrentInstance.SelectedMaterial = null;
                App.CurrentInstance.OpertationPageChanged = true;

                if (sfxEnabled)
                {
                    PlaySFX();
                }
            }
        }

        private void DIYBackground()
        {
            if (App.CurrentInstance.SelectedDIYBackground!=null)
            {
                imgBeijingBrush.ImageSource = App.CurrentInstance.SelectedDIYBackground;
                App.CurrentInstance.SelectedDIYBackground = null;
                App.CurrentInstance.OpertationPageChanged = true;

                if (sfxEnabled)
                {
                    PlaySFX();
                }
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

        //private void InsertSprites()
        //{
        //    SpriteControl sprite = null;

        //    for (int i = 0; i < 1; i++)
        //    {
        //        sprite = new SpriteControl(SpriteType.Photo);
        //        sprite.SetImage("ms-appx:///Assets/TestImages/TestImage001.jpg");
        //        sprites.Add(sprite);

        //        SpriteControl.Initialize(stage);
        //    }
        //}

        //ObservableCollection<BitmapImage> photoList = new ObservableCollection<BitmapImage>();
        //private async void LoadPhotos()
        //{
        //    //picListBox.ItemsSource = photoList;
        //    StorageFolder PictureFolder = KnownFolders.CameraRoll;
        //    var files = await PictureFolder.GetFilesAsync();
        //    foreach (var file in files)
        //    {
        //        BitmapImage bm = new BitmapImage();
        //        var thumbnail = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
        //        bm.SetSource(thumbnail);
        //        photoList.Add(bm);
        //    };
        //}

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
            App.CurrentInstance.OpertationPageChanged = true;

            if (sfxEnabled)
            {
                PlaySFX();
            }
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

                    //display the color list by defualt
                    VisualStateManager.GoToState(this, "vsColorListShown", true);
                    colorListShown = true;
                    fontListShown = false;
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

        void Sprite_OnRemoved(object sender, EventArgs e)
        {
            SpriteControl sprite = sender as SpriteControl;
            if (sprite.SpriteType == SpriteType.Text)
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

        void Sprite_OnSpriteChanged(object sender, EventArgs e)
        {
            App.CurrentInstance.OpertationPageChanged = true;
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
                colorListData.Add("#ffbf3334");
                colorListData.Add("#fff6b9cb");
                colorListData.Add("#ffe95289");
                colorListData.Add("#ffef99b4");
                colorListData.Add("#ffc596c2");
                colorListData.Add("#ff9559a1");
                colorListData.Add("#ff087cc7");
                colorListData.Add("#ff28c0ff");
                colorListData.Add("#ff6cc9f1");
                colorListData.Add("#ffd2ecf9");
                colorListData.Add("#ff26d7b3");
                colorListData.Add("#ff3de99f");
                colorListData.Add("#ff36c829");
                colorListData.Add("#ff67ba5e");
                colorListData.Add("#ffb0f14b");
                colorListData.Add("#ffe7f581");
                colorListData.Add("#ffaaa447");
                colorListData.Add("#ffe6e9b4");
                colorListData.Add("#fffedf13");
                colorListData.Add("#fff59f0e");
                colorListData.Add("#ff7c6b5b");
                colorListData.Add("#ffc6bbb5");
                colorListData.Add("#ff969593");
                colorListData.Add("#ffbec6c2");

                colorListBox.ItemsSource = colorListData;
            }

            if (fontListData == null)
            {
                fontListData = new List<string>();
                fontListData.Add("Arial");
                fontListData.Add("Arial Black");
                fontListData.Add("Arial Unicode MS");
                fontListData.Add("Batang");
                fontListData.Add("BatangChe");
                fontListData.Add("Calibri");
                fontListData.Add("Cambria");
                fontListData.Add("Cambria / Cambria Math");
                fontListData.Add("Cambria Math");
                fontListData.Add("Candara");
                fontListData.Add("Comic Sans MS");
                fontListData.Add("Consolas");
                fontListData.Add("Constantia");
                fontListData.Add("Corbel");
                fontListData.Add("Courier New");
                fontListData.Add("DengXian");
                fontListData.Add("DFKai-SB");
                fontListData.Add("Dotum");
                fontListData.Add("DutumChe");
                fontListData.Add("Ebrima");
                fontListData.Add("Estrangelo Edessa");
                fontListData.Add("FangSong");
                fontListData.Add("Gadugi");
                fontListData.Add("Georgia");
                fontListData.Add("GulimChe");
                fontListData.Add("Gungsuh");
                fontListData.Add("GungsuhChe");
                fontListData.Add("KaiTi");
                fontListData.Add("Khmer UI");
                fontListData.Add("Lao UI");
                fontListData.Add("Leelawadee");
                fontListData.Add("Lucida Sans Unicode");
                fontListData.Add("Malgun Gothic");
                fontListData.Add("Meiryo");
                fontListData.Add("Microsoft Himalaya");
                fontListData.Add("Microsoft JhengHei");
                fontListData.Add("Microsoft Mhei");
                fontListData.Add("Microsoft NeoGothic");
                fontListData.Add("Microsoft New Tai Lue");
                fontListData.Add("Microsoft Tai Le");
                fontListData.Add("Microsoft Uighur");
                fontListData.Add("Microsoft YaHei");
                fontListData.Add("Microsoft Yi Baiti");
                fontListData.Add("MingLiU");
                fontListData.Add("MingLiu_HKSCS");
                fontListData.Add("MingLiu_HKSCS-ExtB");
                fontListData.Add("MingLiu-ExtB");
                fontListData.Add("Mongolian Baiti");
                fontListData.Add("MS Gothic");
                fontListData.Add("MS Mincho");
                fontListData.Add("MS PGothic");
                fontListData.Add("MS PMincho");
                fontListData.Add("MS UI Gothic");
                fontListData.Add("MV Boli");
                fontListData.Add("Nirmala UI");
                fontListData.Add("NSimSun");
                fontListData.Add("NSimSun-18030");
                fontListData.Add("PhagsPa");
                fontListData.Add("PMingLiU");
                fontListData.Add("PMingLiu-ExtB");
                fontListData.Add("Segoe UI");
                fontListData.Add("Segoe UI Symbol");
                fontListData.Add("Segoe WP");
                fontListData.Add("SimHei");
                fontListData.Add("SimSun");
                fontListData.Add("SimSun-18030");
                fontListData.Add("SimSun-ExtB");
                fontListData.Add("Symbol");
                fontListData.Add("Tahoma");
                fontListData.Add("Times New Roman");
                fontListData.Add("Trebuchet MS");
                fontListData.Add("Urdu Typesetting");
                fontListData.Add("Verdana");
                fontListData.Add("Webdings");
                fontListData.Add("Wingdings");
                fontListData.Add("Wingdings 2");
                fontListData.Add("Wingdings 3");
                fontListData.Add("Yu Gothic");

                fontListBox.ItemsSource = fontListData;
            }
        }

        private void SetFont_TTF()
        {
            //selectedSpriteText.Font = new FontFamily(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
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
            sfxEnabled = App.CurrentInstance.GetSetting<bool>(Constants.KEY_SFX, false);
        }

        #endregion

        #region SFX

        private bool sfxEnabled = false;
        MediaElement mediaElement = null;

        private async void PlaySFX()
        {
            BackgroundMediaPlayer.Current.SetUriSource(new Uri("ms-appx:///Assets/Audio/MaterialSFX.wav"));
            BackgroundMediaPlayer.Current.Play();
        }

        //private async void PlaySFX()
        //{
        //    if (mediaElement == null)
        //    {
        //        mediaElement = new MediaElement();
        //        //mediaElement.AutoPlay = false;
        //        mediaElement.Source = new Uri("ms-appx:///Assets/Audio/MaterialSFX.wav", UriKind.Absolute);
        //        this.gridRoot.Children.Add(mediaElement);

        //        //string uri = "ms-appx:///Assets/Audio/MaterialSFX.mp3";
        //        //var rass = RandomAccessStreamReference.CreateFromUri(new Uri(uri, UriKind.RelativeOrAbsolute));
        //        //IRandomAccessStream stream = await rass.OpenReadAsync();
        //        //mediaElement.SetSource(stream, "");
        //    }

        //    mediaElement.AutoPlay = true;
        //    mediaElement.Play();
        //}

        //private void PreventStrangeSFXBehavior()
        //{
        //    if (mediaElement != null)
        //    {
        //        mediaElement.AutoPlay = false;
        //    }
        //}

        #endregion

    }
}
