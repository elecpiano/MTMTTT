﻿using Shared.Common;
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
using Windows.Storage;
using System.Linq;
using Windows.Graphics.Imaging;
using Shared.Global;
using Shared.Utility;

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
            this.image.SizeChanged += image_SizeChanged;
            PrepareEditor();
            this.navigationHelper.CanGobackAsked += navigationHelper_CanGobackAsked;
        }

        void navigationHelper_CanGobackAsked(object sender, ref bool canceled)
        {
            if (sizeMenuPopupShown)
            {
                HideSizeMenuPopup();
                canceled = true;
            }
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            NavigationHelper.ActivePage = this.GetType();
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                Initialize();
            }

            UmengSDK.UmengAnalytics.TrackPageStart(this.GetType().ToString());
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                App.CurrentInstance.ComingBackFrom = "PhotoEditPage";
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            UmengSDK.UmengAnalytics.TrackPageEnd(this.GetType().ToString());
        }
       
        #endregion

        #region Initialization

        private async void Initialize()
        {
            var file = App.CurrentInstance.HomePageMultiPhotoFiles[0];

            //prepare for crop
            PreapreWritableBitmap(file);

            //highlight selected crop ratio
            selectedText = menuItem_arbitrary;
            selectedText.Foreground = selectedBrush;

            SetCropMode("arbitrary");
        }

        #endregion

        #region Editor

        const double clip_MIN = 80d;

        double clip_L = 0d;
        double clip_T = 0d;
        double clip_R = 0d;
        double clip_B = 0d;

        GeometryGroup geoGroup;
        RectangleGeometry rectGeoOut;
        RectangleGeometry rectGeoIn;
        Rect rectIn;

        string currentRatioType = "origin";
        double currentRatio = 1d;

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

            editorPanel.Opacity = 1d;
        }

        private void PrepareEditor()
        {
            this.knobL.ManipulationMode = ManipulationModes.TranslateX;
            this.knobR.ManipulationMode = ManipulationModes.TranslateX;
            this.knobT.ManipulationMode = ManipulationModes.TranslateY;
            this.knobB.ManipulationMode = ManipulationModes.TranslateY;

            var modes = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            this.knobLT.ManipulationMode = modes;
            this.knobRT.ManipulationMode = modes;
            this.knobLB.ManipulationMode = modes;
            this.knobRB.ManipulationMode = modes;
            this.clipGrid.ManipulationMode = modes;

            this.knobL.ManipulationDelta += knobL_ManipulationDelta;
            this.knobR.ManipulationDelta += knobR_ManipulationDelta;
            this.knobT.ManipulationDelta += knobT_ManipulationDelta;
            this.knobB.ManipulationDelta += knobB_ManipulationDelta;

            this.knobLT.ManipulationDelta += knobLT_ManipulationDelta;
            this.knobRT.ManipulationDelta += knobRT_ManipulationDelta;
            this.knobLB.ManipulationDelta += knobLB_ManipulationDelta;
            this.knobRB.ManipulationDelta += knobRB_ManipulationDelta;
            this.clipGrid.ManipulationDelta += clipGrid_ManipulationDelta;
        }

        void knobL_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            bool valid = CheckClipChange(clip_L + delta_x, clip_T, clip_R, clip_B);
            if (valid)
            {
                UpdateClipArea(delta_x, 0, 0, 0);
                Draw();
            }
            e.Handled = true;
        }

        void knobR_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            bool valid = CheckClipChange(clip_L, clip_T, clip_R + delta_x, clip_B);
            if (valid)
            {
                UpdateClipArea(0, 0, delta_x, 0);
                Draw();
            }
            e.Handled = true;
        }

        void knobT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_y = e.Delta.Translation.Y;
            bool valid = CheckClipChange(clip_L, clip_T + delta_y, clip_R, clip_B);
            if (valid)
            {
                UpdateClipArea(0, delta_y, 0, 0);
                Draw();
            }
            e.Handled = true;
        }

        void knobB_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_y = e.Delta.Translation.Y;
            bool valid = CheckClipChange(clip_L, clip_T, clip_R, clip_B + delta_y);
            if (valid)
            {
                UpdateClipArea(0, 0, 0, delta_y);
                Draw();
            }
            e.Handled = true;
        }

        void clipGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = false;
            valid = CheckClipChange(clip_L + delta_x, clip_T, clip_R + delta_x, clip_B);
            if (valid)
            {
                UpdateClipArea(delta_x, 0, delta_x, 0);
                Draw();
            }
            valid = CheckClipChange(clip_L, clip_T + delta_y, clip_R, clip_B + delta_y);
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

            bool valid = ProcessRatio(ref delta_x, ref delta_y, true);
            if (!valid)
            {
                return;
            }

            valid = CheckClipChange(clip_L + delta_x, clip_T + delta_y, clip_R, clip_B);
            if (valid)
            {
                UpdateClipArea(delta_x, delta_y, 0, 0);
                Draw();
            }
        }

        private bool ProcessRatio(ref double delta_x, ref double delta_y, bool LTorRB)
        {
            if (currentRatioType == "origin")
            {
                return false;
            }
            else if (currentRatioType == "arbitrary")
            {
                //do nothing
                return true;
            }
            else
            {
                if (LTorRB)
                {
                    if (delta_x >= 0 && delta_y >= 0)
                    {
                        delta_x = delta_x > delta_y ? delta_x : delta_y;
                        delta_y = delta_x / currentRatio;
                        return true;
                    }
                    else if (delta_x <= 0 && delta_y <= 0)
                    {
                        delta_x = delta_x < delta_y ? delta_x : delta_y;
                        delta_y = delta_x / currentRatio;
                        return true;
                    }
                }
                else
                {
                    if (delta_x >= 0 && delta_y <= 0)
                    {
                        delta_x = delta_x > (0 - delta_y) ? delta_x : (0 - delta_y);
                        delta_y = 0 - delta_x / currentRatio;
                        return true;
                    }
                    else if (delta_x <= 0 && delta_y >= 0)
                    {
                        delta_x = delta_x < (0 - delta_y) ? delta_x : (0 - delta_y);
                        delta_y = 0 - delta_x / currentRatio;
                        return true;
                    }
                }
            }
            return false;
        }

        void knobRT_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var delta_x = e.Delta.Translation.X;
            var delta_y = e.Delta.Translation.Y;

            bool valid = ProcessRatio(ref delta_x, ref delta_y, false);
            if (!valid)
            {
                return;
            }

            valid = CheckClipChange(clip_L, clip_T + delta_y, clip_R + delta_x, clip_B);
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

            bool valid = ProcessRatio(ref delta_x, ref delta_y, false);
            if (!valid)
            {
                return;
            }

            valid = CheckClipChange(clip_L + delta_x, clip_T, clip_R, clip_B + delta_y);
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

            bool valid = ProcessRatio(ref delta_x, ref delta_y, true);
            if (!valid)
            {
                return;
            }

            valid = CheckClipChange(clip_L, clip_T, clip_R + delta_x, clip_B + delta_y);
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

            if ((R - L) < clip_MIN)
            {
                return false;
            }
            if ((B - T) < clip_MIN)
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
            if (geoGroup == null)
            {
                return;
            }

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

        private WriteableBitmap wbOrigin = null;

        private async void PreapreWritableBitmap(StorageFile file)
        {
            wbOrigin = await ImageHelper.Resize(file, Constants.PHOTO_IMPORT_SIZE_MAX);
            image.Source = wbOrigin;
        }

        private async void Crop()
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

            //return wbNew;
        }

        private async void ok_Click(object sender, RoutedEventArgs e)
        {
            //resultImage.Visibility = Visibility.Visible;
            Crop();
            //WriteableBitmap wb = await Crop();
            //resultImage.Source = wb;
        }

        #endregion

        #region Size Menu Popup

        bool sizeMenuPopupShown = false;

        private void ShowSizeMenuPopup()
        {
            if (!sizeMenuPopupShown)
            {
                VisualStateManager.GoToState(this, "vsSizeMenuPopupShown", true);
                sizeMenuPopupShown = true;
            }
        }

        private void HideSizeMenuPopup()
        {
            if (sizeMenuPopupShown)
            {
                VisualStateManager.GoToState(this, "vsSizeMenuPopupHidden", true);
                sizeMenuPopupShown = false;
            }
        }

        //private void selectedSize_Tapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (sizeMenuPopupShown)
        //    {
        //        HideSizeMenuPopup();
        //    }
        //    else
        //    {
        //        ShowSizeMenuPopup();
        //    }
        //}

        SolidColorBrush selectedBrush = new SolidColorBrush(Color.FromArgb(255, 255, 126, 126));// #ffff7e7e
        SolidColorBrush unselectedBrush = new SolidColorBrush(Color.FromArgb(255, 45, 45, 45));// #ff2d2d2d
        TextBlock selectedText = null;

        private void sizeMenuItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string tag = (sender as FrameworkElement).Tag.ToString();
            SetCropMode(tag);
            HideSizeMenuPopup();
        }

        private void SetCropMode(string tag)
        {
            currentRatioType = tag;

            selectedText.Foreground = unselectedBrush;

            switch (tag)
            {
                case "origin":
                    selectedText = menuItem_origin;
                    //selectedSizeText.Text = "原图";
                    break;
                case "fitApp":
                    currentRatio = Window.Current.Bounds.Width / (Window.Current.Bounds.Height - 72d);
                    selectedText = menuItem_fitApp;
                    //selectedSizeText.Text = "适应软件";
                    break;
                case "arbitrary":
                    selectedText = menuItem_arbitrary;
                    //selectedSizeText.Text = "任意";
                    break;
                case "1x1":
                    currentRatio = 1d;
                    selectedText = menuItem_1x1;
                    //selectedSizeText.Text = "1X1";
                    break;
                case "3x4":
                    currentRatio = 0.75d;
                    selectedText = menuItem_3x4;
                    //selectedSizeText.Text = "3X4";
                    break;
                default:
                    break;
            }

            selectedText.Foreground = selectedBrush;

            //reset clip area
            if (tag == "origin")
            {
                clip_L = 0d;
                clip_T = 0d;
                clip_R = image.ActualWidth;
                clip_B = image.ActualHeight;
            }
            else if (tag == "fitApp" || tag == "1x1" || tag == "3x4")
            {
                double shortSide = image.ActualWidth <= image.ActualHeight * currentRatio ? image.ActualWidth : image.ActualHeight;
                double longSide = image.ActualWidth > image.ActualHeight * currentRatio ? image.ActualWidth : image.ActualHeight;
                clip_L = image.ActualWidth == shortSide ? 0d : (image.ActualWidth - image.ActualHeight * currentRatio) / 2;
                clip_R = image.ActualWidth == shortSide ? image.ActualWidth : image.ActualWidth - (image.ActualWidth - image.ActualHeight * currentRatio) / 2;
                clip_T = image.ActualWidth == shortSide ? (image.ActualHeight - image.ActualWidth / currentRatio) / 2 : 0d;
                clip_B = image.ActualWidth == shortSide ? image.ActualHeight - (image.ActualHeight - image.ActualWidth / currentRatio) / 2 : image.ActualHeight;
            }

            //set edge knob visibility
            if (tag == "arbitrary")
            {
                knobL.Visibility = Visibility.Visible;
                knobR.Visibility = Visibility.Visible;
                knobT.Visibility = Visibility.Visible;
                knobB.Visibility = Visibility.Visible;
            }
            else
            {
                knobL.Visibility = Visibility.Collapsed;
                knobR.Visibility = Visibility.Collapsed;
                knobT.Visibility = Visibility.Collapsed;
                knobB.Visibility = Visibility.Collapsed;
            }

            Draw();
        }

        #endregion

        private void crop_Click(object sender, RoutedEventArgs e)
        {
            ShowSizeMenuPopup();
        }

        private void cancelCrop_Click(object sender, RoutedEventArgs e)
        {
            HideSizeMenuPopup();
        }


    }
}
