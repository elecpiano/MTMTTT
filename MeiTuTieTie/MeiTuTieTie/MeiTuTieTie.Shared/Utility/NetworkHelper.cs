using Windows.Networking.Connectivity;

namespace Shared.Utility
{
    public class NetworkHelper
    {
        public static bool CheckInternet()
        {
                ConnectionProfile profile = NetworkInformation.GetInternetConnectionProfile();
                return profile != null;
        }
    }

    public class NetworkHelper2
    {
        #region Singleton

        private static NetworkHelper2 instance = new NetworkHelper2();

        public static NetworkHelper2 Current
        {
            get { return instance; }
        }

        #endregion

        private bool _isInternetConnectionAvailable;

        public bool IsInternetConnectionAvaiable
        {
            get
            {
                return _isInternetConnectionAvailable;
            }
        }

        private bool _IsWifiConnectionAvailable;
        public bool IsWifiConnectionAvailable
        {
            get
            {

                if (_isInternetConnectionAvailable && _IsWifiConnectionAvailable)
                {
                    return true;
                }
                return false;
            }
        }

        private void UpdateStatus()
        {
            _IsWifiConnectionAvailable = false;
            _isInternetConnectionAvailable = false;

            ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (internetConnectionProfile == null)
            {
                _isInternetConnectionAvailable = false;
                return;
            }
            else
            {
                _isInternetConnectionAvailable = true;

                if (internetConnectionProfile.IsWlanConnectionProfile)
                {
                    _IsWifiConnectionAvailable = true;
                }
            }
        }

        public void StartListening()
        {
            UpdateStatus();
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }

        public void StopListening()
        {
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
        }

        private void NetworkInformation_NetworkStatusChanged(object sender)
        {
            UpdateStatus();
        }
    }
}
