using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    public class CacheKeyConfig
    {
        /// <summary>
        /// 1小时
        /// </summary>
        public const int Expiration_1 = 3600;
        /// <summary>
        /// 5分钟
        /// </summary>
        public const int Expiration_2 = 300;
        /// <summary>
        /// 20秒
        /// </summary>
        public const int Expiration_3 = 20;
        /// <summary>
        /// 5秒
        /// </summary>
        public const int Expiration_4 = 5;
        /// <summary>
        /// 首页推荐缓存key，已改为内存5秒
        /// </summary>
        //public const string C_Redis_Recommend_Key = "C_Recommended";
        /// <summary>
        /// 团课课程列表
        /// </summary>
        public const string C_Redis_GroupScheduleList_Key = "C_GroupScheduleList";
        /// <summary>
        /// 预约列表-废弃，旧接口废弃，采用新接口了
        /// </summary>
        public const string C_Redis_ReserveList_Key = "C_ReserveList";
        /// <summary>
        /// 历史预约记录-废弃，旧接口废弃，采用新接口了
        /// </summary>
        public const string C_Redis_ReserveHistory_Key = "C_ReserveHistory";
        /// <summary>
        /// 预约列表-分页
        /// </summary>
        public const string C_Redis_ReserveListPage_Key = "C_ReserveListPage";

        /// <summary>
        /// 优质套餐（团课卡列表）
        /// </summary>
        public const string C_Redis_GroupCardList_Key = "C_GroupCardList";
        /// <summary>
        /// 团课卡详情
        /// </summary>
        public const string C_Redis_GroupCardDetail_Key = "C_GroupCardDetail";
        /// <summary>
        /// 帮助中心问题列表
        /// </summary>
        public const string C_Redis_GetQuestionTypeList_Key = "C_GetQuestionTypeList";
        /// <summary>
        /// 我的卡包
        /// </summary>
        public const string C_Redis_GroupCardOfMine_Key = "C_GroupCardOfMine";
        /// <summary>
        /// 我的优惠券
        /// </summary>
        public const string C_Redis_CouponOfMine_Key = "C_CouponOfMine";
        /// <summary>
        /// 个人资料
        /// </summary>
        public const string C_Redis_MemberInfo_Key = "C_MemberInfo";
        /// <summary>
        /// 秒杀活动列表
        /// </summary>
        public const string C_Redis_SeckillList_Key = "C_SeckillList";
        /// <summary>
        /// 秒杀详情
        /// </summary>
        public const string C_Redis_SeckillDetail_Key = "C_SeckillDetail";
        /// <summary>
        /// 拼团活动列表
        /// </summary>
        public const string C_Redis_GroupBuyList_Key = "C_GroupBuyList";
        /// <summary>
        /// 拼团详情
        /// </summary>
        public const string C_Redis_GroupBuyDetail_Key = "C_GroupBuyDetail";

        /// <summary>
        /// 秒杀活动数量key
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string SeckillQuantityKey(DateTime date)
        {
            return $"SeckillQuantity{date.ToString("yyyyMMdd")}";
        }

        /// <summary>
        /// 秒杀活动列表key
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string SeckillCardsKey(DateTime date)
        {
            return $"SeckillCards{date.ToString("yyyyMMdd")}";
        }
        /// <summary>
        /// 私教预约记录-废弃，旧接口废弃，采用新接口了
        /// </summary>
        public const string C_Redis_PrivateReserveList_Key = "C_PrivateReserveList";
        /// <summary>
        /// 私教历史预约-废弃，旧接口废弃，采用新接口了
        /// </summary>
        public const string C_Redis_PrivateReserveHistory_Key = "C_PrivateReserveHistory";
        /// <summary>
        /// 私教预约记录-分页
        /// </summary>
        public const string C_Redis_PrivateReserveListPage_Key = "C_PrivateReserveListPage";
        /// <summary>
        /// 预约时间
        /// </summary>
        public const string Redis_PrivateReserveTime_Key = "PrivateReserveTime";
        /// <summary>
        /// 私教体验课列表
        /// </summary>
        public const string C_Redis_PrivateClassList_Key = "C_PrivateClassList";
        /// <summary>
        /// 私教体验课详情
        /// </summary>
        public const string C_Redis_PrivateClassDetail_Key = "C_PrivateClassDetail";
        /// <summary>
        /// 私教体验课教练列表
        /// </summary>
        public const string C_Redis_PrivateClassCoachList_Key = "C_PrivateClassCoachList";

        /// <summary>
        /// 私教体验课，健身目标对应标签
        /// </summary>
        public const string C_Redis_PrivateClassLabelIds_Key = "C_PrivateClassLabelIds";
        /// <summary>
        /// 私教列表
        /// </summary>
        public const string C_Redis_PrivateCoachList_Key = "C_PrivateCoachList";
        /// <summary>
        /// 私教详情
        /// </summary>
        public const string C_Redis_PrivateCoachDetail_Key = "C_PrivateCoachDetail";

        /// <summary>
        /// 私教卡列表
        /// </summary>
        public const string C_Redis_PrivateCardList_Key = "C_PrivateCardList";

        /// <summary>
        /// 私教卡详情
        /// </summary>
        public const string C_Redis_PrivateCardDetail_Key = "C_PrivateCardDetail";

        /// <summary>
        /// 教学次数
        /// </summary>
        public const string C_Redis_PrivateCoachTeachCount_Key = "C_PrivateCoachTeachCount";

        public const string C_Redis_PrivatePermission_Key = "C_PrivatePermission";
    }
}
