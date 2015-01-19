﻿using Shared.Common;
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
            Frame.Navigate(typeof(MyThemeDetailPage), sender.GetDataContext());
        }

        #endregion

        #region App Bar

        private void selectItems_Click(object sender, RoutedEventArgs e)
        {
            XmlSerializer serializer;

            DisplayInformation di = DisplayInformation.GetForCurrentView();
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



        #endregion
    }
}
