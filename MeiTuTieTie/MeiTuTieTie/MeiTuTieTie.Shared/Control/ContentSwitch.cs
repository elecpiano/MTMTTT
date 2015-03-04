using Shared.Animation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Shared.Control
{
    public class ContentSwitch : ContentControl
    {
        #region Property

        //CheckedVisual
        public object CheckedVisual
        {
            get { return (object)GetValue(CheckedVisualProperty); }
            set
            {
                SetValue(CheckedVisualProperty, value);
            }
        }

        public static readonly DependencyProperty CheckedVisualProperty =
            DependencyProperty.Register("CheckedVisual", typeof(object), typeof(ContentSwitch), new PropertyMetadata(null));

        //UnCheckedVisual
        public object UnCheckedVisual
        {
            get { return (object)GetValue(UnCheckedVisualProperty); }
            set
            {
                SetValue(UnCheckedVisualProperty, value);
            }
        }

        public static readonly DependencyProperty UnCheckedVisualProperty =
            DependencyProperty.Register("UnCheckedVisual", typeof(object), typeof(ContentSwitch), new PropertyMetadata(null));

        //DisabledCheckedVisual
        public object DisabledCheckedVisual
        {
            get { return (object)GetValue(DisabledCheckedVisualProperty); }
            set
            {
                SetValue(DisabledCheckedVisualProperty, value);
            }
        }

        public static readonly DependencyProperty DisabledCheckedVisualProperty =
            DependencyProperty.Register("DisabledCheckedVisual", typeof(object), typeof(ContentSwitch), new PropertyMetadata(null));

        //DisabledUnCheckedVisual
        public object DisabledUnCheckedVisual
        {
            get { return (object)GetValue(DisabledUnCheckedVisualProperty); }
            set
            {
                SetValue(DisabledUnCheckedVisualProperty, value);
            }
        }

        public static readonly DependencyProperty DisabledUnCheckedVisualProperty =
            DependencyProperty.Register("DisabledUnCheckedVisual", typeof(object), typeof(ContentSwitch), new PropertyMetadata(null));

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set
            {
                SetValue(CheckedProperty, value);
            }
        }

        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(ContentSwitch), new PropertyMetadata(false, OnCheckedPropertyChanged));

        private static void OnCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContentSwitch control = d as ContentSwitch;
            bool newValue = (bool)e.NewValue;
            if (newValue)
            {
                VisualStateManager.GoToState(control, "Checked", true);
            }
            else
            {
                VisualStateManager.GoToState(control, "UnChecked", true);
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
            DependencyProperty.Register("Enabled", typeof(bool), typeof(ContentSwitch), new PropertyMetadata(true, OnEnabledPropertyChanged));

        private static void OnEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ContentSwitch control = d as ContentSwitch;
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
            }
            else
            {
                VisualStateManager.GoToState(this, "UnChecked", false);
            }

            if (this.Enabled == true)
            {
                VisualStateManager.GoToState(this, "Enabled", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", false);
            }

            this.Tapped += ContentSwitch_Tapped;
        }

        void ContentSwitch_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            CheckStateChanged(this, !this.Checked);
            e.Handled = true;
        }


        #endregion

        #region Event

        public delegate void CheckStateChangedEventHandler(ContentSwitch sender, bool suggestedState);
        public event CheckStateChangedEventHandler CheckStateChanged;

        #endregion

    }
}
