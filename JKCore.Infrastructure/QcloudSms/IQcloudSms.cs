using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.QcloudSms
{
    public interface IQcloudSms
    {
        #region 短信

        #region 单发
        Task<QcloudSmsSendSmsResult> SendSms(int type, string msg, string extend, string ext, string mobile);

        Task<QcloudSmsSendSmsResult> SendSms(QcloudSmsSendSmsMessage message);
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
        Task<QcloudSmsSendTplSmsResult> SendTplSms(string sign, int tpl_id, string[] _params, string extend, string ext, string mobile);

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsSendTplSmsResult> SendTplSms(QcloudSmsSendTplSmsMessage message);
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
        Task<QcloudSmsSendMultiSmsResult> SendMultiSms(int type, string msg, string extend, string ext, params string[] mobile);

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsSendMultiSmsResult> SendMultiSms(QcloudSmsSendMultiSmsMessage message);
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
        Task<QcloudSmsSendTplMultiSmsResult> SendTplMultiSms(string sign, int tpl_id, string[] _params, string extend, string ext, params string[] mobile);

        /// <summary>
        /// 指定模板单发短信
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsSendTplMultiSmsResult> SendTplMultiSms(QcloudSmsSendTplMultiSmsMessage message);

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
        Task<QcloudSmsAddTemplateResult> AddTemplate(string title, string remark, string text, int type);

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsAddTemplateResult> AddTemplate(QcloudSmsAddTemplateMessage message);

        #endregion

        #region Get

        /// <summary>
        /// 查看模板
        /// </summary>
        /// <param name="tpl_id">查询指定模版id的</param>
        /// <returns></returns>
        Task<QcloudSmsGetTemplateResult> GetTemplate(params int[] tpl_id);

        /// <summary>
        /// 查看模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsGetTemplateResult> GetTemplate(QcloudSmsGetTemplateMessage message);

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
        Task<QcloudSmsModTemplateResult> ModTemplate(int tpl_id, string title, string remark, string text, int type);

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsModTemplateResult> ModTemplate(QcloudSmsModTemplateMessage message);

        #endregion

        #region Delete

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="tpl_id">模板id</param>
        /// <returns></returns>
        Task<QcloudSmsDelTemplateResult> DelTemplate(params int[] tpl_id);

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsDelTemplateResult> DelTemplate(QcloudSmsDelTemplateMessage message);

        #endregion

        #endregion
        #region 签名
        
        #region Add

        /// <summary>
        /// 添加签名
        /// </summary>
        /// <param name="remark">签名备注，比如申请原因，使用场景等</param>
        /// <param name="text">签名内容，不带【】</param>
        /// <param name="pic">签名内容相关的证件截图base64格式字符串</param>
        /// <returns></returns>
        Task<QcloudSmsAddSignResult> AddSign(string remark, string text, string pic);

        /// <summary>
        /// 添加签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsAddSignResult> AddSign(QcloudSmsAddSignMessage message);

        #endregion

        #region Get

        /// <summary>
        /// 查看签名
        /// </summary>
        /// <param name="sign_id">签名id</param>
        /// <returns></returns>
        Task<QcloudSmsGetSignResult> GetSign(params int[] sign_id);

        /// <summary>
        /// 查看签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsGetSignResult> GetSign(QcloudSmsGetSignMessage message);

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
        Task<QcloudSmsModSignResult> ModSign(int sign_id, string remark, string text, string pic);

        /// <summary>
        /// 修改签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsModSignResult> ModSign(QcloudSmsModSignMessage message);

        #endregion

        #region Delete

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="tpl_id">模板id</param>
        /// <returns></returns>
        Task<QcloudSmsDelSignResult> DelSign(params int[] sign_id);

        /// <summary>
        /// 删除签名
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<QcloudSmsDelSignResult> DelSign(QcloudSmsDelSignMessage message);

        #endregion

        #endregion
        #region 拉取

        /// <summary>
        /// 拉取短信状态
        /// </summary>
        /// <param name="type">0 1分别代表 短信下发，短信回复</param>
        /// <param name="max">最大条数 最多100</param>
        /// <returns></returns>
        Task<BasePullStatusSmsResult> PullStatus(int type, int max);

        /// <summary>
        /// 拉取短信状态
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<BasePullStatusSmsResult> PullStatus(QcloudSmsPullStatusMessage message);

        #endregion

        string CutDesc(string desc);
    }
}
