using Activity.Interfaces.Application.Model;
using Coravel.Invocable;
using JKCore.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Invocable
{
    public class ActivityNoExchangeInvocable : IInvocable
    {
        private readonly IJiakeRepository<object> _jiakeRepository;
        private readonly CronJobParam _cronJobParam;

        public ActivityNoExchangeInvocable(IJiakeRepository<object> jiakeRepository, CronJobParam cronJobParam)
        {
            _jiakeRepository = jiakeRepository;
            _cronJobParam = cronJobParam;
        }

        public async Task Invoke()
        {
            string activityNo = CronJobParam.ActivityNo;

            string strWhere = string.Empty;
            var (startTime, endTime) = await _cronJobParam.GetActivity();

            //活动结束，未选择商户的不发消息
            if (endTime < DateTime.Now)
            {
                strWhere = " AND mak.mch_no IS NOT NULL ";
            }

            string strSql = $@"SELECT
                    mak.id makeWishId,
                    bas.wish_name wishName,
                    bas.wish_unit wishUnit,
                    mak.user_count userCount,
                    bas.rules rulesStr,
	                mem.openId,
	                mem.nick_name,
                    mak.mch_no
                FROM
	                t_make_wish mak
	                LEFT JOIN t_member mem ON mem.id = mak.member_id
                    LEFT JOIN t_wish_base bas ON bas.wish_no = mak.wish_no
                WHERE
                    mak.activity_no = @activityNo
	                AND mak.exchange_status = 0
	                AND mak.`status` IN ( 1, 4 )
                    {strWhere}
	                AND (
		                DATE(
			                DATE_ADD( mak.complated_time, INTERVAL 1 DAY )) = DATE(
		                NOW())
		                OR DATE(
			                DATE_ADD( mak.complated_time, INTERVAL 3 DAY )) = DATE(
		                NOW()));";
            var memberlist = await _jiakeRepository.SQLQueryAsync<NoExchangeMember>(strSql, new { activityNo });

            foreach (var mem in memberlist)
            {
                if (!string.IsNullOrEmpty(mem.wishUnit) && mem.wishUnit != "--")
                {
                    var r = mem.rules.Where(e => e.n == mem.userCount).FirstOrDefault()?.r;
                    if (r != null)
                    {
                        mem.wishName = string.Concat(mem.wishName, r, mem.wishUnit);
                    }
                }

                await _cronJobParam.NoExchangeSendTemplateMessage(mem.makeWishId, mem.wishName, mem.openId, mem.nickName, mem.mch_no);
            }
        }
    }
}
