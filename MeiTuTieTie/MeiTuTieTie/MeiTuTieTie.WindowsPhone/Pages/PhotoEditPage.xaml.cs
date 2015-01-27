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
        private BitmapImage bitmapImage = null;

        #endregion

        #region Lifecycle

        public PhotoEditPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.image.SizeChanged += image_SizeChanged;
            PrepareEditor();
        }

        void image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            column_0_width = 0d;
            column_1_width = image.ActualWidth;
            column_2_width = 0d;

            row_0_height = 0d;
            row_1_height = image.ActualHeight;
            row_2_height = 0d;

            Draw();
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                bitmapImage = (BitmapImage)e.Parameter;
                image.Source = bitmapImage;
            }
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

        #region Editor

        double column_0_width = 0d;
        double column_1_width = 0d;
        double column_2_width = 0d;
        double row_0_height = 0d;
        double row_1_height = 0d;
        double row_2_height = 0d;

        private void PrepareEditor()
        {
            var modes = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            this.knobLT.ManipulationMode = modes;
            this.knobRT.ManipulationMode = modes;
            this.knobLB.ManipulationMode = modes;
            this.knobRB.ManipulationMode = modes;
            this.clipGrid.ManipulationMode = modes;

            this.knobLT.ManipulationDelta += knobLT_ManipulationDelta;
            this.knobRT.ManipulationDelta += knobRT_ManipulationDelta;
            this.knobLB.ManipulationDelta += knobLB_ManipulationDelta;
            this.knobRB.ManipulationDelta += knobRB_ManipulationDelta;
            this.clipGrid.ManipulationDelta += clipGrid_ManipulationDelta;
        }

        void clipGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            column_0_width += delta_x;
            column_2_width -= delta_x;
            
            row_0_height += delta_y;
            row_2_height -= delta_y;

            if (column_0_width < 0 || column_2_width < 0 || row_0_height < 0 || row_2_height < 0)
            {
                return;
            }

            Draw2();
        }

        void knobLT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            column_0_width += delta_x;
            column_1_width -= delta_x;
            row_0_height += delta_y;
            row_1_height -= delta_y;
            Draw();
        }

        void knobRT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            column_1_width += delta_x;
            row_0_height += delta_y;
            row_1_height -= delta_y;
            Draw();
        }

        void knobLB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            column_0_width += delta_x;
            column_1_width -= delta_x;
            row_1_height += delta_y;
            Draw();
        }

        void knobRB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            column_1_width += delta_x;
            row_1_height += delta_y;
            Draw();
        }

        private void Draw()
        {
            column_0_width = column_0_width < 0 ? 0 : column_0_width;
            column_0_width = column_0_width > image.ActualWidth ? image.ActualWidth : column_0_width;

            column_1_width = column_1_width < 0 ? 0 : column_1_width;
            column_1_width = column_1_width > image.ActualWidth ? image.ActualWidth : column_1_width;

            column_2_width = image.ActualWidth - column_0_width - column_1_width;

            row_0_height = row_0_height < 0 ? 0 : row_0_height;
            row_0_height = row_0_height > image.ActualHeight ? image.ActualHeight : row_0_height;

            row_1_height = row_1_height < 0 ? 0 : row_1_height;
            row_1_height = row_1_height > image.ActualHeight ? image.ActualHeight : row_1_height;

            row_2_height = image.ActualHeight - row_0_height - row_1_height;

            column_0.Width = new GridLength(column_0_width);
            column_1.Width = new GridLength(column_1_width);
            //column_2.Width = new GridLength(column_2_width);

            row_0.Height = new GridLength(row_0_height);
            row_1.Height = new GridLength(row_1_height);
            //row_2.Height = new GridLength(row_2_height);
        }

        private void Draw2()
        {
            column_0_width = column_0_width < 0 ? 0 : column_0_width;
            column_0_width = column_0_width > image.ActualWidth ? image.ActualWidth : column_0_width;

            column_2_width = column_2_width < 0 ? 0 : column_2_width;
            column_2_width = column_2_width > image.ActualWidth ? image.ActualWidth : column_2_width;

            column_1_width = image.ActualWidth - column_0_width - column_2_width;

            row_0_height = row_0_height < 0 ? 0 : row_0_height;
            row_0_height = row_0_height > image.ActualHeight ? image.ActualHeight : row_0_height;

            row_2_height = row_2_height < 0 ? 0 : row_2_height;
            row_2_height = row_2_height > image.ActualHeight ? image.ActualHeight : row_2_height;

            row_1_height = image.ActualHeight - row_0_height - row_2_height;

            column_0.Width = new GridLength(column_0_width);
            column_1.Width = new GridLength(column_1_width);
            column_2.Width = new GridLength(column_2_width);

            row_0.Height = new GridLength(row_0_height);
            row_1.Height = new GridLength(row_1_height);
            row_2.Height = new GridLength(row_2_height);
        }

        #endregion

    }
}
