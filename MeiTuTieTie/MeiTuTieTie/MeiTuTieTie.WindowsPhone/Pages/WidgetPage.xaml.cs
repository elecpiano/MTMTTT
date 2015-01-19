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

        #region MyTheme Data

        DataLoader<MyThemeData> myThemeDataLoader = null;
        DataLoader<MaterialGroup> materialDataLoader = null;
        List<Material> materials_all;
        Dictionary<string, List<Material>> material_dict;

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

            listBox_keai.ItemsSource = material_dict[MaterialType.keai.ToString()];
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

            foreach (var m in materialGroup.Materials)
            {
                if (m.visible)
                {
                    materials_all.Add(m);
                }
            }

            var types = Enum.GetValues(typeof(MaterialType));
            foreach (var type in types)
            {
                var key = type.ToString();
                var materials = materials_all.Where(x => x.type == key).ToList();

                foreach (var m in materials)
                {
                    material_dict[key].Add(m);
                }
            }

        }

        #endregion

    }

}
