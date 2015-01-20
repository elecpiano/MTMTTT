using Shared.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Shared.Utility;
using Shared.Model;
using Windows.UI.Xaml.Navigation;
using System.Xml.Serialization;
using Windows.Graphics.Display;
using Shared.Global;
using System.Linq;
using System.IO;

namespace MeiTuTieTie.Pages
{
    public sealed partial class MyThemeDetailPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;
        private MyTheme currentTheme = null;

        #endregion

        #region Lifecycle

        public MyThemeDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                currentTheme = (MyTheme)e.Parameter;
                LoadData_SplitFile(currentTheme);
            }
        }

        #endregion

        #region Data

        DataLoader<MaterialGroup> materialDataLoader = null;
        MaterialGroup materialGroup = null;
        string materialFilePath = string.Empty;

        private async void LoadData_SingleFile(MyTheme theme)
        {
            if (materialDataLoader == null)
            {
                materialDataLoader = new DataLoader<MaterialGroup>();
            }

            materialGroup = await materialDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE);
            if (materialGroup == null)
            {
                materialGroup = new MaterialGroup();
            }

            var materials = from m in materialGroup.Materials
                            where m.themePackID.Equals(theme.id)
                            select m;

            foreach (var material in materials)
            {
                material.ThemeEnabled = theme.visible;
            }

            this.myMaterialListBox.ItemsSource = materials;
        }

        private async void LoadData_SplitFile(MyTheme theme)
        {
            if (materialDataLoader == null)
            {
                materialDataLoader = new DataLoader<MaterialGroup>();
            }

            materialFilePath = Path.Combine(Constants.THEME_MODULE, theme.id, Constants.MATERIAL_DATA_FILE);
            materialGroup = await materialDataLoader.LoadLocalData(materialFilePath);
            if (materialGroup == null)
            {
                materialGroup = new MaterialGroup();
            }

            var materials = from m in materialGroup.Materials
                            where m.themePackID.Equals(theme.id)
                            select m;

            foreach (var material in materials)
            {
                material.ThemeEnabled = theme.visible;
            }

            this.myMaterialListBox.ItemsSource = materials;
        }


        private async void SaveData()
        {
            string json = JsonSerializer.Serialize(materialGroup);
            //await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE, json);
            await IsolatedStorageHelper.WriteToFileAsync(materialFilePath, json);
        }

        #endregion

        private void ImageSwitch_CheckStateChanged(Shared.Control.ImageSwitch sender, bool suggestedState)
        {
            Material material = sender.GetDataContext<Material>();
            if (material != null)
            {
                material.visible = !material.visible;
                SaveData();
            }
        }

    }
}
