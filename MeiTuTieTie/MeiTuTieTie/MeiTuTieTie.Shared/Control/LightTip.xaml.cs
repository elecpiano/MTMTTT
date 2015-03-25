using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Shared.Control
{
    public sealed partial class LightTip : UserControl
    {
        #region Property

        #endregion

        public LightTip()
        {
            this.InitializeComponent();
        }

        public void ShowTip(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            txtMessage.Text = message;
            sbShowTip.Begin();
        }

    }
}
