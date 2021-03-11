using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin.WxOpen.Containers;

namespace JKCore.Activity.Api.Controllers
{
    [ApiController]
    [Route("mini")]
    public class WeixinMiniProgramController : ControllerBase
    {
        // public readonly string _appId = Config.SenparcWeixinSetting.WeixinAppId;//与微信公众账号后台的AppId设置保持一致，区分大小写。
        // private readonly string _appSecret = Config.SenparcWeixinSetting.WeixinAppSecret;//与微信公众账号后台的AppId设置保持一致，区分大小写。

        public readonly string _wxOpenAppId = Config.SenparcWeixinSetting.WxOpenAppId;//与微信小程序后台的AppId设置保持一致，区分大小写。
        private readonly string _wxOpenAppSecret = Config.SenparcWeixinSetting.WxOpenAppSecret;//与微信小程序账号后台的AppId设置保持一致，区分大小写。

        public WeixinMiniProgramController()
        {

        }

        /// <summary>
        /// wx.login登陆成功之后发送的请求
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpPost("OnLogin")]
        public async Task<IActionResult> OnLogin([FromBody] JObject values)
        {
            var code = values["code"].ToString();
            // var encryptedData = values["encryptedData"].ToString();
            // var iv = values["iv"].ToString();

            try
            {
                // 登录
                var jsonResult = SnsApi.JsCode2Json(_wxOpenAppId, _wxOpenAppSecret, code);
                if (jsonResult.errcode == ReturnCode.请求成功)
                {
                    // 开放平台下存在同一主体小程序+公众号且用户已关注返回unionId，否则不返回
                    var unionid = jsonResult.unionid;
                    var issub = string.IsNullOrEmpty(unionid) ? false : true;
                    if (!issub)
                    {
                        Console.WriteLine("未关注公众号");
                    }
                    

                    // // 解密获取unionId
                    // var str = Senparc.Weixin.WxOpen.Helpers.EncryptHelper.DecodeEncryptedData(jsonResult.session_key, encryptedData, iv);
                    // var advancedUserInfo = JsonConvert.DeserializeObject<dynamic>(str);
                    // string unionId = advancedUserInfo.unionId;

                    // //通过unionId 获取 公众号 openid

                    // // 获取公众号access_token
                    // var accessToken = await CommonApi.GetTokenAsync(_appId, _appSecret);
                    // var userInfo = await CommonApi.GetUserInfoAsync(accessToken.access_token, "oHxnJwvaKv04Fhm-5k9Cyu4D6ZCw");

                    // if (userInfo.subscribe == 1 && unionId == userInfo.unionid)
                    // {
                    //     Console.WriteLine("已关注");
                    // }
                    // else
                    // {
                    //     Console.WriteLine("未关注");
                    // }


                    // // Session["WxOpenUser"] = jsonResult;//使用Session保存登陆信息（不推荐）
                    // // 使用SessionContainer管理登录信息（推荐）
                    // var sessionBag = await SessionContainer.UpdateSessionAsync(null, jsonResult.openid, jsonResult.session_key, unionId);

                    // 注意：生产环境下SessionKey属于敏感信息，不能进行传输！
                    return Ok(new { data = issub ? "已关注" : "未关注" });
                }
                else
                {
                    return Ok(new { success = false, msg = jsonResult.errmsg });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, msg = ex.Message });
            }
        }



    }
}