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
    public sealed partial class WidgetPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

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
            LoadData();
        }

        #endregion

        #region Data

        DataLoader<MyThemeData> myThemeDataLoader = null;
        DataLoader<MaterialGroup> materialDataLoader = null;
        List<Material> materials_all;
        Dictionary<string, List<Material>> material_dict;
        Dictionary<string, FrameworkElement> panel_dict = new Dictionary<string, FrameworkElement>();

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

            foreach (var key in panel_dict.Keys)
            {
                panel_dict[key].DataContext = material_dict[key];
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

        #endregion

        private void PivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement control = sender as FrameworkElement;
            panel_dict.Add(control.Tag.ToString(), control);
        }

    }

}
