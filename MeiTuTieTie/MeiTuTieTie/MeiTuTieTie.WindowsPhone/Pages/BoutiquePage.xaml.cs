using System;
using Shared.Common;
using MeiTuTieTie.Controls;
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
using Shared.Model;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class BoutiquePage : Page
    {
        #region Property

        public const string MODULE = "theme";
        public const string CACHE_FILE = "theme_data";

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

        private void LoadData()
        {
            if (dataLoader==null)
            {
                dataLoader = new DataLoader<ThemePacksData>();
            }

            string url = "http://tietie.sucaimgr.meitu.com/json_file/android/allPacks.json";
            dataLoader.Load(url, true, MODULE, CACHE_FILE,
                data =>
                {
                    topThemeListBox.ItemsSource = data.topThemePacks;
                    allThemeListBox.ItemsSource = data.allThemePacks;
                });
        }

        #endregion

        #region Theme Detail View

        private bool themeDetailViewShown = false;

        private void theme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ThemeDetailPage),sender.GetDataContext());
        }

        #endregion

    }
}
