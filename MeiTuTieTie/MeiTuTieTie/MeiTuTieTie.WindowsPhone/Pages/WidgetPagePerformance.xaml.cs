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
    public sealed partial class WidgetPagePerformance : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public WidgetPagePerformance()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadData();
        }

        #endregion

        #region Pivot Item Collection

        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            panel_dict.Add(control.Tag.ToString(), control);
        }

        #endregion

        #region Data

        DataLoader<MyThemeData> myThemeDataLoader = null;
        DataLoader<MaterialGroup> materialDataLoader = null;
        List<Material> materials_all;
        Dictionary<string, List<Material>> material_dict;
        Dictionary<string, FrameworkElement> panel_dict = new Dictionary<string, FrameworkElement>();
        Dictionary<string, List<Triplet<Material>>> triplet_dict = new Dictionary<string, List<Triplet<Material>>>();

        class Triplet<T>
        {
            public T ItemOne { get; set; }
            public T ItemTwo { get; set; }
            public T ItemThree { get; set; }

            //public Triplet(T item1, T item2, T item3)
            //{
            //    ItemOne = item1;
            //    ItemTwo = item2;
            //    ItemThree = item3;
            //}

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

            foreach (var theme in themeData.myThemes)
            {
                if (theme.visible)
                {
                    await LoadMaterial(theme);
                }
            }

            //foreach (var key in panel_dict.Keys)
            //{
            //    //panel_dict[key].DataContext = material_dict[key];
            //    var tripletList = GetTriplets(material_dict[key]);
            //    panel_dict[key].DataContext = tripletList;
            //}

            foreach (var key in material_dict.Keys)
            {
                var tripletList = GetTriplets(material_dict[key]);
                triplet_dict.Add(key, tripletList);
            }

            materialsListView.ItemsSource = triplet_dict[MaterialType.keai.ToString()];
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

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string key = string.Empty;
            switch (pivot.SelectedIndex)
            {
                case 0:
                    key = MaterialType.keai.ToString();
                    break;
                case 1:
                    key = MaterialType.wenzi.ToString();
                    break;
                case 2:
                    key = MaterialType.gaoxiaobiaoqing.ToString();
                    break;
                case 3:
                    key = MaterialType.zhedang.ToString();
                    break;
                case 4:
                    key = MaterialType.katongxingxiang.ToString();
                    break;
                default:
                    break;
            }

            if (triplet_dict.ContainsKey(key))
            {
                scrollViewer.ChangeView(null, 0d, null, true);
                materialsListView.ItemsSource = triplet_dict[key];
            }
        }

        private void materialsListView_ManipulationCompleted(object sender, Windows.UI.Xaml.Input.ManipulationCompletedRoutedEventArgs e)
        {
            int newIndex = pivot.SelectedIndex;
            if (e.Velocities.Linear.X > 1)
            {
                newIndex--;
                newIndex = newIndex < 0 ? (pivot.Items.Count - 1) : newIndex;
            }
            else if (e.Velocities.Linear.X < -1)
            {
                newIndex++;
                newIndex = newIndex >= pivot.Items.Count ? 0 : newIndex;
            }
        }


    }

}
