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
using Windows.UI.Xaml;
using MeiTuTieTie.Animations;

namespace MeiTuTieTie.Controls
{
    public sealed partial class SpriteControl : UserControl
    {
        #region Property

        private static Random RANDOM = new Random();
        private static List<SpriteControl> Sprites = new List<SpriteControl>();
        private const double APPEAR_DURATION = 900d;

        private FrameworkElement _contentPanel = null;
        private FrameworkElement _handle = null;
        private FrameworkElement _removeButton = null;
        private FrameworkElement _border = null;

        private Grid _container = null;

        private bool _HandleVisible = false;
        public bool HandleVisible
        {
            get
            {
                return _HandleVisible;
            }
            set
            {
                if (_HandleVisible != value)
                {
                    _HandleVisible = value;
                    _handle.Visibility = _HandleVisible ? Visibility.Visible : Visibility.Collapsed;
                    _removeButton.Visibility = _HandleVisible ? Visibility.Visible : Visibility.Collapsed;
                    _border.Visibility = _HandleVisible ? Visibility.Visible : Visibility.Collapsed;

                    if (_HandleVisible)
                    {
                        foreach (var sprite in Sprites)
                        {
                            if (sprite != this)
                            {
                                sprite.HandleVisible = false;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        public SpriteControl()
        {
            this.InitializeComponent();
            _contentPanel = contentPanel;
            _handle = handle;
            _removeButton = removeButton;
            _border = border;
        }

        #region Manipulation

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

            borderTransform.TranslateX = g_pos_x;
            borderTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            contentTransform.Rotation = g_rotation;
            borderTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            contentTransform.ScaleX = g_scale;
            contentTransform.ScaleY = g_scale;
            borderTransform.ScaleX = borderTransform.ScaleY = g_scale;

            //keep the points' size unchanged, to avoid accumulated errors
            LTTransform.ScaleX = LTTransform.ScaleY = 1d / g_scale;
            RBTransform.ScaleX = RBTransform.ScaleY = 1d / g_scale;
            centerPointTransform.ScaleX = centerPointTransform.ScaleY = 1d / g_scale;
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

            SyncButtonsPosition();
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

            SyncButtonsPosition();

            e.Handled = true;
        }

        private void removeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RemoveFromContainer();
        }

        private void image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            HandleVisible = true;
        }

        private void image_LayoutUpdated(object sender, object e)
        {
            SetRotation();
            SetPosition();
            SetScale();
            SyncButtonsPosition();
        }

        private void SyncButtonsPosition()
        {
            _border.Width = contentPanel.ActualWidth;
            _border.Height = contentPanel.ActualHeight;

            Point point = RBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            handleTransform.TranslateX = point.X;
            handleTransform.TranslateY = point.Y;

            point = LTPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            removeTransform.TranslateX = point.X;
            removeTransform.TranslateY = point.Y;
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
            _container = grid;

            //contentPanel
            _contentPanel.Name = string.Empty;
            layoutRoot.Children.Remove(_contentPanel);
            _container.Children.Add(_contentPanel);

            //border
            layoutRoot.Children.Remove(_border);
            _container.Children.Add(_border);
            _border.SetValue(Canvas.ZIndexProperty, 99);

            //handle
            layoutRoot.Children.Remove(_handle);
            _container.Children.Add(_handle);
            _handle.SetValue(Canvas.ZIndexProperty, 999);

            //remove button
            layoutRoot.Children.Remove(_removeButton);
            _container.Children.Add(_removeButton);
            _removeButton.SetValue(Canvas.ZIndexProperty, 999);

            Sprites.Add(this);
        }

        public void RemoveFromContainer()
        {
            _container.Children.Remove(_contentPanel);
            _container.Children.Remove(_border);
            _container.Children.Remove(_handle);
            _container.Children.Remove(_removeButton);
        }

        public void Appear()
        {
            this.HandleVisible = false;

            double from_x = RANDOM.NextDouble() * 160d - 80d;
            double from_y = RANDOM.NextDouble() * 240d - 120d;
            MoveAnimation.MoveFromTo(this, from_x, from_y, 0d, 0d, APPEAR_DURATION, null,
                fe =>
                {
                    //ActuallyAddToContainer();
                    //this.HandleVisible = true;
                });

            double rotation = RANDOM.NextDouble() * 90d - 45d;

            RotateAnimation.RotateFromTo(this, 0d, rotation, APPEAR_DURATION);
        }

        #endregion

    }
}
