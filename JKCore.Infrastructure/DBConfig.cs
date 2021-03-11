using System;

namespace JKCore.Infrastructure
{
    public class DBConfig
    {
        public string SassConfiguration { get; set; }
        public string YxvipConfiguration { get; set; }

        public string RedisHost { get; set; }

        public string RedisPassword { get; set; }
        public string SmsAppId { get; set; }
        public string SmsAppKey { get; set; }
        public string JiakeConfiguration { get; set; }
        public string DemoConfiguration { get; set; }

        public string CodeRedisCacheDB { get; set; }
        public int LoginOutDB { get; set; }
        /// <summary>
        /// 用户报表reids db index
        /// </summary>
        public int UserReport { get; set; }
    }
}
