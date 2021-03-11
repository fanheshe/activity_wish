using System;
using System.Collections.Generic;
using System.Text;

namespace Activity.Interfaces.Application.Model
{
    public class MPTemplateID
    {
        /// <summary>
        /// 差2人达成提醒
        /// </summary>
        public const string mpBoost = "w8ONHdJA0GOznqjPz31vIpqLhzDJkZXyuphc30UR84s";

        /// <summary>
        /// 愿望达成提醒
        /// </summary>
        public const string mpArrive = "RoGAtQAW6a6wLxe4_YzNsaqh4N7cVOnZJ-xDXD1Zopg";

        /// <summary>
        /// 愿望达成未兑现提醒
        /// </summary>
        public const string mpNoExchange = "YP0LkQv6nR8j9tPm4u03VVxxBg924NWqDwgXD-PSX3g";

        /// <summary>
        /// 预约愿望提醒
        /// </summary>
        public const string mpReserve = "VUSGT0zvjMo8E8xd6V9MsdgPKdkQKZ1xYuNb4KhmZKQ";

        /// <summary>
        /// 活动开始提醒
        /// </summary>
        public const string mpStart = "yn8VTtskmTZ32XVEBU0K59O9-K7srZck9lFQEA_JR0I";
    }

    public class MPTemplateData
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem remark { get; set; }

        public MPTemplateData(string f, string k1, string k2, string k3, string r)
        {
            first = new TemplateDataItem(f);
            keyword1 = new TemplateDataItem(k1);
            keyword2 = new TemplateDataItem(k2);
            keyword3 = new TemplateDataItem(k3);
            remark = new TemplateDataItem(r);
        }
    }

    public class TemplateDataItem
    {
        /// <summary>
        /// 项目值
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// 16进制颜色代码，如：#FF0000
        /// </summary>
        public string color { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="v">value</param>
        /// <param name="c">color</param>
        public TemplateDataItem(string v, string c = "#173177")
        {
            value = v;
            color = c;
        }
    }

    public class NoExchangeMember
    {
        public int makeWishId { get; set; }
        public string wishName { get; set; }
        public string wishUnit { get; set; }
        public int userCount { get; set; }

        public string rulesStr { get; set; }

        public IEnumerable<WishRuleItem> rules
        {
            get
            {
                return string.IsNullOrEmpty(rulesStr) ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WishRuleItem>>(rulesStr);
            }
        }

        public string openId { get; set; }
        private string _nickName;

        public string nickName
        {
            get { return System.Web.HttpUtility.UrlDecode(_nickName); }
            set { _nickName = value; }
        }

        public string mch_no { get; set; }
    }
}
