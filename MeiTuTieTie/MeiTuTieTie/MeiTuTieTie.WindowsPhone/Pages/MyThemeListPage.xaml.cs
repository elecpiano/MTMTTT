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
using Shared.Animation;
using Windows.UI.Xaml.Media.Animation;
using Windows.Phone.UI.Input;

namespace MeiTuTieTie.Pages
{
    public sealed partial class MyThemeListPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public MyThemeListPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (listEditing)
            {
                EndEditList();
                e.Handled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadData();
            }
            BuildBottomAppBar_Normal();
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

        #endregion

        #region Data

        DataLoader<MyThemeData> dataLoader = null;
        MyThemeData myThemeData = null;

        private async void LoadData()
        {
            if (dataLoader == null)
            {
                dataLoader = new DataLoader<MyThemeData>();
            }

            myThemeData = await dataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);
            if (myThemeData != null)
            {
                myThemeListBox.ItemsSource = myThemeData.myThemes;
            }
        }

        private async void SaveData()
        {
            string json = JsonSerializer.Serialize(myThemeData);
            await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE, json);
        }

        #endregion

        #region Theme Detail View

        private void theme_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (listEditing)
            {
                return;
            }

            Frame.Navigate(typeof(MyThemeDetailPage), sender.GetDataContext());
        }

        #endregion

        #region Theme Visibility Control

        private void ImageSwitch_CheckStateChanged(Shared.Control.ImageSwitch sender, bool suggestedState)
        {
            MyTheme theme = sender.GetDataContext<MyTheme>();
            if (theme != null)
            {
                theme.visible = !theme.visible;
                SaveData();
            }
        }

        #endregion

        #region Edit List

        private bool listEditing = false;
        PowerEase easingFunction = new PowerEase();

        private void StartEditList()
        {
            listEditing = true;
            MoveAnimation.MoveTo(this.myThemeListBox, 0d, 0d, 200d, easingFunction,
                fe =>
                {
                });
            FadeAnimation.Fade(this.switchMask, 0d, 1d, 200d);
        }

        private void EndEditList()
        {
            MoveAnimation.MoveTo(this.myThemeListBox, -64d, 0d, 200d, easingFunction,
                fe =>
                {
                });
            FadeAnimation.Fade(this.switchMask, 1d, 0d, 200d);
            listEditing = false;
        }

        #endregion

        #region AppBar

        AppBarButton appBarButton_edit = null;
        AppBarButton appBarButton_delete = null;
        AppBarButton appBarButton_cancel = null;

        private void BuildBottomAppBar_Normal()
        {
            this.bottomAppBar.PrimaryCommands.Clear();

            //normal
            if (appBarButton_edit == null)
            {
                appBarButton_edit = new AppBarButton();
                appBarButton_edit.Label = "选择";
                appBarButton_edit.Icon = new SymbolIcon(Symbol.List);
                appBarButton_edit.Click += AppbarButton_Edit;
            }
            this.bottomAppBar.PrimaryCommands.Add(appBarButton_edit);
        }

        private void BuildBottomAppBar_Edit()
        {
            this.bottomAppBar.PrimaryCommands.Clear();

            //delete
            if (appBarButton_delete == null)
            {
                appBarButton_delete = new AppBarButton();
                appBarButton_delete.Label = "删除";
                appBarButton_delete.Icon = new SymbolIcon(Symbol.Delete);
                appBarButton_delete.Click += AppbarButton_Delete;
            }
            this.bottomAppBar.PrimaryCommands.Add(appBarButton_delete);

            //cancel
            if (appBarButton_cancel == null)
            {
                appBarButton_cancel = new AppBarButton();
                appBarButton_cancel.Label = "取消选择";
                appBarButton_cancel.Icon = new SymbolIcon(Symbol.Cancel);
                appBarButton_cancel.Click += AppbarButton_Cancel;
            }
            this.bottomAppBar.PrimaryCommands.Add(appBarButton_cancel);
        }

        void AppbarButton_Edit(object sender, RoutedEventArgs e)
        {
            StartEditList();
            BuildBottomAppBar_Edit();
        }

        void AppbarButton_Cancel(object sender, RoutedEventArgs e)
        {
            EndEditList();
            BuildBottomAppBar_Normal();
        }
        void AppbarButton_Delete(object sender, RoutedEventArgs e)
        {
        }

        #endregion

    }
}
