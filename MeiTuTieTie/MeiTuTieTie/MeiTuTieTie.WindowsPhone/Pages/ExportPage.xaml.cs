using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using Shared.Global;
using Shared.Animation;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Control;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using System;

namespace MeiTuTieTie.Pages
{
    public sealed partial class ExportPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private bool autoSave = false;
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
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                bitmap = e.Parameter as RenderTargetBitmap;
                LoadSettings();
                if (autoSave)
                {
                    Save();
                }
            }
        }

        #endregion

        #region Test

        private void Test()
        {
        }

        #endregion

        #region Share

        private void wechat_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Save

        private void saveButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Save();
            saveButtonPanel.Visibility = Visibility.Collapsed;
        }

        private async void Save()
        {
            string fileName = "MeiTuTieTie_" + DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss") + ".jpg";
            await ImageHelper.SaveBitmapToMediaLibrary(bitmap, fileName);

            saveResultPanel.Visibility = Visibility.Visible;
        }

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            autoSave = App.CurrentInstance.GetSetting<bool>(Constants.KEY_AUTO_SAVE, false);
            saveButtonPanel.Visibility = autoSave ? Visibility.Collapsed : Visibility.Visible;
        }

        #endregion

    }
}
