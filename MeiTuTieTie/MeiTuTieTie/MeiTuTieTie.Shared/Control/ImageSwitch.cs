using Shared.Animation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Control
{
    public class ImageSwitch : ContentControl
    {
        #region Property

        public ImageSource KnobImage
        {
            get { return (ImageSource)GetValue(KnobImageProperty); }
            set
            {
                SetValue(KnobImageProperty, value);
            }
        }

        public static readonly DependencyProperty KnobImageProperty =
            DependencyProperty.Register("KnobImage", typeof(ImageSource), typeof(ImageSwitch), new PropertyMetadata(null));

        public ImageSource CheckedImage
        {
            get { return (ImageSource)GetValue(CheckedImageProperty); }
            set
            {
                SetValue(CheckedImageProperty, value);
            }
        }

        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(ImageSource), typeof(ImageSwitch), new PropertyMetadata(null));

        public ImageSource UnCheckedImage
        {
            get { return (ImageSource)GetValue(UnCheckedImageProperty); }
            set
            {
                SetValue(UnCheckedImageProperty, value);
            }
        }

        public static readonly DependencyProperty UnCheckedImageProperty =
            DependencyProperty.Register("UnCheckedImage", typeof(ImageSource), typeof(ImageSwitch), new PropertyMetadata(null));

        public ImageSource DisabledCheckedImage
        {
            get { return (ImageSource)GetValue(DisabledCheckedImageProperty); }
            set
            {
                SetValue(DisabledCheckedImageProperty, value);
            }
        }

        public static readonly DependencyProperty DisabledCheckedImageProperty =
            DependencyProperty.Register("DisabledCheckedImage", typeof(ImageSource), typeof(ImageSwitch), new PropertyMetadata(null));

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set
            {
                SetValue(CheckedProperty, value);
            }
        }

        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(ImageSwitch), new PropertyMetadata(false, OnCheckedPropertyChanged));

        private static void OnCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageSwitch control = d as ImageSwitch;
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                VisualStateManager.GoToState(control, "Checked", true);
                control.KnobMove(true);
            }
            else
            {
                VisualStateManager.GoToState(control, "UnChecked", true);
                control.KnobMove(false);
            }
        }

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty); }
            set
            {
                SetValue(EnabledProperty, value);
            }
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled", typeof(bool), typeof(ImageSwitch), new PropertyMetadata(true, OnEnabledPropertyChanged));

        private static void OnEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageSwitch control = d as ImageSwitch;
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                VisualStateManager.GoToState(control, "Enabled", true);
            }
            else
            {
                VisualStateManager.GoToState(control, "Disabled", true);
            }
        }

        #endregion

        #region Constructor

        Grid knob = null;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            knob = this.GetTemplateChild("knob") as Grid;

            if (this.Checked == true)
            {
                VisualStateManager.GoToState(this, "Checked", false);
                KnobMove(true);
            }
            else
            {
                VisualStateManager.GoToState(this, "UnChecked", false);
                KnobMove(false);
            }

            if (this.Enabled == true)
            {
                VisualStateManager.GoToState(this, "Enabled", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }

            this.ManipulationMode = Windows.UI.Xaml.Input.ManipulationModes.TranslateX;
            this.ManipulationStarting += ImageSwitch_ManipulationStarting;
            this.ManipulationCompleted += ImageSwitch_ManipulationCompleted;
            this.Tapped += ImageSwitch_Tapped;
        }

        void ImageSwitch_ManipulationStarting(object sender, Windows.UI.Xaml.Input.ManipulationStartingRoutedEventArgs e)
        {
            this.ManipulationDelta += ImageSwitch_ManipulationDelta;
        }

        void ImageSwitch_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            this.ManipulationDelta -= ImageSwitch_ManipulationDelta;
        }

        void ImageSwitch_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CheckStateChanged(this, !this.Checked);
        }

        void ImageSwitch_ManipulationDelta(object sender, Windows.UI.Xaml.Input.ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X > 3)
            {
                CheckStateChanged(this, true);
                this.ManipulationDelta -= ImageSwitch_ManipulationDelta;
            }
            else if (e.Cumulative.Translation.X < -3)
            {
                CheckStateChanged(this, false);
                this.ManipulationDelta -= ImageSwitch_ManipulationDelta;
            }
        }

        #endregion

        #region Event

        public delegate void CheckStateChangedEventHandler(ImageSwitch sender, bool suggestedState);
        public event CheckStateChangedEventHandler CheckStateChanged;

        #endregion

        #region Knob Animation

        private void KnobMove(bool right)
        {
            double moveBy = this.ActualWidth * (right ? 0.5 : 0);
            MoveAnimation.MoveTo(knob, moveBy, 0, 200d);
        }

        #endregion

    }
}
