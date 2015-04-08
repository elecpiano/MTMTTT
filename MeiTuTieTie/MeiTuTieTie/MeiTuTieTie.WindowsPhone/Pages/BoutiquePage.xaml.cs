using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using Shared.Global;
using System.Linq;
using System.Threading.Tasks;

namespace MeiTuTieTie.Pages
{
    public sealed partial class BoutiquePage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        
        #endregion

        #region Lifecycle

        public BoutiquePage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            //HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        //void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        //{
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadData();
            }
            else
            {
                CheckDownloaded(themePackData);
            }

            UmengSDK.UmengAnalytics.TrackPageStart(this.GetType().ToString());
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            }
            else
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            }
            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengSDK.UmengAnalytics.TrackPageEnd(this.GetType().ToString());
        }

        #endregion

        #region Data

        DataLoader<ThemePacksData> dataLoader = null;
        DataLoader<MyThemeData> myThemeDataLoader = null;
        ThemePacksData themePackData = null;

        private void LoadData()
        {
            if (dataLoader==null)
            {
                dataLoader = new DataLoader<ThemePacksData>();
            }

            string url = "http://tietie.sucaimgr.meitu.com/json_file/android/allPacks.json";
            dataLoader.Load(url, true, Constants.THEME_MODULE, Constants.THEME_DATA_FILE,
                data =>
                {
                    themePackData = data;
                    ContinueLoadData(themePackData);
                });
        }

        private async void ContinueLoadData(ThemePacksData data)
        {
            await CheckDownloaded(data);
            topThemeListBox.ItemsSource = data.topThemePacks;
            allThemeListBox.ItemsSource = data.allThemePacks;
        }

        private async Task CheckDownloaded(ThemePacksData data)
        {
            if (myThemeDataLoader == null)
            {
                myThemeDataLoader = new DataLoader<MyThemeData>();
            }

            //load data file
            var myThemeData = await myThemeDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);
            if (myThemeData != null)
            {
                foreach (var theme in data.topThemePacks)
                {
                    if (myThemeData.myThemes.Any(x=>x.id == theme.id))
                    {
                        theme.Downloaded = true;
                    }
                    else
                    {
                        theme.Downloaded = false;
                    }
                }

                foreach (var theme in data.allThemePacks)
                {
                    if (myThemeData.myThemes.Any(x => x.id == theme.id))
                    {
                        theme.Downloaded = true;
                    }
                    else
                    {
                        theme.Downloaded = false;
                    }
                }
            }
        }

        #endregion

        #region Theme Detail View

        private void theme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ThemeDetailPage),sender.GetDataContext());
        }

        #endregion

        #region App Bar

        private void BuildAppBar()
        {
            AppBarButton myThemesButton = new AppBarButton();
            myThemesButton.Label = "分类";
            myThemesButton.Icon = new SymbolIcon(Symbol.Favorite);
            myThemesButton.Click += myThemesButton_Click;
        }

        void myThemesButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyThemeListPage));
        }

        #endregion

    }
}
