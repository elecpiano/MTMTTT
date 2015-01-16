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
                await AddMyThemeData(theme);
                await AddMaterialData(theme.id);

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
                var folder = await IsolatedStorageHelper.CreateFolderAsync(folderName);
                await ZipHelper.UnZipFileAsync(zipFile, folder);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region MyTheme Data

        DataLoader<MyThemeData> myThemeDataLoader = null;

        private async Task AddMyThemeData(ThemePack themePack)
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
            newTheme.thumbnail = themePack.thumbnailUrl;
            data.myThemes.Insert(0, newTheme);

            //save data
            string json = JsonSerializer.Serialize(data);
            await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE, json);
        }

        #endregion

        #region Gadget Data

        DataLoader<MaterialData> materialDataLoader = null;
        private async Task AddMaterialData(string themeID)
        {
            MaterialData md = new MaterialData();
            Material m = new Material() { image = "img", themePackID = "pack1", thumbnail = "thm", type = "tp", visible = true };
            md.materials.Add(m);
            var xx = XmlHelper.SerializeToString<MaterialData>(md);

            //load materials file
            string path = string.Format("{0}\\{1}", themeID, Constants.MATERIAL_FILE_FORMAT);
            MaterialData materials = await XmlHelper.Deserialize<MaterialData>(Constants.THEME_MODULE, path);
        }

        #endregion

        #region Button Click

        private void download_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
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

    }
}
