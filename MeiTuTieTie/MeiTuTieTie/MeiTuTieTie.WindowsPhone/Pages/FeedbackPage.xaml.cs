using System;
using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using Windows.UI.Popups;
using System.Text;
using Shared.Model;

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
            NavigationHelper.ActivePage = this.GetType();
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
            feedbackType = (sender as FrameworkElement).Tag.ToString();
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

        DataLoader dataLoader = null;

        private async void send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(feedbackTextBox.Text.Trim()))
            {
                var dialog = new MessageDialog("反馈不能为空哦！");
                var cmdOK = new UICommand("确定");
                dialog.Commands.Add(cmdOK);
                dialog.CancelCommandIndex = 0;
                var result = await dialog.ShowAsync();
                return;
            }
            else if (!NetworkHelper.CheckInternet())
            {
                var dialog = new MessageDialog("当前无网络！");
                var cmdOK = new UICommand("确定");
                dialog.Commands.Add(cmdOK);
                dialog.CancelCommandIndex = 0;
                var result = await dialog.ShowAsync();
                return;
            }

            if (dataLoader == null)
            {
                dataLoader = new DataLoader();
            }

            string url = "http://data.meitu.com/feedback_iphone.php&";
            string param = string.Empty;
            param += "&" + "software" + "=" + "AMTTT";
            param += "&" + "version" + "=" + "1.0.0.0";
            param += "&" + "module" + "=" + "WindowsPhone8.1";
            param += "&" + "message" + "=" + feedbackTextBox.Text.Trim();
            param += "&" + "contact" + "=" + contactTextBox.Text.Trim();
            param += "&" + "itype" + "=" + feedbackType;
            url = string.Format("http://data.meitu.com/feedback_iphone.php?{0}", param);

            dataLoader.Load(url, OnSent);
        }

        private async void OnSent(string message)
        {
            if (message.ToLower() == "ok")
            {
                var dialog = new MessageDialog("发送成功！");
                var cmdOK = new UICommand("确定");
                dialog.Commands.Add(cmdOK);
                dialog.CancelCommandIndex = 0;
                var result = await dialog.ShowAsync();
                navigationHelper.GoBack();
            }
            else
            {
                var dialog = new MessageDialog("发送失败，请重新尝试。");
                var cmdOK = new UICommand("确定");
                dialog.Commands.Add(cmdOK);
                dialog.CancelCommandIndex = 0;
                dialog.ShowAsync();
            }
        }

        #endregion

    }
}
