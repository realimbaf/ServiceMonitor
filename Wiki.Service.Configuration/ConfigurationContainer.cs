using System;

namespace Wiki.Service.Configuration
{
    public class ConfigurationContainer
    {
        private static string _idKey="default";
        private static ServiceConfiguration _config;

        private static string _baseDir;

        internal static void SetId(string id)
        {
            _idKey = id;
            _config = null;
        }

        public static ServiceConfiguration Configuration
        {
            get
            {
                if (_config == null)
                {
                    _config = new ServiceConfiguration(_idKey,_baseDir);
                    _config.Load();
                }
                return _config;
            }
        }


        public static void SetBaseDir(string dir)
        {
            _baseDir = dir;
        }
    }
}