using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    public static class AppSettingConfig
    {
        private static IConfigurationSection _appSection = null;
        public static string AppSetting(string key)
        {
            string str = string.Empty;
            if (_appSection.GetSection(key) != null)
            {
                str = _appSection.GetSection(key).Value;
            }
            return str;
        }
        public static void SetAppSetting(IConfigurationSection section)
        {
            _appSection = section;
        }
        public static string GetAppConfig(string apiName)
        {
            return AppSetting(apiName);
        }

    }
}
