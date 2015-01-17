using Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Shared.Utility;
using Shared.Model;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using Shared.Global;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MeiTuTieTie.Pages
{
    public sealed partial class ThemeDetailPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public ThemeDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadData(e.Parameter);
            return;

            MaterialGroup mg = new MaterialGroup();
            Material m = new Material() { image = "img", themePackID = "pack1", thumbnail = "thm", type = "tp", visible = true };
            mg.Materials.Add(m);
            mg.Materials.Add(m);
            var xx = XmlHelper.SerializeToString<MaterialGroup>(mg);

            return;
        }

        #endregion

        #region load data

        private void LoadData(object themeData)
        {
            this.rootGrid.DataContext = themeData;
        }

        #endregion

        #region File Download

        FileDownloader fileDownloader = null;
        bool downloadCanceled = false;

        private async void Download()
        {
            this.progressBar.Value = 0;
            progressPanel.Visibility = Visibility.Visible;
            downloadPanel.Visibility = Visibility.Collapsed;

            if (fileDownloader == null)
            {
                fileDownloader = new FileDownloader();
            }

            ThemePack theme = this.rootGrid.GetDataContext<ThemePack>();

            if (theme != null)
            {
                string fileName = string.Format(Constants.THEME_PACK_ZIP_FILE_FORMAT, theme.id);
                var storageFile = await fileDownloader.Download(theme.zipUrl, Constants.THEME_MODULE, fileName, progressBar);

                if (downloadCanceled)
                {
                    return;
                }

                string unZipfolderName = string.Format("{0}\\{1}", Constants.THEME_MODULE, theme.id);
                await UnZip(storageFile, unZipfolderName);
                //string thumbnailPath = await ImageHelper.DownloadThumbnail(theme.thumbnailUrl, ".png");
                await AddMyThemeData(theme);//, thumbnailPath);
                await AddMaterialData(theme, unZipfolderName);

                theme.Downloaded = true;

                progressPanel.Visibility = Visibility.Collapsed;
                downloadPanel.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Zip

        private async Task UnZip(StorageFile zipFile, string folderName)
        {
            try
            {
                var folder = await IsolatedStorageHelper.GetFolderAsync(folderName);
                await ZipHelper.UnZipFileAsync(zipFile, folder);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region MyTheme Data

        DataLoader<MyThemeData> myThemeDataLoader = null;

        private async Task AddMyThemeData(ThemePack themePack)//, string thumbnailPath)
        {
            if (myThemeDataLoader == null)
            {
                myThemeDataLoader = new DataLoader<MyThemeData>();
            }

            //load data file
            var data = await myThemeDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);
            if (data == null)
            {
                data = new MyThemeData();
            }

            if (data.myThemes.Any(x => x.id == themePack.id))
            {
                return;
            }

            //add new theme
            MyTheme newTheme = new MyTheme();
            newTheme.id = themePack.id;
            newTheme.name = themePack.name;
            newTheme.thumbnail = themePack.thumbnailUrl;// thumbnailPath;
            data.myThemes.Insert(0, newTheme);

            //save data
            string json = JsonSerializer.Serialize(data);
            await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE, json);
        }

        #endregion

        #region Material Data

        DataLoader<MaterialGroup> materialDataLoader = null;
        private async Task AddMaterialData(ThemePack theme, string folder)
        {
            //read theme pack materials file (xml)
            string path = Path.Combine(folder,"materials.xml");
            MaterialGroup newMaterials = await XmlHelper.Deserialize<MaterialGroup>(path);

            //load my material file
            if (materialDataLoader == null)
            {
                materialDataLoader = new DataLoader<MaterialGroup>();
            }
            var myMaterials = await materialDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE);
            if (myMaterials == null)
            {
                myMaterials = new MaterialGroup();
            }

            //add new materials
            foreach (var m in newMaterials.Materials)
            {
                m.themePackID = theme.id;

                //thumbnail
                string thumbnailPath = Path.Combine(folder, m.thumbnail);
                m.thumbnail = thumbnailPath;

                myMaterials.Materials.Add(m);
            }

            //save data
            string json = JsonSerializer.Serialize(myMaterials);
            await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE, json);
        }

        #endregion

        #region Button Click

        private void download_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Test();
            Download();
        }

        private void cancelDownload_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            fileDownloader.Cancel();
            downloadCanceled = true;

            progressPanel.Visibility = Visibility.Collapsed;
            downloadPanel.Visibility = Visibility.Visible;
        }

        #endregion

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


    }
}
