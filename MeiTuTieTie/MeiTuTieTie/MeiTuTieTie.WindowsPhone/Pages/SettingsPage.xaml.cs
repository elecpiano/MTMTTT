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

namespace MeiTuTieTie.Pages
{
    public sealed partial class SettingsPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public SettingsPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.CanGobackAsked += navigationHelper_CanGobackAsked;
        }

        void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
            if (imageSizePopupShown)
            {
                HideImageSizePopup();
                canceled = true;
            }
            else if (edgeShadowPopupShown)
            {
                HideEdgeShadowPopup();
                canceled = true;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadImageSizeSetting();
                LoadEdgeShadowSetting();
            }
        }

        #endregion

        #region Popup

        bool imageSizePopupShown = false;
        bool edgeShadowPopupShown = false;

        private void ShowImageSizePopup()
        {
            if (!imageSizePopupShown)
            {
                VisualStateManager.GoToState(this, "vsImageSizePopupShown", true);
                imageSizePopupShown = true;
            }
        }

        private void HideImageSizePopup()
        {
            if (imageSizePopupShown)
            {
                VisualStateManager.GoToState(this, "vsImageSizePopupHidden", true);
                imageSizePopupShown = false;
            }
        }

        private void ShowEdgeShadowPopup()
        {
            if (!edgeShadowPopupShown)
            {
                VisualStateManager.GoToState(this, "vsEdgeShadowPopupShown", true);
                edgeShadowPopupShown = true;
            }
        }

        private void HideEdgeShadowPopup()
        {
            if (edgeShadowPopupShown)
            {
                VisualStateManager.GoToState(this, "vsEdgeShadowPopupHidden", true);
                edgeShadowPopupShown = false;
            }
        }

        #endregion

        #region Image Size

        private void LoadImageSizeSetting()
        {
            double width = App.CurrentInstance.GetSetting<double>(Constants.KEY_EXPORT_WIDTH, 1248d);
            imageSizeCheckMark1.Visibility = width == 640d ? Visibility.Visible : Visibility.Collapsed;
            imageSizeCheckMark2.Visibility = width == 1248d ? Visibility.Visible : Visibility.Collapsed;
            imageSizeCheckMark3.Visibility = width == 1600d ? Visibility.Visible : Visibility.Collapsed;
        }

        private void imageSize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowImageSizePopup();
        }

        private void imageSizeOption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            double width = 1248d;

            switch (tag)
            {
                case "1":
                    width = 640d;
                    break;
                case "2":
                    width = 1248d;
                    break;
                case "3":
                    width = 1600d;
                    break;
                default:
                    break;
            }

            App.CurrentInstance.UpdateSetting(Constants.KEY_EXPORT_WIDTH, width);
            imageSizeCheckMark1.Visibility = tag == "1" ? Visibility.Visible : Visibility.Collapsed;
            imageSizeCheckMark2.Visibility = tag == "2" ? Visibility.Visible : Visibility.Collapsed;
            imageSizeCheckMark3.Visibility = tag == "3" ? Visibility.Visible : Visibility.Collapsed;

            HideImageSizePopup();
        }

        #endregion

        #region edge, shadow

        private void LoadEdgeShadowSetting()
        {
            bool edgeEnabled = App.CurrentInstance.GetSetting<bool>(Constants.KEY_EDGE, false);
            edgeSwitch.Checked = edgeEnabled;

            bool shadowEnabled = App.CurrentInstance.GetSetting<bool>(Constants.KEY_SHADOW, false);
            shadowSwitch.Checked = shadowEnabled;
        }

        private void edge_CheckStateChanged(Shared.Control.ImageSwitch sender, bool suggestedState)
        {
            App.CurrentInstance.UpdateSetting(Constants.KEY_EDGE, suggestedState);
            sender.Checked = suggestedState;
        }

        private void shadow_CheckStateChanged(Shared.Control.ImageSwitch sender, bool suggestedState)
        {
            App.CurrentInstance.UpdateSetting(Constants.KEY_SHADOW, suggestedState);
            sender.Checked = suggestedState;
        }

        #endregion

        private void autoSave_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void edgeShadow_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowEdgeShadowPopup();
        }

        private void soundEffect_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void help_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void feedback_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void about_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }


      
    }
}
