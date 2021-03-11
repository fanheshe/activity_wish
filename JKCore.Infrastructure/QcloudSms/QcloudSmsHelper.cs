using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.QcloudSms
{
    public class QcloudSmsHelper: IQcloudSms
    {
        private const string endpoint = "https://yun.tim.qq.com/v5/{0}/{1}?sdkappid={2}&random={3}";
        private static string AppId = string.Empty;
        private static string AppKey = string.Empty;

        private readonly DBConfig _settings;
        public QcloudSmsHelper(IOptions<DBConfig> settings)
        {
            _settings = settings.Value;
            AppId = _settings.SmsAppId;
            AppKey = _settings.SmsAppKey;
        }

        public readonly static JsonSerializerSettings serializerSettings = new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        #region 短信

        #region 单发

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="type">0:普通短信;1:营销短信</param>
        /// <param name="msg">需要匹配审核通过的模板内容 例:【签名】内容 (签名不填则为默认签名，指定签名需为已通过审核的签名)</param>
        /// <param name="extend">通道扩展码，可选字段，默认没有开通(需要填空字符)</param>
        /// <param name="ext">用户的session内容，腾讯server回包中会原样返回，可选字段，不需要就填空字符</param>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        public async Task<QcloudSmsSendSmsResult> SendSms(int type, string msg, string extend, string ext, string mobile) => await SendSms(new QcloudSmsSendSmsMessage { type = type, msg = msg, extend = extend ?? "", ext = ext ?? "", tel = new BaseSmsMessage.Tel { mobile = mobile, nationcode = "86" } });

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendSmsResult> SendSms(QcloudSmsSendSmsMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}&mobile={message.tel.mobile}");
            return await UtilPost<QcloudSmsSendSmsResult>(string.Format(endpoint, "tlssmssvr", "sendsms", AppId, random), message.ToString());
        }

        #endregion

        #region 模板单发

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="sign">短信签名，不带【】</param>
        /// <param name="tpl_id">审核通过的模板ID</param>
        /// <param name="_params">参数，分别对应模板的{1}，{2}，{3}参数</param>
        /// <param name="extend">通道扩展码，可选字段，默认没有开通(需要填空字符)</param>
        /// <param name="ext">用户的session内容，腾讯server回包中会原样返回，可选字段，不需要就填空字符</param>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendTplSmsResult> SendTplSms(string sign, int tpl_id, string[] _params, string extend, string ext, string mobile) => await SendTplSms(new QcloudSmsSendTplSmsMessage { sign = sign, tpl_id = tpl_id, @params = _params, extend = extend ?? "", ext = ext ?? "", tel = new BaseSmsMessage.Tel { mobile = mobile, nationcode = "86" } });

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendTplSmsResult> SendTplSms(QcloudSmsSendTplSmsMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}&mobile={message.tel.mobile}");
            return await UtilPost<QcloudSmsSendTplSmsResult>(string.Format(endpoint, "tlssmssvr", "sendsms", AppId, random), message.ToString());
        }

        #endregion

        #region 群发

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="sign">短信签名，不带【】</param>
        /// <param name="tpl_id">审核通过的模板ID</param>
        /// <param name="_params">参数，分别对应模板的{1}，{2}，{3}参数</param>
        /// <param name="extend">通道扩展码，可选字段，默认没有开通(需要填空字符)</param>
        /// <param name="ext">用户的session内容，腾讯server回包中会原样返回，可选字段，不需要就填空字符</param>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendMultiSmsResult> SendMultiSms(int type, string msg, string extend, string ext, params string[] mobile) => await SendMultiSms(new QcloudSmsSendMultiSmsMessage { type = type, msg = msg, extend = extend ?? "", ext = ext ?? "", tel = mobile.Select(a => new BaseSmsMessage.Tel { mobile = a, nationcode = "86" }).ToArray() });

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendMultiSmsResult> SendMultiSms(QcloudSmsSendMultiSmsMessage message)
        {
            if (message.tel == null)
                throw new ArgumentNullException("群发手机号不得为空！");
            else if (message.tel.Length > 200)
                throw new ArgumentException("群发手机号不得大于200个！");
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}&mobile={string.Join(",", message.tel.Select(a => a.mobile))}");
            return await UtilPost<QcloudSmsSendMultiSmsResult>(string.Format(endpoint, "tlssmssvr", "sendmultisms2", AppId, random), message.ToString());
        }

        #endregion

        #region 模板群发

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="sign">短信签名，不带【】</param>
        /// <param name="tpl_id">审核通过的模板ID</param>
        /// <param name="_params">参数，分别对应模板的{1}，{2}，{3}参数</param>
        /// <param name="extend">通道扩展码，可选字段，默认没有开通(需要填空字符)</param>
        /// <param name="ext">用户的session内容，腾讯server回包中会原样返回，可选字段，不需要就填空字符</param>
        /// <param name="mobile">手机号码</param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendTplMultiSmsResult> SendTplMultiSms(string sign, int tpl_id, string[] _params, string extend, string ext, params string[] mobile) => await SendTplMultiSms(new QcloudSmsSendTplMultiSmsMessage { sign = sign, tpl_id = tpl_id, @params = _params, extend = extend ?? "", ext = ext ?? "", tel = mobile.Select(a => new BaseSmsMessage.Tel { mobile = a, nationcode = "86" }).ToArray() });

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsSendTplMultiSmsResult> SendTplMultiSms(QcloudSmsSendTplMultiSmsMessage message)
        {
            if (message.tel == null)
                throw new ArgumentNullException("群发手机号不得为空！");
            else if (message.tel.Length > 200)
                throw new ArgumentException("群发手机号不得大于200个！");
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}&mobile={string.Join(",", message.tel.Select(a => a.mobile))}");
            return await UtilPost<QcloudSmsSendTplMultiSmsResult>(string.Format(endpoint, "tlssmssvr", "sendmultisms2", AppId, random), message.ToString());
        }

        #endregion

        #endregion

        #region 模板

        #region Add

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="title">模板名称</param>
        /// <param name="remark">模板备注，比如申请原因，使用场景等</param>
        /// <param name="text">模板内容</param>
        /// <param name="type">0：普通短信模板；1：营销短信模板；2：语音模板</param>
        /// <returns></returns>
        public  async Task<QcloudSmsAddTemplateResult> AddTemplate(string title, string remark, string text, int type) => await AddTemplate(new QcloudSmsAddTemplateMessage { title = title, remark = remark ?? "", text = text, type = type });

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsAddTemplateResult> AddTemplate(QcloudSmsAddTemplateMessage message)
        {
            if (message.text == null)
                throw new ArgumentNullException("模板内容不得为空！");
            if (message.text.Length > 440)
                throw new ArgumentException("模板内容不得大于400个字！");
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            return await UtilPost<QcloudSmsAddTemplateResult>(string.Format(endpoint, "tlssmssvr", "add_template", AppId, random), message.ToString());
        }

        #endregion

        #region Get

        /// <summary>
        /// 查看模板
        /// </summary>
        /// <param name="tpl_id">查询指定模版id的</param>
        /// <returns></returns>
        public  async Task<QcloudSmsGetTemplateResult> GetTemplate(params int[] tpl_id) => await GetTemplate(new QcloudSmsGetTemplateMessage { tpl_id = tpl_id });

        /// <summary>
        /// 查看模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsGetTemplateResult> GetTemplate(QcloudSmsGetTemplateMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            return await UtilPost<QcloudSmsGetTemplateResult>(string.Format(endpoint, "tlssmssvr", "get_template", AppId, random), message.ToString());
        }

        #endregion

        #region Modify

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="tpl_id">待修改的模板id</param>
        /// <param name="title">模板名称</param>
        /// <param name="remark">模板备注，比如申请原因，使用场景等</param>
        /// <param name="text">模板内容</param>
        /// <param name="type">0：普通短信模板；1：营销短信模板；2：语音模板</param>
        /// <returns></returns>
        public  async Task<QcloudSmsModTemplateResult> ModTemplate(int tpl_id, string title, string remark, string text, int type) => await ModTemplate(new QcloudSmsModTemplateMessage { tpl_id = tpl_id, title = title, remark = remark ?? "", text = text, type = type });

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsModTemplateResult> ModTemplate(QcloudSmsModTemplateMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            return await UtilPost<QcloudSmsModTemplateResult>(string.Format(endpoint, "tlssmssvr", "mod_template", AppId, random), message.ToString());
        }

        #endregion

        #region Delete

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="tpl_id">模板id</param>
        /// <returns></returns>
        public  async Task<QcloudSmsDelTemplateResult> DelTemplate(params int[] tpl_id) => await DelTemplate(new QcloudSmsDelTemplateMessage { tpl_id = tpl_id });

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsDelTemplateResult> DelTemplate(QcloudSmsDelTemplateMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            return await UtilPost<QcloudSmsDelTemplateResult>(string.Format(endpoint, "tlssmssvr", "del_template", AppId, random), message.ToString());
        }

        #endregion

        #endregion

        #region 签名

        /// <summary>
        /// 图片Base64最大字符长度
        /// </summary>
        private const int maxBase64PicSize = 1000000;

        #region Add

        /// <summary>
        /// 添加签名
        /// </summary>
        /// <param name="remark">签名备注，比如申请原因，使用场景等</param>
        /// <param name="text">签名内容，不带【】</param>
        /// <param name="pic">签名内容相关的证件截图base64格式字符串</param>
        /// <returns></returns>
        public  async Task<QcloudSmsAddSignResult> AddSign(string remark, string text, string pic) => await AddSign(new QcloudSmsAddSignMessage { remark = remark ?? "", text = text, pic = pic });

        /// <summary>
        /// 添加签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsAddSignResult> AddSign(QcloudSmsAddSignMessage message)
        {
            if (message.pic.Length > maxBase64PicSize)
            {
                string msg = $"图片内容不得超过{maxBase64PicSize},当前长度为{message.pic.Length}";
                throw new ArgumentException(msg);
            }
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            var url = string.Format(endpoint, "tlssmssvr", "add_sign", AppId, random);
            return await UtilPost<QcloudSmsAddSignResult>(url, message.ToString());
        }

        #endregion

        #region Get

        /// <summary>
        /// 查看签名
        /// </summary>
        /// <param name="sign_id">签名id</param>
        /// <returns></returns>
        public  async Task<QcloudSmsGetSignResult> GetSign(params int[] sign_id) => await GetSign(new QcloudSmsGetSignMessage { sign_id = sign_id });

        /// <summary>
        /// 查看签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsGetSignResult> GetSign(QcloudSmsGetSignMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            var url = string.Format(endpoint, "tlssmssvr", "get_sign", AppId, random);
            return await UtilPost<QcloudSmsGetSignResult>(url, message.ToString());
        }

        #endregion

        #region Modify

        /// <summary>
        /// 修改签名
        /// </summary>
        /// <param name="sign_id">签名id</param>
        /// <param name="remark">签名备注，比如申请原因，使用场景等</param>
        /// <param name="text">签名内容，不带【】</param>
        /// <param name="pic">签名内容相关的证件截图base64格式字符串</param>
        /// <returns></returns>
        public  async Task<QcloudSmsModSignResult> ModSign(int sign_id, string remark, string text, string pic) => await ModSign(new QcloudSmsModSignMessage { sign_id = sign_id, remark = remark ?? "", text = text, pic = pic });

        /// <summary>
        /// 修改签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsModSignResult> ModSign(QcloudSmsModSignMessage message)
        {
            if (message.pic.Length > maxBase64PicSize)
            {
                string msg = $"图片内容不得超过{maxBase64PicSize},当前长度为{message.pic.Length}";
                throw new ArgumentException(msg);
            }
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            var url = string.Format(endpoint, "tlssmssvr", "mod_sign", AppId, random);
            Console.WriteLine(JsonConvert.SerializeObject(message));
            Console.WriteLine(url);
            return await UtilPost<QcloudSmsModSignResult>(url, message.ToString());
        }

        #endregion

        #region Delete

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="tpl_id">模板id</param>
        /// <returns></returns>
        public  async Task<QcloudSmsDelSignResult> DelSign(params int[] sign_id) => await DelSign(new QcloudSmsDelSignMessage { sign_id = sign_id });

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<QcloudSmsDelSignResult> DelSign(QcloudSmsDelSignMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            return await UtilPost<QcloudSmsDelSignResult>(string.Format(endpoint, "tlssmssvr", "del_sign", AppId, random), message.ToString());
        }

        #endregion

        #endregion

        #region 拉取

        /// <summary>
        /// 拉取短信状态
        /// </summary>
        /// <param name="type">0 1分别代表 短信下发，短信回复</param>
        /// <param name="max">最大条数 最多100</param>
        /// <returns></returns>
        public  async Task<BasePullStatusSmsResult> PullStatus(int type, int max) => await PullStatus(new QcloudSmsPullStatusMessage { type = type, max = max });

        /// <summary>
        /// 拉取短信状态
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public  async Task<BasePullStatusSmsResult> PullStatus(QcloudSmsPullStatusMessage message)
        {
            var random = RandomString(10);
            message.time = Time();
            message.sig = Sign($"random={random}&time={message.time}");
            string result = await UtilPost(string.Format(endpoint, "tlssmssvr", "pullstatus", AppId, random), message.ToString());
            if (message.type == 0)
            {
                return JsonConvert.DeserializeObject<QcloudSmsCallBackSmsResult>(result);
            }
            else
            {
                return JsonConvert.DeserializeObject<QcloudSmsReplySmsResult>(result);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 调用接口方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task<string> UtilPost(string url, string message)
        {
            var content = new StringContent(message, Encoding.UTF8, "application/json");
            var query = await HttpClientHelper.Client.PostAsync(url, content);
            return await query.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 调用接口方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task<T> UtilPost<T>(string url, string message)
        {
            var content = new StringContent(message, Encoding.UTF8, "application/json");
            var query = await HttpClientHelper.Client.PostAsync(url, content);
            var result = await query.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private static long Time()
        {
            return DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Ticks / TimeSpan.TicksPerSecond;
        }

        /// <summary>
        /// 生成随机值
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        private static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string Sign(string data)
        {
            data = $"appkey={AppKey}&{data}";
            using (var hashAlgorithm = SHA256.Create())
            {
                var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data));
                return ByteArrayToHex(hash);
            }
        }

        /// <summary>
        /// 转hash
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        private static string ByteArrayToHex(byte[] byteArray)
        {
            string returnStr = "";
            if (byteArray != null)
            {
                for (int i = 0; i < byteArray.Length; i++)
                {
                    returnStr += byteArray[i].ToString("x2");
                }
            }
            return returnStr;
        }

        #endregion

        public string CutDesc(string desc)
        {
            string str = string.Empty;
            desc = desc.Substring(0, desc.Length - 1);
            if (desc.IndexOf("如有其他疑问") != -1)
            {
                str = desc.Substring(0, desc.IndexOf("如有其他疑问") - 1).Replace(" ", "") + "。";
            }
            else
            {
                str = desc.Substring(0, desc.LastIndexOf("。") + 1).Replace(" ", "");
            }
            return str;
        }
    }
}
