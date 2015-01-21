using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Shared.Control
{
    public sealed partial class SpriteTextBox : UserControl
    {
        #region Property

        private bool _IsVirgin = true;
        private bool IsVirgin
        {
            get { return _IsVirgin; }
            set
            {
                if (_IsVirgin != value)
                {
                    _IsVirgin = value;
                    virginTextBlock.Visibility = _IsVirgin ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private static SolidColorBrush TransparentBrush = new SolidColorBrush(Color.FromArgb(0,0,0,0));
        private static SolidColorBrush EditBackgroundBrush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

        #endregion

        public SpriteTextBox()
        {
            this.InitializeComponent();
            this.textBox.LostFocus += textBox_LostFocus;
        }

        void textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                IsVirgin = true;
            }
            else
            {
                IsVirgin = false;
            }
            rootGrid.Background = TransparentBrush;
        }

        private void mask_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!IsVirgin)
            {
                BeginEdit();
            }
        }

        private void mask_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (IsVirgin)
            {
                BeginEdit();
            }
        }

        private void BeginEdit()
        {
            this.textBox.Focus(FocusState.Programmatic);
            rootGrid.Background = EditBackgroundBrush;
        }
    }
}
