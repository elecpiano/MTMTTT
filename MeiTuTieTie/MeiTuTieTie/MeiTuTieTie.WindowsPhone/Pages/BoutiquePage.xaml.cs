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
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadData();
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
                    themeListBox.ItemsSource = data.allThemePacks;
                });
        }

        #endregion

    }
}
