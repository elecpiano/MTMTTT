using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace MeiTuTieTie
{
    public sealed partial class App
    {
        #region Settings

        ApplicationDataContainer _Settings = Windows.Storage.ApplicationData.Current.LocalSettings;

        private void LoadSettings()
        {
            ////banner
            //if (_Settings.Contains(KEY_DISMISSED_BANNER))
            //{
            //    _DismissedBannerId = (string)_Settings[KEY_DISMISSED_BANNER];
            //}
            //else
            //{
            //    _Settings.Add(KEY_DISMISSED_BANNER, string.Empty);
            //}
        }

        public T GetSetting<T>(string key, T defaultValue) //where T : class
        {
            if (_Settings.Values.ContainsKey(key))
            {
                return (T)_Settings.Values[key];
            }
            else if (defaultValue != null)
            {
                UpdateSetting(key, defaultValue);
            }
            return defaultValue;
            //return default(T);
        }

        public void UpdateSetting(string key, object value)
        {
            if (_Settings.Values.ContainsKey(key))
            {
                _Settings.Values[key] = value;
            }
            else
            {
                _Settings.Values.Add(key, value);
            }
        }

        #endregion

        #region Banner

        //private const string KEY_DISMISSED_BANNER = "banner";

        //private string _DismissedBannerId = string.Empty;
        //public string DismissedBannerId
        //{
        //    get
        //    {
        //        return _DismissedBannerId;
        //    }
        //    set
        //    {
        //        if (_DismissedBannerId != value)
        //        {
        //            _DismissedBannerId = value;
        //            UpdateSetting(KEY_DISMISSED_BANNER, value);
        //        }
        //    }
        //}

        #endregion
    }
}
