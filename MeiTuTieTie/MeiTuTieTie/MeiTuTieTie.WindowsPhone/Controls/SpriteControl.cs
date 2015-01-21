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
    public enum SpriteType
    {
        Image,
        Text
    }

    public class SpriteControl
    {
        #region Property

        private static Random RANDOM = new Random();
        private static List<SpriteControl> Sprites = new List<SpriteControl>();
        private const double APPEAR_DURATION = 900d;
        private static PowerEase EASING = new PowerEase();
        private const int BORDER_Z_INDEX = 999;
        private const int BUTTON_Z_INDEX = 9999;

        private static Grid _container = null;

        private static Grid handle = null;
        private static Ellipse handlePoint = null;
        private static Image removeButton = null;
        private static Border border = null;

        private static CompositeTransform _borderTransform = null;
        private static CompositeTransform _removeTransform = null;
        private static CompositeTransform _handleTransform = null;

        private Grid contentPanel = null;
        private Image image = null;
        private TextBox textBox = null;
        private Ellipse LTPoint = null;
        private Ellipse RBPoint = null;
        private Ellipse centerPoint = null;

        private CompositeTransform _contentTransform = null;
        private CompositeTransform _LTTransform = null;
        private CompositeTransform _RBTransform = null;
        private CompositeTransform _centerPointTransform = null;

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

                    if (_Selected)
                    {
                        SelectedSprite = this;
                        SetButtonVisibility(true);
                        SyncButtonsPosition();

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

        private bool _Locked = false;
        public bool Locked
        {
            get { return _Locked; }
            set
            {
                if (_Locked != value)
                {
                    _Locked = value;
                    if (_Locked)
                    {
                    }
                    else
                    {
                    }
                }
            }
        }

        private bool _Manipulatable = false;
        public bool Manipulatable
        {
            get { return _Manipulatable; }
            set
            {
                if (_Manipulatable != value)
                {
                    _Manipulatable = value;
                    if (_Manipulatable)
                    {
                        if (!Locked)
                        {
                            contentPanel.IsHitTestVisible = true;
                        }
                    }
                    else
                    {
                        contentPanel.IsHitTestVisible = false;
                    }
                }
            }
        }

        private static List<SpriteControl> ZIndexStack = new List<SpriteControl>();
        public int ZIndex
        {
            get { return (int)this.contentPanel.GetValue(Canvas.ZIndexProperty); }
            set
            {
                if (ZIndex != value)
                {
                    contentPanel.SetValue(Canvas.ZIndexProperty, value);
                }
            }
        }

        public static SpriteControl SelectedSprite { get; set; }

        private SpriteType _SpriteType = SpriteType.Image;
        public SpriteType SpriteType
        {
            get { return _SpriteType; }
        }

        #endregion

        #region Lifecycle

        public SpriteControl(SpriteType type)
        {
            _SpriteType = type;
            CreateSprite();
        }

        #endregion

        #region Factory

        private static void CreateCommonComponents()
        {
            //border
            if (border == null)
            {
                border = new Border();
                border.IsHitTestVisible = false;
                border.Opacity = 0d;
                border.BorderThickness = new Thickness(1d);
                border.BorderBrush = new SolidColorBrush(Colors.Orange);
                border.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                EnsureTransform(border);
                _borderTransform = border.RenderTransform as CompositeTransform;
            }

            //removeButton
            if (removeButton == null)
            {
                removeButton = new Image();
                removeButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/Remove.png", UriKind.RelativeOrAbsolute));
                removeButton.Visibility = Visibility.Collapsed;
                removeButton.VerticalAlignment = VerticalAlignment.Top;
                removeButton.HorizontalAlignment = HorizontalAlignment.Left;
                removeButton.Width = 32d;
                removeButton.Height = 32d;
                removeButton.Margin = new Thickness(-16, -16, 0, 0);
                removeButton.Tapped += removeButton_Tapped;
                EnsureTransform(removeButton);

                _removeTransform = removeButton.RenderTransform as CompositeTransform;
            }

            //handle
            if (handle == null)
            {
                handle = new Grid();
                handle.Background = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/Rotate.png", UriKind.Absolute)) };
                handle.Visibility = Visibility.Collapsed;
                handle.VerticalAlignment = VerticalAlignment.Top;
                handle.HorizontalAlignment = HorizontalAlignment.Left;
                handle.Width = 32d;
                handle.Height = 32d;
                handle.Margin = new Thickness(-16, -16, 0, 0);
                handle.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                handle.ManipulationDelta += handle_ManipulationDelta;
                EnsureTransform(handle);
                _handleTransform = handle.RenderTransform as CompositeTransform;

                //handle point
                handlePoint = new Ellipse();
                handlePoint.Fill = new SolidColorBrush(Colors.Red);
                handlePoint.Width = 1d;
                handlePoint.Height = 1d;
                handlePoint.VerticalAlignment = VerticalAlignment.Center;
                handlePoint.HorizontalAlignment = HorizontalAlignment.Center;

                handle.Children.Add(handlePoint);
            }
        }

        protected virtual void CreateSprite()
        {
            if (this.SpriteType == Controls.SpriteType.Image)
            {
                //image
                image = new Image();
                image.Stretch = Stretch.Uniform;
                image.CacheMode = new BitmapCache();
            }
            else if (this.SpriteType == Controls.SpriteType.Text)
            {

            }

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

            contentPanel.PointerPressed += contentPanel_PointerPressed;
            contentPanel.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate | ManipulationModes.Scale;
            contentPanel.ManipulationDelta += contentPanel_ManipulationDelta;
            contentPanel.ManipulationStarting += contentPanel_ManipulationStarting;
            contentPanel.ManipulationCompleted += contentPanel_ManipulationCompleted;
        }

        #endregion

        #region Manipulation

        static Point ZERO_POINT = new Point(0, 0);
        double g_scale = 1d;
        double g_rotation = 0d;
        double g_pos_x = 0d;
        double g_pos_y = 0d;

        private void SetPosition(double delta_x = 0d, double delta_y = 0d)
        {
            g_pos_x += delta_x;
            g_pos_y += delta_y;

            _contentTransform.TranslateX = g_pos_x;
            _contentTransform.TranslateY = g_pos_y;

            //_borderTransform.TranslateX = g_pos_x;
            //_borderTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            _contentTransform.Rotation = g_rotation;
            //_borderTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            if (_contentTransform != null)
            {
                _contentTransform.ScaleX = g_scale;
                _contentTransform.ScaleY = g_scale;
            }
            //_borderTransform.ScaleX = _borderTransform.ScaleY = g_scale;

            //keep the points' size unchanged, to avoid accumulated errors
            _LTTransform.ScaleX = _LTTransform.ScaleY = 1d / g_scale;
            _RBTransform.ScaleX = _RBTransform.ScaleY = 1d / g_scale;
            _centerPointTransform.ScaleX = _centerPointTransform.ScaleY = 1d / g_scale;
        }

        void contentPanel_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            foreach (var sprite in Sprites)
            {
                if (sprite != this)
                {
                    sprite.Manipulatable = false; //.contentPanel.IsHitTestVisible = false;
                }
            }
        }

        void contentPanel_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            foreach (var sprite in Sprites)
            {
                sprite.Manipulatable = true; //.contentPanel.IsHitTestVisible = true;
            }
        }

        private void contentPanel_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            bool isDrag = e.Delta.Rotation == 0 && e.Delta.Expansion == 0;

            if (isDrag)
            {
                this.SetPosition(e.Delta.Translation.X, e.Delta.Translation.Y);
            }
            else
            {
                this.SetScale(e.Delta.Scale);
                this.SetRotation(e.Delta.Rotation);
            }

            SyncButtonsPosition();
        }

        private static void handle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _handleTransform.TranslateX += e.Delta.Translation.X;
            _handleTransform.TranslateY += e.Delta.Translation.Y;

            //RB position
            var oldPoint = SelectedSprite.RBPoint.TransformToVisual(SelectedSprite.centerPoint).TransformPoint(ZERO_POINT);
            var oldDistance = Math.Sqrt(oldPoint.X * oldPoint.X + oldPoint.Y * oldPoint.Y);
            var oldAngle = GetAngle(oldPoint.X, oldPoint.Y);

            //Handle position
            var newPoint = handlePoint.TransformToVisual(SelectedSprite.centerPoint).TransformPoint(ZERO_POINT);
            var newDistance = Math.Sqrt(newPoint.X * newPoint.X + newPoint.Y * newPoint.Y);
            var newAngle = GetAngle(newPoint.X, newPoint.Y);

            //update rotation
            var rotation_delta = newAngle - oldAngle;
            SelectedSprite.SetRotation(rotation_delta);

            //update scale
            var scale_delta = newDistance / oldDistance;
            SelectedSprite.SetScale(scale_delta);

            SelectedSprite.SyncButtonsPosition();

            e.Handled = true;
        }

        private static void removeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RemoveSelectedSprite();
        }

        void contentPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Selected = true;
            e.Handled = true;
        }

        private void PrepareForManipulation()
        {
            //for the property which had been accessed by Storyboard, seems like reclaiming is necessary in order to change the property manually again 
            _contentTransform = contentPanel.RenderTransform as CompositeTransform;
            _LTTransform = LTPoint.RenderTransform as CompositeTransform;
            _RBTransform = RBPoint.RenderTransform as CompositeTransform;
            _centerPointTransform = centerPoint.RenderTransform as CompositeTransform;

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

            //border
            _borderTransform.TranslateX = g_pos_x;
            _borderTransform.TranslateY = g_pos_y;
            _borderTransform.Rotation = g_rotation;
            _borderTransform.ScaleX = _borderTransform.ScaleY = g_scale;
        }

        private static double GetAngle(double x, double y)
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

        #region Button Visibility

        public static void SetButtonVisibility(bool visible)
        {
            handle.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            removeButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            border.Opacity = visible ? 1d : 0d;
            if (visible)
            {
                border.Width = SelectedSprite.contentPanel.ActualWidth;
                border.Height = SelectedSprite.contentPanel.ActualHeight;
            }
        }

        #endregion

        #region Private Method

        private static CompositeTransform EnsureTransform(FrameworkElement cell)
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

        private static void DetachFromParent(FrameworkElement fe)
        {
            Panel parent = fe.Parent as Panel;
            if (parent != null)
            {
                parent.Children.Remove(fe);
            }
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

        public static void Initialize(Grid container)
        {
            CreateCommonComponents();

            _container = container;

            //border
            if (!_container.Children.Contains(border))
            {
                _container.Children.Add(border);
                border.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
            }

            //handle
            if (!_container.Children.Contains(handle))
            {
                _container.Children.Add(handle);
                handle.SetValue(Canvas.ZIndexProperty, BUTTON_Z_INDEX);
            }

            //remove button
            if (!_container.Children.Contains(removeButton))
            {
                _container.Children.Add(removeButton);
                removeButton.SetValue(Canvas.ZIndexProperty, BUTTON_Z_INDEX);
            }

        }

        public void AddToContainer()
        {
            Sprites.Add(this);
            int index = Sprites.IndexOf(this);

            //contentPanel
            contentPanel.Name = "contentPanel_" + index.ToString();
            _container.Children.Add(contentPanel);

            //z index
            contentPanel.SetValue(Canvas.ZIndexProperty, index);
            ZIndexStack.Add(this);

            Appear();
        }

        public static void RemoveSelectedSprite()
        {
            if (SelectedSprite != null)
            {
                _container.Children.Remove(SelectedSprite.contentPanel);
                //_container.Children.Remove(border);
                //_container.Children.Remove(handle);
                //_container.Children.Remove(removeButton);
                Sprites.Remove(SelectedSprite);
                ZIndexStack.Remove(SelectedSprite);
                SetButtonVisibility(false);
            }
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
            SetButtonVisibility(false);
            foreach (var sprite in Sprites)
            {
                sprite.Manipulatable = true; //.contentPanel.IsHitTestVisible = true;
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
                }
            }
            ZIndexStack.Remove(this);
            ZIndexStack.Insert(index, this);

            foreach (var sprite in ZIndexStack)
            {
                sprite.ZIndex = ZIndexStack.IndexOf(sprite);
            }
        }

        public static void DetachContainer()
        {
            DetachFromParent(border);
            DetachFromParent(handle);
            DetachFromParent(removeButton);
            border = null;
            handle = null;
            handlePoint = null;
            removeButton = null;

            foreach (var sprite in Sprites)
            {
                DetachFromParent(sprite.contentPanel);
            }
            Sprites.Clear();
            ZIndexStack.Clear();
            _container = null;
            SelectedSprite = null;
        }

        #endregion

    }
}
