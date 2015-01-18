﻿using Shared.Common;
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

namespace MeiTuTieTie.Pages
{
    public sealed partial class MyThemeDetailPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

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
                LoadData(((MyTheme)e.Parameter).id);
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
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            }

            SaveData();

            base.OnNavigatingFrom(e);
        }

        #endregion

        #region Data

        DataLoader<MaterialGroup> materialDataLoader = null;
        MaterialGroup myMaterials = null;

        private async void LoadData(string themeID)
        {
            if (materialDataLoader == null)
            {
                materialDataLoader = new DataLoader<MaterialGroup>();
            }

            myMaterials = await materialDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE);
            if (myMaterials == null)
            {
                myMaterials = new MaterialGroup();
            }

            var materials = from m in myMaterials.Materials
                            where m.themePackID.Equals(themeID)
                            select m;

            this.myMaterialListBox.ItemsSource = materials;
        }
        private async void SaveData()
        {
            string json = JsonSerializer.Serialize(myMaterials);
            await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE, json);
        }

        #endregion

        private void ImageSwitch_CheckStateChanged(Shared.Control.ImageSwitch sender, bool suggestedState)
        {
            Material material = sender.GetDataContext<Material>();
            if (material != null)
            {
                material.visible = !material.visible;
            }
        }

    }
}
