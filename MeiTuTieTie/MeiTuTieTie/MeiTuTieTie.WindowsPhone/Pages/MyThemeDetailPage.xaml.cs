using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;
using Windows.Graphics.Display;
using Shared.Global;

namespace MeiTuTieTie.Pages
{
    public sealed partial class MyThemeDetailPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public MyThemeDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadData(e.Parameter as MyTheme);
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

        private async void LoadData(MyTheme theme)
        {
            if (dataLoader == null)
            {
                dataLoader = new DataLoader<ThemePacksData>();
            }

            var data = await dataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);

        }

        #endregion

    }
}
