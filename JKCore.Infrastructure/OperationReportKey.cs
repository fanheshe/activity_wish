using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    /// <summary>
    /// 运营报表key
    /// </summary>
    public static class OperationReportKey
    {
        /// <summary>
        /// mongo数据库collection
        /// </summary>
        public static class MongoCollection
        {
            /// <summary>
            /// 周类型分布（与WeeklyTypeDistribution类同名）
            /// </summary>
            public const string WeeklyTypeDistribution = "WeeklyTypeDistribution";

            /// <summary>
            /// 渠道占比与转化率 - 月
            /// </summary>
            public const string MonthlyChannelSource = "MonthlyChannelSource";

            /// <summary>
            /// 活跃用户概览 - 月
            /// </summary>
            public const string MonthlyMemberActivity = "MonthlyMemberActivity";
            /// <summary>
            /// 团课卡使用情况
            /// </summary>
            public const string GroupCardUseStatistic = "GroupCardUseStatistic";
            /// <summary>
            /// 私教卡使用情况
            /// </summary>
            public const string PrivateCardUseStatistic = "PrivateCardUseStatistic";
            /// <summary>
            /// 自由训练卡卡使用情况
            /// </summary>
            public const string TrainingCardUseStatistic = "TrainingCardUseStatistic";
            /// <summary>
            /// 日营收数据
            /// </summary>
            public const string DailyIncomeStatistic = "DailyIncomeStatistic";
            /// <summary>
            /// 新增会员数
            /// </summary>
            public const string DailyNewMemberStatistic = "DailyNewMemberStatistic";
            public const string WeeklyNewMemberStatistic = "WeeklyNewMemberStatistic";
            public const string MonthlyNewMemberStatistic = "MonthlyNewMemberStatistic";
        }
        /// <summary>
        /// redis key
        /// </summary>
        public static class RedisKey
        {
            /// <summary>
            /// 门店总访问数
            /// </summary>
            public const string TotalVisitor = "TotalVisitor";
            /// <summary>
            /// 门店总访问数-周
            /// </summary>
            public const string WeeklyTotalVisitor = "WeeklyTotalVisitor";
            /// <summary>
            /// 未注册访问用户-周
            /// </summary>
            public const string WeeklyNotRegistered = "WeeklyNotRegistered";
            /// <summary>
            /// 新注册用户-周
            /// </summary>
            public const string WeeklyNewMember = "WeeklyNewMember";
            /// <summary>
            /// 活跃用户概览 - 月
            /// </summary>
            public const string MonthlyMemberActivity = "MonthlyMemberActivity";
        }
    }
}
