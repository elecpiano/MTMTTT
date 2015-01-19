using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using MeiTuTieTie;
using System.Threading.Tasks;

namespace Shared.Utility
{
    public class DataLoader<T> where T : class
    {
        public bool Busy = false;
        public bool Loaded = false;

        private Action<T> onCallback;
        string moduleName = string.Empty;
        string fileName = string.Empty;
        private bool toCacheData = false;

        public void LoadWithoutCaching(string dataURL, Action<T> callback)
        {
            this.Load(dataURL, false, string.Empty, string.Empty, callback);
        }

        public async void Load(string dataURL, bool cacheData, string module, string file, Action<T> callback)
        {
            if (cacheData && (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(file)))
            {
                return;
            }

            //for callback
            onCallback = callback;
            toCacheData = cacheData;
            moduleName = module;
            fileName = file;

            if (!NetworkHelper.Current.IsInternetConnectionAvaiable)
            {
                //load cache
                if (cacheData)
                {
                    try
                    {
                        var cachedJson = await IsolatedStorageHelper.ReadFileAsync(moduleName, fileName);
                        T obj = JsonSerializer.Deserialize<T>(cachedJson);
                        if (obj != null)
                        {
                            App.CurrentInstance.RunAsync(() =>
                            {
                                onCallback(obj);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return;
            }

            //download new
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(new Uri(dataURL));
                request.Method = "GET";
                request.BeginGetResponse(GetData_Callback, request);

                Loaded = false;
                Busy = true;
            }
            catch (WebException e)
            {
            }
            catch (Exception e)
            {
            }
        }

        private async void GetData_Callback(IAsyncResult result)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                WebResponse response = request.EndGetResponse(result);

                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string json = reader.ReadToEnd();
                    T obj = JsonSerializer.Deserialize<T>(json);
                    if (obj != null)
                    {
                        App.CurrentInstance.RunAsync(() =>
                        {
                            onCallback(obj);
                        });
                    }

                    if (toCacheData)
                    {
                        await IsolatedStorageHelper.WriteToFileAsync(moduleName, fileName, json);
                    }
                }
                Loaded = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                Busy = false;
            }
        }

        public async Task<T> LoadLocalData(string module, string file)
        {
            if ((string.IsNullOrEmpty(module) || string.IsNullOrEmpty(file)))
            {
                return null;
            }

            T result = null;

            //load cache
            try
            {
                //ensure file existence
                //await IsolatedStorageHelper.GetFileAsync(moduleName, fileName, Windows.Storage.CreationCollisionOption.OpenIfExists);
                var json = await IsolatedStorageHelper.ReadFileAsync(module, file);
                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public async Task<T> LoadLocalData(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            T result = null;

            //load cache
            try
            {
                var json = await IsolatedStorageHelper.ReadFileAsync(filePath);
                if (!string.IsNullOrEmpty(json))
                {
                    result = JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }


    }

}
