using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Activity.Interfaces.Application.Model
{
    public class LoginMemberResponse
    {
        public int memberId { get; set; }

        /// <summary>
        /// 性别：1-男 2-女
        /// </summary>
        public int gender { get; set; } = 0;

        /// <summary>
        /// 是否授权 0未授权 1已授权
        /// </summary>
        public int isBind { get; set; } = 0;

        /// <summary>
        /// 昵称
        /// </summary>
        public string nickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string avatarUrl { get; set; }

        /// <summary>
        ///
        /// </summary>
        //public LoginActivityResponse activity { get; set; }
    }

    public class ActivityResponse
    {
        public int activityId { get; set; }

        /// <summary>
        /// 活动编号
        /// </summary>
        public string activityNo { get; set; }

        /// <summary>
        /// 活动名称
        /// </summary>
        public string activityName { get; set; }

        /// <summary>
        /// 活动状态：0-未开始 1-进行中 2-已结束
        /// </summary>
        public int status
        {
            get
            {
                var now = DateTime.Now;
                if (now < startTime)
                {
                    return 0;
                }
                else if (now >= startTime && now <= endTime)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public DateTime startTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime endTime { get; set; }

        /// <summary>
        /// 服务器时间
        /// </summary>
        public DateTime serverTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 开启愿望人数
        /// </summary>
        public int fakeBase { get; set; }
    }

    /// <summary>
    /// 绑定手机号入参
    /// </summary>
    public class BindTelRequestParam
    {
        public string code { get; set; }
        /// <summary>
        /// 微信获取encryptedData
        /// </summary>
        public string encryptedData { get; set; }

        /// <summary>
        /// 微信获取iv
        /// </summary>
        public string iv { get; set; }
    }

    /// <summary>
    /// 绑定信息入参
    /// </summary>
    public class BindInfoRequestParam
    {
        /// <summary>
        /// 头像
        /// </summary>
        public string avatarUrl { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string nickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public int gender { get; set; }

        /// <summary>
        /// 国家
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        public string province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string city { get; set; }
    }

    #region 我的首页

    //public class MyWishListResponse
    //{
    //    public List<MyWishResponse> dataList { get; set; }
    //    /// <summary>
    //    /// 愿望值
    //    /// </summary>
    //    public int scores { get; set; }
    //    /// <summary>
    //    /// 许愿人数
    //    /// </summary>
    //    public int wishMakers { get; set; }
    //}
    public class MyWishResponse
    {
        /// <summary>
        /// 发起的愿望id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 愿望名称
        /// </summary>
        public string name { get; set; }

        public string mchNo { get; set; }

        /// <summary>
        /// 愿望图片
        /// </summary>
        public string pic { get; set; }

        /// <summary>
        /// 愿望单位
        /// </summary>
        [JsonIgnore]
        public string wishUnit { get; set; }

        /// <summary>
        /// 愿望规则
        /// </summary>
        [JsonIgnore]
        public string rules { get; set; }

        /// <summary>
        /// 解析愿望规则
        /// </summary>
        [JsonIgnore]
        public List<WishRuleItem> ruleItems
        {
            get
            {
                if (string.IsNullOrEmpty(rules))
                {
                    return new List<WishRuleItem>();
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<WishRuleItem>>(rules);
                }
            }
        }

        /// <summary>
        /// 愿望状态 0-许愿中，1-达成，2-未达成-系统自动取消，3-未达成-活动结束取消,4-达成-提前结束
        /// 未达成的不返回
        /// </summary>
        [JsonIgnore]
        public int status { get; set; }

        /// <summary>
        /// 0-进行中 1-已达成
        /// </summary>
        public int process
        {
            get
            {
                if (status == 4)
                {
                    return 1;
                }
                return status;
            }
        }

        /// <summary>
        /// 所需人数
        /// </summary>
        public int needed { get; set; }

        /// <summary>
        /// 目标
        /// </summary>
        public string target { get; set; }

        /// <summary>
        /// 是否使用助力值 1-是（助力成功，并使用过），0-否
        /// </summary>
        public bool isPower { get; set; }

        /// <summary>
        /// 当前助力人数
        /// </summary>
        public int helpers { get; set; }

        /// <summary>
        /// 多档位的时候用户选择的目标档位
        /// </summary>
        [JsonIgnore]
        public int userCount { get; set; }
    }

    #region 选择愿望接口对象

    /// <summary>
    /// 选择愿望请求对象
    /// </summary>
    public class WishListRequestParam
    {
        /// <summary>
        ///  1-男 2-女
        /// </summary>
        public int gender { get; set; }

        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }

    /// <summary>
    /// 选择愿望响应对象
    /// </summary>
    public class WishListResponse
    {
        /// <summary>
        /// 愿望编号
        /// </summary>
        public string wishNo { get; set; }

        /// <summary>
        /// 愿望名称
        /// </summary>
        public string name { get; set; }
    }

    #endregion 选择愿望接口对象

    #region 愿望详情

    public class WishDetailResponse
    {
        /// <summary>
        /// 发起的愿望id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 愿望名称
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 愿望图片
        /// </summary>
        public string pic { get; set; }

        /// <summary>
        /// 愿望单位
        /// </summary>
        public string unit { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string remark { get; set; }

        /// <summary>
        /// 许愿人数
        /// </summary>
        public int wishMakers { get; set; }

        /// <summary>
        /// 愿望规则
        /// </summary>
        [JsonIgnore]
        public string ruleStr { get; set; }

        /// <summary>
        /// 解析愿望规则
        /// </summary>
        public List<WishRuleItem> rules
        {
            get
            {
                if (string.IsNullOrEmpty(ruleStr))
                {
                    return new List<WishRuleItem>();
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<List<WishRuleItem>>(ruleStr);
                }
            }
        }
    }

    #endregion 愿望详情

    #endregion 我的首页
}
