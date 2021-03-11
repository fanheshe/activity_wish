using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JKCore.Infrastructure
{
    public class NetEaseCommonHelper
    {
        private static IConfigurationSection _appSection = null;
        public static void SetAppSetting(IConfigurationSection section)
        {
            _appSection = section;
        }

        /// <summary>
        /// 网易云信AppKey
        /// </summary>
        /// <returns></returns>
        public static string GetAppKey()
        {
            //return "8d4f15775c9cb2a2a44fca0025e4c0a0";
            return _appSection.GetSection("NetEaseAppKey").Value;
        }

        /// <summary>
        /// 网易云信AppSecret
        /// </summary>
        /// <returns></returns>
        public static string GetAppSecret()
        {
            //return "2ac66aac61aa";
            return _appSection.GetSection("NetEaseAppSecret").Value;
        }

        /// <summary>
        /// 网易云信注册AccId的前缀，账号只能封禁，不能删除
        /// </summary>
        /// <returns></returns>
        public static string GetAccIdPrefix()
        {
            return _appSection.GetSection("NetEaseAccIdPrefix").Value;
        }

        /// <summary>
        /// 直播间状态回调地址
        /// </summary>
        /// <returns></returns>
        public static string GetChannelCallBack()
        {
            return _appSection.GetSection("NetEaseCallBack").Value;
        }

        /// <summary>
        /// 网易云信接口鉴权
        /// </summary>
        /// <param name="appSecret"></param>
        /// <param name="nonce"></param>
        /// <param name="curTime"></param>
        /// <returns></returns>
        public static string GetCheckSum(string appSecret, string nonce, string curTime)
        {
            var checkSum = SHA1Encrypt(appSecret + nonce + curTime);
            return checkSum;
        }

        /// <summary>
        /// 时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetCurTime()
        {
            return DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }

        /// <summary>
        /// 生成6位随机密码
        /// </summary>
        /// <returns></returns>
        public static string GetRandom()
        {
            Random rnd = new Random();
            //length是你需要几个随机数
            string str = "";//这里随便定义了一个string类型你可以int数组  
            for (int i = 0; i < 1; i++)
            {
                int n = rnd.Next(100000, 999999);
                //n就是你要的随机数，如果你要5位的就将上面改成(10000,99999),6位：(100000,999999)
                str += n;//如果是数组这里需要 str。add（n）；
            }
            return str;
        }

        /// <summary>
        /// SHA1 加密，16进制小写
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SHA1Encrypt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                string shaStr = BitConverter.ToString(hash);

                shaStr = shaStr.Replace("-", "");
                shaStr = shaStr.ToLower();

                return shaStr;
            }
        }

        /// <summary>
        /// md5 参数加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string str)
        {
            var paramArray = str.Split('&');
            var paramDic = new Dictionary<string, string>();
            foreach (var item in paramArray)
            {
                paramDic.Add(item.Split('=')[0], item.Split('=')[1]);
            }
            paramDic = paramDic.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var source = "";
            foreach (var item in paramDic)
            {
                source += $"&{item.Key}={item.Value}";
            }
            source = source.TrimStart('&') + "signKey";
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.ASCII.GetBytes(source));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }


        /// <summary>
        /// 公共验签方法
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        public static string Sign(Dictionary<string, object> dic, string signKey = "vcloud")
        {
            var data = string.Join("&", dic.Select(i => i.Key + "=" + i.Value).OrderBy(v => v));

            return ToMd5(data + signKey);
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToMd5(string s) => string.Join("", MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(s)).Select(v => v.ToString("x2")));
    }
}
