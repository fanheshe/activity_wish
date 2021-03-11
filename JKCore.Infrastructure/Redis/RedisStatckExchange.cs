using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace JKCore.Infrastructure.Redis
{
    public class RedisStatckExchange : IRedis
    {//Redis地址
        private string _redisHost = string.Empty;
        //Redis密码
        private string _redisPass = string.Empty;
        private readonly DBConfig _settings;
        private readonly ILogger _logger;
        //Redis 客户端
        private ConnectionMultiplexer client = null;

        public Action<ConnectionFailedEventArgs> ConnectionFailedAction = null;
        public Action<ConnectionFailedEventArgs> ConnectionRestoredAction = null;
        public Action<RedisErrorEventArgs> ErrorMessageAction = null;

        // <summary>
        /// 构造方法，链接Redis
        /// </summary>
        /// <param name="redisHost">地址</param>
        /// <param name="redisPass">密码</param>
        /// <param name="dbs">所使用的库</param>
        public RedisStatckExchange(IOptions<DBConfig> settings, ILoggerFactory loggerFactory)
        {
            _settings = settings.Value;
            _redisHost = _settings.RedisHost;
            _redisPass = _settings.RedisPassword;
            _logger = loggerFactory.CreateLogger("Error-Redis");
            ConfigurationOptions config = GetConnectConfig();
            this.client = CreateConnect(config);
        }

        #region Redis 配置
        private void plexer_ErrorMessage(object sender, RedisErrorEventArgs e)
        {
            if (ErrorMessageAction != null)
            {
                ErrorMessageAction(e);
            }
        }

        private void plexer_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            if (ConnectionRestoredAction != null)
            {
                ConnectionRestoredAction(e);
            }
        }

        private void plexer_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            if (ConnectionFailedAction != null)
            {
                ConnectionFailedAction(e);
            }
        }

        private ConfigurationOptions GetConnectConfig()
        {
            string redisIp = this._redisHost.Split(new char[] { ':' })[0];
            int port = int.Parse(this._redisHost.Split(new char[] { ':' })[1]);

            return new ConfigurationOptions()
            {
                ConnectTimeout = 1000,
                EndPoints = { { redisIp, port } },
                AllowAdmin = true,
                SyncTimeout = 2000,
                KeepAlive = 100,
                AbortOnConnectFail = false,
                Password = this._redisPass,
                ConnectRetry = 3,//重试连接的次数

            };
        }

        private ConnectionMultiplexer CreateConnect(ConfigurationOptions config)
        {
            ConnectionMultiplexer plexer = ConnectionMultiplexer.Connect(config);
            plexer.ConnectionFailed += plexer_ConnectionFailed;
            plexer.ConnectionRestored += plexer_ConnectionRestored;
            plexer.ErrorMessage += plexer_ErrorMessage;
            return plexer;
        }

        private IDatabase GetDatabase(int dbIndex)
        {
            return client.GetDatabase(dbIndex);
        }


        #endregion

        #region List
        /// <summary>
        /// List队列插入值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        public void ListPush(string key, RedisValue value, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                db.ListLeftPushAsync(key, value, flags: CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.ListPush");

            }
        }

        /// <summary>
        /// List队列取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        public async Task<object> ListPopAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                var value = await db.ListRightPopAsync(key);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.ListPopAsync");
                return null;
            }
        }

        /// <summary>
        /// List队列插入值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        public void ListPushs(string key, List<int> value, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                List<RedisValue> redisValues = new List<RedisValue>();
                foreach (var item in value)
                {
                    redisValues.Add(item);
                }
                var valueArray = redisValues.ToArray();

                db.ListLeftPushAsync(key, valueArray, flags: CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.ListPushs");
            }
        }
        public void ListPushs<T>(string key, IList<T> value, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                List<RedisValue> redisValues = new List<RedisValue>();
                foreach (var item in value)
                {
                    redisValues.Add(Serialize(item));
                }
                var valueArray = redisValues.ToArray();

                db.ListLeftPushAsync(key, valueArray, flags: CommandFlags.FireAndForget);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.ListPushs");
            }
        }
        #endregion
        #region Set集合
        /// <summary>
        /// 插入集合记录（无重复）,重复数据结果返回false不插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        public async Task<bool> SetAddAsync(string key, RedisValue value, int dbIndex,DateTime expire)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                bool result = await db.SetAddAsync(key, value);
                if(result && expire != null)
                    await db.KeyExpireAsync(key, expire);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.SetAddAsync");
                return false;
            }
        }
        public async Task<object> SetAddAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                var value = await db.SetPopAsync(key);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.SetAddAsync");
                return null;
            }
        }
        /// <summary>
        /// 集合插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        public async Task SAddAsync(string key, RedisValue value, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                await db.SetAddAsync(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.SetAddAsync");

            }
        }
        #endregion
        #region String
        public async Task StringSetAsync(string key, RedisValue value, TimeSpan? expiry, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                await db.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.StringSetAsync");

            }
        }
        public async Task<string> StringGetAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                string value = await db.StringGetAsync(key);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.StringGetAsync");
                return null;
            }
        }
        /// <summary>
        /// 原子性加一
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<long> StringIncrement(string key, int dbIndex, long value = 1)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                long count = await db.StringIncrementAsync(key,value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.StringGetAsync");
                return 0;
            }
        }
        /// <summary>
        /// 原子性减一
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<long> StringDecrement(string key, int dbIndex, long value = 1)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                long count = await db.StringDecrementAsync(key, value);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.StringGetAsync");
                return 0;
            }
        }
        #endregion
        public async Task<bool> KeyExistsAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                return await db.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.KeyExistsAsync");
                return false;
            }
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return (new BinaryFormatter().Deserialize(ms));
            }
        }
        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, value);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// 删除key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        public async Task KeyDeleteAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);
                
                await db.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.KeyDeleteAsync");

            }
        }

        #region HyperLogLog
        public async Task<bool> HyperLogLogAddAsync(string key, int dbIndex, RedisValue value)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                bool result = await db.HyperLogLogAddAsync(key, value);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.HyperLogLogAddAsync");
                return false;
            }
        }
        public async Task<long> HyperLogLogLengthAsync(string key, int dbIndex)
        {
            try
            {
                IDatabase db = GetDatabase(dbIndex);

                long result = await db.HyperLogLogLengthAsync(key);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis.HyperLogLogAddAsync");
                return 0;
            }
        }
        #endregion
    }
}
