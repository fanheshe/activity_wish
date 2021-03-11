using System;
using System.Threading.Tasks;
using Activity.Interfaces.Application;
using Microsoft.AspNetCore.Mvc;
using JKCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using JKCore.Activity.Api.Common;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Activity.Interfaces.Application.Model;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Template;
using Senparc.Weixin.MP.Containers;

namespace JKCore.Activity.Api.Controllers
{
    /// <summary>
    /// 活动api接口类
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly string appId; //小程序AppID
        private readonly string appSecret; //小程序AppSecret
        private readonly ILoggerFactory _loggerFactory;
        private readonly IActivityApplication _activityApplication;

        private readonly string mpAppId; //公众号AppID

        private static int testVal = 0;

        public ActivityController(IConfiguration configuration, IHttpClientFactory clientFactory, ILoggerFactory loggerFactory, IActivityApplication activityApplication)
        {
            appId = configuration.GetSection("SenparcWeixinSetting:WxOpenAppId").Value;
            appSecret = configuration.GetSection("SenparcWeixinSetting:WxOpenAppSecret").Value;
            _loggerFactory = loggerFactory;
            _activityApplication = activityApplication;
            mpAppId = configuration.GetSection("SenparcWeixinSetting:WeixinAppId").Value;
        }

        #region haozh

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> login([FromBody] JObject jObject)
        {
            var code = jObject["code"].ToString();
            var activityNo = this.GetActivityNo();

            #region 校验参数

            StringBuilder sb = new StringBuilder();

            if (string.IsNullOrWhiteSpace(code))
            {
                sb.AppendLine("code无效！");
            }

            if (string.IsNullOrWhiteSpace(appId))
            {
                sb.AppendLine("appid无效！");
            }

            if (!string.IsNullOrEmpty(sb.ToString()))
                return BadRequest(sb.ToString());

            #endregion 校验参数

            #region 获取微信openId

            var jsonResult = new JsCode2JsonResult();
            try
            {
                jsonResult = SnsApi.JsCode2Json(appId, appSecret, code);
                //Console.WriteLine("jsonResult:" + Newtonsoft.Json.JsonConvert.SerializeObject(jsonResult));
                if (jsonResult.errcode == ReturnCode.请求成功)
                {
                    if (string.IsNullOrEmpty(jsonResult.openid))
                    {
                        return BadRequest("获取OpenId失败");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("登录失败：" + ex.Message);
                return BadRequest("登录失败");
            }

            #endregion 获取微信openId

            //开放平台下存在同一主体小程序+公众号且用户已关注返回unionId，否则不返回
            //同主体公众号小程序 用户是否关注 1：关注 0：否
            var subscsribe = !string.IsNullOrEmpty(jsonResult.unionid) ? 1 : 0;
            //获取会员信息，每次都会更新sessionKey
            var data = await _activityApplication.GetMemberByOpenIdAsync(activityNo, jsonResult.openid, jsonResult.session_key);
            //获取活动信息
            var activityInfo = await _activityApplication.GetActivityResponseAsync(this.GetActivityNo());
            // 愿望状态
            var wish = await _activityApplication.MemberWishStatusAsync(activityNo, data.memberId);

            return this.MyOK(
                new
                {
                    subscsribe,
                    data.memberId,
                    data.gender,
                    data.isBind,
                    data.nickName,
                    data.avatarUrl,
                    activity = new
                    {
                        activityInfo?.status,
                        activityInfo?.startTime,
                        activityInfo?.endTime,
                        activityInfo?.serverTime
                    },
                    myWish = new { wishId = wish.Item1, status = wish.Item2, mchNo = wish.Item3 }
                });
        }

        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BindPhone([FromBody] BindTelRequestParam request)
        {
            if (request == null)
            {
                return BadRequest("参数错误");
            }
            if (string.IsNullOrEmpty(request.encryptedData) || string.IsNullOrEmpty(request.iv))
            {
                return BadRequest("参数错误");
            }
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            #region 更新session_key
            if (!string.IsNullOrEmpty(request.code))
            {
                var jsonResult = new JsCode2JsonResult();
                try
                {
                    jsonResult = SnsApi.JsCode2Json(appId, appSecret, request.code);
                    await _activityApplication.UpdateSessionKeyAsync(memberId, jsonResult.session_key);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("更新session_key失败：" + ex.Message);
                }
            }
            #endregion 更新session_key


            string sessionKey = await _activityApplication.GetSessionKeyAsync(memberId);
            string tel = "";

            #region 解析手机号

            try
            {
                var jsonStr = Senparc.Weixin.WxOpen.Helpers.EncryptHelper.DecodeEncryptedData(sessionKey, request.encryptedData, request.iv);
                var phoneNumber = Newtonsoft.Json.JsonConvert.DeserializeObject<Senparc.Weixin.WxOpen.Entities.DecodedPhoneNumber>(jsonStr);
                tel = phoneNumber.purePhoneNumber;
            }
            catch (Exception ex)
            {
                var logger = _loggerFactory.CreateLogger($"Error-BindPhoneNumber：{DateTime.Now}");
                logger.LogError(ex.Message);

                return BadRequest("手机号解析失败");
            }

            #endregion 解析手机号

            if (string.IsNullOrEmpty(tel))
            {
                return BadRequest("手机号解析失败");
            }
            var (resCode, res) = await _activityApplication.BindTelAsync(memberId, tel);
            if (resCode == 0)
            {
                return BadRequest(res);
            }
            return this.MyOK(new { resCode = 1 });
        }

        /// <summary>
        /// 授权信息-授权头像昵称
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BindInfo([FromBody] BindInfoRequestParam request)
        {
            if (request == null)
            {
                return BadRequest("参数错误");
            }
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            var resCode = await _activityApplication.BindInfoAsync(memberId, request);
            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 我的首页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MyWishList()
        {
            var memberId = this.GetMemberId();
            var activityNo = this.GetActivityNo();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            //获取会员的许愿列表
            var dataList = await _activityApplication.GetMyWishListAsync(activityNo, memberId);
            //获取活动信息
            var activityInfo = await _activityApplication.GetActivityResponseAsync(activityNo);
            //活动许愿人数
            var wishMakers = activityInfo.fakeBase;
            //愿望值
            var scores = await _activityApplication.GetWishValueAsync(memberId);

            #region 其他心愿

            //计算其他心愿 0-不可选，1-可选
            //var gender = await _activityApplication.GetMemberGenderAsync(memberId);
            //获取当前会员可选择的愿望列表，只取一个，有就算还可以许愿
            var otherWishDataList = await _activityApplication.GetWishListAsync(
                activityNo,
                memberId,
                new WishListRequestParam()
                {
                    gender = 0,
                    pageIndex = 1,
                    pageSize = 1
                });
            var otherWish = otherWishDataList.Item2 > 0 ? 1 : 0;

            #endregion 其他心愿

            return this.MyOK(new { dataList, wishMakers, scores, otherWish });
        }

        /// <summary>
        /// 选择愿望
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> WishList([FromQuery] WishListRequestParam request)
        {
            var memberId = this.GetMemberId();
            var activityNo = this.GetActivityNo();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (request == null)
            {
                return BadRequest("参数错误");
            }
            if (request.gender != 1 && request.gender != 2)
            {
                return BadRequest("性别选择有误");
            }
            //获取会员的许愿列表
            var (dataList, total) = await _activityApplication.GetWishListAsync(activityNo, memberId, request);
            //判断其他愿望显示不显示，规则是提交过一次其他愿望就不再显示 其他愿望0-不显示 1-显示
            var HaveotherWish = await _activityApplication.IsHaveOtherWishAsync(activityNo, memberId);
            return this.MyOK(new { dataList, total, otherWish = HaveotherWish == 0 ? 1 : 0 });
        }

        /// <summary>
        /// 愿望收集
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            var activityNo = this.GetActivityNo();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null)
            {
                return BadRequest("参数错误");
            }
            var content = jObject["content"].ToString();
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("请输入愿望内容");
            }

            //获取会员的许愿列表
            var (resCode, res) = await _activityApplication.SubmitWishAsync(activityNo, memberId, content);
            if (resCode == 0)
            {
                return BadRequest(res);
            }
            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 愿望详情
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> WishDetail(string wishNo, int gender)
        {
            var memberId = this.GetMemberId();
            var activityNo = this.GetActivityNo();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (string.IsNullOrEmpty(wishNo))
            {
                return BadRequest("愿望编号不能为空");
            }
            if (gender != 1 && gender != 2)
            {
                return BadRequest("性别选择有误");
            }

            //获取会员的许愿列表
            var data = await _activityApplication.GetWishDetailAsync(activityNo, wishNo, gender);
            //获取许愿人数造假数据
            if (data != null)
            {
                data.wishMakers = await _activityApplication.GetWishMakersAsync(wishNo);
            }
            return this.MyOK(data);
        }

        #endregion haozh

        /// <summary>
        /// 愿望加速
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AccelerateWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["wishId"] == null || !int.TryParse(jObject["wishId"].ToString(), out int wishId))
            {
                return BadRequest("参数错误");
            }
            var (id, mpArrive, mpBoost) = await _activityApplication.AccelerateWishAsync(memberId, wishId);

            #region 发送模板消息 1、即将达成，2、愿望达成

            if (mpArrive == 1)
            {
                await UniformSendMPTemplateAsync(wishId, MPTemplateID.mpArrive);
            }
            if (mpBoost == 1)
            {
                await UniformSendMPTemplateAsync(wishId, MPTemplateID.mpBoost);
            }

            #endregion 发送模板消息 1、即将达成，2、愿望达成

            return this.MyOK(new { resCode = id > 0 ? 1 : 0 });
        }

        /// <summary>
        /// 问题反馈
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SubmitQuestion([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["content"] == null)
            {
                return BadRequest("参数错误");
            }
            var content = jObject["content"].ToString();
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("请填写问题内容");
            }
            var activityNo = this.GetActivityNo();
            var id = await _activityApplication.SaveProblemAsync(memberId, activityNo, content);
            return this.MyOK(new { resCode = id > 0 ? 1 : 0 });
        }

        /// <summary>
        /// 兑现愿望
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ExchangeWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["mchNo"] == null)
            {
                return BadRequest("参数错误");
            }
            var (resCode, wishId, msg) = await _activityApplication.TakeWishAsync(memberId, jObject["mchNo"].ToString());
            if (resCode == 0)
            {
                return BadRequest(msg);
            }
            return this.MyOK(new { resCode, wishId });
        }

        /// <summary>
        /// 实现愿望||手动达成
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ComplateWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["wishId"] == null || jObject["mchNo"] == null
                || !int.TryParse(jObject["wishId"].ToString(), out int wishId))
            {
                return BadRequest("参数错误");
            }
            var (resCode, msg, isSend) = await _activityApplication.ComplateWishAsync(memberId, wishId, jObject["mchNo"].ToString());
            if (resCode == 0)
            {
                return BadRequest(msg);
            }

            #region 发送模板消息 愿望达成

            if (isSend == 1)
            {
                await UniformSendMPTemplateAsync(wishId, MPTemplateID.mpArrive);
            }

            #endregion 发送模板消息 愿望达成

            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 门店列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MchList([FromQuery] MchListQuery request)
        {
            var memberId = this.GetMemberId();
            var activityNo = this.GetActivityNo();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            var (dataList, total) = await _activityApplication.GetMchListAsync(request);
            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 助力列表
        /// </summary>
        /// <param name="wishId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> BoostList(int wishId)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            var dataList = await _activityApplication.GetBoostListAsync(wishId);
            return this.MyOK(new { dataList });
        }

        /// <summary>
        /// 查看愿望
        /// </summary>
        /// <param name="wishId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ViewWish(int wishId)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            var data = await _activityApplication.GetMakeWishAsync(memberId, wishId);
            return this.MyOK(data);
        }

        /// <summary>
        /// 帮TA助力
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> BoostWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["wishId"] == null
                || !int.TryParse(jObject["wishId"].ToString(), out int wishId))
            {
                return BadRequest("参数错误");
            }
            if (await _activityApplication.GetMemberBoostCountAsync(memberId) > 0)
            {
                return this.MyOK(new { resCode = 2 });
            }

            var (resCode, msg, mpArrive, mpBoost) = await _activityApplication.BoostWishAsync(this.GetActivityNo(), memberId, wishId);
            if (resCode == 0)
            {
                return BadRequest(msg);
            }

            #region 发送模板消息 1、即将达成，2、愿望达成

            if (mpArrive == 1)
            {
                await UniformSendMPTemplateAsync(wishId, MPTemplateID.mpArrive);
            }
            if (mpBoost == 1)
            {
                await UniformSendMPTemplateAsync(wishId, MPTemplateID.mpBoost);
            }

            #endregion 发送模板消息 1、即将达成，2、愿望达成

            return this.MyOK(new
            {
                resCode
            });
        }

        /// <summary>
        /// 许愿
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> MakeWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }

            if (jObject == null || jObject["wishNo"] == null || jObject["userCount"] == null ||
                !int.TryParse(jObject["userCount"].ToString(), out int userCount)
                || jObject["gender"] == null || !int.TryParse(jObject["gender"].ToString(), out int gender))
            {
                return BadRequest("参数错误");
            }

            var (id, msg) = await _activityApplication.MakeWishAsync(memberId, jObject["wishNo"].ToString(), userCount, this.GetActivityNo(), gender);
            if (id == 0)
            {
                return BadRequest(msg);
            }
            return this.MyOK(new { id });
        }

        /// <summary>
        /// 预约愿望
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MPReserve()
        {
            var memberId = this.GetMemberId();

            #region 发送模板消息 预约愿望提醒

            int resCode = await UniformSendMPTemplateAsync(0, MPTemplateID.mpReserve, memberId);

            #endregion 发送模板消息 预约愿望提醒

            return this.MyOK(new { resCode });
        }

        private async Task<int> UniformSendMPTemplateAsync(int makeWishId, string mPTemplateID, int memberId = 0)
        {
            int resCode = 0;
            string pagepath = mPTemplateID == MPTemplateID.mpReserve ? string.Empty : $"pages/detail/index?wishId={makeWishId}";
            string tOpenId = string.Empty;
            try
            {
                MPTemplateData tData = null;

                (tData, tOpenId) = await _activityApplication.GetWishOpenId(makeWishId, mPTemplateID, memberId);

                if (tData != null)
                {
                    var accessToken = await AccessTokenContainer.TryGetAccessTokenAsync(appId, appSecret);
                    var msgData = new UniformSendData
                    {
                        touser = tOpenId,
                        mp_template_msg = new Mp_Template_Msg
                        {
                            appid = mpAppId,
                            template_id = mPTemplateID,
                            miniprogram = new Miniprogram
                            {
                                appid = appId,
                                pagepath = pagepath,
                            },
                            data = tData
                        }
                    };

                    var result = await TemplateApi.UniformSendAsync(accessToken, msgData);
                    resCode = 1;

                    if (mPTemplateID == MPTemplateID.mpReserve) //预约提醒只发送一次，发送成功更新
                    {
                        await _activityApplication.SaveMemberIsSend(memberId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(string.Concat("mPTemplateID：", mPTemplateID, "----memberID：", memberId, "----openId：", tOpenId));
            }

            return resCode;
        }

        public IActionResult Test1()
        {
            return Ok(testVal++);
        }

        /// <summary>
        /// 扫一扫，检查愿望
        /// </summary>
        /// <param name="jObject"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CheckWish([FromBody] JObject jObject)
        {
            var memberId = this.GetMemberId();
            if (memberId == 0)
            {
                return BadRequest("未获取会员id");
            }
            if (jObject == null || jObject["mchNo"] == null)
            {
                return BadRequest("参数错误");
            }
            var (resCode, wishId) = await _activityApplication.GetCheckoutWishAsync(this.GetActivityNo(), memberId, jObject["mchNo"].ToString());
            return this.MyOK(new { resCode, wishId });
        }
    }
}
