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
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

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
            pathMask.Width = image.ActualWidth;
            pathMask.Height = image.ActualHeight;

            clip_L = 0d;
            clip_T = 0d;
            clip_R = image.ActualWidth;
            clip_B = image.ActualHeight;

            PrepareMask(image.ActualWidth, image.ActualHeight);
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

        double clip_L = 0d;
        double clip_T = 0d;
        double clip_R = 0d;
        double clip_B = 0d;

        GeometryGroup geoGroup;
        RectangleGeometry rectGeoOut;
        RectangleGeometry rectGeoIn;
        Rect rectIn;

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

        private void PrepareMask(double width, double height)
        {
            geoGroup = new GeometryGroup();
            pathMask.Fill = new SolidColorBrush(Colors.Black);
            rectGeoOut = new RectangleGeometry();
            rectGeoOut.Rect = new Rect(0, 0, width, height);
            rectGeoIn = new RectangleGeometry();
            rectGeoIn.Rect = new Rect(0, 0, width, height);
            geoGroup.Children.Add(rectGeoOut);
            geoGroup.Children.Add(rectGeoIn);

            pathMask.Data = geoGroup;
            pathMask.Opacity = 0.5;
        }

        void clipGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            //knobTransform_LT.TranslateX += delta_x;
            //knobTransform_LT.TranslateY += delta_y;
            //knobTransform_RT.TranslateX += delta_x;
            //knobTransform_RT.TranslateY += delta_y;
            //knobTransform_LB.TranslateX += delta_x;
            //knobTransform_LB.TranslateY += delta_y;
            //knobTransform_RB.TranslateX += delta_x;
            //knobTransform_RB.TranslateY += delta_y;

            clip_L += delta_x;
            clip_R += delta_x;
            clip_T += delta_y;
            clip_B += delta_y;

            Draw();
        }

        void knobLT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            //knobTransform_LT.TranslateX += delta_x;
            //knobTransform_LT.TranslateY += delta_y;

            clip_L += delta_x;
            clip_T += delta_y;
            Draw();
        }

        void knobRT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            //knobTransform_RT.TranslateX += delta_x;
            //knobTransform_RT.TranslateY += delta_y;

            clip_R += delta_x;
            clip_T += delta_y;
            Draw();
        }

        void knobLB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            //knobTransform_LB.TranslateX += delta_x;
            //knobTransform_LB.TranslateY += delta_y;

            clip_L += delta_x;
            clip_B += delta_y;
            Draw();
        }

        void knobRB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            //knobTransform_RB.TranslateX += delta_x;
            //knobTransform_RB.TranslateY += delta_y;

            clip_R += delta_x;
            clip_B += delta_y;
            Draw();
        }

        #endregion

        #region Geometry

        private void Draw()
        {
            geoGroup.Children.Clear();

            rectIn.X = clip_L;
            rectIn.Y = clip_T;
            rectIn.Width = clip_R - clip_L;
            rectIn.Height = clip_B - clip_T;

            rectGeoIn.Rect = rectIn;// new Rect(60, 110, 100, 200);

            geoGroup.Children.Add(rectGeoOut);
            geoGroup.Children.Add(rectGeoIn);

            UpdateKnobPosition();
        }

        private void UpdateKnobPosition()
        {
            knobTransform_LT.TranslateX = clip_L;
            knobTransform_LT.TranslateY = clip_T;
            knobTransform_RT.TranslateX = clip_R;
            knobTransform_RT.TranslateY = clip_T;
            knobTransform_LB.TranslateX = clip_L;
            knobTransform_LB.TranslateY = clip_B;
            knobTransform_RB.TranslateX = clip_R;
            knobTransform_RB.TranslateY = clip_B;
        }

        #endregion

    }
}
