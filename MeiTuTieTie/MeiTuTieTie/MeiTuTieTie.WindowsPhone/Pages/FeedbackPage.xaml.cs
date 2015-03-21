using System;
using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.UI.Popups;

namespace MeiTuTieTie.Pages
{
    public sealed partial class FeedbackPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public FeedbackPage()
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
                VisualStateManager.GoToState(this, "vsFeedbackType1", true);
            }
        }

        #endregion

        private string feedbackType = "suggestion";

        private void feedbackType_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string feedbackType = (sender as FrameworkElement).Tag.ToString();
            if (feedbackType == "error")
            {
                VisualStateManager.GoToState(this, "vsFeedbackType1", true);
            }
            else if (feedbackType == "suggestion")
            {
                VisualStateManager.GoToState(this, "vsFeedbackType2", true);
            }
            else if (feedbackType == "other")
            {
                VisualStateManager.GoToState(this, "vsFeedbackType3", true);
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        #region Send Data

        private DataSender dataSender = null;

        private async void send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(feedbackTextBox.Text.Trim()))
            {
                var dialog = new MessageDialog("反馈不能为空哦！");
                var result = await dialog.ShowAsync();
                return;
            }

            if (dataSender==null)
            {
                dataSender = new DataSender();
            }

            string url = "http://data.meitu.com/feedback_iphone.php";
            string data = "MTTT WP 1.0.0.0";
            Dictionary<string, string> param = new Dictionary<string, string>();

            param.Add("software", "AMTTT");
            param.Add("version", "1.0.0.0");
            param.Add("module", "Windows Phone");
            //param.Add("os", "Windows Phone 8.1");
            //param.Add("nickname", "");
            //param.Add("from", "");
            param.Add("message", feedbackTextBox.Text.Trim());
            param.Add("contact", contactTextBox.Text.Trim());
            param.Add("itype", feedbackType);

            dataSender.POSTAsync(url, data, param, 
                result =>
                {
                    OnSent();
                });
        }

        private async void OnSent()
        {
            var dialog = new MessageDialog("发送成功！");
            var result = await dialog.ShowAsync();
            navigationHelper.GoBack();
        }

        #endregion

    }
}
