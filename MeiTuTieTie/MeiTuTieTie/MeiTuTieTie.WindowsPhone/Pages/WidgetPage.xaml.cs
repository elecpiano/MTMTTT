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

namespace MeiTuTieTie.Pages
{
    public enum WidgetPageType
    {
        Shipin,
        BianKuang,
        Beijing
    }

    public sealed partial class WidgetPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private WidgetPageType pageType = WidgetPageType.Shipin;

        #endregion

        #region Lifecycle

        public WidgetPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
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
                        break;
                    case WidgetPageType.BianKuang:
                        pageTitle.Text = "边框";
                        break;
                    case WidgetPageType.Beijing:
                        pageTitle.Text = "背景";
                        break;
                    default:
                        break;
                }
            }

            LoadData();
        }

        protected override void OnNavigatedFrom(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
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

            //populate list itemsources
            if (pageType == WidgetPageType.Shipin)
            {
                shipinPanel.Visibility = Visibility.Visible;
                pivotItem_1.DataContext = GetTriplets(material_dict[MaterialType.keai.ToString()]);
                pivotItem_2.DataContext = GetTriplets(material_dict[MaterialType.wenzi.ToString()]);
                pivotItem_3.DataContext = GetTriplets(material_dict[MaterialType.gaoxiaobiaoqing.ToString()]);
                pivotItem_4.DataContext = GetTriplets(material_dict[MaterialType.zhedang.ToString()]);
                pivotItem_5.DataContext = GetTriplets(material_dict[MaterialType.katongxingxiang.ToString()]);
            }
            else if (pageType == WidgetPageType.BianKuang)
            {
                biankuangPanel.Visibility = Visibility.Visible;
                biankuangPanel.DataContext = GetTriplets(material_dict[MaterialType.biankuang.ToString()]);
            }
            else if (pageType == WidgetPageType.Beijing)
            {
                beijingPanel.Visibility = Visibility.Visible;
                beijingPanel.DataContext = GetTriplets(material_dict[MaterialType.beijing.ToString()]);
            }
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
            while (index < maxIndex)
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
            OperationPage.SelectedMaterial = material;
            OperationPage.MaterialSelectedBy = this.pageType;
            navigationHelper.GoBack();
        }



    }

}
