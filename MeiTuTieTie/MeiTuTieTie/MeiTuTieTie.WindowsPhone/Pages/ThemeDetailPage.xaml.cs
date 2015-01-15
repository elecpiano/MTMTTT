using Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Shared.Utility;
using Shared.Model;
using System.Threading.Tasks;
using Windows.Storage;
using System;

namespace MeiTuTieTie.Pages
{
    public sealed partial class ThemeDetailPage : Page
    {
        #region Property

        public const string MODULE = "theme";
        public const string THEME_PACK_ZIP_FILE_FORMAT = "theme_pack_{0}.zip";
        public const string MY_THEME_DATA_FILE = "my_theme_data";

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

        private async void Download()
        {
            progressPanel.Visibility = Visibility.Visible;
            downloadButton.Visibility = Visibility.Collapsed;

            if (fileDownloader == null)
            {
                fileDownloader = new FileDownloader();
            }

            ThemePack theme = this.rootGrid.GetDataContext<ThemePack>();

            if (theme != null)
            {
                string fileName = string.Format(THEME_PACK_ZIP_FILE_FORMAT, theme.id);
                var storageFile = await fileDownloader.Download(theme.zipUrl, MODULE, fileName, progressBar);
                progressPanel.Visibility = Visibility.Collapsed;
                downloadButton.Visibility = Visibility.Visible;

                string unZipfolderName = string.Format("theme\\{0}", theme.id);
                await UnZip(storageFile, unZipfolderName);
                await AddMyThemeData(theme);
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

        #region Update MyTheme Data

        DataLoader<MyThemeData> myThemeDataLoader = null;

        private async Task AddMyThemeData(ThemePack themePack)
        {
            if (myThemeDataLoader==null)
            {
                myThemeDataLoader = new DataLoader<MyThemeData>();
            }

            //load data file
            await IsolatedStorageHelper.EnsureFileExistence(MODULE, MY_THEME_DATA_FILE);
            var data = await myThemeDataLoader.LoadLocalData(MODULE, MY_THEME_DATA_FILE);
            if (data ==null)
            {
                data = new MyThemeData();
            }

            //add new theme
            MyTheme newTheme = new MyTheme();
            newTheme.id = themePack.id;
            newTheme.name = themePack.name;
            newTheme.thumbnail = themePack.thumbnailUrl;
            data.myThemes.Add(newTheme);

            //save data
            string json = JsonSerializer.Serialize(data);
            await IsolatedStorageHelper.WriteToFileAsync(MODULE, MY_THEME_DATA_FILE, json);
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
            progressPanel.Visibility = Visibility.Collapsed;
            downloadButton.Visibility = Visibility.Visible;
        }

        #endregion

    }
}
