using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace JKCore.Infrastructure.QcloudSms
{
    public sealed class HttpClientHelper
    {
        public static readonly HttpClient Client;
        private HttpClientHelper() { }
        static HttpClientHelper() { Client = new HttpClient(); }
    }
    #region 请求类

    #region 请求基类

    public class BaseMessage
    {
        /// <summary>
        /// app凭证
        /// </summary>
        public string sig { get; set; }
        /// <summary>
        /// unix时间戳，请求发起时间，如果和系统时间相差超过10分钟则会返回失败
        /// </summary>
        public long time { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, QcloudSmsHelper.serializerSettings);
        }
    }

    #endregion

    #region 短信

    public class BaseSmsMessage : BaseMessage
    {
        /// <summary>
        /// 通道扩展码，可选字段，默认没有开通(需要填空)
        /// </summary>
        public string extend { get; set; } = "";
        /// <summary>
        /// 用户的session内容，腾讯server回包中会原样返回，可选字段，不需要就填空
        /// </summary>
        public string ext { get; set; } = "";
        public class Tel
        {
            /// <summary>
            /// 国家码 默认86
            /// </summary>
            public string nationcode { get; set; }
            /// <summary>
            /// 手机号码
            /// </summary>
            public string mobile { get; set; }
        }
    }

    public class BaseSendTplSmsMessage : BaseSmsMessage
    {
        /// <summary>
        /// 短信签名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 审核通过的模板ID
        /// </summary>
        public int tpl_id { get; set; }
        /// <summary>
        /// 参数，分别对应模板的{1}，{2}，{3}参数
        /// </summary>
        public string[] @params { get; set; }
    }

    public class BaseSendSmsMessage : BaseSmsMessage
    {
        /// <summary>
        /// 0:普通短信;1:营销短信
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// utf8编码，需要匹配审核通过的模板内容
        /// 例:【签名】内容
        /// 签名不填则为默认签名，指定签名需为已通过审核的签名
        /// </summary>
        public string msg { get; set; }
    }

    public class QcloudSmsSendSmsMessage : BaseSendSmsMessage
    {
        public Tel tel { get; set; }
    }

    public class QcloudSmsSendMultiSmsMessage : BaseSendSmsMessage
    {
        public Tel[] tel { get; set; }
    }

    public class QcloudSmsSendTplSmsMessage : BaseSendTplSmsMessage
    {
        public Tel tel { get; set; }
    }

    public class QcloudSmsSendTplMultiSmsMessage : BaseSendTplSmsMessage
    {
        public Tel[] tel { get; set; }
    }

    #endregion

    #region 模板

    public class BaseTemplateMessage : BaseMessage
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 模板备注，比如申请原因，使用场景等
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 0：普通短信模板；1：营销短信模板；2：语音模板
        /// </summary>
        public int type { get; set; }
    }

    public class QcloudSmsAddTemplateMessage : BaseTemplateMessage { }

    public class QcloudSmsGetTemplateMessage : BaseMessage
    {
        /// <summary>
        /// 查询指定模版id
        /// </summary>
        public int[] tpl_id { get; set; }
    }


    public class QcloudSmsModTemplateMessage : BaseTemplateMessage
    {
        /// <summary>
        /// 待修改的模板id
        /// </summary>
        public int tpl_id { get; set; }
    }

    public class QcloudSmsDelTemplateMessage : BaseMessage
    {
        /// <summary>
        /// 模板id
        /// </summary>
        public int[] tpl_id { get; set; }
    }

    #endregion

    #region 签名

    public class BaseSignMessage : BaseMessage
    {
        /// <summary>
        /// 签名备注，比如申请原因，使用场景等
        /// </summary>
        public string remark { get; set; }
        /// <summary>
        /// 签名内容，不带【】
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 签名内容相关的证件截图base64格式字符串
        /// </summary>
        public string pic { get; set; }
    }

    public class QcloudSmsAddSignMessage : BaseSignMessage { }

    public class QcloudSmsGetSignMessage : BaseMessage
    {
        /// <summary>
        /// 签名id
        /// </summary>
        public int[] sign_id { get; set; }
    }

    public class QcloudSmsModSignMessage : BaseSignMessage
    {
        /// <summary>
        /// 签名id
        /// </summary>
        public int sign_id { get; set; }
    }

    public class QcloudSmsDelSignMessage : BaseMessage
    {
        /// <summary>
        /// 签名id
        /// </summary>
        public int[] sign_id { get; set; }
    }

    #endregion

    #endregion

    #region 应答类

    #region 应答基类

    public class BaseResult { }

    public class BaseNormalResult : BaseResult
    {
        /// <summary>
        /// 0表示成功，非0表示失败
        /// </summary>
        public int result { get; set; }
        /// <summary>
        /// result非0时的具体错误信息
        /// </summary>
        public string msg { get; set; }
    }

    #endregion

    #region 短信

    public class BaseSmsResult : BaseResult
    {
        /// <summary>
        /// 0表示成功(计费依据)，非0表示失败
        /// </summary>
        public int result { get; set; }
        /// <summary>
        /// result非0时的具体错误信息
        /// </summary>
        public string errmsg { get; set; }
    }

    public class QcloudSmsSendSmsResult : BaseSmsResult
    {
        /// <summary>
        /// 用户的session内容，腾讯server回包中会原样返回
        /// </summary>
        public string ext { get; set; }
        /// <summary>
        /// 标识本次发送id，标识一次短信下发记录
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 短信计费的条数
        /// </summary>
        public int fee { get; set; }
    }

    public class BaseDetail : BaseSmsResult
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 国家码
        /// </summary>
        public string nationcode { get; set; }
        /// <summary>
        /// 标识本次发送id，标识一次短信下发记录
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 短信计费的条数
        /// </summary>
        public int fee { get; set; }
    }

    public class QcloudSmsSendMultiSmsResult : BaseSmsResult
    {
        /// <summary>
        /// 用户的session内容，腾讯server回包中会原样返回
        /// </summary>
        public string ext { get; set; }
        public BaseDetail[] detail { get; set; }
    }


    public class QcloudSmsSendTplSmsResult : BaseSmsResult
    {
        /// <summary>
        /// 用户的session内容，腾讯server回包中会原样返回
        /// </summary>
        public string ext { get; set; }
        /// <summary>
        /// 标识本次发送id，标识一次短信下发记录
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 短信计费的条数
        /// </summary>
        public int fee { get; set; }
    }


    public class QcloudSmsSendTplMultiSmsResult : BaseSmsResult
    {
        /// <summary>
        /// 用户的session内容，腾讯server回包中会原样返回
        /// </summary>
        public string ext { get; set; }
        public BaseDetail[] detail { get; set; }
    }

    #endregion

    #region 模板

    public class BaseTemplateResultData
    {
        /// <summary>
        /// 模板id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 模板内容
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 0：已通过；1：待审核；2：已拒绝
        /// </summary>
        public int status { get; set; }
        /// <summary>
        /// 0：普通短信模板；1：营销短信模板；2：语音模板
        /// </summary>
        public int type { get; set; }
    }

    public class QcloudSmsAddTemplateResult : BaseNormalResult
    {
        public BaseTemplateResultData data { get; set; }
    }

    public class QcloudSmsGetTemplateResult : BaseNormalResult
    {
        /// <summary>
        /// 模版总数
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// 信息条数，信息内容在data字段中
        /// </summary>
        public int count { get; set; }
        public Datum[] data { get; set; }
        public class Datum : BaseTemplateResultData
        {
            /// <summary>
            /// 审批信息，如果status为2，会说明拒绝原因
            /// </summary>
            public string reply { get; set; }
        }
    }

    public class QcloudSmsModTemplateResult : BaseNormalResult
    {
        public BaseTemplateResultData data { get; set; }
    }

    public class QcloudSmsDelTemplateResult : BaseNormalResult { }

    #endregion

    #region 签名

    public class BaseSignResultData
    {
        /// <summary>
        /// 签名id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 签名内容
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 0：已通过；1：待审核；2：已拒绝
        /// </summary>
        public int status { get; set; }
    }

    public class QcloudSmsAddSignResult : BaseNormalResult
    {
        public BaseSignResultData data { get; set; }
    }

    public class QcloudSmsGetSignResult : BaseNormalResult
    {
        /// <summary>
        /// 信息条数，信息内容在data字段中
        /// </summary>
        public int count { get; set; }
        public Datum[] data { get; set; }
        public class Datum : BaseSignResultData
        {
            /// <summary>
            /// 审批信息，如果status为2，会说明拒绝原因
            /// </summary>
            public string reply { get; set; }
        }
    }

    public class QcloudSmsModSignResult : BaseNormalResult
    {
        public BaseSignResultData data { get; set; }
    }

    public class QcloudSmsDelSignResult : BaseNormalResult { }

    #endregion

    #endregion

    #region 拉取类

    #region 拉取请求类

    public class QcloudSmsPullStatusMessage : BaseMessage
    {
        /// <summary>
        /// 0 1分别代表 短信下发，短信回复
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 最大条数 最多100
        /// </summary>
        public int max { get; set; }
    }

    #endregion

    #region 拉取结果类

    public class BasePullStatusSmsResult : BaseSmsResult
    {
        /// <summary>
        /// result为0时有效，返回的信息条数
        /// </summary>
        public int count { get; set; }
    }

    public class QcloudSmsCallBackSmsResult : BasePullStatusSmsResult
    {
        public SmsCallBackMessage[] Data { get; set; }
    }

    public class QcloudSmsReplySmsResult : BasePullStatusSmsResult
    {
        public SmsReplyMessage[] Data { get; set; }
    }

    #endregion

    #region 下发通知结果类

    public class SmsCallBackMessage
    {
        /// <summary>
        /// 用户实际接收到短信的时间
        /// </summary>
        public DateTime user_receive_time { get; set; }
        /// <summary>
        /// 国家码
        /// </summary>
        public string nationcode { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 实际是否收到短信接收状态。SUCCESS（成功）、FAIL（失败）
        /// </summary>
        public string report_status { get; set; }
        /// <summary>
        /// 用户接收短信状态码
        /// </summary>
        public string errmsg { get; set; }
        /// <summary>
        /// 用户接收短信状态描述
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 标识本次发送id
        /// </summary>
        public string sid { get; set; }
    }

    #endregion

    #region 短信回复通知类

    public class SmsReplyMessage
    {
        /// <summary>
        /// 国家码
        /// </summary>
        public string nationcode { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 用户回复的内容
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// 短信签名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// unix时间戳
        /// </summary>
        public int time { get; set; }
        /// <summary>
        /// 通道扩展码 在短信回复时，腾讯server会原样返回，开发者可依此区分是哪种类型的回复
        /// </summary>
        public string extend { get; set; }
    }

    #endregion

    #endregion
}
