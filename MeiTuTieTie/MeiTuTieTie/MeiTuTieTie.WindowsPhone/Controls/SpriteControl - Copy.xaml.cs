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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MeiTuTieTie.Controls
{
    public sealed partial class SpriteControl : UserControl
    {
        Point ZERO_POINT = new Point(0, 0);
        double g_scale = 1d;

        public SpriteControl()
        {
            this.InitializeComponent();
        }

        bool isDrag = false;
        private void image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            isDrag = e.Delta.Rotation == 0 && e.Delta.Expansion == 0;

            if (isDrag)
            {
                transform.TranslateX += e.Delta.Translation.X;
                transform.TranslateY += e.Delta.Translation.Y;
            }
            else
            {
                transform.Rotation += e.Delta.Rotation;
                transform.ScaleX *= e.Delta.Scale;
                transform.ScaleY *= e.Delta.Scale;
                g_scale *= e.Delta.Scale;

                UpdateHandleScale();
            }
        }

        private void handle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //old positions
            var oldPoint = handleGhost.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var oldDistance = Math.Sqrt(oldPoint.X * oldPoint.X + oldPoint.Y * oldPoint.Y);
            var oldAngle = GetAngle(oldPoint.X, oldPoint.Y);

            handleGhostTransform.TranslateX += e.Delta.Translation.X;
            handleGhostTransform.TranslateY += e.Delta.Translation.Y;

            //new positions
            var newPoint = handleGhost.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var newDistance = Math.Sqrt(newPoint.X * newPoint.X + newPoint.Y * newPoint.Y);
            var newAngle = GetAngle(newPoint.X, newPoint.Y);

            

            //update scale
            var scale = newDistance / oldDistance;
            innerTransform.ScaleX *= scale;
            innerTransform.ScaleY *= scale;
            g_scale *= scale;

            UpdateHandleScale();

            //update rotation
            innerTransform.Rotation += (newAngle - oldAngle);

            e.Handled = true;
        }

        private void UpdateHandleScale()
        {
            //keep the handle size unchanged
            handleTransform.ScaleX = handleTransform.ScaleY = 1d / g_scale;
            handleGhostTransform.ScaleX = handleGhostTransform.ScaleY = 1d / g_scale;
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

        private void handle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

        }

        private void image_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            return;
            //reset ghost
            //var point = handlePoint.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            //handleGhostTransform.TranslateX = point.X;
            //handleGhostTransform.TranslateY = point.Y;
        }

    }
}
