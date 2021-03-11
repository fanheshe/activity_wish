using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Activity.Interfaces.Application.Model
{
    public class WishRuleItem
    {
        /// <summary>
        /// 助力人数
        /// </summary>
        public int n { get; set; }

        /// <summary>
        /// ????
        /// </summary>
        public double r { get; set; }
    }

    public class MchListQuery
    {
        /// <summary>
        /// 发起的愿望id
        /// </summary>
        public int wishId { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public double lo { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double la { get; set; }

        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }

    /// <summary>
    /// 商户列表
    /// </summary>
    public class MchItem
    {
        /// <summary>
        /// 商户id
        /// </summary>
        [JsonIgnore]
        public int id { get; set; }
        [JsonIgnore]
        public string tel { get; set; }
        public string mchNo { get; set; }
        public string name { get; set; }
        public string pic { get; set; }
        public bool isShorted { get; set; }
        public int wishComplated { get; set; }

        [JsonIgnore]
        public double shopDistance { get; set; }

        [JsonIgnore]
        public double longitude { get; set; }

        [JsonIgnore]
        public double latitude { get; set; }
    }
    public class MakeWishMchInfo
    {
        public string tel { get; set; }
        public string pic { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public double lo { get; set; }
        public double la { get; set; }
    }
    /// <summary>
    /// 发起的愿望详情
    /// </summary>
    public class ViewWishDetail
    {
        public MakeWishMchInfo mchInfo { get; set; }

        /// <summary>
        /// makeWishId
        /// </summary>
        public int id { get; set; }
        public string pic { get; set; }
        public string unit { get; set; }
        [JsonIgnore]
        public string wishNo { get; set; }
        public string name { get; set; }
        public string remark { get; set; }
        /// <summary>
        /// 会员选择的目标档位（无档位的等于最少助力人数）
        /// </summary>
        public int userCount { get; set; }
        public int gender { get; set; }
        public int boosts { get; set; }
        /// <summary>
        /// 愿望进度：0-进行中 1-已达成 2-未达成
        /// </summary>
        public int process { get; set; }
        /// <summary>
        /// 兑换状态：-1 不可兑现 0-未兑现 1-已兑现
        /// </summary>
        public int receiveStatus { get; set; }
        /// <summary>
        /// 否为愿望的发起人:0-否 1-是
        /// </summary>
        public int isWishMaker { get; set; }
        /// <summary>
        /// 愿望值：用过之后就没有了
        /// </summary>
        public int scores { get; set; }
        [JsonIgnore]
        public int memberId { get; set; }

        /// <summary>
        /// 状态 0-许愿中，1-达成，2-未达成-系统自动取消，3-未达成-活动结束取消,4-达成-提前结束
        /// </summary>
        [JsonIgnore]
        public int status { get; set; }
        /// <summary>
        /// 兑现状态
        /// </summary>
        [JsonIgnore]
        public int exStatus { get; set; }

        /// <summary>
        /// 助力状态--好友查看时有效
        /// </summary>
        public int boostStatus { get; set; }
        public IEnumerable<WishRuleItem> rules { get; set; }

        [JsonIgnore]
        public string ruleStr { get; set; }

        /// <summary>
        /// 设置愿望进度
        /// </summary>
        public void SetProcess()
        {
            if (status == 1 || status == 4)
                process = 1;
            else if (status == 2 || status == 3)
            {
                process = 2;
            }
        }
        /// <summary>
        /// 重置档位规则
        /// </summary>
        public void ResetRule()
        {
            rules = JsonConvert.DeserializeObject<IEnumerable<WishRuleItem>>(ruleStr);
            if (rules == null || rules.Count() == 1)
            {
                return;
            }
            rules = rules.OrderBy(p => p.n);
            var ruleList = new List<WishRuleItem>(3)
            {
                rules.FirstOrDefault()
            };
            if (rules.Count() > 2 && !ruleList.Any(p => p.n == userCount) && rules.Any(p => p.n == userCount))
            {
                ruleList.Add(rules.FirstOrDefault(p => p.n == userCount));
            }
            ruleList.Add(rules.LastOrDefault());
            rules = ruleList;
        }

        /// <summary>
        /// 设置兑现状态
        /// </summary>
        /// <param name="boostCount"></param>
        public void SetReceiveStatus(int boostCount)
        {
            receiveStatus = -1;
            if (status == 1 || status == 4 || (status == 0 && rules != null && rules.Any(p => boostCount >= p.n)))
            {
                receiveStatus = exStatus;
            }
        }
    }
}
