
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Activity.Interfaces.Application.Model;
using Senparc.Weixin.TenPay.V3;

namespace Activity.Interfaces.Application
{
    /// <summary>
    /// 活动接口
    /// </summary>
    public interface IActivityApplication
    {
        /*****************************************分隔符*************************************************/

        /// <summary>
        /// 获取会员信息
        /// 如果存在直接取回，如果不存在就创建
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="seeionKey"></param>
        /// <returns></returns>
        Task<LoginMemberResponse> GetMemberByOpenIdAsync(string activityNo, string openId, string sessionKey);

        /// <summary>
        /// 获取session_key
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<string> GetSessionKeyAsync(int memberId);
        /// <summary>
        /// 更新session_key
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task UpdateSessionKeyAsync(int memberId, string sessionKey);

        /// <summary>
        /// 获取活动信息（全）
        /// </summary>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        Task<ActivityResponse> GetActivityResponseAsync(string activityNo);

        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        Task<(int, string)> BindTelAsync(int memberId, string tel);

        /// <summary>
        /// 授权头像昵称等
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        Task<int> BindInfoAsync(int memberId, BindInfoRequestParam requestParam);

        /// <summary>
        /// 我的首页
        /// 我的愿望列表未达成的愿望不返回
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<IEnumerable<MyWishResponse>> GetMyWishListAsync(string activityNo, int memberId);

        ///// <summary>
        ///// 获取会员性别
        ///// </summary>
        ///// <param name="activityNo"></param>
        ///// <param name="memberId"></param>
        ///// <returns></returns>
        //Task<int> GetMemberGenderAsync(int memberId);
        /// <summary>
        /// 选择愿望列表
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="gender">0 不限 1 男 2 女</param>
        /// <returns></returns>
        Task<(IEnumerable<WishListResponse>, int)> GetWishListAsync(string activityNo, int memberId, WishListRequestParam request);

        /// <summary>
        /// 选择愿望时的其他愿望显示不显示，规则是提交过一次其他愿望就不再显示
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <returns>是否提交过其他愿望 1提交过 0未提交过</returns>
        Task<int> IsHaveOtherWishAsync(string activityNo, int memberId);

        /// <summary>
        /// 愿望收集
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<(int, string)> SubmitWishAsync(string activityNo, int memberId, string content);

        /// <summary>
        /// 愿望详情
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        Task<WishDetailResponse> GetWishDetailAsync(string activityNo, string wishNo, int gender);

        /*****************************************分隔符*************************************************/

        /// <summary>
        /// 获取愿望助力人数
        /// </summary>
        /// <param name="WishId"></param>
        /// <returns></returns>
        Task<int> GetBoostCountByWishIdAsync(int WishId);

        /// <summary>
        /// 问题反馈
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="activityNo"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<int> SaveProblemAsync(int memberId, string activityNo, string content);

        /// <summary>
        /// 兑现愿望--必须是达成的愿望才能兑现
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        Task<(int, int, string)> TakeWishAsync(int memberId, string mchNo);

        /// <summary>
        /// 获取愿望档位规则
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>

        Task<WishRuleItem[]> GetWishRulesByWishNoAsync(string wishNo);

        /// <summary>
        /// 达成愿望
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        Task<(int, string, int)> ComplateWishAsync(int memberId, int makeWishId, string mchNo);

        /// <summary>
        /// 门店列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<(IEnumerable<MchItem>, int)> GetMchListAsync(MchListQuery query);

        /// <summary>
        /// 助力列表
        /// </summary>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetBoostListAsync(int makeWishId);

        /// <summary>
        /// 获取会员愿望值
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<int> GetWishValueAsync(int memberId);

        /// <summary>
        /// 获取愿望许愿人数
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<int> GetWishMakersAsync(string wishNo);

        /// <summary>
        /// 查看愿望
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        Task<ViewWishDetail> GetMakeWishAsync(int memberId, int makeWishId);

        /// <summary>
        /// 获取会员助力次数
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<int> GetMemberBoostCountAsync(int memberId);

        /// <summary>
        /// 帮TA助力
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        Task<(int, string, int, int)> BoostWishAsync(string activityNo, int memberId, int makeWishId);

        /// <summary>
        /// 获取活动时间
        /// </summary>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        Task<DateTime?> GetOverTimeAsync(string activityNo);

        /// <summary>
        /// 许愿
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="wishNo"></param>
        /// <param name="userCount"></param>
        /// <param name="activityNo"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        Task<(int, string)> MakeWishAsync(int memberId, string wishNo, int userCount, string activityNo, int gender);

        /// <summary>
        /// 愿望加速
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        Task<(int, int, int)> AccelerateWishAsync(int memberId, int makeWishId);

        /// <summary>
        /// 愿望是否存在
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        Task<bool> IsExistsAsync(string activityNo, string wishNo);

        /// <summary>
        /// 会员愿望状态-登录接口使用
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<(int, int,string)> MemberWishStatusAsync(string activityNo, int memberId);

        /// <summary>
        /// 获取愿望名称，openid,nick_name
        /// </summary>
        /// <param name="WishId"></param>
        /// <returns></returns>
        Task<(MPTemplateData, string)> GetWishOpenId(int makeWishId, string mPTemplateID, int memberId);

        /// <summary>
        /// 预约成功更新发送记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        Task<int> SaveMemberIsSend(int memberId);

        /// <summary>
        /// 1-可以正常核销 2-愿望已达成但未选择门店 3-没有愿望或没有达成的愿望 4-扫码门店与所选门店不符 5-已正常核销（不区分门店）
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        Task<(int, int)> GetCheckoutWishAsync(string activityNo, int memberId, string mchNo);
    }
}
