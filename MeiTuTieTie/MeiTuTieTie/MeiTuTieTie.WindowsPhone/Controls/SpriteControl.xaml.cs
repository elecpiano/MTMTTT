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
        private const double APPEAR_DURATION = 500d;

        private FrameworkElement _contentPanel = null;
        private FrameworkElement _handle = null;
        private FrameworkElement _removeButton = null;
        private FrameworkElement _border = null;

        private CompositeTransform _contentTransform = null;
        private CompositeTransform _LTTransform = null;
        private CompositeTransform _RBTransform = null;
        private CompositeTransform _centerPointTransform = null;
        private CompositeTransform _borderTransform = null;
        private CompositeTransform _removeTransform = null;
        private CompositeTransform _handleTransform = null;

        private bool transformRecognized = false;

        private Grid _container = null;

        private bool _Selected = false;
        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    _handle.Visibility = _Selected ? Visibility.Visible : Visibility.Collapsed;
                    _removeButton.Visibility = _Selected ? Visibility.Visible : Visibility.Collapsed;
                    _border.Visibility = _Selected ? Visibility.Visible : Visibility.Collapsed;

                    if (_Selected)
                    {
                        SelectedSprite = this;
                        foreach (var sprite in Sprites)
                        {
                            if (sprite != this)
                            {
                                sprite.Selected = false;
                            }
                        }
                    }
                    else
                    {
                        if (SelectedSprite == this)
                        {
                            SelectedSprite = null;
                        }
                    }
                }
            }
        }

        public static SpriteControl SelectedSprite { get; set; }

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

            _contentTransform.TranslateX = g_pos_x;
            _contentTransform.TranslateY = g_pos_y;

            _borderTransform.TranslateX = g_pos_x;
            _borderTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            _contentTransform.Rotation = g_rotation;
            _borderTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            if (_contentTransform != null)
            {
                _contentTransform.ScaleX = g_scale;
                _contentTransform.ScaleY = g_scale;
            }
            _borderTransform.ScaleX = _borderTransform.ScaleY = g_scale;

            //keep the points' size unchanged, to avoid accumulated errors
            _LTTransform.ScaleX = _LTTransform.ScaleY = 1d / g_scale;
            _RBTransform.ScaleX = _RBTransform.ScaleY = 1d / g_scale;
            _centerPointTransform.ScaleX = _centerPointTransform.ScaleY = 1d / g_scale;
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
            _handleTransform.TranslateX += e.Delta.Translation.X;
            _handleTransform.TranslateY += e.Delta.Translation.Y;

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
            Selected = true;
            e.Handled = true;
        }

        private void image_LayoutUpdated(object sender, object e)
        {
            if (!transformRecognized)
            {
                return;
            }
            SetRotation();
            SetPosition();
            SetScale();
            SyncButtonsPosition();
        }

        private void SyncButtonsPosition()
        {
            Point point = RBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            _handleTransform.TranslateX = point.X;
            _handleTransform.TranslateY = point.Y;

            point = LTPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            _removeTransform.TranslateX = point.X;
            _removeTransform.TranslateY = point.Y;
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
            Sprites.Add(this);
            int index = Sprites.IndexOf(this);

            //contentPanel
            layoutRoot.Children.Remove(_contentPanel);
            _contentPanel.Name = "_contentPanel_" + index.ToString();
            _container.Children.Add(_contentPanel);

            //border
            layoutRoot.Children.Remove(_border);
            layoutRoot.Name = "layoutRoot" + index.ToString();
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


            Appear();
        }
        public void PrepareForManipulation()
        {
            _border.Width = contentPanel.ActualWidth;
            _border.Height = contentPanel.ActualHeight;

            //for the property which had been accessed by Storyboard, seems like reclaiming is necessary in order to change the property manually again 
            _contentTransform = _contentPanel.RenderTransform as CompositeTransform;
            _LTTransform = LTPoint.RenderTransform as CompositeTransform;
            _RBTransform = RBPoint.RenderTransform as CompositeTransform;
            _centerPointTransform = centerPoint.RenderTransform as CompositeTransform;
            _borderTransform = border.RenderTransform as CompositeTransform;
            _removeTransform = removeButton.RenderTransform as CompositeTransform;
            _handleTransform = handle.RenderTransform as CompositeTransform;

            transformRecognized = true;
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
            g_rotation = RANDOM.NextDouble() * 90d - 45d;
            g_pos_x = RANDOM.NextDouble() * 160d - 80d;
            g_pos_y = RANDOM.NextDouble() * 240d - 120d;
            g_scale = RANDOM.NextDouble() * 0.3d + 0.8d;

            PrepareForManipulation();
            return;

            double from_x = RANDOM.NextDouble() * 160d - 80d;
            double from_y = RANDOM.NextDouble() * 240d - 120d;
            MoveAnimation.MoveFromTo(_contentPanel, from_x, from_y, g_pos_x, g_pos_y, APPEAR_DURATION, null,
                fe =>
                {
                    PrepareForManipulation();
                    //this.Selected = true;
                });

            RotateAnimation.RotateFromTo(_contentPanel, 0d, g_rotation, APPEAR_DURATION);
            //ScaleAnimation.ScaleFromTo(_contentPanel, 1d, 1d, g_scale, g_scale, APPEAR_DURATION);
            ScaleAnimation.SetScale(_contentPanel, g_scale, g_scale);
        }

        public static void DismissActiveSprite()
        {
            if (SelectedSprite != null)
            {
                SelectedSprite.Selected = false;
            }
        }

        #endregion

    }
}
