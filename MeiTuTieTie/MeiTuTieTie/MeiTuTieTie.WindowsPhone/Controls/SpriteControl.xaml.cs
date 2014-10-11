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
        double g_rotation = 0d;

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
                var x = e.Delta.Translation.X;
                var y = e.Delta.Translation.Y;

                outerTransform.TranslateX += x;
                outerTransform.TranslateY += y;

                centerPointTransform.TranslateX += x;
                centerPointTransform.TranslateY += y;
            }
            else
            {
                g_rotation += e.Delta.Rotation;
                g_scale *= e.Delta.Scale;

                outerTransform.Rotation += e.Delta.Rotation;
                outerTransform.ScaleX *= e.Delta.Scale;
                outerTransform.ScaleY *= e.Delta.Scale;

                UpdateHandleScale();
            }

            SyncGhostPosition();
        }

        private void ghost_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ghostTransform.TranslateX += e.Delta.Translation.X;
            ghostTransform.TranslateY += e.Delta.Translation.Y;

            //old positions
            var oldPoint = handle.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var oldDistance = Math.Sqrt(oldPoint.X * oldPoint.X + oldPoint.Y * oldPoint.Y);
            var oldAngle = GetAngle(oldPoint.X, oldPoint.Y);

            //new positions
            var newPoint = handleGhost.TransformToVisual(centerPoint).TransformPoint(ZERO_POINT);
            var newDistance = Math.Sqrt(newPoint.X * newPoint.X + newPoint.Y * newPoint.Y);
            var newAngle = GetAngle(newPoint.X, newPoint.Y);

            //update rotation
            var rotation_delta = newAngle - oldAngle;
            outerTransform.Rotation += rotation_delta;
            e.Handled = true;

            return;

            //update scale
            var scale_delta = newDistance / oldDistance;
            outerTransform.ScaleX *= scale_delta;
            outerTransform.ScaleY *= scale_delta;
            g_scale *= scale_delta;

            UpdateHandleScale();

            return;

        }

        private void UpdateHandleScale()
        {
            //keep the handle size unchanged
            handleTransform.ScaleX = handleTransform.ScaleY = 1d / g_scale;
            //handleGhostTransform.ScaleX = handleGhostTransform.ScaleY = 1d / g_scale;
        }

        private void image_LayoutUpdated(object sender, object e)
        {
            //var point = handle.TransformToVisual(layoutRoot).TransformPoint(ZERO_POINT);
            //ghostTransform.TranslateX = point.X;
            //ghostTransform.TranslateY = point.Y;
            SyncGhostPosition();
        }

        private void SyncGhostPosition()
        {
            var point = handlePoint.TransformToVisual(layoutRoot).TransformPoint(ZERO_POINT);
            ghostTransform.TranslateX = point.X;
            ghostTransform.TranslateY = point.Y;
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


    }
}
