using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{

    public enum MpTemplateType
    {
        /// <summary>
        /// 团课预约成功
        /// </summary>
        Reservation = 1,
        /// <summary>
        /// 上课提醒
        /// </summary>
        AttendanceToGroupClass = 2,
        /// <summary>
        /// 团课核销
        /// </summary>
        WriteOffGroupClass = 3,
        /// <summary>
        /// 预约退款
        /// </summary>
        RefundOfReserve = 4,
        /// <summary>
        /// 团课爽约
        /// </summary>
        AbsentCroupClass = 5,
        /// <summary>
        /// 购买卡
        /// </summary>
        BuyCard = 6,
        /// <summary>
        /// 续费团课卡-库中已删除
        /// </summary>
        RenewalGroupCard = 7,
        /// <summary>
        /// 卡暂停
        /// </summary>
        BreakCard = 8,
        /// <summary>
        /// 卡恢复通知
        /// </summary>
        RecoverCard = 9,
        /// <summary>
        /// 团课卡转卡-2020-06-09 删除 shortid:OPENTM410181983-办理成功通知
        /// </summary>
        TradeGroupCard = 10,
        /// <summary>
        /// 卡添加成员
        /// </summary>
        AddMemberOfCard = 11,
        /// <summary>
        /// 课表更新通知
        /// </summary>
        /// <remark>
        /// 2020-02-19：废弃CardExpire shortid: OPENTM411453501
        /// 2020-03-04：shortid由OPENTM411453501变更为OPENTM207225015，名称由CardExpire变更为ScheduleUpdate
        /// </remark>
        ScheduleUpdate = 12,
        /// <summary>
        /// 上课提醒-员工
        /// </summary>
        AttendanceToStaff = 13,
        /// <summary>
        /// 拼团成功
        /// </summary>
        GroupBuySuccess = 14,
        /// <summary>
        /// 拼团失败
        /// </summary>
        GroupBuyfailure = 15,
        /// <summary>
        /// 私教上课提醒-教练
        /// </summary>
        PrivateAttendanceToStaff = 16,
        /// <summary>
        /// 余额充值
        /// </summary>
        BalanceCharge = 17,
        /// <summary>
        /// 余额变动
        /// </summary>
        BalanceModify = 18,
        /// <summary>
        /// 用卡提醒
        /// </summary>
        CardApply = 19
    }
    public class TemplateMessageData
    {
        public string we_appid { get; set; }
        //public string mp_appid { get; set; }
        public string openId { get; set; }
        /// <summary>
        /// t_merchant_template.template_id
        /// </summary>
        public string templatid { get; set; }
        public MPTemplateMessage message { get; set; }
        public string path { get; set; }
        public TemplateMessageData()
        {
            //默认首页
            path = "pages/index";
        }
    }

    public class MPTemplateMessage
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }
        public MPTemplateMessage(string f, string k1, string k2, string k3, string k4, string k5, string r)
        {
            first = new TemplateDataItem(f);
            keyword1 = new TemplateDataItem(k1);
            keyword2 = new TemplateDataItem(k2);
            keyword3 = new TemplateDataItem(k3);
            keyword4 = new TemplateDataItem(k4);
            keyword5 = new TemplateDataItem(k5);
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
}
