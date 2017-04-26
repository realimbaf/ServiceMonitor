using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace Wiki.ServiceMonitor.Utils
{
    public static class MonitorSettings
    {
        private const string _enableBroadcast = "EnableBroadcast";
        private const string _discoveryUrl = "DiscoveryUrl";
        private const string _isDebug = "IsDebug";

        public static bool EnableBroadcast
        {
            get
            {
                return ContainKey(_enableBroadcast) && GetSetting(_enableBroadcast) == "true";
            }
        }

        public static bool IsDebug
        {
            get
            {
                return (ContainKey(_isDebug) && GetSetting(_isDebug) == "true") || (!ContainKey(_isDebug)) ;
            }
        }

        public static bool isExistDiscoveryUrl
        {
            get
            {
                return ContainKey(_enableBroadcast) &&
                       GetSetting(_enableBroadcast) == "false" &&
                       ContainKey(_discoveryUrl) &&
                       GetSetting(_discoveryUrl) != string.Empty;
            }
        }

        public static string DiscoveryUrl { get { return _discoveryUrl;} }

        public static NameValueCollection AppSettings
        {
            get { return ConfigurationManager.AppSettings; }
        }


        public static string GetSetting(string key)
        {
            try
            {
                return AppSettings[key];
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error getSetting AppSetting" + ex);
                return null;
            }
            
        }

        public static bool ContainKey(string key)
        {
            var findKey = AppSettings.AllKeys.FirstOrDefault(x => x == key);
            return findKey != null;
        }
    }
}
