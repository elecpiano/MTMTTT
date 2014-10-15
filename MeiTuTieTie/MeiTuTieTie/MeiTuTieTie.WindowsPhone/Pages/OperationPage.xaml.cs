using MeiTuTieTie.Common;
using MeiTuTieTie.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace MeiTuTieTie.Pages
{
    public sealed partial class OperationPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        private List<SpriteControl> spriteList = new List<SpriteControl>();

        #endregion

        public OperationPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ThrowPhotos();

            return;
            foreach (var sprite in spriteList)
            {
                sprite.Selected = false;
            }
            //ImageHelper.CaptureToMediaLibrary(this.stagePanel, "1.jpg");
        }

        private void stageBackground_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SpriteControl.DismissActiveSprite();
        }

        private void ThrowPhotos()
        {
            SpriteControl sprite = null;

            for (int i = 0; i < 1; i++)
            {
                sprite = new SpriteControl();
                sprite.SetImage("ms-appx:///Assets/TestImages/TestImage001.jpg");
                spriteList.Add(sprite);

                sprite.SetContainer(stage);
            }
        }
    }
}
