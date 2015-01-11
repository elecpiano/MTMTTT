using Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Shared.Utility;
using Shared.Model;

namespace MeiTuTieTie.Pages
{
    public sealed partial class ThemeDetailPage : Page
    {
        #region Property

        public const string MODULE = "theme";
        public const string THEME_FILE_FORMAT = "theme_pack_{0}";

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

        private void Download()
        {
            if (fileDownloader == null)
            {
                fileDownloader = new FileDownloader();
            }

            ThemePack theme = this.rootGrid.GetDataContext<ThemePack>();
            if (theme != null)
            {
                string fileName = string.Format(THEME_FILE_FORMAT, theme.id);
                fileDownloader.Download(theme.zipUrl, MODULE, fileName, progressBar,
                    () =>
                    {
                        int i = 0;
                        i++;
                    });
            }
        }

        #endregion

        private void download_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            progressPanel.Visibility = Visibility.Visible;
            downloadButton.Visibility = Visibility.Collapsed;
            Download();
        }

        private void cancelDownload_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            progressPanel.Visibility = Visibility.Collapsed;
            downloadButton.Visibility = Visibility.Visible;

        }

    }
}
