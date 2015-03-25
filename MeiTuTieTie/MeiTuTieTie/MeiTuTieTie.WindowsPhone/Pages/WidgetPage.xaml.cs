using Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Shared.Utility;
using Shared.Model;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using Shared.Global;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Shared.Enum;
using Shared.Control;
using Windows.ApplicationModel.Activation;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MeiTuTieTie.Pages
{
    public sealed partial class WidgetPage : Page, IFileOpenPickerPageBase
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private WidgetPageType pageType = WidgetPageType.Shipin;
        private string CurrentListKey = "keai";

        #endregion

        #region Lifecycle

        public WidgetPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            appBar.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                pageType = (WidgetPageType)e.Parameter;
                switch (pageType)
                {
                    case WidgetPageType.Shipin:
                        pageTitle.Text = "饰品";
                        appBar.Visibility = Visibility.Visible;
                        break;
                    case WidgetPageType.BianKuang:
                        pageTitle.Text = "边框";
                        break;
                    case WidgetPageType.Beijing:
                        pageTitle.Text = "背景";
                        btnDIY.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }

            LoadData();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            SaveScrollPositon(CurrentListKey);
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
            }
            else
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            }

            base.OnNavigatedFrom(e);
        }

        #endregion

        #region Data

        DataLoader<MyThemeData> myThemeDataLoader = null;
        DataLoader<MaterialGroup> materialDataLoader = null;
        List<Material> materials_all;
        Dictionary<string, List<Material>> material_dict;
        Dictionary<string, List<Triplet<Material>>> triplet_dict = new Dictionary<string, List<Triplet<Material>>>();

        class Triplet<T>
        {
            public T ItemOne { get; set; }
            public T ItemTwo { get; set; }
            public T ItemThree { get; set; }

            public void Add(T item)
            {
                if (ItemOne == null)
                {
                    ItemOne = item;
                }
                else if (ItemTwo == null)
                {
                    ItemTwo = item;
                }
                else if (ItemThree == null)
                {
                    ItemThree = item;
                }
            }
        }

        private async Task LoadData()
        {
            materials_all = new List<Material>();

            if (myThemeDataLoader == null)
            {
                myThemeDataLoader = new DataLoader<MyThemeData>();
            }

            if (material_dict == null)
            {
                material_dict = new Dictionary<string, List<Material>>();
                var types = Enum.GetValues(typeof(MaterialType));
                foreach (var type in types)
                {
                    var key = type.ToString();
                    material_dict.Add(key, new List<Material>());
                }
            }
            else
            {
                foreach (var key in material_dict.Keys)
                {
                    material_dict[key] = new List<Material>();
                }
            }

            //load data file
            var themeData = await myThemeDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);
            if (themeData == null)
            {
                themeData = new MyThemeData();
            }

            //load materials
            foreach (var theme in themeData.myThemes)
            {
                if (theme.visible)
                {
                    await LoadMaterial(theme);
                }
            }

            if (pageType == WidgetPageType.Shipin)
            {
                materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.keai.ToString()]);
                VisualStateManager.GoToState(this, "vsKeai", true);
                CurrentListKey = "keai";
            }
            else if (pageType == WidgetPageType.BianKuang)
            {
                materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.biankuang.ToString()]);
                CurrentListKey = "biankuang";
            }
            else if (pageType == WidgetPageType.Beijing)
            {
                materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.beijing.ToString()]);
                CurrentListKey = "beijing";
            }

            RestoreScrollPosition(CurrentListKey);
        }

        private async Task LoadMaterial(MyTheme theme)
        {
            if (materialDataLoader == null)
            {
                materialDataLoader = new DataLoader<MaterialGroup>();
            }

            string materialFilePath = Path.Combine(Constants.THEME_MODULE, theme.id, Constants.MATERIAL_DATA_FILE);
            var materialGroup = await materialDataLoader.LoadLocalData(materialFilePath);
            if (materialGroup == null)
            {
                materialGroup = new MaterialGroup();
            }

            var visibleMaterials = materialGroup.Materials.Where(x => x.visible);

            foreach (var m in visibleMaterials)
            {
                materials_all.Add(m);
            }

            var types = Enum.GetValues(typeof(MaterialType));
            foreach (var type in types)
            {
                var key = type.ToString();
                var mList = visibleMaterials.Where(x => x.type == key);

                foreach (var m in mList)
                {
                    material_dict[key].Add(m);
                }
            }
        }

        private List<Triplet<Material>> GetTriplets(List<Material> list)
        {
            List<Triplet<Material>> triplets = new List<Triplet<Material>>();

            int index = 0;
            int maxIndex = list.Count - 1;
            Triplet<Material> triplet = null;
            while (index <= maxIndex)
            {
                var column = index % 3;
                if (column == 0)
                {
                    triplet = new Triplet<Material>();
                    triplets.Add(triplet);
                }
                triplet.Add(list[index]);
                index++;
            }
            return triplets;
        }

        #endregion

        #region DIY Background

        private const string Continuation_Key_Operation = "Operation";
        private const string Continuation_WidgetPage_DIY = "PickPhotos";

        private void diy_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            DIYBackground();
        }

        private void DIYBackground()
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.ContinuationData[Continuation_Key_Operation] = Continuation_WidgetPage_DIY;
            //picker.PickMultipleFilesAndContinue();
            picker.PickSingleFileAndContinue();
        }

        public async void PickPhotosContiue(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(Continuation_Key_Operation)
                && args.ContinuationData[Continuation_Key_Operation].ToString() == Continuation_WidgetPage_DIY)
            {
                if (args.Files != null && args.Files.Count == 1)
                {
                    var file = args.Files[0];
                    IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                    BitmapImage bi = new BitmapImage();
                    bi.SetSource(stream);
                    App.CurrentInstance.SelectedDIYBackground = bi;

                    navigationHelper.GoBack();
                }
            }
        }

        #endregion

        #region Scroll Position

        private void SaveScrollPositon(string key)
        {
            double pos = scrollViewer.VerticalOffset;
            App.CurrentInstance.UpdateSetting(key, pos);
        }

        private void RestoreScrollPosition(string key)
        {
            double pos = App.CurrentInstance.GetSetting<double>(key, 0d);
            DelayExecutor.Delay(200, 
                () =>
                {
                    scrollViewer.ChangeView(null,pos,null,true);
                });
        }

        #endregion

        private void material_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            string tag = ((FrameworkElement)sender).Tag.ToString();
            var triplet = sender.GetDataContext<Triplet<Material>>();
            Material material = null;
            switch (tag)
            {
                case "one":
                    material = triplet.ItemOne;
                    break;
                case "two":
                    material = triplet.ItemTwo;
                    break;
                case "three":
                    material = triplet.ItemThree;
                    break;
                default:
                    break;
            }
            App.CurrentInstance.SelectedMaterial = material;
            App.CurrentInstance.MaterialSelectedBy = this.pageType;
            navigationHelper.GoBack();
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            SaveScrollPositon(CurrentListKey);
            string type = (sender as FrameworkElement).Tag.ToString();
            switch (type)
            {
                case "keai":
                    materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.keai.ToString()]);
                    VisualStateManager.GoToState(this, "vsKeai", true);
                    break;
                case "wenzi":
                    materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.wenzi.ToString()]);
                    VisualStateManager.GoToState(this, "vsWenzi", true);
                    break;
                case "gaoxiaobiaoqing":
                    materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.gaoxiaobiaoqing.ToString()]);
                    VisualStateManager.GoToState(this, "vsGaoxiao", true);
                    break;
                case "zhedangwu":
                    materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.zhedang.ToString()]);
                    VisualStateManager.GoToState(this, "vsZhedang", true);
                    break;
                case "katongxingxiang":
                    materialListBox.ItemsSource = GetTriplets(material_dict[MaterialType.katongxingxiang.ToString()]);
                    VisualStateManager.GoToState(this, "vsKatong", true);
                    break;
                default:
                    break;
            }
            CurrentListKey = type;
            RestoreScrollPosition(CurrentListKey);
        }



    }

}
