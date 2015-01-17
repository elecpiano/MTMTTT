using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Shared.Control
{
    public class ImageSwitch : ContentControl
    {
        #region Property

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
            }
            else
            {
                VisualStateManager.GoToState(control, "UnChecked", true);
            }
        }

        #endregion

        #region Constructor

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.Checked == true)
            {
                VisualStateManager.GoToState(this, "Checked", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "UnChecked", false);
            }
        }

        #endregion

    }
}
