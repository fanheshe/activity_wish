using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Activity.Interfaces.Application.Model
{
    #region Request

    /// <summary>
    /// 许愿列表，搜索
    /// </summary>
    public class RequestMakeWishInfo
    {
        /// <summary>
        /// 愿望名称
        /// </summary>
        public string wishName { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        public string mchName { get; set; }

        /// <summary>
        /// 许愿人手机号
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        ///  0 不可量化 1 可量化
        /// </summary>
        public int? wishType { get; set; }

        /// <summary>
        /// 0-许愿中，1-达成，2-未达成-系统自动取消，3-未达成-活动结束取消,4-达成-提前结束
        /// </summary>
        public int? status { get; set; }

        /// <summary>
        /// 0-未兑换，1-已兑换，3-未知（null）
        /// </summary>
        public int? exchangeStatus { get; set; }

        /// <summary>
        /// 许愿开始时间
        /// </summary>
        public DateTime? startTime { get; set; }

        /// <summary>
        /// 许愿结束时间
        /// </summary>
        public DateTime? endTime { get; set; }

        /// <summary>
        /// 更新开始时间
        /// </summary>
        public DateTime? upStartTime { get; set; }

        /// <summary>
        /// 更新结束时间
        /// </summary>
        public DateTime? upEndTime { get; set; }

        /// <summary>
        /// 兑现开始时间
        /// </summary>
        public DateTime? exStartTime { get; set; }

        /// <summary>
        /// 兑现结束时间
        /// </summary>
        public DateTime? exEndTime { get; set; }

        public int pageIndex { get; set; } = 1;
        public int pageSize { get; set; } = 10;

        /// <summary>
        /// 许愿人id
        /// </summary>
        public int? wishId { get; set; }
    }

    #endregion Request

    /// <summary>
    /// 愿望信息
    /// </summary>
    public class WishInfo
    {
        /// <summary>
        /// 愿望编码
        /// </summary>
        public string wishNo { get; set; }

        /// <summary>
        /// 愿望名称
        /// </summary>
        public string wishName { get; set; }

        /// <summary>
        /// 愿望图片（男）
        /// </summary>
        public string wishPic { get; set; }

        /// <summary>
        /// 愿望图片（女）
        /// </summary>
        public string wishPic2 { get; set; }

        /// <summary>
        /// 愿望单位
        /// </summary>
        public string wishUnit { get; set; }

        /// <summary>
        /// 适用人群 0 不限 1 男 2 女
        /// </summary>
        public int forPeople { get; set; }

        /// <summary>
        /// 上下架状态 1 上架 0 下架
        /// </summary>
        public int isOnline { get; set; }

        /// <summary>
        /// 参与商户数
        /// </summary>
        public int totalMch { get; set; }
    }

    /// <summary>
    /// 商户信息
    /// </summary>
    public class MchInfo
    {
        /// <summary>
        /// 商户编码
        /// </summary>
        public string mchNo { get; set; }

        /// <summary>
        /// 商户名称
        /// </summary>
        public string mchName { get; set; }

        /// <summary>
        /// 商户电话
        /// </summary>
        public string mchTel { get; set; }

        /// <summary>
        /// 商户地址
        /// </summary>
        public string mchAddress { get; set; }

        /// <summary>
        /// 愿望总数
        /// </summary>
        public int? wishTotal { get; set; }

        /// <summary>
        /// 剩余愿望数
        /// </summary>
        public int? wishStock { get; set; }

        /// <summary>
        /// 兑换愿望数
        /// </summary>
        public int? wishExchange { get; set; }
    }

    /// <summary>
    /// 许愿信息
    /// </summary>
    public class MakeWishInfo
    {
        /// <summary>
        /// 愿望名称
        /// </summary>
        public string wishName { get; set; }

        /// <summary>
        /// 许愿人昵称
        /// </summary>
        private string _nickName;

        public string nickName
        {
            get { return System.Web.HttpUtility.UrlDecode(_nickName); }
            set { _nickName = value; }
        }

        /// <summary>
        /// 许愿人手机号
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        /// 许愿时间
        /// </summary>
        public DateTime createdTime { get; set; }

        /// <summary>
        /// 0 不可量化 1 可量化
        /// </summary>
        public int wishType { get; set; }

        public int wishId { get; set; }

        /// <summary>
        /// 助力人数
        /// </summary>
        public int helpTotal { get; set; }

        [JsonIgnore]
        public string rulesStr { get; set; }

        /// <summary>
        /// 助力规则
        /// </summary>
        public IEnumerable<WishRuleItem> rules
        {
            get
            {
                return string.IsNullOrEmpty(rulesStr) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WishRuleItem>>(rulesStr);
            }
        }

        /// <summary>
        /// 愿望单位
        /// </summary>
        public string wishUnit { get; set; }

        /// <summary>
        /// 0-许愿中，1-达成，2-未达成-系统自动取消，3-未达成-活动结束取消,4-达成-提前结束
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 更新愿望时间
        /// </summary>
        public DateTime? updatedTime { get; set; }

        /// <summary>
        /// 兑换商户名称
        /// </summary>
        public string mchName { get; set; }

        /// <summary>
        /// 0-未兑换，1-已兑换，3-未知（null）
        /// </summary>
        public int? exchangeStatus { get; set; }

        /// <summary>
        /// 兑换时间
        /// </summary>
        public DateTime? exchangeTime { get; set; }

        public string code { get; set; }
    }

    /// <summary>
    /// 好友助力信息
    /// </summary>
    public class HelpInfo
    {
        /// <summary>
        /// 会员昵称
        /// </summary>
        private string _nickName;

        public string nickName
        {
            get { return System.Web.HttpUtility.UrlDecode(_nickName); }
            set { _nickName = value; }
        }

        /// <summary>
        /// 会员手机号
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        /// 会员注册时间
        /// </summary>
        public DateTime createdTime { get; set; }

        /// <summary>
        /// 邀请人昵称
        /// </summary>
        private string _inviteNickName;

        public string inviteNickName
        {
            get { return System.Web.HttpUtility.UrlDecode(_inviteNickName); }
            set { _inviteNickName = value; }
        }

        /// <summary>
        /// 邀请人手机号
        /// </summary>
        public string inviteTel { get; set; }

        /// <summary>
        /// 愿望值是否使用 1-使用，0-未使用
        /// </summary>
        public int isPower { get; set; }

        /// <summary>
        /// 助力的愿望id
        /// </summary>
        public int wishId { get; set; }
    }

    /// <summary>
    /// 反馈信息
    /// </summary>
    public class ProblemInfo
    {
        /// <summary>
        /// 会员昵称
        /// </summary>
        private string _nickName;

        public string nickName
        {
            get { return System.Web.HttpUtility.UrlDecode(_nickName); }
            set { _nickName = value; }
        }

        /// <summary>
        /// 会员手机号
        /// </summary>
        public string tel { get; set; }

        /// <summary>
        /// 许愿时间
        /// </summary>
        public DateTime createdTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }
    }
}
