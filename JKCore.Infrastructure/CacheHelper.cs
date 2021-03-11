using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using static JKCore.Infrastructure.VerifyCodeHelper;

namespace JKCore.Infrastructure
{
    public static class CacheHelper
    {
        public static IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        public static void SetMemoryCache(string key, string value, TimeSpan expiresIn)
        {
            cache.Set(key, value);
            var exi = cache.TryGetValue(key, out var va);
        }

        public static object GetMemoryCache(string key)
        {
            object value = null;
            cache.TryGetValue(key, out value);
            return value;
        }

        public static void RemoveMemoryCache(string key)
        {
            cache.Remove(key);
        }

        //static CacheHelper()
        //{
        //    MemoryCache = new InMemoryCache();
        //}

        //public static IMemoryCache cache
        //{
        //    get { return new MemoryCache(Options.Create(new MemoryCacheOptions())); }
        //}

        public static void SetCache<T>(IEnumerable<T> result, string Key)
        {
            cache.Set(Key, result, DateTime.Now.AddMinutes(10));
        }

        public static void SetCache(string key, object obj)
        {
            cache.Set(key, obj, DateTime.Now.AddMinutes(10));
        }

        public static void SetCache(string key, object obj, DateTime expTime)
        {
            cache.Set(key, obj, expTime);
        }

        public static void SetCache(string key, object obj, TimeSpan slidingExpiration)
        {
            cache.Set(key, obj, slidingExpiration);
        }

        public static IEnumerable<T> GetCache<T>(string Key)
        {
            object obj = cache.Get(Key);
            IEnumerable<T> result = obj == null ? null : (IEnumerable<T>)obj;
            return result;
        }
        public enum EnumValidateCode
        {
            Login = 0,
            Register = 1
        }
        public static string ValidationCodeKey(this EnumValidateCode type, string sid)
        {
            return string.Format("{0}_{1}_code", type, sid);
            //return $"{type}_{sid}_code";
        }

        public static void CacheValidationCode(this EnumValidateCode type, string code,string sessionId)
        {
            string key = ValidationCodeKey(type,sessionId);
            SetMemoryCache(key, code, TimeSpan.FromMinutes(5));
        }

        public static string GetValidationCode(this EnumValidateCode type, string sessionId)
        {
            string key = ValidationCodeKey(type,sessionId);
            var obj = GetMemoryCache(key);
            if (obj != null)
            {
                return obj.ToString();
            }
            return string.Empty;
        }
    }
}
