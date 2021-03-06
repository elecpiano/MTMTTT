﻿using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Windows.UI.Xaml.Navigation;
using Shared.Global;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Shared.SNS;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace MeiTuTieTie.Pages
{
    public sealed partial class ExportPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private bool autoSave = true;
        RenderTargetBitmap bitmap = null;

        #endregion

        #region Lifecycle

        public ExportPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.CanGobackAsked += navigationHelper_CanGobackAsked;
        }

        void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NavigationHelper.ActivePage = this.GetType();

            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                bitmap = e.Parameter as RenderTargetBitmap;
                LoadSettings();
                if (autoSave)
                {
                    if (App.CurrentInstance.OpertationPageChanged)
                    {
                        saveButtonPanel.Visibility = Visibility.Collapsed;
                        saveResultPanel.Visibility = Visibility.Collapsed;
                        Save();
                    }
                    else
                    {
                        saveButtonPanel.Visibility = Visibility.Collapsed;
                        saveResultPanel.Visibility = Visibility.Visible;
                    }

                    App.CurrentInstance.OpertationPageChanged = false;
                }
                else
                {
                    saveButtonPanel.Visibility = Visibility.Visible;
                    saveResultPanel.Visibility = Visibility.Collapsed;
                }
            }

            UmengSDK.UmengAnalytics.TrackPageStart(this.GetType().ToString());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengSDK.UmengAnalytics.TrackPageEnd(this.GetType().ToString());
        }

        #endregion

        #region Test

        private void Test()
        {
        }

        #endregion

        #region Share

        string shareText = "#美图贴贴Windows Phone版#";
        bool snsSharingBusy = false;

        private void shareToSNS_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (snsSharingBusy)
            {
                return;
            }

            string tag = (sender as FrameworkElement).Tag.ToString();
            switch (tag)
            {
                case "weibo":
                    ShareToWeibo();
                    break;
                default:
                    break;
            }
        }

        private async void ShareToWeibo()
        {
            snsSharingBusy = true;
            string fileName = "TempShareWeibo";
            var stream = await ImageHelper.SaveBitmapToLocal(bitmap, fileName, true);

            ////var stream = await ImageHelper.BitmapToStream(bitmap);
            //var buffer = await bitmap.GetPixelsAsync();
            //var arr = buffer.ToArray();
            //MemoryStream ms = new MemoryStream(arr);

            await WeiboHelper.Share(stream, shareText);
            snsSharingBusy = false;
        }

        #endregion

        #region Save

        private bool busy = false;

        private async void saveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Save();
        }

        private async void Save()
        {
            if (busy)
            {
                return;
            }
            busy = true;

            string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            await ImageHelper.SaveBitmapToMediaLibrary(bitmap, fileName);

            App.CurrentInstance.OpertationPageChanged = false;

            saveButtonPanel.Visibility = Visibility.Collapsed;
            saveResultPanel.Visibility = Visibility.Visible;

            busy = false;
        }

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            autoSave = App.CurrentInstance.GetSetting<bool>(Constants.KEY_AUTO_SAVE, true);
            saveButtonPanel.Visibility = autoSave ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

        #region Store

        private const string AppID_meitu = "fd7fcb4d-122c-45cd-952a-efb3a1293963";
        private const string AppID_meiyan = "fb760ab1-fb6d-4145-9c99-3baaf45f2581";

        private void OpenStore(string appid)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:navigate?appid={0}", appid)));
        }

        private void moreApp_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            if (tag == "meitu")
            {
                OpenStore(AppID_meitu);
            }
            else if (tag == "meiyan")
            {
                OpenStore(AppID_meiyan);
            }
        }

        #endregion

    }
}
