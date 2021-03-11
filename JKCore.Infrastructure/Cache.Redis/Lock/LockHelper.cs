using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure.Cache.Redis.Lock
{
    public class LockHelper
    {
        /// <summary>
        /// 尝试获取锁
        /// </summary>
        /// <param name="resourceName">锁定资源</param>
        /// <param name="seconds">锁定秒数</param>
        /// <param name="lockObject">锁定对象</param>
        /// <param name="redLock">锁信息</param>
        /// <returns></returns>
        public static bool TryGetLock(string resourceName, int seconds, out Lock lockObject, out Redlock redLock, out string err)
        {
            lockObject = null;
            redLock = null;
            err = null;
            try
            {
                redLock = new Redlock(Manager);
                var locked = redLock.Lock(resourceName, new TimeSpan(0, 0, seconds), out lockObject);
                return locked;
            }
            catch (Exception ex)
            {
                err = ex.ToString();
                return true;
            }
        }

        private static ConnectionMultiplexer _redis;
        private static object _locker = new object();
        public static ConnectionMultiplexer Manager
        {
            get
            {
                if (_redis == null)
                {
                    lock (_locker)
                    {
                        if (_redis != null) return _redis;

                        _redis = GetManager();
                        return _redis;
                    }
                }

                return _redis;
            }
        }
        private static ConnectionMultiplexer GetManager()
        {
            var server = RedisConfig.GetRedisServer(DictRedisServer.RedisHost.ToString());
            return ConnectionMultiplexer.Connect(server);
        }
    }
}
