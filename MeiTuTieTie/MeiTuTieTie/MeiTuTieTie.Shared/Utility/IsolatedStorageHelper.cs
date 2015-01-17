using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using System.Linq;

namespace Shared.Utility
{
    public class IsolatedStorageHelper
    {
        //public const string USER_DATA_FOLDER_NAME = "udata";

        //public static string EnsureUserDataRoot(string folderName)
        //{
        //    if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
        //    {
        //        folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
        //    }
        //    return folderName;
        //}

        public static async Task<StorageFile> GetFileAsync(string folderName, string fileName, CreationCollisionOption option)
        {
            //if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
            //}

            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new folder
            var dataFolder = await local.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

            // Create a new file
            var file = await dataFolder.CreateFileAsync(fileName, option);

            return file;
        }

        public static async Task<StorageFolder> GetFolderAsync(string folderName)
        {
            //if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
            //}

            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            //string[] folderNameList = folderName.Split('\\');
            //StorageFolder folder = local;
            //for (int i = 0; i < folderNameList.Length; i++)
            //{
            //    folder = await folder.CreateFolderAsync(folderNameList[i], CreationCollisionOption.OpenIfExists);
            //}

            // Create a new folder
            var dataFolder = await local.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

            return dataFolder;
        }

        public static async Task WriteToFileAsync(string folderName, string fileName, string content)
        {
            //if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
            //}

            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new folder
            var dataFolder = await local.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);

            // Create a new file
            var file = await dataFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            // Write the data
            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter writer = new DataWriter(stream))
                {
                    writer.WriteString(content);
                    await writer.StoreAsync();
                }
            }
        }

        public static async Task<string> ReadFileAsync(string filePath)
        {
            //if (!filePath.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    filePath = USER_DATA_FOLDER_NAME + "\\" + filePath;
            //}

            try
            {
                StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                var storageFile = await local.CreateFileAsync(filePath, CreationCollisionOption.OpenIfExists);
                using (var stream = await storageFile.OpenStreamForReadAsync())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }
            return null;
        }

        public static async Task<string> ReadFileAsync(string folderName, string fileName)
        {
            try
            {
                var storageFile = await GetFileAsync(folderName, fileName, CreationCollisionOption.OpenIfExists);

                // Read the data.
                using (var stream = await storageFile.OpenStreamForReadAsync())
                {
                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }

            }
            catch (FileNotFoundException)
            {
                return null;
            }
            return null;
        }

        public static async Task<ulong> GetUserDataSizeAsync()
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            //var folder = await local.CreateFolderAsync(USER_DATA_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
            return await GetFolderSizeAsync(local);
        }

        public static async Task<ulong> GetUserDataSizeAsync(params string[] excludedFolders)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            ulong totalSize = 0;

            if (local == null)
            {
                return totalSize;
            }

            //var folder = await local.CreateFolderAsync(USER_DATA_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
            var subFolders = await local.GetFoldersAsync();
            foreach (var subFolder in subFolders)
            {
                if (!excludedFolders.Contains(subFolder.Name))
                {
                    totalSize += await GetFolderSizeAsync(subFolder);
                }
            }

            return totalSize;
        }

        public static async Task<ulong> GetFolderSizeAsync(StorageFolder folder)
        {
            ulong size = 0;

            try
            {
                foreach (StorageFolder thisFolder in await folder.GetFoldersAsync())
                {
                    size += await GetFolderSizeAsync(thisFolder);
                }

                foreach (StorageFile thisFile in await folder.GetFilesAsync())
                {
                    BasicProperties props = await thisFile.GetBasicPropertiesAsync();
                    size += props.Size;
                }
            }
            catch (Exception ex)
            {
            }

            return size;
        }

        public static async Task ClearUserDataAsync()
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            //var folder = await local.GetFolderAsync(USER_DATA_FOLDER_NAME);
            if (local != null)
            {
                await local.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public static async Task ClearUserDataAsync(params string[] excludedFolders)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (local == null)
            {
                return;
            }

            //var folder = await local.GetFolderAsync(USER_DATA_FOLDER_NAME);
            if (local != null)
            {
                var subFolders = await local.GetFoldersAsync();
                foreach (var subFolder in subFolders)
                {
                    if (!excludedFolders.Contains(subFolder.Name))
                    {
                        await subFolder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }
                }
            }
        }

        public static async Task DeleteFolderAsync(string folderName)
        {
            //if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
            //}

            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (local != null)
            {
                var dataFolder = await local.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
                await dataFolder.DeleteAsync();
            }
        }

        public static async Task DeleteFileAsync(string folderName, string fileName)
        {
            //if (!folderName.StartsWith(USER_DATA_FOLDER_NAME + "\\"))
            //{
            //    folderName = USER_DATA_FOLDER_NAME + "\\" + folderName;
            //}

            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            if (local != null)
            {
                var dataFolder = await local.GetFolderAsync(folderName);
                var file = await dataFolder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
        }
    }

}
