using Coravel.Invocable;
using JKCore.Domain.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Invocable
{
    public class ActivityFakeInvocable : IInvocable
    {
        private readonly IJiakeRepository<object> _jiakeRepository;
        private readonly CronJobParam _cronJobParam;

        public ActivityFakeInvocable(IJiakeRepository<object> jiakeRepository, CronJobParam cronJobParam)
        {
            _jiakeRepository = jiakeRepository;
            _cronJobParam = cronJobParam;
        }

        // 2分钟
        public async Task Invoke()
        {
            #region 批量

            // 获取开始活动的ids
            // string acSql = $@"SELECT id,start_time FROM t_activity WHERE NOW() BETWEEN start_time AND end_time;";
            // var datas = (await _jiakeRepository.SQLQueryAsync<(int, DateTime)>(acSql)).ToList();

            // // 当前时间
            // var now = DateTime.Now;
            // foreach (var item in datas)
            // {
            //     var start_date = item.Item2;
            //     var fakeUp = GetFakeByStartDatetime(start_date, now);
            // }

            #endregion 批量

            // 活动编号
            // string acSql = $@"SELECT start_time FROM t_activity WHERE NOW() BETWEEN start_time AND end_time AND activity_no = @activity_no;";
            // var start_date = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<DateTime>(acSql, new { activity_no });

            var (start_date, end_date) = await _cronJobParam.GetActivity();

            var now = DateTime.Now;
            var activity_no = CronJobParam.ActivityNo;

            if (now >= start_date && now <= end_date)
            {
                // 计算天数获得应累加的值
                var fakeUp = GetFakeByStartDatetime(start_date, now);

                string sqlStr = $@"UPDATE t_activity SET fake_base = fake_base + @fakeUp WHERE activity_no = @activity_no;";
                var result = await _jiakeRepository.SQLExecute(sqlStr, new { fakeUp, activity_no });
                if (result > 0)
                {
                    // log .. fake 成功
                }
            }
        }

        /// <summary>
        /// 两个时间计算差按规则累计fake值
        /// 1,5-7：1-2,
        //  2-4 : 3-4,
        /// </summary>
        /// <param name="start_date"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        private int GetFakeByStartDatetime(DateTime start_date, DateTime now)
        {
            // 活动开始时间 相差天数
            var dayNum = start_date.Date.Subtract(now.Date);
            var subDay = (dayNum.Days * -1) + 1;

            // 根据天数差区间累计随机数，

            switch (subDay)
            {
                case 1: return new Random().Next(1, 3);
                case 2:
                case 3:
                case 4: return new Random().Next(3, 5);
                case 5:
                case 6:
                case 7: return new Random().Next(1, 3);
            }
            return 0;
        }
    }
}
