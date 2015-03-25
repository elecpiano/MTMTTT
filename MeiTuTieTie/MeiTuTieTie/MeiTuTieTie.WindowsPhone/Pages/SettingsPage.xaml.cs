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
using Windows.Graphics.Display;

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
            this.pivot.Margin = new Thickness(0);

        }

        void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadSettings();

            if (e.NavigationMode == NavigationMode.New)
            {
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

        #region Image Size

        private void imageSizeOption_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            double width = 960d;

            switch (tag)
            {
                case "1":
                    width = 640d;
                    break;
                case "2":
                    width = 960d;
                    break;
                case "3":
                    width = 1024d;
                    break;
                default:
                    break;
            }

            App.CurrentInstance.UpdateSetting(Constants.KEY_EXPORT_WIDTH, width);

            imageSizeRadio1.IsChecked = tag == "1" ? true : false;
            imageSizeRadio2.IsChecked = tag == "2" ? true : false;
            imageSizeRadio3.IsChecked = tag == "3" ? true : false;
        }

        #endregion

        #region others

        private void help_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(HelpPage));
        }

        private void feedback_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(FeedbackPage));
        }

        private void about_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(AboutPage));
        }

        #endregion

        #region Load Settings

        private void LoadSettings()
        {
            //image size
            double width = App.CurrentInstance.GetSetting<double>(Constants.KEY_EXPORT_WIDTH, 960d);
            imageSizeRadio1.IsChecked = width == 640d ? true : false;
            imageSizeRadio2.IsChecked = width == 960d ? true : false;
            imageSizeRadio3.IsChecked = width == 1024d ? true : false;

            //edge
            switchEdge.IsOn = App.CurrentInstance.GetSetting<bool>(Constants.KEY_EDGE, false);
            edgeStatus.Text = switchEdge.IsOn ? "已开启" : "已关闭";

            //shadow
            switchShadow.IsOn = App.CurrentInstance.GetSetting<bool>(Constants.KEY_SHADOW, false);
            shadowStatus.Text = switchShadow.IsOn ? "已开启" : "已关闭";

            //auto save
            switchAutoSave.IsOn = App.CurrentInstance.GetSetting<bool>(Constants.KEY_AUTO_SAVE, true);
            autoSaveStatus.Text = switchAutoSave.IsOn ? "已开启" : "已关闭";

            //SFX
            switchSFX.IsOn = App.CurrentInstance.GetSetting<bool>(Constants.KEY_SFX, false);
            sfxStatus.Text = switchSFX.IsOn ? "已开启" : "已关闭";
        }

        #endregion

        private void switchEdge_Toggled(object sender, RoutedEventArgs e)
        {
            bool toggled = (sender as ToggleSwitch).IsOn;
            App.CurrentInstance.UpdateSetting(Constants.KEY_EDGE, toggled);
            edgeStatus.Text = toggled ? "已开启" : "已关闭";
        }

        private void switchShadow_Toggled(object sender, RoutedEventArgs e)
        {
            bool toggled = (sender as ToggleSwitch).IsOn;
            App.CurrentInstance.UpdateSetting(Constants.KEY_SHADOW, toggled);
            shadowStatus.Text = toggled ? "已开启" : "已关闭";
        }

        private void switchAutoSave_Toggled(object sender, RoutedEventArgs e)
        {
            bool toggled = (sender as ToggleSwitch).IsOn;
            App.CurrentInstance.UpdateSetting(Constants.KEY_AUTO_SAVE, toggled);
            autoSaveStatus.Text = toggled ? "已开启" : "已关闭";
        }

        private void switchSFX_Toggled(object sender, RoutedEventArgs e)
        {
            bool toggled = (sender as ToggleSwitch).IsOn;
            App.CurrentInstance.UpdateSetting(Constants.KEY_SFX, toggled);
            sfxStatus.Text = toggled ? "已开启" : "已关闭";
        }


    }
}
