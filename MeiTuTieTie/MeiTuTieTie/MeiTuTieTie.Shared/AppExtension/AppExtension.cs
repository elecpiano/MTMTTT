using Shared.Global;
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
            ////edge & shadow
            //if (_Settings.Values.ContainsKey(KEY_EDGE))
            //{
            //    _EdgeEnabled = GetSetting(KEY_EDGE, false);
            //}

            //if (_Settings.Values.ContainsKey(KEY_SHADOW))
            //{
            //    _ShadowEnabled = GetSetting(KEY_SHADOW, false);
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

        #region Edge & Shadow

        //private bool _EdgeEnabled = false;
        //public bool EdgeEnabled
        //{
        //    get
        //    {
        //        return _EdgeEnabled;
        //    }
        //    set
        //    {
        //        if (_EdgeEnabled != value)
        //        {
        //            _EdgeEnabled = value;
        //            UpdateSetting(Constants.KEY_EDGE, value);
        //        }
        //    }
        //}

       
        //private bool _ShadowEnabled = false;
        //public bool ShadowEnabled
        //{
        //    get
        //    {
        //        return _ShadowEnabled;
        //    }
        //    set
        //    {
        //        if (_ShadowEnabled != value)
        //        {
        //            _ShadowEnabled = value;
        //            UpdateSetting(Constants.KEY_SHADOW, value);
        //        }
        //    }
        //}

        #endregion
    }
}
