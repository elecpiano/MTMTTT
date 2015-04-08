using Shared.Common;
using Shared.Enum;
using Shared.Global;
using Shared.Model;
using Shared.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel.Activation;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class HomePage : Page, IFileOpenPickerPageBase
    {
        #region Property

        #endregion

        #region Lifecycle

        public HomePage()
        {
            this.InitializeComponent();
            this.Loaded += HomePage_Loaded;
            HideStatusBar();
        }

        void HomePage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TryShowWelcome();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.ActivePage = this.GetType();

            base.OnNavigatedTo(e);
            LoadSettings();

            if (e.NavigationMode == NavigationMode.New)
            {
                TryBuildInMaterials();
            }
            else if (e.NavigationMode == NavigationMode.Back)
            {
                if (App.CurrentInstance.ComingBackFrom == "PhotoEditPage" || App.CurrentInstance.ComingBackFrom == "OperationPage_Single")
                {
                    PickPhotos(true);
                }
                else if (App.CurrentInstance.ComingBackFrom == "OperationPage_Multi")
                {
                    PickPhotos(false);
                }
                App.CurrentInstance.ComingBackFrom = string.Empty;
            }

            UmengSDK.UmengAnalytics.TrackPageStart(this.GetType().ToString());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengSDK.UmengAnalytics.TrackPageEnd(this.GetType().ToString());
        }

        #endregion

        #region Tile Click

        private void singlePhoto_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PickPhotos(true);
        }

        private void multiPhoto_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            PickPhotos(false);
        }

        private void boutique_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            return;
            Frame.Navigate(typeof(BoutiquePage));
        }

        private void dailyAD_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            //Frame.Navigate(typeof(PhotoPickerPage));
        }

        #endregion

        #region Screen Size Related

        //private void UpdateScreenSize()
        //{
        //    Application.Current.Resources["ScreenWidthHalf"] = Window.Current.Bounds.Width * 0.5d;
        //    Application.Current.Resources["MaterialHeight"] = (Window.Current.Bounds.Width - 48d) / 3d;
        //}

        #endregion

        #region StatusBar

        private void HideStatusBar()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.HideAsync();
        }

        #endregion

        #region Load Photo

        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_HomePage_PickPhotoSingle = "PickPhotoForSingle";
        private const string Continuation_HomePage_PickPhotoMulti = "PickPhotoForMulti";

        private void PickPhotos(bool single)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            if (single)
            {
                picker.ContinuationData[Continuation_Key_Operation] = Continuation_HomePage_PickPhotoSingle;
                picker.PickSingleFileAndContinue();
            }
            else
            {
                picker.ContinuationData[Continuation_Key_Operation] = Continuation_HomePage_PickPhotoMulti;
                picker.PickMultipleFilesAndContinue();
            }
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation))
            {
                if (args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_HomePage_PickPhotoSingle)
                {
                    if (args.Files != null && args.Files.Count == 1)
                    {
                        App.CurrentInstance.HomePageMultiPhotoFiles = args.Files;
                        Frame.Navigate(typeof(PhotoEditPage));
                    }
                }
                else if (args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_HomePage_PickPhotoMulti)
                {
                    if (args.Files != null && args.Files.Count > 0)
                    {
                        App.CurrentInstance.HomePageMultiPhotoFiles = args.Files;
                        Frame.Navigate(typeof(OperationPage), OperationPageType.Multi);
                    }
                }
            }
        }

        #endregion

        #region Test
        private void test_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
        }

        /* Material XML Test

        private async Task<MaterialGroup> Test()
        {
            MaterialGroup val;
            string content = string.Empty;

            var uri = new System.Uri("ms-appx:///Assets/materials.xml");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenStreamForReadAsync();

            using (StreamReader streamReader = new StreamReader(stream))
            {
                content = streamReader.ReadToEnd();
            }

            using (StringReader sr = new StringReader(content))
            {
                XmlSerializer xs = new XmlSerializer(typeof(MaterialGroup));
                val = (MaterialGroup)xs.Deserialize(sr);
            }

            return val;
        }

        private async Task Test2()
        {
            MaterialGroup val;
            string content = string.Empty;

            var uri = new System.Uri("ms-appx:///Assets/materials.xml");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenStreamForReadAsync();

            try
            {

                XDocument xdoc = XDocument.Load(stream);
                var elements = xdoc.Elements();
                foreach (var node in elements)
                {
                    var xxx = node.Element("thumbnailname");
                }
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private async Task Test3()
        {
            MaterialGroup val;
            string content = string.Empty;

            var uri = new System.Uri("ms-appx:///Assets/materials.xml");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenStreamForReadAsync();
            using (StreamReader streamReader = new StreamReader(stream))
            {
                content = streamReader.ReadToEnd();
            }

            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(content);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        private async Task Test4()
        {
            MaterialGroup val;
            string content = string.Empty;

            var uri = new System.Uri("ms-appx:///Assets/materials.xml");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenStreamForReadAsync();
            using (StreamReader streamReader = new StreamReader(stream))
            {
                content = streamReader.ReadToEnd();
            }

            try
            {
                Material material = null;
                string id = string.Empty;
                string name = string.Empty;
                string thumbnailname = string.Empty;

                XmlHelper.FastIterate(content,
                    (propertyName, propertyValue) =>
                    {
                        switch (propertyName)
                        {
                            case "id":
                                id = propertyValue;
                                break;
                            case "name":
                                name = propertyValue;
                                break;
                            case "thumbnailname":
                                thumbnailname = propertyValue;
                                break;
                            case "material":
                                material = new Material();
                                material.type = id;
                                material.image = name;
                                material.thumbnail = thumbnailname;
                                break;
                            default:
                                break;
                        }
                    });
            }
            catch (Exception)
            {
            }
        }

        */

        /* font test

        List<string> fontList = null;
        int fontIndex = 0;
        private async void ChangeFont()
        {
            if (fontList==null)
            {
                fontList = new List<string>();
                //fontList.Add("");
                fontList.Add("Arial");
                fontList.Add("Arial Black");
                fontList.Add("Arial Unicode MS");
                fontList.Add("Batang");
                fontList.Add("BatangChe");
                fontList.Add("Calibri");
                fontList.Add("Cambria");
                fontList.Add("Cambria / Cambria Math");
                fontList.Add("Cambria Math");
                fontList.Add("Candara");
                fontList.Add("Comic Sans MS");
                fontList.Add("Consolas");
                fontList.Add("Constantia");
                fontList.Add("Corbel");
                fontList.Add("Courier New");
                fontList.Add("DengXian");
                fontList.Add("DFKai-SB");
                fontList.Add("Dotum");
                fontList.Add("DutumChe");
                fontList.Add("Ebrima");
                fontList.Add("Estrangelo Edessa");
                fontList.Add("FangSong");
                fontList.Add("Gadugi");
                fontList.Add("Georgia");
                fontList.Add("GulimChe");
                fontList.Add("Gungsuh");
                fontList.Add("GungsuhChe");
                fontList.Add("KaiTi");
                fontList.Add("Khmer UI");
                fontList.Add("Lao UI");
                fontList.Add("Leelawadee");
                fontList.Add("Lucida Sans Unicode");
                fontList.Add("Malgun Gothic");
                fontList.Add("Meiryo");
                fontList.Add("Microsoft Himalaya");
                fontList.Add("Microsoft JhengHei");
                fontList.Add("Microsoft Mhei");
                fontList.Add("Microsoft NeoGothic");
                fontList.Add("Microsoft New Tai Lue");
                fontList.Add("Microsoft Tai Le");
                fontList.Add("Microsoft Uighur");
                fontList.Add("Microsoft YaHei");
                fontList.Add("Microsoft Yi Baiti");
                fontList.Add("MingLiU");
                fontList.Add("MingLiu_HKSCS");
                fontList.Add("MingLiu_HKSCS-ExtB");
                fontList.Add("MingLiu-ExtB");
                fontList.Add("Mongolian Baiti");
                fontList.Add("MS Gothic");
                fontList.Add("MS Mincho");
                fontList.Add("MS PGothic");
                fontList.Add("MS PMincho");
                fontList.Add("MS UI Gothic");
                fontList.Add("MV Boli");
                fontList.Add("Nirmala UI");
                fontList.Add("NSimSun");
                fontList.Add("NSimSun-18030");
                fontList.Add("PhagsPa");
                fontList.Add("PMingLiU");
                fontList.Add("PMingLiu-ExtB");
                fontList.Add("Segoe UI");
                fontList.Add("Segoe UI Symbol");
                fontList.Add("Segoe WP");
                fontList.Add("SimHei");
                fontList.Add("SimSun");
                fontList.Add("SimSun-18030");
                fontList.Add("SimSun-ExtB");
                fontList.Add("Symbol");
                fontList.Add("Tahoma");
                fontList.Add("Times New Roman");
                fontList.Add("Trebuchet MS");
                fontList.Add("Urdu Typesetting");
                fontList.Add("Verdana");
                fontList.Add("Webdings");
                fontList.Add("Wingdings");
                fontList.Add("Wingdings 2");
                fontList.Add("Wingdings 3");
                fontList.Add("Yu Gothic");
            }

            if (fontIndex>=fontList.Count)
            {
                fontIndex = 0;
            }

            string font = fontList[fontIndex];
            fontText.FontFamily = new FontFamily(font);
            fontIndexText.Text = fontIndex.ToString();

            fontIndex++;
        }

        */

        #endregion

        #region App Bar

        private void settings_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            materialBuiltIn = App.CurrentInstance.GetSetting<bool>(Constants.KEY_MATERIAL_BUILT_IN, false);
            welcomeShown = App.CurrentInstance.GetSetting<bool>(Constants.KEY_WELCOME_SHOWN, false);
        }

        #endregion

        #region Built in materials

        private bool materialBuiltIn = false;

        private async void TryBuildInMaterials()
        {
            if (!materialBuiltIn)
            {
                //copy files
                List<string> files = new List<string>();

                files.Add("theme/my_theme_data.txt");
                files.Add("theme/0/material_data.txt");

                for (int i = 1; i <= 18; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/biankuang_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_biankuang_{0}.png", index));
                }

                for (int i = 0; i <= 17; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/beijing_{0}.jpg", index));
                    files.Add(string.Format("theme/0/thumbnail_beijing_{0}.png", index));
                }

                for (int i = 1; i <= 27; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/gaoxiaobiaoqing_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_gaoxiaobiaoqing_{0}.png", index));
                }

                for (int i = 1; i <= 42; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/keai_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_keai_{0}.png", index));
                }

                for (int i = 1; i <= 12; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/zhedang_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_zhedang_{0}.png", index));
                }

                for (int i = 1; i <= 27; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/wenzi_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_wenzi_{0}.png", index));
                }

                for (int i = 1; i <= 15; i++)
                {
                    string index = i.ToString().PadLeft(2, '0');
                    files.Add(string.Format("theme/0/katongxingxiang_{0}.png", index));
                    files.Add(string.Format("theme/0/thumbnail_katongxingxiang_{0}.png", index));
                }

                foreach (var file in files)
                {
                    await IsolatedStorageHelper.CopyContentFileToLocalFolder(file);
                }

                materialBuiltIn = true;
                App.CurrentInstance.UpdateSetting(Constants.KEY_MATERIAL_BUILT_IN, true);
            }
        }

        #endregion

        #region Welcome

        private bool welcomeShown = false;

        private void TryShowWelcome()
        {
            if (!welcomeShown)
            {
                Frame.Navigate(typeof(WelcomePage));
                welcomeShown = true;
                App.CurrentInstance.UpdateSetting(Constants.KEY_WELCOME_SHOWN, true);
            }
        }

        #endregion

        private void Grid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Grid_PointerPressed_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Grid_PointerPressed_2(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

    }
}
