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
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        private void singlePic_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(OperationPage), "single");
        }

        private void multiPic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void boutique_Click(object sender, RoutedEventArgs e)
        {

        }

        private void more_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
