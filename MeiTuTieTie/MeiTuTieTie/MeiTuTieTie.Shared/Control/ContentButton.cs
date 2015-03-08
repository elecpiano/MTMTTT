using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Control
{
    public class ContentButton : ContentControl
    {
        #region Property

        Storyboard sbNormal = null;
        Storyboard sbPressed = null;
        DoubleAnimation daPressedScaleX = null;
        DoubleAnimation daPressedScaleY = null;

        //PressedScaleX
        public double PressedScaleX
        {
            get { return (double)GetValue(PressedScaleXProperty); }
            set
            {
                SetValue(PressedScaleXProperty, value);
            }
        }

        public static readonly DependencyProperty PressedScaleXProperty =
            DependencyProperty.Register("PressedScaleX", typeof(double), typeof(ContentButton), new PropertyMetadata(0.95d));

        //PressedScaleY
        public double PressedScaleY
        {
            get { return (double)GetValue(PressedScaleYProperty); }
            set
            {
                SetValue(PressedScaleYProperty, value);
            }
        }

        public static readonly DependencyProperty PressedScaleYProperty =
            DependencyProperty.Register("PressedScaleY", typeof(double), typeof(ContentButton), new PropertyMetadata(0.95d));

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            sbNormal = GetTemplateChild("sbNormal") as Storyboard;
            sbPressed = GetTemplateChild("sbPressed") as Storyboard;
            daPressedScaleX = GetTemplateChild("daPressedScaleX") as DoubleAnimation;
            daPressedScaleX.To = PressedScaleX;
            daPressedScaleY = GetTemplateChild("daPressedScaleY") as DoubleAnimation;
            daPressedScaleY.To = PressedScaleY;
        }

        protected override void OnPointerPressed(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);
//            VisualStateManager.GoToState(this, "Pressed", false);
            sbPressed.Begin();
        }

        protected override void OnPointerExited(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);
            //VisualStateManager.GoToState(this, "Normal", true);
            sbNormal.Begin();
        }

        protected override void OnPointerReleased(Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            base.OnPointerReleased(e);
            //VisualStateManager.GoToState(this, "Normal", true);
            sbNormal.Begin();
        }

    }
}
