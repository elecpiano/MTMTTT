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
                Test();
            }
        }

        #endregion

        #region Test

        private void Test()
        {
        }

        #endregion

        private void feedbackType_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            if (tag == "1")
            {
                VisualStateManager.GoToState(this, "vsFeedbackType1", true);
            }
            else if (tag == "2")
            {
                VisualStateManager.GoToState(this, "vsFeedbackType2", true);
            }
            else if (tag == "3")
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
            if (dataSender==null)
            {
                dataSender = new DataSender();
            }

            string url = "http://data.meitu.com/feedback_iphone.php";
            string data = "wp_test";
            Dictionary<string, string> param = new Dictionary<string, string>();

            param.Add("software", "AMTTT");
            param.Add("version", "1.0.0.0");
            param.Add("module", "Windows Phone");
            //param.Add("os", "Windows Phone 8.1");
            //param.Add("nickname", "");
            //param.Add("from", "");
            param.Add("message", "test message");
            param.Add("contact", "test@test.com");
            param.Add("itype", "suggestion");

            dataSender.POSTAsync(url, data, param, 
                result =>
                {
                    int xx = 0;
                    xx++;
                });
        }

        #endregion

    }
}
