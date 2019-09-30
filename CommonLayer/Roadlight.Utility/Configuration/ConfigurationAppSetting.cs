using System.Configuration;

namespace Roadlight.Utility
{
    public static class ConfigurationAppSetting
    {

        /// <summary>
        /// 应用程序ID
        /// </summary>
        private static string _appId;
        public static string AppId
        {
            get
            {
                if (string.IsNullOrEmpty(_appId))
                {
                    _appId = ConfigurationManager.AppSettings["AppId"];
                }
                return _appId;
            }
        }
    }
}
