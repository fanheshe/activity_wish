using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.Redis
{
    public interface IRedis
    {
        /// <summary>
        /// List队列插入值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        void ListPush(string key, RedisValue value, int dbIndex);

        /// <summary>
        /// List队列批量插入值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        void ListPushs(string key, List<int> value, int dbIndex);
        void ListPushs<T>(string key, IList<T> value, int dbIndex);
        /// <summary>
        /// List队列取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        Task<object> ListPopAsync(string key, int dbIndex);

        /// <summary>
        /// 插入集合记录（无重复）,重复数据结果返回false不插入
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dbIndex"></param>
        /// <param name="expire"></param>
        /// <returns></returns>
        Task<bool> SetAddAsync(string key, RedisValue value, int dbIndex, DateTime expire);
        Task<object> SetAddAsync(string key, int dbIndex);

        Task StringSetAsync(string key, RedisValue value, TimeSpan? expiry, int dbIndex);
        Task<string> StringGetAsync(string key, int dbIndex);
        Task<bool> KeyExistsAsync(string key, int dbIndex);
        Task<long> StringIncrement(string key, int dbIndex, long value=1);

        Task<long> StringDecrement(string key, int dbIndex, long value=1);
        /// <summary>
        /// 删除key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dbIndex"></param>
        /// <returns></returns>
        Task KeyDeleteAsync(string key, int dbIndex);


        Task<bool> HyperLogLogAddAsync(string key, int dbIndex, RedisValue value);
        Task<long> HyperLogLogLengthAsync(string key, int dbIndex);
    }
}
