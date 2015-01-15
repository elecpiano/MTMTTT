﻿using Shared.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class HomePage : Page
    {
        #region Lifecycle

        public HomePage()
        {
            this.InitializeComponent();
            HideStatusBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateScreenSize();
        }
        
        #endregion

        #region Tile Click

        #endregion

        private void singlePic_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OperationPage), "single");
        }

        private void multiPic_Click(object sender, RoutedEventArgs e)
        {
        }

        private void boutique_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BoutiquePage));
        }

        private void more_Click(object sender, RoutedEventArgs e)
        {

        }

        #region Screen Size Related

        private void UpdateScreenSize()
        {
            Application.Current.Resources["ScreenWidthHalf"] = Window.Current.Bounds.Width * 0.5d;
        }

        #endregion

        #region StatusBar

        private void HideStatusBar()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.HideAsync();
        }

        #endregion

    }
}
