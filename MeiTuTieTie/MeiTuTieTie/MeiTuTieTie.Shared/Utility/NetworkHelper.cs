using Windows.Networking.Connectivity;

namespace Shared.Utility
{
    public class NetworkHelper
    {
        #region Singleton

        private static NetworkHelper instance = new NetworkHelper();

        public static NetworkHelper Current
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

            if(internetConnectionProfile == null)
            {
                _isInternetConnectionAvailable = false;
                return;
            }
            else
            {
                _isInternetConnectionAvailable = true;

                if(internetConnectionProfile.IsWlanConnectionProfile)
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
