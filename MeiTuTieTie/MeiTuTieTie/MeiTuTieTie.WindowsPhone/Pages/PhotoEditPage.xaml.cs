using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using System;
using System.Threading.Tasks;
using Shared.Enum;

namespace MeiTuTieTie.Pages
{
    public sealed partial class PhotoEditPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private WriteableBitmap wbOrigin = null;

        #endregion

        #region Lifecycle

        public PhotoEditPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.image.SizeChanged += image_SizeChanged;
            PrepareEditor();
        }


        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                IRandomAccessStream stream = (IRandomAccessStream)e.Parameter;

                BitmapImage bi = new BitmapImage();
                bi.SetSource(stream);
                image.Source = bi;

                //prepare for crop
                PreapreWritableBitmap(stream, bi);
            }
        }

        private async void PreapreWritableBitmap(IRandomAccessStream stream, BitmapImage bi)
        {
            WriteableBitmap wbTemp = new WriteableBitmap(bi.PixelWidth, bi.PixelHeight);
            wbOrigin = await wbTemp.FromStream(stream);
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

            bool valid = false;
            valid = CheckClipChange(clip_L + delta_x, 0, clip_R + delta_x, 0);
            if (valid)
            {
                UpdateClipArea(delta_x, 0, delta_x, 0);
                Draw();
            }
            valid = CheckClipChange(0, clip_T + delta_y, 0, clip_B + delta_y);
            if (valid)
            {
                UpdateClipArea(0, delta_y, 0, delta_y);
                Draw();
            }
        }

        void knobLT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = CheckClipChange(clip_L + delta_x, clip_T + delta_y, clip_R, clip_B);
            if (valid)
            {
                UpdateClipArea(delta_x, delta_y, 0, 0);
                Draw();
            }
        }

        void knobRT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = CheckClipChange(clip_L, clip_T + delta_y, clip_R + delta_x, clip_B);
            if (valid)
            {
                UpdateClipArea(0, delta_y, delta_x, 0);
                Draw();
            }
        }

        void knobLB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = CheckClipChange(clip_L + delta_x, clip_T, clip_R, clip_B + delta_y);
            if (valid)
            {
                UpdateClipArea(delta_x, 0, 0, delta_y);
                Draw();
            }
        }

        void knobRB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = CheckClipChange(clip_L, clip_T, clip_R + delta_x, clip_B + delta_y);
            if (valid)
            {
                UpdateClipArea(0, 0, delta_x, delta_y);
                Draw();
            }
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

        private bool CheckClipChange(double L, double T, double R, double B)
        {
            if (L < rectGeoOut.Rect.Left)
            {
                return false;
            }
            if (T < rectGeoOut.Rect.Top)
            {
                return false;
            }
            if (R > rectGeoOut.Rect.Right)
            {
                return false;
            }
            if (B > rectGeoOut.Rect.Bottom)
            {
                return false;
            }

            return true;
        }

        private void UpdateClipArea(double delta_L, double delta_T, double delta_R, double delta_B)
        {
            clip_L += delta_L;
            clip_T += delta_T;
            clip_R += delta_R;
            clip_B += delta_B;
        }

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

            clipGrid.Margin = new Thickness(clip_L, clip_T, image.ActualWidth - clip_R, image.ActualHeight - clip_B);
        }

        #endregion

        #region Crop

        private async Task<WriteableBitmap> Crop()
        {
            double ratio = (double)wbOrigin.PixelWidth / image.ActualWidth;
            int L = (int)(clip_L * ratio);
            int T = (int)(clip_T * ratio);
            int R = (int)(clip_R * ratio);
            int B = (int)(clip_B * ratio);

            int width = R - L;
            int height = B - T;

            WriteableBitmap wbNew = wbOrigin.Crop(L, T, width, height);

            App.CurrentInstance.wbForSingleMode = wbNew;
            Frame.Navigate(typeof(OperationPage), OperationPageType.Single);

            //IRandomAccessStream stream = new MemoryStream().AsRandomAccessStream();
            //await wbNew.ToStreamAsJpeg(stream);

            //byte[] buffer = wbNew.ToByteArray();// new byte[1];
            //IRandomAccessStream stream = new MemoryStream(buffer).AsRandomAccessStream();
            //BitmapImage bi = new BitmapImage();
            //bi.SetSource(stream);
            //resultImage.Visibility = Visibility.Visible;
            //resultImage.Source = bi;

            return wbNew;
        }

        private async void Test()
        {
            resultImage.Visibility = Visibility.Visible;
            WriteableBitmap wb = await Crop();
            resultImage.Source = wb;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }

        #endregion

    }
}
