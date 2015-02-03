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

        private void imageSize_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowImageSizePopup();
        }

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
