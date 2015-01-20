using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Shared.Animation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;

namespace MeiTuTieTie.Controls
{
    public class SpriteControl
    {
        #region Property

        private static Random RANDOM = new Random();
        private static List<SpriteControl> Sprites = new List<SpriteControl>();
        private const double APPEAR_DURATION = 900d;
        private static PowerEase EASING = new PowerEase();
        private const int BORDER_Z_INDEX = 999;
        private const int BUTTON_Z_INDEX = 9999;

        private Grid contentPanel = null;
        private Image image = null;
        private Grid handle = null;
        private Image removeButton = null;
        private Border border = null;
        private Ellipse LTPoint = null;
        private Ellipse RBPoint = null;
        private Ellipse centerPoint = null;
        private Ellipse handlePoint = null;

        private CompositeTransform _contentTransform = null;
        private CompositeTransform _LTTransform = null;
        private CompositeTransform _RBTransform = null;
        private CompositeTransform _centerPointTransform = null;
        private CompositeTransform borderTransform = null;
        private CompositeTransform _removeTransform = null;
        private CompositeTransform handleTransform = null;

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
                    handle.Visibility = _Selected ? Visibility.Visible : Visibility.Collapsed;
                    removeButton.Visibility = _Selected ? Visibility.Visible : Visibility.Collapsed;
                    border.Opacity = _Selected ? 1d : 0d;

                    if (_Selected)
                    {
                        SelectedSprite = this;
                        //border.SetValue(Canvas.ZIndexProperty, BORDER_ACTIVE_Z_INDEX);
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
                        //border.SetValue(Canvas.ZIndexProperty, BORDER_INACTIVE_Z_INDEX);
                        if (SelectedSprite == this)
                        {
                            SelectedSprite = null;
                        }
                    }
                }
            }
        }

        private static List<SpriteControl> ZIndexStack = new List<SpriteControl>();
        private int _ZIndex = 0;
        public int ZIndex
        {
            get { return _ZIndex; }
            set
            {
                if (_ZIndex != value)
                {
                    _ZIndex = value;
                    contentPanel.SetValue(Canvas.ZIndexProperty, _ZIndex);
                }
            }
        }

        public static SpriteControl SelectedSprite { get; set; }

        #endregion

        public SpriteControl()
        {
            CreateSprite();
        }

        #region Factory

        private CompositeTransform EnsureTransform(FrameworkElement cell)
        {
            CompositeTransform transform = cell.RenderTransform as CompositeTransform;
            if (transform == null)
            {
                cell.RenderTransform = transform = new CompositeTransform();
                cell.RenderTransformOrigin = new Point(0.5d, 0.5d);
            }
            transform = cell.RenderTransform as CompositeTransform;
            return transform;
        }

        private void CreateSprite()
        {
            //image
            image = new Image();
            image.Stretch = Stretch.Uniform;
            image.CacheMode = new BitmapCache();

            //LTPoint
            LTPoint = new Ellipse();
            LTPoint.Fill = new SolidColorBrush(Colors.Red);
            LTPoint.Width = 1d;
            LTPoint.Height = 1d;
            LTPoint.VerticalAlignment = VerticalAlignment.Top;
            LTPoint.HorizontalAlignment = HorizontalAlignment.Left;
            EnsureTransform(LTPoint);

            //RBPoint
            RBPoint = new Ellipse();
            RBPoint.Fill = new SolidColorBrush(Colors.Red);
            RBPoint.Width = 1d;
            RBPoint.Height = 1d;
            RBPoint.VerticalAlignment = VerticalAlignment.Bottom;
            RBPoint.HorizontalAlignment = HorizontalAlignment.Right;
            EnsureTransform(RBPoint);

            //centerPoint
            centerPoint = new Ellipse();
            centerPoint.Fill = new SolidColorBrush(Colors.Red);
            centerPoint.Width = 1d;
            centerPoint.Height = 1d;
            centerPoint.VerticalAlignment = VerticalAlignment.Center;
            centerPoint.HorizontalAlignment = HorizontalAlignment.Center;
            EnsureTransform(centerPoint);

            //contentPanel
            contentPanel = new Grid();
            contentPanel.VerticalAlignment = VerticalAlignment.Center;
            contentPanel.HorizontalAlignment = HorizontalAlignment.Center;
            contentPanel.MaxWidth = 150d;
            contentPanel.MaxHeight = 200d;
            EnsureTransform(contentPanel);

            contentPanel.Children.Add(image);
            contentPanel.Children.Add(LTPoint);
            contentPanel.Children.Add(RBPoint);
            contentPanel.Children.Add(centerPoint);

            //border
            border = new Border();
            border.Opacity = 0d;
            border.BorderThickness = new Thickness(1d);
            border.BorderBrush = new SolidColorBrush(Colors.Orange);
            border.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            border.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate | ManipulationModes.Scale;
            border.ManipulationDelta += this.border_ManipulationDelta;
            border.PointerPressed += this.border_PointerPressed;
            EnsureTransform(border);

            //removeButton
            removeButton = new Image();
            removeButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/Remove.png", UriKind.RelativeOrAbsolute));
            removeButton.Visibility = Visibility.Collapsed;
            removeButton.VerticalAlignment = VerticalAlignment.Top;
            removeButton.HorizontalAlignment = HorizontalAlignment.Left;
            removeButton.Width = 32d;
            removeButton.Height = 32d;
            removeButton.Margin = new Thickness(-16, -16, 0, 0);
            removeButton.Tapped += this.removeButton_Tapped;
            EnsureTransform(removeButton);

            //handlePoint
            handlePoint = new Ellipse();
            handlePoint.Fill = new SolidColorBrush(Colors.Red);
            handlePoint.Width = 1d;
            handlePoint.Height = 1d;
            handlePoint.VerticalAlignment = VerticalAlignment.Center;
            handlePoint.HorizontalAlignment = HorizontalAlignment.Center;

            //handle
            handle = new Grid();
            handle.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/Rotate.png", UriKind.Absolute)) };
            handle.Visibility = Visibility.Collapsed;
            handle.VerticalAlignment = VerticalAlignment.Top;
            handle.HorizontalAlignment = HorizontalAlignment.Left;
            handle.Width = 32d;
            handle.Height = 32d;
            handle.Margin = new Thickness(-16, -16, 0, 0);
            handle.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
            handle.ManipulationDelta += this.handle_ManipulationDelta;
            EnsureTransform(handle);

            handle.Children.Add(handlePoint);
        }

        #endregion

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

            borderTransform.TranslateX = g_pos_x;
            borderTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            _contentTransform.Rotation = g_rotation;
            borderTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            if (_contentTransform != null)
            {
                _contentTransform.ScaleX = g_scale;
                _contentTransform.ScaleY = g_scale;
            }
            borderTransform.ScaleX = borderTransform.ScaleY = g_scale;

            //keep the points' size unchanged, to avoid accumulated errors
            _LTTransform.ScaleX = _LTTransform.ScaleY = 1d / g_scale;
            _RBTransform.ScaleX = _RBTransform.ScaleY = 1d / g_scale;
            _centerPointTransform.ScaleX = _centerPointTransform.ScaleY = 1d / g_scale;
        }

        private void border_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
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

        private void border_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Selected = true;
            e.Handled = true;
        }

        private void PrepareForManipulation()
        {
            border.Width = contentPanel.ActualWidth;
            border.Height = contentPanel.ActualHeight;

            //for the property which had been accessed by Storyboard, seems like reclaiming is necessary in order to change the property manually again 
            _contentTransform = contentPanel.RenderTransform as CompositeTransform;
            _LTTransform = LTPoint.RenderTransform as CompositeTransform;
            _RBTransform = RBPoint.RenderTransform as CompositeTransform;
            _centerPointTransform = centerPoint.RenderTransform as CompositeTransform;
            borderTransform = border.RenderTransform as CompositeTransform;
            _removeTransform = removeButton.RenderTransform as CompositeTransform;
            handleTransform = handle.RenderTransform as CompositeTransform;

            SetRotation();
            SetPosition();
            SetScale();
            SyncButtonsPosition();
        }

        private void SyncButtonsPosition()
        {
            Point point = RBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            handleTransform.TranslateX = point.X;
            handleTransform.TranslateY = point.Y;

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
            this.image.Source = new BitmapImage(new Uri(source, UriKind.Absolute));
        }

        public void SetImage(BitmapImage bi)
        {
            this.image.Source = bi;
        }

        public void SetContainer(Grid grid)
        {
            _container = grid;
            Sprites.Add(this);
            int index = Sprites.IndexOf(this);

            //contentPanel
            contentPanel.Name = "contentPanel_" + index.ToString();
            _container.Children.Add(contentPanel);

            //z index
            contentPanel.SetValue(Canvas.ZIndexProperty, index);
            ZIndexStack.Add(this);

            //border
            _container.Children.Add(border);
            border.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);

            //handle
            _container.Children.Add(handle);
            handle.SetValue(Canvas.ZIndexProperty, BUTTON_Z_INDEX);

            //remove button
            _container.Children.Add(removeButton);
            removeButton.SetValue(Canvas.ZIndexProperty, BUTTON_Z_INDEX);

            Appear();
        }

        public void RemoveFromContainer()
        {
            _container.Children.Remove(contentPanel);
            _container.Children.Remove(border);
            _container.Children.Remove(handle);
            _container.Children.Remove(removeButton);
            Sprites.Remove(this);
        }

        public void Appear()
        {
            g_rotation = RANDOM.NextDouble() * 90d - 45d;
            g_pos_x = RANDOM.NextDouble() * 160d - 80d;
            g_pos_y = RANDOM.NextDouble() * 240d - 120d;
            g_scale = RANDOM.NextDouble() * 0.3d + 0.8d;

            double from_x = RANDOM.NextDouble() * 160d - 80d;
            double from_y = RANDOM.NextDouble() * 240d - 120d;
            MoveAnimation.MoveFromTo(contentPanel, from_x, from_y, g_pos_x, g_pos_y, APPEAR_DURATION, EASING,
                fe =>
                {
                    PrepareForManipulation();
                    //this.Selected = true;
                });

            RotateAnimation.RotateFromTo(contentPanel, 0d, g_rotation, APPEAR_DURATION);
            //ScaleAnimation.ScaleFromTo(contentPanel, 1d, 1d, g_scale, g_scale, APPEAR_DURATION);
            ScaleAnimation.SetScale(contentPanel, g_scale, g_scale);
        }

        public static void DismissActiveSprite()
        {
            if (SelectedSprite != null)
            {
                SelectedSprite.Selected = false;
            }
        }

        public void ChangeZIndex(bool up)
        {
            int index = ZIndexStack.IndexOf(this);

            if (up)
            {
                if (index < (ZIndexStack.Count - 1))
                {
                    index++;
                }
            }
            else
            {
                if (index > 0)
                {
                    index--;
                    //SpriteControl spriteToGoUp = ZIndexStack[index - 1];
                    //this.ZIndex--;
                    //spriteToGoUp.ZIndex++;
                }
            }
            ZIndexStack.Remove(this);
            ZIndexStack.Insert(index, this);

            foreach (var sprite in ZIndexStack)
            {
                sprite.ZIndex = ZIndexStack.IndexOf(sprite);
            }
        }

        #endregion

    }
}
