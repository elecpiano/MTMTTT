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
    public sealed partial class ThemeDetailPage : Page
    {
        #region Property

        private readonly NavigationHelper navigationHelper;

        #endregion

        #region Lifecycle

        public ThemeDetailPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadData(e.Parameter);
        }

        #endregion

        #region load data

        private void LoadData(object themeData)
        {
            this.rootGrid.DataContext = themeData;
        }

        #endregion

        #region File Download

        FileDownloader fileDownloader = null;
        bool downloadCanceled = false;
        bool somethingWrong = false;

        private async void Download()
        {
            progressPanel.Visibility = Visibility.Visible;
            downloadPanel.Visibility = Visibility.Collapsed;

            bool downloadSucceed = await ActualDownload();

            progressPanel.Visibility = Visibility.Collapsed;
            downloadPanel.Visibility = Visibility.Visible;
        }

        private async Task<bool> ActualDownload()
        {
            somethingWrong = false;
            this.progressBar.Value = 0;

            if (fileDownloader == null)
            {
                fileDownloader = new FileDownloader();
            }

            ThemePack theme = this.rootGrid.GetDataContext<ThemePack>();

            if (theme == null)
            {
                return false;
            }

            //download
            string fileName = string.Format(Constants.THEME_PACK_ZIP_FILE_FORMAT, theme.id);
            string zipFilePath = Path.Combine(Constants.THEME_MODULE, fileName);
            var storageFile = await fileDownloader.Download(theme.zipUrl, zipFilePath, progressBar);
            if (storageFile == null)
            {
                somethingWrong = true;
            }

            if (downloadCanceled || somethingWrong)
            {
                return false;
            }

            //unzip
            string unZipfolderName = string.Format("{0}\\{1}", Constants.THEME_MODULE, theme.id);
            await UnZip(storageFile, unZipfolderName);
            if (somethingWrong)
            {
                return false;
            }

            //delete zip
            somethingWrong = await DeleteZipFile(zipFilePath);
            if (somethingWrong)
            {
                return false;
            }

            //add materials data
            int materialCount = await AddMaterialData_SplitFile(theme, unZipfolderName);
            if (somethingWrong)
            {
                return false;
            }

            // claim downloaded
            await AddMyThemeData(theme, materialCount);

            if (somethingWrong)
            {
                return false;
            }
            else
            {
                theme.Downloaded = true;
            }

            return true;
        }

        #endregion

        #region Delete File

        private async Task<bool> DeleteZipFile(string zipFilePath)
        {
            bool somethingWrong = false;
            try
            {
                await IsolatedStorageHelper.DeleteFileAsync(zipFilePath);
            }
            catch (Exception ex)
            {
                somethingWrong = true;
                throw;
            }
            return somethingWrong;
        }

        #endregion

        #region Zip

        private async Task UnZip(StorageFile zipFile, string folderName)
        {
            try
            {
                var folder = await IsolatedStorageHelper.GetFolderAsync(folderName);
                await ZipHelper.UnZipFileAsync(zipFile, folder);
            }
            catch (Exception ex)
            {
                somethingWrong = true;
            }
        }

        #endregion

        #region MyTheme Data

        DataLoader<MyThemeData> myThemeDataLoader = null;

        private async Task AddMyThemeData(ThemePack themePack, int materialCount)
        {
            try
            {
                if (myThemeDataLoader == null)
                {
                    myThemeDataLoader = new DataLoader<MyThemeData>();
                }

                //load data file
                var data = await myThemeDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE);
                if (data == null)
                {
                    data = new MyThemeData();
                }

                if (data.myThemes.Any(x => x.id == themePack.id))
                {
                    return;
                }

                //add new theme
                MyTheme newTheme = new MyTheme();
                newTheme.id = themePack.id;
                newTheme.name = themePack.name;
                newTheme.thumbnail = themePack.thumbnailUrl;
                newTheme.materialCount = materialCount;
                data.myThemes.Insert(0, newTheme);

                //save data
                string json = JsonSerializer.Serialize(data);
                await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_THEME_DATA_FILE, json);
            }
            catch (Exception ex)
            {
                somethingWrong = true;
            }
        }

        #endregion

        #region Material Data

        DataLoader<MaterialGroup> materialDataLoader = null;
        private async Task AddMaterialData_SingleFile(ThemePack theme, string folder)
        {
            //read theme pack materials file (xml)
            string path = Path.Combine(folder, "materials.xml");

            try
            {
                //MaterialGroup newMaterials = await XmlHelper.Deserialize<MaterialGroup>(path); /* the xml file is incomplete sometimes */
                List<Material> newMaterials = await LoadMaterialXML(path);

                //load my material file
                if (materialDataLoader == null)
                {
                    materialDataLoader = new DataLoader<MaterialGroup>();
                }
                var myMaterials = await materialDataLoader.LoadLocalData(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE);
                if (myMaterials == null)
                {
                    myMaterials = new MaterialGroup();
                }

                //add new materials
                foreach (var m in newMaterials)
                {
                    m.themePackID = theme.id;

                    //image full path
                    string imagePath = Path.Combine(folder, m.image);
                    m.image = imagePath;

                    //thumbnail full path
                    string thumbnailPath = Path.Combine(folder, m.thumbnail);
                    m.thumbnail = thumbnailPath;

                    myMaterials.Materials.Add(m);
                }

                //save data
                string json = JsonSerializer.Serialize(myMaterials);
                await IsolatedStorageHelper.WriteToFileAsync(Constants.THEME_MODULE, Constants.MY_MATERIAL_FILE, json);
            }
            catch (Exception ex)
            {
                somethingWrong = true;
            }
        }

        private async Task<int> AddMaterialData_SplitFile(ThemePack theme, string folder)
        {
            int materialCount = 0;

            //read theme pack materials file (xml)
            string xmlFilePath = Path.Combine(folder, "materials.xml");

            try
            {
                //MaterialGroup newMaterials = await XmlHelper.Deserialize<MaterialGroup>(path); /* the xml file is incomplete sometimes */
                List<Material> newMaterials = await LoadMaterialXML(xmlFilePath);
                materialCount = newMaterials.Count;

                //load my material file
                if (materialDataLoader == null)
                {
                    materialDataLoader = new DataLoader<MaterialGroup>();
                }

                string materialFilePath = Path.Combine(folder, Constants.MATERIAL_DATA_FILE);
                var materialGroup = await materialDataLoader.LoadLocalData(materialFilePath);
                if (materialGroup == null)
                {
                    materialGroup = new MaterialGroup();
                }

                //add new materials
                foreach (var m in newMaterials)
                {
                    m.themePackID = theme.id;

                    //image full path
                    string imagePath = Path.Combine(folder, m.image);
                    m.image = imagePath;

                    //thumbnail full path
                    string thumbnailPath = Path.Combine(folder, m.thumbnail);
                    m.thumbnail = thumbnailPath;

                    materialGroup.Materials.Add(m);
                }

                //save data
                string json = JsonSerializer.Serialize(materialGroup);
                await IsolatedStorageHelper.WriteToFileAsync(materialFilePath, json);
            }
            catch (Exception ex)
            {
                somethingWrong = true;
            }

            return materialCount;
        }


        private async Task<List<Material>> LoadMaterialXML(string file)
        {
            //MaterialGroup materialGroup = new MaterialGroup();
            List<Material> materials = new List<Material>();
            try
            {
                Material material = null;
                string id = string.Empty;
                string name = string.Empty;
                string thumbnailname = string.Empty;

                await XmlHelper.FastIterate(file,
                    (propertyName, propertyValue) =>
                    {
                        switch (propertyName)
                        {
                            case "id":
                                id = propertyValue;
                                break;
                            case "name":
                                name = propertyValue;
                                break;
                            case "thumbnailname":
                                thumbnailname = propertyValue;
                                break;
                            case "material":
                                material = new Material();
                                material.type = id;
                                material.image = name;
                                material.thumbnail = thumbnailname;
                                //materialGroup.Materials.Add(material);
                                materials.Add(material);
                                break;
                            default:
                                break;
                        }
                    });
            }
            catch (Exception)
            {
            }

            //return materialGroup;
            return materials;
        }

        #endregion

        #region Button Click

        private void download_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Test();
            Download();
        }

        private void cancelDownload_Click(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            fileDownloader.Cancel();
            downloadCanceled = true;

            progressPanel.Visibility = Visibility.Collapsed;
            downloadPanel.Visibility = Visibility.Visible;
        }

        #endregion

    }
}
