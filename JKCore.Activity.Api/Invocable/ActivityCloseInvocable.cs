using Activity.Interfaces.Application.Model;
using Coravel.Invocable;
using JKCore.Activity.Api.Common;
using JKCore.Domain.IRepository;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Invocable
{
    public class ActivityCloseInvocable : IInvocable
    {
        private readonly IJiakeRepository<object> _jiakeRepository;
        private readonly CronJobParam _cronJobParam;

        public ActivityCloseInvocable(IJiakeRepository<object> jiakeRepository, CronJobParam cronJobParam)
        {
            _jiakeRepository = jiakeRepository;
            _cronJobParam = cronJobParam;
        }

        public async Task Invoke()
        {
            string activityNo = CronJobParam.ActivityNo;
            var (startTime, endTime) = await _cronJobParam.GetActivity();

            if (endTime.Date == DateTime.Now.Date) // 活动结束
            {
                string strSql = $@"UPDATE t_make_wish
                SET `status`=3,updated_time=NOW() WHERE activity_no = @activityNo AND `status`=0;";
                await _jiakeRepository.SQLExecute(strSql, new { activityNo });
            }

            if (startTime.Date == DateTime.Now.Date)
            {
                // 活动开始，关注公众号全部用户发消息
                await _cronJobParam.AllSendTemplateMessage();
            }
        }
    }
}
