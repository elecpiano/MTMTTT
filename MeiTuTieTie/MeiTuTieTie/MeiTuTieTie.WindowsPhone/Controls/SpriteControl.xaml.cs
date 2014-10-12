using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Controls
{
    public sealed partial class SpriteControl : UserControl
    {
        private static Random RANDOM = new Random();

        public SpriteControl()
        {
            this.InitializeComponent();
            RandomAppear();
        }

        #region Manipulation

        Grid container;

        Point ZERO_POINT = new Point(0, 0);
        double g_scale = 1d;
        double g_rotation = 0d;
        double g_pos_x = 0d;
        double g_pos_y = 0d;

        bool isDrag = false;

        private void SetPosition(double delta_x = 0d, double delta_y = 0d)
        {
            g_pos_x += delta_x;
            g_pos_y += delta_y;

            contentTransform.TranslateX = g_pos_x;
            contentTransform.TranslateY = g_pos_y;

            //centerPointTransform.TranslateX = g_pos_x;
            //centerPointTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            contentTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            contentTransform.ScaleX = g_scale;
            contentTransform.ScaleY = g_scale;

            //keep the points' size unchanged, to avoid accumulated errors
            RBTransform.ScaleX = RBTransform.ScaleY = 1d / g_scale;
            centerPointTransform.ScaleX = centerPointTransform.ScaleY = 1d / g_scale;
            removeTransform.ScaleX = removeTransform.ScaleY = 1d / g_scale;
        }

        private void image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            isDrag = e.Delta.Rotation == 0 && e.Delta.Expansion == 0;

            if (isDrag)
            {
                SetPosition(e.Delta.Translation.X, e.Delta.Translation.Y);
            }
            else
            {
                SetScale(e.Delta.Scale);
                SetRotation(e.Delta.Rotation);
            }

            SyncHandlePosition();
        }

        private void handle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            handleTransform.TranslateX += e.Delta.Translation.X;
            handleTransform.TranslateY += e.Delta.Translation.Y;

            //RB position
            var oldPoint = RBPoint.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var oldDistance = Math.Sqrt(oldPoint.X * oldPoint.X + oldPoint.Y * oldPoint.Y);
            var oldAngle = GetAngle(oldPoint.X, oldPoint.Y);

            //Handle position
            var newPoint = handlePoint.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var newDistance = Math.Sqrt(newPoint.X * newPoint.X + newPoint.Y * newPoint.Y);
            var newAngle = GetAngle(newPoint.X, newPoint.Y);

            //update rotation
            var rotation_delta = newAngle - oldAngle;
            SetRotation(rotation_delta);

            //update scale
            var scale_delta = newDistance / oldDistance;
            SetScale(scale_delta);

            SyncHandlePosition();

            e.Handled = true;
        }

        private void removeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RemoveFromContainer();
        }

        private void image_LayoutUpdated(object sender, object e)
        {
            SyncHandlePosition();
            SetRotation();
            SetPosition();
            SetScale();
        }

        private void SyncHandlePosition()
        {
            Point point = RBPoint.TransformToVisual(container).TransformPoint(ZERO_POINT);
            handleTransform.TranslateX = point.X;
            handleTransform.TranslateY = point.Y;
        }

        private double GetAngle(double x, double y)
        {
            // Note that this function works in xaml coordinates, where positive y is down, and the
            // angle is computed clockwise from the x-axis. 
            double angle = Math.Atan2(y, x);

            // Atan2() returns values between pi and -pi.  We want a value between
            // 0 and 2 pi.  In order to compensate for this, we'll add 2 pi to the angle
            // if it's less than 0, and then multiply by 180 / pi to get the angle
            // in degrees rather than radians, which are the expected units in XAML.
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            return angle * 180 / Math.PI;
        }

        #endregion

        #region Randomize

        private void RandomAppear()
        {
            g_rotation = RANDOM.NextDouble() * 90d - 45d;
            g_pos_x = RANDOM.NextDouble() * 160d - 80d;
            g_pos_y = RANDOM.NextDouble() * 240d - 120d;
            g_scale = RANDOM.NextDouble() * 0.3d + 0.8d;
        }

        #endregion

        #region Public Method

        public void SetImage(string source)
        {
            this.image.DataContext = source;
        }

        public void SetContainer(Grid grid)
        {
            container = grid;

            layoutRoot.Children.Remove(contentPanel);
            container.Children.Add(contentPanel);

            layoutRoot.Children.Remove(handle);
            container.Children.Add(handle);
        }

        public void RemoveFromContainer()
        {
            container.Children.Remove(contentPanel);
            container.Children.Remove(handle);
        }

        #endregion

        
    }
}
