using Coravel.Invocable;
using JKCore.Activity.Api.Common;
using JKCore.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Invocable
{
    public class ActivityMchWishInvocable : IInvocable
    {
        private readonly IJiakeRepository<object> _jiakeRepository;
        private readonly CronJobParam _cronJobParam;

        public ActivityMchWishInvocable(IJiakeRepository<object> jiakeRepository, CronJobParam cronJobParam)
        {
            _jiakeRepository = jiakeRepository;
            _cronJobParam = cronJobParam;
        }

        public async Task Invoke()
        {
            string activityNo = CronJobParam.ActivityNo;
            var (startTime, endTime) = await _cronJobParam.GetActivity();

            if (startTime.AddDays(2).Date == DateTime.Now.Date) // 活动第三天上午十点
            {
                string strSql = $@"UPDATE t_merchant
                SET fake_wish_complated=fake_wish_complated+152 WHERE activity_no=@activityNo;";

                await _jiakeRepository.SQLExecute(strSql, new { activityNo });
            }
        }
    }
}
