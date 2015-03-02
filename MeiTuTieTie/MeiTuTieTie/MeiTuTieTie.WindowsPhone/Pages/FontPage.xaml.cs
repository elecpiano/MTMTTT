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
    public sealed partial class FontPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public FontPage()
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
                Test();
            }
        }

        #endregion

        #region Test

        private void Test()
        {
            App.CurrentInstance.SelectedFont = @"ms-appx:/Assets/Fonts/SHOWG.TTF#Showcard Gothic";
            App.CurrentInstance.SelectedTextColor = new SolidColorBrush(Color.FromArgb(255, 190, 30, 0));
        }

        #endregion

    }
}
