using Activity.Interfaces.Application.Model;
using JKCore.Domain.IRepository;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Invocable
{
    public class CronJobParam
    {
        public static string ActivityNo = "AC20210101";
        private readonly IJiakeRepository<object> _jiakeRepository;
        private static DateTime startTime;
        private static DateTime endTime;

        private readonly string mpAppId; //公众号AppID
        private readonly string mpAppSecret; //公众号AppID
        private readonly string appId; //小程序AppID
        private readonly string appSecret; //小程序AppSecret

        public CronJobParam(IConfiguration configuration, IJiakeRepository<object> jiakeRepository)
        {
            _jiakeRepository = jiakeRepository;
            mpAppId = configuration.GetSection("SenparcWeixinSetting:WeixinAppId").Value;
            mpAppSecret = configuration.GetSection("SenparcWeixinSetting:WeixinAppSecret").Value;
            appId = configuration.GetSection("SenparcWeixinSetting:WxOpenAppId").Value;
            appSecret = configuration.GetSection("SenparcWeixinSetting:WxOpenAppSecret").Value;
        }

        /// <summary>
        /// 获取活动时间
        /// </summary>
        /// <returns></returns>
        public async Task<(DateTime, DateTime)> GetActivity()
        {
            if (startTime == DateTime.MinValue || endTime == DateTime.MinValue)
            {
                string strSql = $@"SELECT start_time,end_time FROM t_activity WHERE activity_no=@ActivityNo;";

                (startTime, endTime) = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<(DateTime, DateTime)>(strSql, new { ActivityNo });
            }
            return (startTime, endTime);
        }

        /// <summary>
        /// 活动开始，关注公众号全部用户发消息
        /// </summary>
        /// <returns></returns>
        public async Task AllSendTemplateMessage()
        {
            try
            {
                var accessToken = await AccessTokenContainer.TryGetAccessTokenAsync(mpAppId, mpAppSecret);

                var result = await Senparc.Weixin.MP.AdvancedAPIs.UserApi.GetAsync(accessToken, null);

                if (result.total > 0)
                {
                    var msgData = new MPTemplateData("您好，您预约的实现吧！愿望君！活动已经开启，快来实现你的愿望吧~",
                        "实现吧！2021愿望君！",
                        $"{DateTime.Now.ToString("yyyy年MM月dd日")}",
                        "快来前往月野兔星球小程序~",
                        "点击即可立即进入哦~");

                    var miniProgram = new TemplateModel_MiniProgram()
                    {
                        appid = appId,
                        pagepath = "pages/index/index"
                    };

                    // 因为是同一个公众号，根据环境变量处理测试数据
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    {
                        var betaList = new List<string>()
                        {
                            "oHxnJwqEKSsUnMq8vpbwJQ3k5jYQ",//fly
                            "oHxnJwuuMR3eZ04mhVcNLfY3Mouo",//LIsa
                            "oHxnJwvwhG-hTcdfzWySqRkaqYkY",//鑫鑫
                            "oHxnJwl_xD09u_2hZB-Y5FiMrYWQ",//C.
                            "oHxnJwpAl4RU7OLy6q1ZhQKbMbNo",//烟花
                            "oHxnJwqXoOrJw2hI3Z3FHZB1H0VM",//太阳女神
                        };

                        foreach (var openid in result.data.openid)
                        {
                            if (betaList.Where(e => e.Contains(openid)).Any()) // 测试白名单
                            {
                                await Senparc.Weixin.MP.AdvancedAPIs.TemplateApi.SendTemplateMessageAsync(accessToken, openid, MPTemplateID.mpStart, null, msgData, miniProgram);
                            }
                        }
                    }
                    else
                    {
                        foreach (var openid in result.data.openid)
                        {
                            await Senparc.Weixin.MP.AdvancedAPIs.TemplateApi.SendTemplateMessageAsync(accessToken, openid, MPTemplateID.mpStart, null, msgData, miniProgram);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("AllSendTemplateMessage：", ex.Message));
            }
        }

        /// <summary>
        /// 未兑换愿望提醒
        /// </summary>
        /// <param name="wishId"></param>
        /// <param name="wishName"></param>
        /// <param name="openId"></param>
        /// <param name="nickName"></param>
        /// <param name="mch_no"></param>
        /// <returns></returns>
        public async Task NoExchangeSendTemplateMessage(int wishId, string wishName, string openId, string nickName, string mch_no)
        {
            try
            {
                var pagepath = string.IsNullOrEmpty(mch_no) ? $"pages/detail/index?wishId={wishId}" : $"pages/conversion/index?wishId={wishId}";

                var accessToken = await AccessTokenContainer.TryGetAccessTokenAsync(appId, appSecret);

                var msgData = new UniformSendData
                {
                    touser = openId,
                    mp_template_msg = new Mp_Template_Msg
                    {
                        appid = mpAppId,
                        template_id = MPTemplateID.mpNoExchange,
                        miniprogram = new Miniprogram
                        {
                            appid = appId,
                            pagepath = pagepath,
                        },
                        data = new MPTemplateData(
                                    "您还有愿望未进行兑现哦！快来进行愿望兑现吧~",
                                    $"{wishName}",
                                    $"{nickName}",
                                    "",
                                    "点击查看详情哟~"
                                    )
                    }
                };

                await Senparc.Weixin.WxOpen.AdvancedAPIs.Template.TemplateApi.UniformSendAsync(accessToken, msgData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Concat("NoExchangeSendTemplateMessage：", ex.Message));
            }
        }
    }
}
