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
using MeiTuTieTie;

namespace Shared.Control
{
    public enum SpriteType
    {
        Photo,
        Material,
        Text
    }

    public class SpriteControl
    {
        #region Property

        private static Random RANDOM = new Random();
        private static List<SpriteControl> Sprites = new List<SpriteControl>();
        private const double APPEAR_DURATION = 500d;
        private static PowerEase EASING = new PowerEase();
        private const int BORDER_Z_INDEX = 999;
        private const int BUTTON_Z_INDEX = 9999;
        private const double BORDER_THICKNESS = 2d;
        private const double HANDLE_BUTTON_WIDTH = 40d;

        private static Grid _container = null;

        private static Grid handle = null;
        private static Ellipse handlePoint = null;
        private static Image removeButton = null;

        //private static Border border = null;
        private static Line borderL = null;
        private static Line borderR = null;
        private static Line borderT = null;
        private static Line borderB = null;

        //private static CompositeTransform _borderTransform = null;
        private static CompositeTransform _removeTransform = null;
        private static CompositeTransform _handleTransform = null;

        private Grid contentPanel = null;
        private Image image = null;
        public SpriteTextBox spriteText = null;
        private Ellipse LTPoint = null;
        private Ellipse RTPoint = null;
        private Ellipse LBPoint = null;
        private Ellipse RBPoint = null;
        private Ellipse centerPoint = null;

        private SpriteFrame spriteFrame = null;

        private CompositeTransform _contentTransform = null;
        private CompositeTransform _LTTransform = null;
        private CompositeTransform _RBTransform = null;
        private CompositeTransform _centerPointTransform = null;

        private double imageWidth = 0d;
        private double imageHeight = 0d;

        private double imageWidthInitial = 0d;
        private double imageHeightInitial = 0d;

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

                        RaiseOnSelected();
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
                    RaiseSpriteChanged();
                }
            }
        }

        public static SpriteControl SelectedSprite { get; set; }

        private SpriteType _SpriteType = SpriteType.Photo;
        public SpriteType SpriteType
        {
            get { return _SpriteType; }
        }

        public static bool EdgeEnabled { get; set; }
        public static bool ShadowEnabled { get; set; }

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
            if (borderL == null)
            {
                //border = new Border();
                //border.IsHitTestVisible = false;
                //border.Opacity = 0d;
                //border.BorderThickness = new Thickness(BORDER_THICKNESS);
                //border.BorderBrush = new SolidColorBrush(Colors.Orange);
                //border.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                //EnsureTransform(border);
                //_borderTransform = border.RenderTransform as CompositeTransform;
                //border.HorizontalAlignment = HorizontalAlignment.Center;
                //border.VerticalAlignment = VerticalAlignment.Center;
                //border.Margin = new Thickness(-99999);

                borderL = new Line();
                borderR = new Line();
                borderT = new Line();
                borderB = new Line();
                borderL.Opacity = borderR.Opacity = borderT.Opacity = borderB.Opacity = 0d;
                borderL.Stroke = borderR.Stroke = borderT.Stroke = borderB.Stroke = new SolidColorBrush(Colors.Orange);
                borderL.StrokeThickness = borderR.StrokeThickness = borderT.StrokeThickness = borderB.StrokeThickness = BORDER_THICKNESS;
            }

            //removeButton
            if (removeButton == null)
            {
                removeButton = new Image();
                removeButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/Images/Remove.png", UriKind.RelativeOrAbsolute));
                removeButton.Visibility = Visibility.Collapsed;
                removeButton.VerticalAlignment = VerticalAlignment.Top;
                removeButton.HorizontalAlignment = HorizontalAlignment.Left;
                removeButton.Width = HANDLE_BUTTON_WIDTH;
                removeButton.Height = HANDLE_BUTTON_WIDTH;
                removeButton.Margin = new Thickness(-HANDLE_BUTTON_WIDTH / 2, -HANDLE_BUTTON_WIDTH / 2, 0, 0);
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
                handle.Width = HANDLE_BUTTON_WIDTH;
                handle.Height = HANDLE_BUTTON_WIDTH;
                handle.Margin = new Thickness(-HANDLE_BUTTON_WIDTH / 2, -HANDLE_BUTTON_WIDTH / 2, 0, 0);
                handle.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
                handle.ManipulationDelta += handle_ManipulationDelta;
                handle.ManipulationCompleted += handle_ManipulationCompleted;
                EnsureTransform(handle);
                _handleTransform = handle.RenderTransform as CompositeTransform;

                //handle point
                handlePoint = new Ellipse();
                handlePoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                handlePoint.Width = 1d;
                handlePoint.Height = 1d;
                handlePoint.VerticalAlignment = VerticalAlignment.Center;
                handlePoint.HorizontalAlignment = HorizontalAlignment.Center;

                handle.Children.Add(handlePoint);
            }
        }

        private void CreateSprite()
        {
            //contentPanel
            contentPanel = new Grid();
            contentPanel.VerticalAlignment = VerticalAlignment.Center;
            contentPanel.HorizontalAlignment = HorizontalAlignment.Center;
            contentPanel.Margin = new Thickness(-99999);
            EnsureTransform(contentPanel);

            //LTPoint
            LTPoint = new Ellipse();
            LTPoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            LTPoint.Width = 1d;
            LTPoint.Height = 1d;
            LTPoint.VerticalAlignment = VerticalAlignment.Top;
            LTPoint.HorizontalAlignment = HorizontalAlignment.Left;
            EnsureTransform(LTPoint);

            //RTPoint
            RTPoint = new Ellipse();
            RTPoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            RTPoint.Width = 1d;
            RTPoint.Height = 1d;
            RTPoint.VerticalAlignment = VerticalAlignment.Top;
            RTPoint.HorizontalAlignment = HorizontalAlignment.Right;
            EnsureTransform(RTPoint);

            //LBPoint
            LBPoint = new Ellipse();
            LBPoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            LBPoint.Width = 1d;
            LBPoint.Height = 1d;
            LBPoint.VerticalAlignment = VerticalAlignment.Bottom;
            LBPoint.HorizontalAlignment = HorizontalAlignment.Left;
            EnsureTransform(LBPoint);

            //RBPoint
            RBPoint = new Ellipse();
            RBPoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            RBPoint.Width = 1d;
            RBPoint.Height = 1d;
            RBPoint.VerticalAlignment = VerticalAlignment.Bottom;
            RBPoint.HorizontalAlignment = HorizontalAlignment.Right;
            EnsureTransform(RBPoint);

            //centerPoint
            centerPoint = new Ellipse();
            centerPoint.Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            centerPoint.Width = 1d;
            centerPoint.Height = 1d;
            centerPoint.VerticalAlignment = VerticalAlignment.Center;
            centerPoint.HorizontalAlignment = HorizontalAlignment.Center;
            EnsureTransform(centerPoint);

            contentPanel.Children.Add(LTPoint);
            contentPanel.Children.Add(RTPoint);
            contentPanel.Children.Add(LBPoint);
            contentPanel.Children.Add(RBPoint);
            contentPanel.Children.Add(centerPoint);

            if (this.SpriteType == SpriteType.Photo)
            {
                //spriteFrame
                if (EdgeEnabled || ShadowEnabled)
                {
                    spriteFrame = new SpriteFrame();
                    contentPanel.Children.Add(spriteFrame);
                }

                //image
                image = new Image();
                image.Stretch = Stretch.Uniform;
                image.CacheMode = new BitmapCache();
                contentPanel.Children.Add(image);
            }
            else if (this.SpriteType == SpriteType.Material)
            {
                //image
                image = new Image();
                image.Stretch = Stretch.Uniform;
                image.CacheMode = new BitmapCache();
                contentPanel.Children.Add(image);
            }
            else if (this.SpriteType == SpriteType.Text)
            {
                spriteText = new SpriteTextBox() { ContainerSpriteControl = this };
                spriteText.TextChanged += spriteText_TextChanged;
                spriteText.EditingStarted += spriteText_EditingStarted;
                spriteText.EditingEnded += spriteText_EditingEnded;
                contentPanel.Children.Add(spriteText);
                contentPanel.MaxWidth = 300d;
                contentPanel.MinHeight = 80d;
            }

            //AttachManipulationEvents();
        }

        #endregion

        #region Manipulation

        static Point ZERO_POINT = new Point(0, 0);
        double g_scale = 1d;
        double g_rotation = 0d;
        double g_pos_x = 0d;
        double g_pos_y = 0d;

        private void PrepareForManipulation()
        {
            contentPanel.PointerPressed += contentPanel_PointerPressed;
            contentPanel.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.Rotate | ManipulationModes.Scale;
            contentPanel.ManipulationDelta += contentPanel_ManipulationDelta;
            contentPanel.ManipulationStarting += contentPanel_ManipulationStarting;
            contentPanel.ManipulationCompleted += contentPanel_ManipulationCompleted;

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

        private void SetPosition(double delta_x = 0d, double delta_y = 0d)
        {
            g_pos_x += delta_x;
            g_pos_y += delta_y;

            _contentTransform.TranslateX = g_pos_x;
            _contentTransform.TranslateY = g_pos_y;
        }

        private void SetRotation(double delta_r = 0d)
        {
            g_rotation += delta_r;
            _contentTransform.Rotation = g_rotation;
        }

        private void SetScale(double delta_scale = 1d)
        {
            g_scale *= delta_scale;

            if (SpriteType == SpriteType.Photo || SpriteType == SpriteType.Material)
            {
                imageWidth = imageWidthInitial * g_scale;
                imageHeight = imageHeightInitial * g_scale;
                contentPanel.Width = imageWidth;
                contentPanel.Height = imageHeight;
            }
            else if (SpriteType == SpriteType.Text)
            {
                if (_contentTransform != null)
                {
                    _contentTransform.ScaleX = g_scale;
                    _contentTransform.ScaleY = g_scale;
                }

                //keep the points' size unchanged, to avoid accumulated errors
                _LTTransform.ScaleX = _LTTransform.ScaleY = 1d / g_scale;
                _RBTransform.ScaleX = _RBTransform.ScaleY = 1d / g_scale;
                _centerPointTransform.ScaleX = _centerPointTransform.ScaleY = 1d / g_scale;
            }
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
            RaiseSpriteChanged();
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

        static bool handleDrag = false;
        private static void handle_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            handleDrag = true;

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

        static void handle_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            handleDrag = false;
            SelectedSprite.RaiseSpriteChanged();
        }

        private static void removeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SelectedSprite.RaiseSpriteChanged();
            RemoveSelectedSprite();
        }

        void contentPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Selected = true;
            e.Handled = true;
        }

        //public static void SetButtonVisibility2(bool visible)
        //{
        //    handle.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        //    removeButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        //    border.Opacity = visible ? 1d : 0d;
        //    if (visible)
        //    {
        //        border.Width = SelectedSprite.imageWidthInitial;// SelectedSprite.contentPanel.ActualWidth;
        //        border.Height = SelectedSprite.imageHeightInitial;// SelectedSprite.contentPanel.ActualHeight;
        //    }
        //}

        public static void SetButtonVisibility(bool visible)
        {
            handle.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            removeButton.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            //border.Opacity = visible ? 1d : 0d;
            borderL.Opacity = borderR.Opacity = borderT.Opacity = borderB.Opacity = visible ? 1d : 0d;
        }

        public void SyncButtonsPosition()
        {
            Point pointLT = LTPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            Point pointRT = RTPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            Point pointLB = LBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
            Point pointRB = RBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);

            if (!handleDrag)
            {
                _handleTransform.TranslateX = pointRB.X;
                _handleTransform.TranslateY = pointRB.Y;
            }

            _removeTransform.TranslateX = pointLT.X;
            _removeTransform.TranslateY = pointLT.Y;

            ////border
            //_borderTransform.TranslateX = g_pos_x;
            //_borderTransform.TranslateY = g_pos_y;
            //_borderTransform.Rotation = g_rotation;
            //border.Width = image.ActualWidth;
            //border.Height = image.ActualHeight;

            borderL.X1 = pointLT.X;
            borderL.Y1 = pointLT.Y;
            borderL.X2 = pointLB.X;
            borderL.Y2 = pointLB.Y;

            borderR.X1 = pointRT.X;
            borderR.Y1 = pointRT.Y;
            borderR.X2 = pointRB.X;
            borderR.Y2 = pointRB.Y;

            borderT.X1 = pointLT.X;
            borderT.Y1 = pointLT.Y;
            borderT.X2 = pointRT.X;
            borderT.Y2 = pointRT.Y;

            borderB.X1 = pointLB.X;
            borderB.Y1 = pointLB.Y;
            borderB.X2 = pointRB.X;
            borderB.Y2 = pointRB.Y;
        }

        //private void SyncButtonsPosition2()
        //{
        //    Point point = RBPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
        //    _handleTransform.TranslateX = point.X;
        //    _handleTransform.TranslateY = point.Y;

        //    point = LTPoint.TransformToVisual(_container).TransformPoint(ZERO_POINT);
        //    _removeTransform.TranslateX = point.X;
        //    _removeTransform.TranslateY = point.Y;

        //    //border
        //    _borderTransform.TranslateX = g_pos_x;
        //    _borderTransform.TranslateY = g_pos_y;
        //    _borderTransform.Rotation = g_rotation;
        //    _borderTransform.ScaleX = _borderTransform.ScaleY = g_scale;

        //    //keep borderWhite thickness unchanged
        //    double thickness = BORDER_THICKNESS / g_scale;
        //    border.BorderThickness = new Thickness(thickness);
        //}

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

        #region Text Editing

        void spriteText_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetButtonVisibility(true);
            SyncButtonsPosition();
            RaiseSpriteChanged();
        }

        void spriteText_EditingStarted(object sender, EventArgs e)
        {
            if (this.EditingStarted != null)
            {
                EditingStarted(this, EventArgs.Empty);
            }
        }

        void spriteText_EditingEnded(object sender, EventArgs e)
        {
            if (this.EditingEnded != null)
            {
                EditingEnded(this, EventArgs.Empty);
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

        public void Appear()
        {
            g_rotation = RANDOM.NextDouble() * 90d - 45d;
            g_pos_x = RANDOM.NextDouble() * 160d - 80d;
            g_pos_y = RANDOM.NextDouble() * 240d - 120d;

            if (_SpriteType == Control.SpriteType.Photo)
            {
                g_scale = 1d;
                double from_x = RANDOM.Next(2) == 0 ? -imageWidthInitial : App.SCREEN_WIDTH + imageWidthInitial;
                double from_y = RANDOM.Next(2) == 0 ? -imageHeightInitial : App.SCREEN_HEIGHT + imageHeightInitial;

                RotateAnimation.RotateFromTo(contentPanel, 0d, g_rotation, APPEAR_DURATION);
                MoveAnimation.MoveFromTo(contentPanel, from_x, from_y, g_pos_x, g_pos_y, APPEAR_DURATION, EASING,
                    fe =>
                    {
                        PrepareForManipulation();
                    });
            }
            else if (_SpriteType == Control.SpriteType.Material || _SpriteType == Control.SpriteType.Text)
            {
                g_scale = RANDOM.NextDouble() * 0.3d + 0.8d;
                MoveAnimation.SetPosition(contentPanel, g_pos_x, g_pos_y);
                FadeAnimation.Fade(contentPanel, 0d, 1d, APPEAR_DURATION);
                RotateAnimation.RotateFromTo(contentPanel, 0d, g_rotation, APPEAR_DURATION);
                ScaleAnimation.ScaleFromTo(contentPanel, 1.5d, 1.5d, g_scale, g_scale, APPEAR_DURATION,
                fe =>
                {
                    ScaleAnimation.SetScale(fe, 1d, 1d);
                    PrepareForManipulation();
                });
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

            double width = _SpriteType == SpriteType.Photo ? 240d : 160;
            double maxHeight = _SpriteType == SpriteType.Photo ? 320d : 240;
            double height = width * (double)bi.PixelHeight / (double)bi.PixelWidth;
            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * (double)bi.PixelWidth / (double)bi.PixelHeight;
            }
            contentPanel.Width = imageWidth = imageWidthInitial = width;
            contentPanel.Height = imageHeight = imageHeightInitial = height;
        }

        public void SetImage(WriteableBitmap wb)
        {
            this.image.Source = wb;
        }

        public static void Initialize(Grid container)
        {
            CreateCommonComponents();

            _container = container;

            ////border
            //if (!_container.Children.Contains(border))
            //{
            //    _container.Children.Add(border);
            //    border.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
            //}

            if (!_container.Children.Contains(borderL))
            {
                _container.Children.Add(borderL);
                borderL.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
                _container.Children.Add(borderR);
                borderR.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
                _container.Children.Add(borderT);
                borderT.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
                _container.Children.Add(borderB);
                borderB.SetValue(Canvas.ZIndexProperty, BORDER_Z_INDEX);
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
            //contentPanel.Name = "contentPanel_" + index.ToString();
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
                Sprites.Remove(SelectedSprite);
                ZIndexStack.Remove(SelectedSprite);
                SetButtonVisibility(false);
                RaiseRemoved();
                SelectedSprite = null;
            }
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

            RaiseOnSelected();
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
            //DetachFromParent(border);
            DetachFromParent(borderL);
            DetachFromParent(borderR);
            DetachFromParent(borderT);
            DetachFromParent(borderB);

            DetachFromParent(handle);
            DetachFromParent(removeButton);
            //border = null;
            borderL = borderR = borderT = borderB = null;

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

        #region Event

        public event EventHandler EditingStarted;
        public event EventHandler EditingEnded;
        public static event EventHandler OnSelected;
        public static event EventHandler OnRemoved;
        public static event EventHandler OnSpriteChanged;

        private static void RaiseOnSelected()
        {
            if (OnSelected != null)
            {
                OnSelected(SelectedSprite, EventArgs.Empty);
            }
        }

        private static void RaiseRemoved()
        {
            if (OnRemoved != null)
            {
                OnRemoved(SelectedSprite, EventArgs.Empty);
            }
        }

        public void RaiseSpriteChanged()
        {
            if (OnSpriteChanged != null)
            {
                OnSpriteChanged(SelectedSprite, EventArgs.Empty);
            }
        }

        #endregion
    }
}
