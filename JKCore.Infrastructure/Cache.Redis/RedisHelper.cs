using JKCore.Infrastructure.Cache.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    public static partial class RedisHelper
    {
        static object _locker = new object();
        static IDictionary<string, ConnectionMultiplexer> _creaters
           = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        /// <summary>
        /// 获取Redis的IDatabase
        /// </summary>
        /// <param name="db">存储库位置</param>
        /// <param name="redisServer">连接串配置[枚举表示]</param>
        /// <returns></returns>
        public static IDatabase GetDB(int db = -1, DictRedisServer redisServer = DictRedisServer.RedisHost)
        {
            var redisServerKey = DictRedisServer.RedisHost.ToString();
            if (!_creaters.TryGetValue(redisServerKey, out ConnectionMultiplexer con) || !con.IsConnected)
            {
                lock (_locker)
                {
                    if (!_creaters.TryGetValue(redisServerKey, out con) || !con.IsConnected)
                    {
                        try
                        {
                            con = ConnectionMultiplexer.Connect(RedisConfig.GetRedisServer(redisServerKey));
                            con.ConnectionFailed += MuxerConnectionFailed;
                            con.ConnectionRestored += MuxerConnectionRestored;
                            con.ErrorMessage += MuxerErrorMessage;
                            con.ConfigurationChanged += MuxerConfigurationChanged;
                            con.HashSlotMoved += MuxerHashSlotMoved;
                            con.InternalError += MuxerInternalError;
                            _creaters[redisServerKey] = con;
                        }
                        catch (Exception ex)
                        {
                            //LogHelper.Error($"init-connectionMultiplexer:{DictRedisServer.YX_Manager_DataLoader_RedisHost}\r\n{ex}");
                        }
                    }
                }
            }
            return con?.GetDatabase(db);
        }

        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            //LogHelper.Error("Configuration changed: " + e.EndPoint);
        }
        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            //LogHelper.Error("ErrorMessage: " + e.Message);
        }
        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            //LogHelper.Error("ConnectionRestored: " + e.EndPoint);
        }
        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            //LogHelper.Error("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }
        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            //LogHelper.Error("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }
        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            //LogHelper.Error("InternalError:Message" + e.Exception.Message);
        }


        /// <summary>
        /// 使用的是Lazy，在真正需要连接时创建连接。
        /// 延迟加载技术
        /// 微软azure中的配置 连接模板
        /// </summary>
        //private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        //{
        //    //var options = ConfigurationOptions.Parse(constr);
        //    ////options.ClientName = GetAppName(); // only known at runtime
        //    //options.AllowAdmin = true;
        //    //return ConnectionMultiplexer.Connect(options);
        //    ConnectionMultiplexer muxer = ConnectionMultiplexer.Connect(Coonstr);
        //    muxer.ConnectionFailed += MuxerConnectionFailed;
        //    muxer.ConnectionRestored += MuxerConnectionRestored;
        //    muxer.ErrorMessage += MuxerErrorMessage;
        //    muxer.ConfigurationChanged += MuxerConfigurationChanged;
        //    muxer.HashSlotMoved += MuxerHashSlotMoved;
        //    muxer.InternalError += MuxerInternalError;
        //    return muxer;
        //});
    }
}
