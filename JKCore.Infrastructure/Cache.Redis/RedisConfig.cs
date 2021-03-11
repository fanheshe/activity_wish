using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure.Cache.Redis
{
    public class RedisConfig
    {
        /// <summary>
        /// 获取redisSerer配置节点
        /// </summary>
        /// <param name="key">连接串配置[枚举表示]</param>
        /// <returns></returns>
        public static string GetRedisServer(string key)
        {
            if (key == DictRedisServer.RedisHost.ToString())
            {
                string password = AppSettingConfig.GetAppConfig("RedisPassword");
                if (!string.IsNullOrWhiteSpace(password))
                {
                    return $"{AppSettingConfig.GetAppConfig(key)},password={password}";
                }
            }
            return AppSettingConfig.GetAppConfig(key);
        }
    }
    /// <summary>
    /// redisServer枚举字典
    /// </summary>
    public enum DictRedisServer
    {
        RedisHost = 1
    }
}
