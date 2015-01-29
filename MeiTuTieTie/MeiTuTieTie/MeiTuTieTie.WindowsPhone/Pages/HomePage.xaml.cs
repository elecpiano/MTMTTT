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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class HomePage : Page, IFileOpenPickerPageBase
    {
        #region Lifecycle

        public HomePage()
        {
            this.InitializeComponent();
            HideStatusBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateScreenSize();
        }

        #endregion

        #region Tile Click

        private void singlePic_Click(object sender, RoutedEventArgs e)
        {
            PickPhoto();
        }

        private void multiPic_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OperationPage), OperationPageType.Multi);
        }

        private void boutique_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BoutiquePage));
        }

        private void more_Click(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region Screen Size Related

        private void UpdateScreenSize()
        {
            Application.Current.Resources["ScreenWidthHalf"] = Window.Current.Bounds.Width * 0.5d;
            Application.Current.Resources["MaterialHeight"] = (Window.Current.Bounds.Width - 48d) / 3d;
        }

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
        private const string Continuation_HomePage_PickPhoto = "PickPhotos";

        private void PickPhoto()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.ContinuationData[Continuation_Key_Operation] = Continuation_HomePage_PickPhoto;
            //picker.PickMultipleFilesAndContinue();
            picker.PickSingleFileAndContinue();
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation)
                && args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_HomePage_PickPhoto)
            {
                if (args.Files != null && args.Files.Count == 1)
                {
                    var file = args.Files[0];
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    //BitmapImage bi = new BitmapImage();
                    //bi.SetSource(stream);

                    Frame.Navigate(typeof(PhotoEditPage), stream);
                }
            }
        }

        #endregion


        #region Test

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

        //private void Test5()
        //{
        //    this.imageSwitch.Checked = !this.imageSwitch.Checked;
        //}

        //private void ImageSwitch_CheckStateChanged(ImageSwitch sender, bool suggestedState)
        //{
        //    ImageSwitch control = sender as ImageSwitch;
        //    if (control.Checked != suggestedState)
        //    {
        //        control.Checked = !control.Checked;
        //    }
        //}

        private async void Test6()
        {
            spriteTextBox.Font = new FontFamily(@"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic");
            spriteTextBox.TextColor = new SolidColorBrush(Color.FromArgb(255, 190, 30, 0));
        }

        #endregion

    }
}
