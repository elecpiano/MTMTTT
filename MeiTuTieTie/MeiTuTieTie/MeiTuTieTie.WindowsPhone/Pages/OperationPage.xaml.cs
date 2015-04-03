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
using Windows.ApplicationModel.Activation;
using Windows.Storage.Streams;
using Shared.Utility;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Shared.Control;
using Shared.Enum;
using Shared.Global;
using Windows.Media.Playback;
using Shared.Animation;
using Windows.UI.Popups;
using System.Linq;
using Shared.Model;
using System.Threading.Tasks;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page, IFileOpenPickerPageBase
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private OperationPageType pageType = OperationPageType.Single;

        private int ExistingPhotoCount
        {
            get
            {
                return SpriteControl.Sprites.Count(x => x.SpriteType == SpriteType.Photo);
            }
        }
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

        private CompositeTransform transformSingleModeImage
        {
            get
            {
                return imgSingleMode.RenderTransform as CompositeTransform;
            }
        }

        private bool _busy = true;
        private bool Busy
        {
            get
            {
                return _busy;
            }
            set
            {
                if (_busy != value)
                {
                    _busy = value;
                    busyMask.Visibility = _busy ? Visibility.Visible : Visibility.Collapsed;
                    //if (!_busy)
                    //{
                    //    VisualStateManager.GoToState(this, "vsAppBarShown", true);
                    //}
                }
            }
        }

        #endregion

        #region Lifecycle

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.CanGobackAsked += navigationHelper_CanGobackAsked;
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

            NavigationHelper.ActivePage = this.GetType();
        }

        private void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
            if (candidatePanelShown)
            {
                canceled = true;
                HideCandidatePanel();
                Busy = false;
            }
            else if (App.CurrentInstance.OpertationPageChanged)
            {
                canceled = true;
                CheckQuit();
            }
        }

        private async void CheckQuit()
        {
            var dialog = new MessageDialog("是否放弃当前编辑的图片？");
            var cmdOK = new UICommand("确定", (cmd) => navigationHelper.GoBack());
            var cmdCancel = new UICommand("取消");
            dialog.Commands.Add(cmdOK);
            dialog.Commands.Add(cmdCancel);
            dialog.CancelCommandIndex = 1;
            dialog.ShowAsync();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                if (pageType == OperationPageType.Single)
                {
                    App.CurrentInstance.ComingBackFrom = "OperationPage_Single";
                }
                else if (pageType == OperationPageType.Multi)
                {
                    App.CurrentInstance.ComingBackFrom = "OperationPage_Multi";
                }
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
            HideSystemAppBar();
            InitColorFontList();

            SpriteControl.Initialize(stage);
            SpriteControl.OnSelected += Sprite_OnSelected;
            SpriteControl.OnRemoved += Sprite_OnRemoved;
            SpriteControl.OnSpriteChanged += Sprite_OnSpriteChanged;
            SpriteControl.Holding += SpriteControl_Holding;
            SpriteControl.OnSpritePressed += SpriteControl_OnSpritePressed;

            VisualStateManager.GoToState(this, "vsLayerButtonShown", false);

            switch (pageType)
            {
                case OperationPageType.Single:
                    VisualStateManager.GoToState(this, "vsSingleModeButtons", false);
                    this.Frame.BackStack.RemoveAt(this.Frame.BackStack.Count - 1);
                    imgBeijingBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/DefaultBackgroundSingle.jpg", UriKind.Absolute));
                    PreapreSingleModeImage();
                    Busy = false;
                    break;
                case OperationPageType.Multi:
                    VisualStateManager.GoToState(this, "vsMultiModeButtons", false);
                    btnPhotoLock.Visibility = btnPhotoUnLock.Visibility = Visibility.Collapsed;
                    imgBeijingBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/DefaultBackgroundMulti.jpg", UriKind.Absolute));
                    CheckAndAddPhotos(App.CurrentInstance.HomePageMultiPhotoFiles);
                    break;
                default:
                    break;
            }

            App.CurrentInstance.OpertationPageChanged = true;
        }

        #endregion

        #region Image Processing

        double exportWidth = 960d;

        private async void GenerateImage()
        {
            if (Busy)
            {
                return;
            }
            Busy = true;

            SpriteControl.DismissActiveSprite();
            AppBarNormal();
            HideContextMenu();

            //http://social.technet.microsoft.com/wiki/contents/articles/20648.using-the-rendertargetbitmap-in-windows-store-apps-with-xaml-and-c.aspx
            RectangleGeometry cropArea = new RectangleGeometry() { Rect = new Rect(0d, 0d, stagePanel.ActualWidth, stagePanel.ActualHeight) };

            /* IMPORTANT: in order to get the expected result, the clipped element and the captured element should NOT be the same one */
            stagePanelForClipping.Clip = cropArea;

            //string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            //ImageHelper.CaptureToMediaLibrary(this.stagePanel, fileName, exportWidth);
            var bitmap = await ImageHelper.Capture(this.stagePanel, exportWidth);

            Frame.Navigate(typeof(ExportPage), bitmap);
            Busy = false;
        }

        #endregion

        #region Single Mode Image Manipulation

        private double SingleImageSizeMin = 120d;
        private double SingleModeImageScaleMin = 0.1d;
        private double SingleModeImageScaleMax = 2d;
        private double SingleModeImageTranslateX = 0d;
        private double SingleModeImageTranslateY = 0d;

        private void imgSingleMode_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = imgSingleModeGhost.Width = imgSingleMode.ActualWidth;
            double height = imgSingleModeGhost.Height = imgSingleMode.ActualHeight;
            double scaleMinX = SingleImageSizeMin / width;
            double scaleMinY = SingleImageSizeMin / height;
            SingleModeImageScaleMin = scaleMinX < scaleMinY ? scaleMinX : scaleMinY;

            SingleModeImageCenterPoint = new Point(imgSingleMode.ActualWidth / 2d, imgSingleMode.ActualHeight / 2d);
        }

        private void PreapreSingleModeImage()
        {
            imgSingleMode.Source = App.CurrentInstance.wbForSingleMode;
            imgSingleMode.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Scale | ManipulationModes.Rotate;
            imgSingleMode.ManipulationDelta += imgSingleMode_ManipulationDelta;
            imgSingleMode.ManipulationCompleted += imgSingleMode_ManipulationCompleted;
            imgSingleMode.PointerPressed += stageBackground_PointerPressed;
        }

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
            AppBarNormal();
            HideContextMenu();
        }

        void imgSingleMode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            bool isDrag = e.Delta.Rotation == 0 && e.Delta.Expansion == 0;
            if (isDrag)
            {
                transformSingleModeImageGhost.TranslateX += e.Delta.Translation.X;
                transformSingleModeImageGhost.TranslateY += e.Delta.Translation.Y;

                var transform = imgSingleModeGhost.TransformToVisual(stagePanel);
                var p = transform.TransformPoint(SingleModeImageCenterPoint);
                if (p.X < 0 || p.X > stagePanel.ActualWidth || p.Y < 0 || p.Y > stagePanel.ActualHeight)
                {
                    transformSingleModeImageGhost.TranslateX = transformSingleModeImage.TranslateX;
                    transformSingleModeImageGhost.TranslateY = transformSingleModeImage.TranslateY;
                    return;
                }

                transformSingleModeImage.TranslateX = transformSingleModeImageGhost.TranslateX;//+= e.Delta.Translation.X;
                transformSingleModeImage.TranslateY = transformSingleModeImageGhost.TranslateY;//+= e.Delta.Translation.Y;
            }
            else
            {
                transformSingleModeImageGhost.Rotation = transformSingleModeImage.Rotation += e.Delta.Rotation;
                transformSingleModeImageGhost.ScaleX = transformSingleModeImage.ScaleX *= e.Delta.Scale;
                transformSingleModeImageGhost.ScaleY = transformSingleModeImage.ScaleY *= e.Delta.Scale;
            }

            Test();
        }

        private void imgSingleMode_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (transformSingleModeImage.ScaleX > SingleModeImageScaleMax)
            {
                ScaleAnimation.ScaleTo(imgSingleMode, SingleModeImageScaleMax, SingleModeImageScaleMax, 200);
                transformSingleModeImageGhost.ScaleX = transformSingleModeImageGhost.ScaleY = SingleModeImageScaleMax;
            }
            else if (transformSingleModeImage.ScaleX < SingleModeImageScaleMin)
            {
                ScaleAnimation.ScaleTo(imgSingleMode, SingleModeImageScaleMin, SingleModeImageScaleMin, 200);
                transformSingleModeImageGhost.ScaleX = transformSingleModeImageGhost.ScaleY = SingleModeImageScaleMin;
            }

            App.CurrentInstance.OpertationPageChanged = true;
        }

        Point SingleModeImageCenterPoint = new Point(0, 0);
        private void Test()
        {
        }

        #endregion

        #region Load Photo

        private const int PhotoCountMax = 9;
        private int photoToProcess = 0;

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
            PickPhotos();
            HideContextMenu();
        }

        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_OperationPage_PickPhotos = "PickPhotos";
        //this method is ONLY used by the PickPhoto button, not for the initial photo picking under multi mode
        private void PickPhotos()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            picker.ContinuationData[Continuation_Key_Operation] = Continuation_OperationPage_PickPhotos;
            picker.PickMultipleFilesAndContinue();
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation)
                && args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_OperationPage_PickPhotos)
            {
                CheckAndAddPhotos(args.Files);
            }
        }

        private void CheckAndAddPhotos(IReadOnlyList<StorageFile> files)
        {
            int newPhotoCount = files.Count;
            if (newPhotoCount > 0)
            {
                Busy = true;
            }

            if ((ExistingPhotoCount + newPhotoCount) > PhotoCountMax)
            {
                ShowCandidateList(files);
            }
            else
            {
                //AddPhotosToStage(args);
                DelayExecutor.Delay(200d, () =>
                    {
                        AddPhotosToStage(files);
                    });
            }
        }

        private async void AddPhotosToStage(IEnumerable<StorageFile> files)
        {
            photoToProcess = files.Count();
            if (photoToProcess == 0)
            {
                Busy = false;
            }
            foreach (var file in files)
            {
                AddPhotoToStage(file);
            }
            App.CurrentInstance.OpertationPageChanged = true;
        }

        private async void AddPhotoToStage(StorageFile file)
        {
            string tempFileName = Guid.NewGuid().ToString();
            var resizedFile = await ImageHelper.MakeResizedImage(file, tempFileName, Constants.PHOTO_IMPORT_SIZE_MAX);

            IRandomAccessStream stream = await resizedFile.OpenAsync(FileAccessMode.Read);
            BitmapImage bi = new BitmapImage();
            bi.SetSource(stream);

            //sprite
            SpriteControl sprite = new SpriteControl(SpriteType.Photo);
            sprite.SetImage(bi);
            sprite.AddToContainer();

            //delete tempfile
            if (resizedFile != file)
            {
                await resizedFile.DeleteAsync();
            }

            photoToProcess--;
            if (photoToProcess == 0)
            {
                Busy = false;
            }
        }

        #endregion

        #region Candidate

        List<Candidate> candidates = new List<Candidate>();
        private bool candidatePanelShown = false;

        private async void ShowCandidateList(IReadOnlyList<StorageFile> files)
        {
            candidates.Clear();
            foreach (var file in files)
            {
                WriteableBitmap wb = await ImageHelper.Resize(file, 100);
                Candidate data = new Candidate();
                data.File = file;
                data.Thumbnail = wb;
                data.Selected = 0d;
                candidates.Add(data);
            }
            candidateListBox.ItemsSource = candidates;
            int availableCount = PhotoCountMax - ExistingPhotoCount - candidates.Count(x => x.Selected == 1);
            txtPhotoCount.Text = availableCount.ToString();

            ShowCandidatePanel();
        }

        private void candidateSelect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            int photoCount = ExistingPhotoCount + candidates.Count(x => x.Selected == 1);

            var data = sender.GetDataContext<Candidate>();
            if (data != null)
            {
                if (data.Selected == 0 && photoCount >= PhotoCountMax)
                {
                    return;
                }
                else
                {
                    data.Selected = data.Selected == 0d ? 1d : 0d;
                }
            }

            int availableCount = PhotoCountMax - ExistingPhotoCount - candidates.Count(x => x.Selected == 1);
            txtPhotoCount.Text = availableCount.ToString();
        }

        private void candidateOK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedCandidates = candidates.Where(x => x.Selected == 1d).Select(x => x.File).ToList();
            AddPhotosToStage(selectedCandidates);
            HideCandidatePanel();
        }

        private void ShowCandidatePanel()
        {
            candidatePanelShown = true;
            VisualStateManager.GoToState(this, "vsCandidateShown", true);
        }

        private void HideCandidatePanel()
        {
            candidatePanelShown = false;
            VisualStateManager.GoToState(this, "vsCandidateHidden", true);
            DelayExecutor.Delay(200,
                () =>
                {
                    candidateListBox.ItemsSource = null;
                    candidates.Clear();
                });
        }

        #endregion

        #region Shipin, Biankuang, Beijing

        private void Shipin_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.Shipin);
            HideContextMenu();
        }

        private void Biankuang_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.BianKuang);
            HideContextMenu();
        }

        private void Beijing_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WidgetPage), WidgetPageType.Beijing);
            HideContextMenu();
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
            if (App.CurrentInstance.SelectedDIYBackground != null)
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
                SpriteControl.SelectedSprite.MoveZIndex(true);
            }
            HideContextMenu();
        }

        private void spriteDown_Click(object sender, TappedRoutedEventArgs e)
        {
            if (SpriteControl.SelectedSprite != null)
            {
                SpriteControl.SelectedSprite.MoveZIndex(false);
            }
            HideContextMenu();
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

            if (SingleImageLocked)
            {
                lightTip.ShowTip("当前图片已锁定");
            }
            HideContextMenu();
        }

        private void spriteFuncOff_Click(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "vsLayerButtonHidden", true);
            HideContextMenu();
        }

        private void spriteFuncOn_Click(object sender, TappedRoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "vsLayerButtonShown", true);
            HideContextMenu();
        }

        private void spriteContextMenu_Tapped(object sender, string type)
        {
            if (SpriteControl.SelectedSprite == null)
            {
                return;
            }

            switch (type)
            {
                case "up_most":
                    SpriteControl.SelectedSprite.ZIndexUpMost();
                    break;
                case "down_most":
                    SpriteControl.SelectedSprite.ZIndexDownMost();
                    break;
                case "copy":
                    CopySprite();
                    break;
                case "delete":
                    SpriteControl.RemoveSelectedSprite();
                    break;
                default:
                    break;
            }
            HideContextMenu();
        }

        #endregion

        #region Context Menu

        private void SpriteControl_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (textEditing)
            {
                return;
            }
            var x = e.GetPosition(stagePanel).X;
            var y = e.GetPosition(stagePanel).Y;
            spriteContextMenu.Show(stagePanel, x, y);
        }

        void SpriteControl_OnSpritePressed(object sender, PointerRoutedEventArgs e)
        {
            HideContextMenu();
        }

        private void HideContextMenu()
        {
            spriteContextMenu.Hide();
        }

        private void CopySprite()
        {
            SpriteControl sprite = null;
            if (SpriteControl.SelectedSprite.SpriteType == SpriteType.Photo)
            {
                sprite = new SpriteControl(SpriteType.Photo);
                sprite.SetImage(SpriteControl.SelectedSprite.ImageSource);
            }
            else if (SpriteControl.SelectedSprite.SpriteType == SpriteType.Material)
            {
                sprite = new SpriteControl(SpriteType.Material);
                sprite.SetImage(SpriteControl.SelectedSprite.ImageSource);
            }
            else if (SpriteControl.SelectedSprite.SpriteType == SpriteType.Text)
            {
                sprite = new SpriteControl(SpriteType.Text);
                sprite.EditingStarted += sprite_EditingStarted;
                sprite.EditingEnded += sprite_EditingEnded;
                sprite.spriteText.Text = selectedSpriteText.Text;
                sprite.spriteText.TextColor = selectedSpriteText.TextColor;
                sprite.spriteText.Font = selectedSpriteText.Font;
            }

            sprite.AddToContainer();

            App.CurrentInstance.OpertationPageChanged = true;

            if (sfxEnabled)
            {
                PlaySFX();
            }
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
        private bool textEditing = false;

        private void Text_Click(object sender, RoutedEventArgs e)
        {
            AddTextSprite();
            HideContextMenu();
        }

        private void AddTextSprite()
        {
            SpriteControl sprite = new SpriteControl(SpriteType.Text);
            sprite.EditingStarted += sprite_EditingStarted;
            sprite.EditingEnded += sprite_EditingEnded;
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
                HideSystemAppBar();
            }
            else
            {
                if (sprite.SpriteType == SpriteType.Text)
                {
                    selectedSpriteText = (sender as SpriteControl).spriteText;
                    VisualStateManager.GoToState(this, "vsTextModeButtons", false);

                    //set font list index
                    int index = 0;
                    string fontName = selectedSpriteText.Font.Source;
                    if (fontListData.Contains(fontName))
                    {
                        index = fontListData.IndexOf(fontName);
                    }
                    fontListBox.SelectedIndex = index;
                    fontListBox.ScrollIntoView(fontName);

                    //set color list index
                    string colorHex = ColorUtil.GetHexFromBrush(selectedSpriteText.TextColor as SolidColorBrush).ToLower();
                    if (colorListData.Contains(colorHex))
                    {
                        index = colorListData.IndexOf(colorHex);
                    }
                    colorListBox.SelectedIndex = index;
                    colorListBox.ScrollIntoView(colorHex);

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
                AppBarNormal();
            }
        }

        void Sprite_OnSpriteChanged(object sender, EventArgs e)
        {
            App.CurrentInstance.OpertationPageChanged = true;
        }

        void sprite_EditingStarted(object sender, EventArgs e)
        {
            textEditing = true;
            BuildSystemAppBar_TextEditor();
            DelayExecutor.Delay(20, () =>
                {
                    (sender as SpriteControl).SyncButtonsPosition();
                });
        }

        void sprite_EditingEnded(object sender, EventArgs e)
        {
            HideSystemAppBar();
            DelayExecutor.Delay(20, () =>
            {
                (sender as SpriteControl).SyncButtonsPosition();
                textEditing = false;
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
            HideContextMenu();
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
            HideContextMenu();
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

        private void HideSystemAppBar()
        {
            this.bottomAppBar.PrimaryCommands.Clear();
            bottomAppBar.Visibility = Visibility.Collapsed;

            appBarPanel.Visibility = Visibility.Visible;
            colorListPanel.Visibility = Visibility.Visible;
            fontListPanel.Visibility = Visibility.Visible;
        }

        private void BuildSystemAppBar_TextEditor()
        {
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

            appBarPanel.Visibility = Visibility.Collapsed;
            colorListPanel.Visibility = Visibility.Collapsed;
            fontListPanel.Visibility = Visibility.Collapsed;
        }

        private void AppbarButton_TextOK_Click(object sender, RoutedEventArgs e)
        {
            //no need to do anything
        }

        private void AppbarButton_OK_Tapped(object sender, TappedRoutedEventArgs e)
        {
            GenerateImage();
        }

        private void AppBarNormal()
        {
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
            exportWidth = App.CurrentInstance.GetSetting<double>(Constants.KEY_EXPORT_WIDTH, 960d);
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
