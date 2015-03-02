using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Shared.Control
{
    public sealed partial class SpriteFrame : UserControl
    {
        public SpriteFrame()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            shadowPanel.Visibility = SpriteControl.ShadowEnabled ? Visibility.Visible : Visibility.Collapsed;
            borderPanel.Visibility = SpriteControl.WhiteBorderEnabled ? Visibility.Visible : Visibility.Collapsed;
            var margin = SpriteControl.WhiteBorderEnabled ? -15d : -6d;
            this.Margin = new Thickness(margin);
        }
    }
}
