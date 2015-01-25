using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;
using Windows.Graphics.Display;
using Shared.Global;
using System.Linq;
using System.IO;
using Shared.ViewModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace MeiTuTieTie.Pages
{
    public sealed partial class PhotoEditPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public PhotoEditPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
        }

        #endregion

        #region Crop

        private void Crop(IRandomAccessStream stream)
        {
            BitmapImage bi = new BitmapImage();
            bi.SetSource(stream);
            int width = bi.PixelWidth;
            int height = bi.PixelHeight;
            WriteableBitmap wbOld = new WriteableBitmap(width, height);
            WriteableBitmap wbNew = wbOld.Crop(25, 25, 50, 50);
        }

        #endregion

    }
}
