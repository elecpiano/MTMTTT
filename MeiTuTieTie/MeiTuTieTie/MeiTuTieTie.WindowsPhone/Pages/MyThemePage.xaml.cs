using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;
using Windows.Graphics.Display;

namespace MeiTuTieTie.Pages
{
    public sealed partial class MyThemePage : Page
    {
        #region Property

        public const string MODULE = "theme";
        public const string CACHE_FILE = "theme_data";

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public MyThemePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadData();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            }
            else
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            }
            base.OnNavigatingFrom(e);
        }

        #endregion

        #region Data

        DataLoader<ThemePacksData> dataLoader = null;

        private async void LoadData()
        {
            if (dataLoader == null)
            {
                dataLoader = new DataLoader<ThemePacksData>();
            }

            var data = await dataLoader.LoadLocalData(MODULE, CACHE_FILE);
        }

        #endregion

        #region Theme Detail View

        private bool themeDetailViewShown = false;

        private void theme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ThemeDetailPage), sender.GetDataContext());
        }

        #endregion

        #region App Bar

        private void selectItems_Click(object sender, RoutedEventArgs e)
        {
            XmlSerializer serializer;

            DisplayInformation di = DisplayInformation.GetForCurrentView();
        }

        #endregion

    }
}
