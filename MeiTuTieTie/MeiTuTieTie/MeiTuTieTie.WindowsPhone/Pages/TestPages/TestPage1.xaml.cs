using Shared.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace MeiTuTieTie.Pages
{
    public class MyModel
    {
        public string img { get; set; }
        public int index { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage1 : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        public TestPage1()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                listbox.ItemsSource = listData;
            }
            else
            {
                AvoidScrollJump();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            }
            else
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            }
            base.OnNavigatingFrom(e);
        }

        #region Load Data

        ObservableCollection<MyModel> listData = new ObservableCollection<MyModel>();
        int index = 0;
        private void LoadMore_Click(object sender, RoutedEventArgs e)
        {
            LoadMore();
        }

        private void LoadMore()
        {
            for (int i = 0; i < 20; i++)
            {
                index++;
                MyModel model = new MyModel();
                model.img = "http://2.bp.blogspot.com/_qOmHhMh3aqQ/TT0vdftNJ2I/AAAAAAAAABY/xO86A9XwhU8/s1600/buterfly+1.jpeg";
                model.index = index;
                listData.Add(model);
            }
        }

        private void Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(TestPage2));
        }

        #endregion

        #region To avoid scroll jump

        VirtualizingStackPanel vsp = null;
        private void VirtualizingStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            vsp = sender as VirtualizingStackPanel;
        }

        MyModel tempItem = new MyModel();
        private void AvoidScrollJump()
        {
            vsp.LayoutUpdated += vsp_LayoutUpdated;
            listData.Add(tempItem);
        }

        void vsp_LayoutUpdated(object sender, object e)
        {
            listData.Remove(tempItem);
            vsp.LayoutUpdated -= vsp_LayoutUpdated;
        }

        #endregion



    }
}
