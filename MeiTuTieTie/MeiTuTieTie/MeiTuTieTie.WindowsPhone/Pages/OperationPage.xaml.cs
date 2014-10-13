using MeiTuTieTie.Common;
using MeiTuTieTie.Controls;
using MeiTuTieTie.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SpriteControl sprite;
            
            sprite = new SpriteControl();
            sprite.SetImage("/Assets/TestImages/TestImage001.jpg");
            sprite.SetContainer(stagePanel);

            sprite = new SpriteControl();
            sprite.SetImage("/Assets/TestImages/TestImage001.jpg");
            sprite.SetContainer(stagePanel);

            sprite = new SpriteControl();
            sprite.SetImage("/Assets/TestImages/TestImage001.jpg");
            sprite.SetContainer(stagePanel);

            sprite.HandleVisible = true;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ImageHelper.CaptureToMediaLibrary(this.stagePanel, "1.jpg");
        }
    }
}
