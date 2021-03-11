using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Activity.Interfaces.Application;
using Activity.Interfaces.Application.Model;
using JKCore.Domain.IData;
using JKCore.Domain.IRepository;
using System.Linq;
using Dapper;

namespace Activity.Application
{
    public class OperatApplication : IOperatApplication
    {
        private readonly IJiakeRepository<object> _jiakeRepository;

        public OperatApplication(IJiakeRepository<object> jiakeRepository)
        {
            _jiakeRepository = jiakeRepository;
        }

        /// <summary>
        /// 愿望列表
        /// </summary>
        /// <param name="wishName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<WishInfo>, int)> WishList(string activityNo, string wishName, int pageIndex, int pageSize)
        {
            int newPageIndex = (pageIndex - 1) * pageSize;
            string strWhere = string.Empty;
            if (!string.IsNullOrEmpty(wishName))
            {
                strWhere += " AND bas.wish_name LIKE CONCAT('%',@wishName,'%')";
            }
            string strSql = $@"SELECT
	                bas.wish_no wishNo,
	                bas.wish_pic wishPic,
                    bas.wish_pic2 wishPic2,
	                bas.wish_name wishName,
	                bas.wish_unit wishUnit,
	                bas.for_people forPeople,
	                bas.is_online isOnline,
	                COUNT(DISTINCT wis.mch_no) totalMch
                FROM
	                t_wish_base bas
                    LEFT JOIN t_wish wis on wis.wish_no = bas.wish_no AND wis.is_deleted = 0
                WHERE bas.activity_no = @activityNo
                    {strWhere}
                GROUP BY bas.wish_no,
	                bas.wish_pic,
                    bas.wish_pic2,
	                bas.wish_name,
	                bas.wish_unit,
	                bas.for_people,
	                bas.is_online,
                    bas.created_time,
					bas.wish_no
                ORDER BY
	                bas.wish_no ASC
                LIMIT {newPageIndex},{pageSize};
                SELECT
	                COUNT(DISTINCT bas.wish_no)
                FROM
	                t_wish_base bas
                    LEFT JOIN t_wish wis on wis.wish_no = bas.wish_no AND wis.is_deleted = 0
                WHERE bas.activity_no = @activityNo
                    {strWhere};";
            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<WishInfo>(strSql, new { activityNo, wishName });

            return (dataList, total);
        }

        /// <summary>
        /// 愿望上下架
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="isOnline"></param>
        /// <returns></returns>
        public async Task<int> ChangeStatus(string activityNo, string wishNo, int isOnline)
        {
            string strSql = $@"UPDATE t_wish_base
                SET is_online={ isOnline } WHERE activity_no=@activityNo AND wish_no=@wishNo AND is_online<> { isOnline };";

            return await _jiakeRepository.SQLExecute(strSql, new { activityNo, wishNo });
        }

        /// <summary>
        /// 商户列表
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<MchInfo>, int)> MchList(string activityNo, string wishNo, int pageIndex, int pageSize)
        {
            int newPageIndex = (pageIndex - 1) * pageSize;
            string strWhere = string.Empty;
            string strParam = string.Empty;
            if (!string.IsNullOrEmpty(wishNo))
            {
                strParam += ",wis.wish_total wishTotal,wis.wish_stock wishStock,COUNT(mak.id) wishExchange";
                strWhere += " AND wis.wish_no = @wishNO";
            }

            string strSql = $@"SELECT DISTINCT
	                mch.mch_no mchNo,
	                mch.mch_name mchName,
	                mch.mch_tel mchTel,
	                mch.mch_address mchAddress
                    {strParam}
                FROM
	                t_wish wis
	                LEFT JOIN t_merchant mch ON mch.mch_no = wis.mch_no
                    LEFT JOIN t_make_wish mak ON mak.exchange_status=1 AND mak.mch_no=wis.mch_no AND mak.wish_no=wis.wish_no
                WHERE
                    wis.activity_no=@activityNo
                    AND wis.is_deleted = 0
                    {strWhere}
                GROUP BY mch.mch_no,
	                mch.mch_name,
	                mch.mch_tel,
	                mch.mch_address,
	                wis.wish_total,
	                wis.wish_stock
                ORDER BY
	                mch.mch_no ASC
	                LIMIT {newPageIndex},{pageSize};
                SELECT
	                COUNT(DISTINCT mch.mch_no)
                FROM
	                t_wish wis
                    LEFT JOIN t_merchant mch ON mch.mch_no = wis.mch_no
                    LEFT JOIN t_make_wish mak ON mak.exchange_status=1 AND mak.mch_no=wis.mch_no AND mak.wish_no=wis.wish_no
                WHERE
                    wis.activity_no=@activityNo
                    AND wis.is_deleted = 0
	                {strWhere};";

            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<MchInfo>(strSql, new { activityNo, wishNo });

            return (dataList, total);
        }

        /// <summary>
        /// 添加商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <param name="wishTotal"></param>
        /// <returns></returns>
        public async Task<(int, string)> AddMch(string activityNo, string wishNo, string mchNo, int wishTotal)
        {
            string strSql = $@"SELECT mch_name FROM t_merchant WHERE activity_no=@activityNo AND mch_no=@mchNo;";
            string mchName = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<string>(strSql, new { activityNo, mchNo });

            if (string.IsNullOrEmpty(mchName))
            {
                return (0, "商户不存在！");
            }

            strSql = $@"SELECT id FROM t_wish WHERE activity_no=@activityNo AND wish_no=@wishNo AND mch_no=@mchNo AND is_deleted=0;";
            int wishId = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql, new { activityNo, wishNo, mchNo });

            if (wishId > 0)
            {
                return (0, "商户已存在！"); // 已经存在
            }
            else
            {
                strSql = $@"SELECT wish_name,wish_type,rules FROM t_wish_base WHERE activity_no=@activityNo AND wish_no=@wishNo;";
                var (wishName, isDiscount, rulesStr) = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<(string, int, string)>(strSql, new { activityNo, wishNo });

                var rules = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WishRuleItem>>(rulesStr);
                int minUserCount = minUserCount = rules.FirstOrDefault().n;
                int maxUserCount = 0;
                if (rules.Count() > 1)
                {
                    maxUserCount = rules.LastOrDefault().n;
                }
                else
                {
                    maxUserCount = rules.FirstOrDefault().n;
                }

                strSql = $@"INSERT INTO t_wish
                    (activity_no,mch_no,mch_name,wish_no,wish_name,wish_total,wish_stock,is_discount,min_user_count,max_user_count,created_time,is_deleted)
                VALUES
                    (@activityNo,@mchNo,@mchName,@wishNo,@wishName,@wishTotal,@wishTotal,@isDiscount,@minUserCount,@maxUserCount,NOW(),0);";

                int resCode = await _jiakeRepository.SQLExecute(strSql, new { activityNo, mchNo, mchName, wishNo, wishName, wishTotal, isDiscount, minUserCount, maxUserCount });

                return (resCode, "");
            }
        }

        /// <summary>
        /// 删除商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        public async Task<int> DeletedMch(string activityNo, string wishNo, string mchNo)
        {
            string strSql = $@"UPDATE t_wish SET is_deleted=1 WHERE activity_no=@activityNo AND wish_no=@wishNo AND mch_no=@mchNo AND is_deleted=0;";

            return await _jiakeRepository.SQLExecute(strSql, new { activityNo, wishNo, mchNo });
        }

        /// <summary>
        /// 许愿列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<MakeWishInfo>, int)> MakeWishList(string activityNo, RequestMakeWishInfo request)
        {
            int newPageIndex = (request.pageIndex - 1) * request.pageSize;
            string strWhere = string.Empty;

            if (!string.IsNullOrEmpty(request.wishName))
            {
                strWhere += " AND bas.wish_name LIKE CONCAT('%',@wishName,'%')";
            }
            if (!string.IsNullOrEmpty(request.mchName))
            {
                strWhere += " AND mch.mch_name LIKE CONCAT('%',@mchName,'%')";
            }
            if (!string.IsNullOrEmpty(request.tel))
            {
                strWhere += " AND mem.tel = @tel";
            }
            if (request.wishType.HasValue)
            {
                strWhere += " AND bas.wish_type = @wishType";
            }
            if (request.status.HasValue)
            {
                strWhere += request.status == 1 ? " AND mak.`status` in (1,4)" : " AND mak.`status` = @status";
            }
            if (request.exchangeStatus.HasValue)
            {
                strWhere += request.exchangeStatus == 3 ? " AND mak.mch_no is NULL" : " AND mak.exchange_status = @exchangeStatus";
            }
            if (request.startTime.HasValue && request.endTime.HasValue)
            {
                request.endTime = request.endTime.Value.AddDays(1).AddSeconds(-1);
                strWhere += " AND mak.created_time BETWEEN @startTime AND @endTime";
            }
            if (request.upStartTime.HasValue && request.upEndTime.HasValue)
            {
                request.upEndTime = request.upEndTime.Value.AddDays(1).AddSeconds(-1);
                strWhere += " AND mak.updated_time BETWEEN @upStartTime AND @upEndTime";
            }
            if (request.exStartTime.HasValue && request.exEndTime.HasValue)
            {
                request.exEndTime = request.exEndTime.Value.AddDays(1).AddSeconds(-1);
                strWhere += " AND mak.exchange_time BETWEEN @exStartTime AND @exEndTime";
            }
            if (request.wishId > 0)
            {
                strWhere += " AND mak.id=@wishId";
            }

            string strSql = $@"SELECT
	                bas.wish_name wishName,
	                mem.nick_name nickName,
	                mem.tel,
	                mak.created_time createdTime,
	                bas.wish_type wishType,
	                bas.rules rulesStr,
                    bas.wish_unit wishUnit,
	                mak.`status`,
	                mak.updated_time updatedTime,
	                mch.mch_name mchName,
	                IF(mak.mch_no is NULL,NULL,mak.exchange_status) exchangeStatus,
	                mak.exchange_time exchangeTime,
	                mak.`code`,
	                COUNT( hel.id ) helpTotal,
                    mak.id wishId
                FROM
	                t_make_wish mak
	                LEFT JOIN t_wish_base bas ON bas.wish_no = mak.wish_no
	                LEFT JOIN t_member mem ON mem.id = mak.member_id
	                LEFT JOIN t_merchant mch ON mch.mch_no = mak.mch_no
	                LEFT JOIN t_help_wish hel ON hel.make_wish_id = mak.id
                WHERE
	                mak.activity_no = @activityNo
                    {strWhere}
                GROUP BY
	                bas.wish_name,
	                mem.nick_name,
	                mem.tel,
	                mak.created_time,
	                bas.wish_type,
	                bas.rules,
                    bas.wish_unit,
	                mak.`status`,
	                mak.updated_time,
	                mch.mch_name,
	                mak.exchange_status,
	                mak.exchange_time,
	                mak.`code`,
                    mak.id
                ORDER BY
	                mak.created_time ASC,
	                mak.id ASC
                LIMIT {newPageIndex},{request.pageSize};
                SELECT
	                COUNT( DISTINCT mak.id )
                FROM
	                t_make_wish mak
                    LEFT JOIN t_wish_base bas ON bas.wish_no = mak.wish_no
	                LEFT JOIN t_member mem ON mem.id = mak.member_id
	                LEFT JOIN t_merchant mch ON mch.mch_no = mak.mch_no
	                LEFT JOIN t_help_wish hel ON hel.make_wish_id = mak.id
                WHERE
	                mak.activity_no = @activityNO
                    {strWhere};";

            var param = new DynamicParameters();
            param.Add("activityNo", activityNo);
            param.AddDynamicParams(request);
            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<MakeWishInfo>(strSql, param);

            return (dataList, total);
        }

        /// <summary>
        /// 好友助力列表
        /// </summary>
        /// <param name="wishId"></param>
        /// <param name="activityNo"></param>
        /// <param name="tel"></param>
        /// <param name="inviteTel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<HelpInfo>, int)> HelpList(int wishId, string activityNo, string tel, string inviteTel, int pageIndex, int pageSize)
        {
            int newPageIndex = (pageIndex - 1) * pageSize;
            string strWhere = string.Empty;

            if (!string.IsNullOrEmpty(tel))
            {
                strWhere += " AND mem.tel=@tel";
            }
            if (!string.IsNullOrEmpty(inviteTel))
            {
                strWhere += " AND inv.tel=@inviteTel";
            }
            if (wishId > 0)
            {
                strWhere += " AND hel.make_wish_id=@wishId";
            }

            string strSql = $@"SELECT
	                mem.nick_name nickName,
	                mem.tel,
	                mem.created_time createdTime,
	                inv.nick_name inviteNickName,
	                inv.tel inviteTel,
                    IF ( ismak.id > 0, 1, 0 ) isPower,
                    ismak.id wishId
                FROM
	                t_help_wish hel
	                LEFT JOIN t_member mem ON mem.id = hel.member_id
	                LEFT JOIN t_make_wish mak ON mak.id = hel.make_wish_id
	                LEFT JOIN t_member inv ON inv.id = mak.member_id
	                LEFT JOIN t_make_wish ismak ON ismak.member_id = hel.member_id
	                AND ismak.is_power = 1
                WHERE
	                hel.activity_no = @activityNo
                    {strWhere}
                ORDER BY
	                mem.created_time DESC,
	                mem.id DESC
                LIMIT {newPageIndex},{pageSize};
                SELECT
	                COUNT(DISTINCT hel.id)
                FROM
	                t_help_wish hel
                    LEFT JOIN t_member mem ON mem.id = hel.member_id
	                LEFT JOIN t_make_wish mak ON mak.id = hel.make_wish_id
	                LEFT JOIN t_member inv ON inv.id = mak.member_id
	                LEFT JOIN t_make_wish ismak ON ismak.member_id = hel.member_id
	                AND ismak.is_power = 1
                WHERE
	                hel.activity_no = @activityNo
                    {strWhere};";

            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<HelpInfo>(strSql, new { wishId, activityNo, tel, inviteTel });

            return (dataList, total);
        }

        /// <summary>
        /// 许愿反馈列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<ProblemInfo>, int)> OtherWishList(string activityNo, string content, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            int newPageIndex = (pageIndex - 1) * pageSize;
            string strWhere = string.Empty;

            if (!string.IsNullOrEmpty(content))
            {
                strWhere += " AND oth.wish_content LIKE CONCAT('%',@content,'%')";
            }
            if (startTime.HasValue && endTime.HasValue)
            {
                endTime = endTime.Value.AddDays(1).AddSeconds(-1);
                strWhere += " AND oth.created_time BETWEEN @startTime AND @endTime";
            }

            string strSql = $@"SELECT
	                mem.nick_name nickName,
	                mem.tel,
	                oth.created_time createdTime,
	                oth.wish_content content
                FROM
	                t_other_wish oth
	                LEFT JOIN t_member mem ON mem.id = oth.member_id
                WHERE
	                oth.activity_no = @activityNo
                    {strWhere}
                ORDER BY
	                oth.created_time ASC,
	                oth.id ASC
                LIMIT {newPageIndex},{pageSize};
                SELECT
	                COUNT(DISTINCT oth.id)
                FROM
	                t_other_wish oth
                    LEFT JOIN t_member mem ON mem.id = oth.member_id
                WHERE
	                oth.activity_no = @activityNo
                    {strWhere};";

            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<ProblemInfo>(strSql, new { activityNo, content, startTime, endTime });

            return (dataList, total);
        }

        /// <summary>
        /// 客服问题列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<ProblemInfo>, int)> ProblemList(string activityNo, string content, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            int newPageIndex = (pageIndex - 1) * pageSize;
            string strWhere = string.Empty;

            if (!string.IsNullOrEmpty(content))
            {
                strWhere += " AND pro.content LIKE CONCAT('%',@content,'%')";
            }
            if (startTime.HasValue && endTime.HasValue)
            {
                endTime = endTime.Value.AddDays(1).AddSeconds(-1);
                strWhere += " AND pro.created_time BETWEEN @startTime AND @endTime";
            }

            string strSql = $@"SELECT
	                mem.nick_name nickName,
	                mem.tel,
	                pro.created_time createdTime,
	                pro.content
                FROM
	                t_problem pro
	                LEFT JOIN t_member mem ON mem.id = pro.member_id
                WHERE
	                pro.activity_no = @activityNo
                    {strWhere}
                ORDER BY
	                pro.created_time ASC,
	                pro.id ASC
                LIMIT {newPageIndex},{pageSize};
                SELECT
	                COUNT(DISTINCT pro.id)
                FROM
	                t_problem pro
                    LEFT JOIN t_member mem ON mem.id = pro.member_id
                WHERE
	                pro.activity_no = @activityNo
                    {strWhere};";

            var (dataList, total) = await _jiakeRepository.SQLQueryWithReturnAsync<ProblemInfo>(strSql, new { activityNo, content, startTime, endTime });

            return (dataList, total);
        }
    }
}
