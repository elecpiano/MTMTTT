using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Shared.Control
{
    public sealed partial class SpriteContextMenu : UserControl
    {
        #region Property

        const double ARROW_HALF_WIDTH = 16d;
        double half_W = 0d;
        double half_H = 0d;

        #endregion

        public SpriteContextMenu()
        {
            this.InitializeComponent();
            this.Loaded += SpriteContextMenu_Loaded;
        }

        void SpriteContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SpriteContextMenu_Loaded;
            half_W = this.ActualWidth / 2d;
            half_H = this.ActualHeight / 2d;
            this.Margin = new Thickness(-half_W, -half_H, -half_W, -half_H);
            Hide();
        }

        #region Show & Hide

        public void Show(FrameworkElement container, double x, double y)
        {
            double origin_x = x;
            double origin_y = y;
            y = y - 96d;

            var t = this.TransformToVisual(container);
            var centerPoint = new Point(half_W, half_H);
            var p = t.TransformPoint(centerPoint);

            x = (x - half_W) < 0 ? half_W : x;
            x = (x + half_W) > container.ActualWidth ? (container.ActualWidth - half_W) : x;

            y = (y - half_H) < 0 ? half_H : y;
            y = (y + half_H) > container.ActualHeight ? (container.ActualHeight - half_H) : y;

            transform.TranslateX = x;
            transform.TranslateY = y;

            double arrow_x = origin_x - x + half_W;
            arrow_x = arrow_x < ARROW_HALF_WIDTH ? ARROW_HALF_WIDTH : arrow_x;
            arrow_x = arrow_x > (half_W * 2d - ARROW_HALF_WIDTH) ? (half_W * 2d - ARROW_HALF_WIDTH) : arrow_x;
            arrowTransform.TranslateX = arrow_x;

            this.Opacity = 1d;
            this.IsHitTestVisible = true;
        }

        public void Hide()
        {
            this.Opacity = 0d;
            this.IsHitTestVisible = false;
        }

        #endregion

        #region Event

        public event EventHandler<string> MenuTapped;
        private void RaiseMenuTapped(string type)
        {
            if (MenuTapped != null)
            {
                MenuTapped(this, type);
            }
        }

        #endregion

        private void UpMost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RaiseMenuTapped("up_most");
        }

        private void DownMost_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RaiseMenuTapped("down_most");
        }

        private void Copy_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RaiseMenuTapped("copy");
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            RaiseMenuTapped("delete");
        }

    }
}
