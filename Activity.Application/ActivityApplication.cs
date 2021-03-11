using Activity.Interfaces.Application;
using Activity.Interfaces.Application.Model;
using Dapper;
using JKCore.Domain.IData;
using JKCore.Domain.IRepository;
using JKCore.Infrastructure;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using OfficeOpenXml.Packaging.Ionic.Zip;
using Senparc.Weixin.TenPay;
using Senparc.Weixin.TenPay.V3;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Activity.Application
{
    /// <summary>
    /// 活动接口实现类
    /// </summary>
    public class ActivityApplication : IActivityApplication
    {
        private readonly IJiakeRepository<object> _jiakeRepository;
        private readonly IJiakeUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger<ActivityApplication> _logger;
        private static DateTime? overTime;

        public ActivityApplication(IJiakeRepository<object> jiakeRepository, IJiakeUnitOfWorkFactory unitOfWorkFactory, ILogger<ActivityApplication> logger)
        {
            _logger = logger;
            _jiakeRepository = jiakeRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        /*****************************************分隔符*************************************************/

        /// <summary>
        /// 获取会员信息
        /// 如果存在直接取回，如果不存在就创建
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="seeionKey"></param>
        /// <returns></returns>
        public async Task<LoginMemberResponse> GetMemberByOpenIdAsync(string activityNo, string openId, string sessionKey)
        {
            //查询会员是否存在
            string strSql = $@"SELECT
	                                id AS memberId,
	                                gender,
                                CASE
		                                WHEN LENGTH( tel ) > 0 THEN
		                                1 ELSE 0
	                                END AS isBind,
	                                nick_name AS nickName,avatar_url avatarUrl
                                FROM
	                                t_member
                                WHERE
	                                openId = @openId and is_deleted=0;";
            var member = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<LoginMemberResponse>(strSql, new { openId });
            //会员存在，返回信息
            if (member != null && member.memberId > 0)
            {
                strSql = $@"UPDATE t_member SET session_key=@sessionKey,updated_time = NOW( ) WHERE id={member.memberId};";
                await _jiakeRepository.SQLExecute(strSql, new { sessionKey });
            }
            else //会员不存在，创建会员
            {
                strSql = $@"INSERT INTO t_member (activity_no,openId,session_key,is_deleted,created_time)
                            VALUES (@activityNo,@openId,@sessionKey,0,NOW());
                            SELECT @@IDENTITY;";
                member = new LoginMemberResponse();
                member.memberId = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql, new { activityNo, openId, sessionKey });
            }
            return member;
        }

        /// <summary>
        /// 获取session_key
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<string> GetSessionKeyAsync(int memberId)
        {
            string strSql = $@"SELECT session_key FROM t_member WHERE id={memberId};";
            return await _jiakeRepository.SQLQueryFirstOrDefaultAsync<string>(strSql);
        }

        /// <summary>
        /// 更新session_key
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task UpdateSessionKeyAsync(int memberId, string sessionKey)
        {
            string strSql = $@"UPDATE t_member SET session_key=@sessionKey,updated_time = NOW( ) WHERE id={memberId};";
            await _jiakeRepository.SQLExecute(strSql, new { sessionKey });
        }

        /// <summary>
        /// 获取活动信息（全）
        /// </summary>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        public async Task<ActivityResponse> GetActivityResponseAsync(string activityNo)
        {
            string strSql = $@"SELECT
	                                id AS activityId,
	                                activity_no AS activityNo,
	                                activity_name AS activityName,
	                                start_time AS startTime,
	                                end_time AS endTime,
	                                fake_base AS fakeBase
                                FROM
	                                t_activity
                                WHERE
	                                activity_no = @activityNo;";

            var data = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<ActivityResponse>(strSql, new { activityNo });
            data.fakeBase += await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>("select count(*) from t_make_wish where member_id>0");
            return data;
        }

        /// <summary>
        /// 绑定手机号
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="tel"></param>
        /// <returns></returns>
        public async Task<(int, string)> BindTelAsync(int memberId, string tel)
        {
            string strSql = $@"SELECT id,tel FROM t_member WHERE id = {memberId} and is_deleted=0;";
            var member = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<dynamic>(strSql);
            if (member == null)
            {
                return (0, "会员不存在~");
            }
            if (!string.IsNullOrEmpty(member.tel))
            {
                return (0, "该会员已已绑定手机号~");
            }
            //手机号验重
            strSql = $@"SELECT Count(*) FROM t_member WHERE id<>{memberId} AND tel=@tel;";
            var exist = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql, new { tel });
            if (exist > 0)
            {
                return (0, "该手机号已被其他会员绑定，请换手机号重试~");
            }

            //更新手机号
            strSql = $@"UPDATE t_member
                            SET tel = @tel
                            WHERE id ={ memberId };";
            await _jiakeRepository.SQLExecute(strSql, new { tel });
            return (1, "ok");
        }

        /// <summary>
        /// 授权头像昵称等
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="requestParam"></param>
        /// <returns></returns>
        public async Task<int> BindInfoAsync(int memberId, BindInfoRequestParam requestParam)
        {
            string strSql = $@"UPDATE t_member
                SET updated_time = NOW( ),
	                avatar_url = @avatarUrl,
	                nick_name = @nickName,
	                gender = @gender,
	                country = @country,
	                province = @province,
	                city = @city
                WHERE
	                id = {memberId};";

            return await _jiakeRepository.SQLExecute(strSql, requestParam);
        }

        /// <summary>
        /// 我的首页
        /// 我的愿望列表未达成的愿望不返回
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<MyWishResponse>> GetMyWishListAsync(string activityNo, int memberId)
        {
            //查询会员参加的愿望列表
            string strSql = $@"SELECT
	                                a.id,
	                                b.wish_name AS `name`,
	                                CASE a.for_people WHEN 1 THEN b.wish_pic ELSE b.wish_pic2 END AS pic,
	                                a.`status`,
	                                b.wish_unit AS wishUnit,
	                                b.rules,
	                                a.is_power AS isPower,
                                    IFNULL(c.helpers,0) helpers,
	                                a.user_count AS userCount,a.mch_no mchNo
                                FROM
	                                t_make_wish a
	                                INNER JOIN t_wish_base b ON a.wish_no = b.wish_no AND a.activity_no=b.activity_no
	                                LEFT JOIN ( SELECT make_wish_id, COUNT( DISTINCT member_id ) helpers FROM t_help_wish WHERE activity_no = @activityNo GROUP BY make_wish_id ) c ON c.make_wish_id = a.id
                                WHERE a.member_id={memberId} AND a.`status` IN (0,1,4) AND a.activity_no=@activityNo;";
            var wishList = await _jiakeRepository.SQLQueryAsync<MyWishResponse>(strSql, new { activityNo });

            //判断有没有达成的愿望，如果有就不用处理那一堆返回参数了（因为只能实现一个愿望，如果有实现的返回的只是这一条）
            if (!wishList.Any(x => x.process == 1))
            {
                foreach (var wish in wishList)
                {
                    //如果使用了助力值，助力人数+5
                    if (wish.isPower)
                    {
                        wish.helpers += 5;
                    }
                    //进行中许愿的处理返回值目标值和需要人数
                    if (wish.process == 0)
                    {
                        //判断助力人数和达到的档位
                        if (wish.ruleItems.Count == 1)//无档位处理
                        {
                            wish.target = "实现愿望";
                            wish.needed = wish.ruleItems[0].n - wish.helpers;
                        }
                        else if (wish.ruleItems.Count > 1)//多档位处理
                        {
                            //多档位的，只取第一个档位、目标档位、最后一个档位
                            var retItems = new List<WishRuleItem>();
                            //将第一个加入进去
                            retItems.Add(wish.ruleItems[0]);
                            //将最后一个加入进去
                            retItems.Add(wish.ruleItems[wish.ruleItems.Count - 1]);
                            //将会员选择的目标加入进去
                            if (!retItems.Any(x => x.n == wish.userCount))
                            {
                                var item = wish.ruleItems.Where(x => x.n == wish.userCount).FirstOrDefault();
                                if (item != null)
                                {
                                    retItems.Add(item);
                                }
                            }
                            retItems = retItems.OrderBy(p => p.n).ToList();
                            //查询下一个要达到的档位
                            var targetRule = retItems.Where(x => x.n > wish.helpers).FirstOrDefault();
                            if (targetRule != null)
                            {
                                wish.target = $"{targetRule.r}{wish.wishUnit}";
                                wish.needed = targetRule.n - wish.helpers;
                            }
                        }
                    }
                }
                wishList = wishList.OrderBy(p => p.needed).ThenByDescending(p => p.id);
            }

            return wishList;
        }

        ///// <summary>
        ///// 获取会员性别
        ///// </summary>
        ///// <param name="activityNo"></param>
        ///// <param name="memberId"></param>
        ///// <returns></returns>
        //public async Task<int> GetMemberGenderAsync(int memberId)
        //{
        //    //获取会员性别
        //    string strSql = $@"SELECT gender FROM t_member WHERE id={memberId};";
        //    var gender = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql);
        //    return gender;
        //}
        /// <summary>
        /// 选择愿望列表
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="gender">0 不限 1 男 2 女</param>
        /// <returns></returns>
        public async Task<(IEnumerable<WishListResponse>, int)> GetWishListAsync(string activityNo, int memberId, WishListRequestParam request)
        {
            string strWhere = "";
            if (request.gender != 0)
            {
                strWhere += $" AND (for_people = {request.gender} or for_people=0)";
            }
            //先查询符合性别的所有的愿望
            string strSql = $@"SELECT
	                                wish_no AS wishNo,
	                                wish_name AS `name`
                                FROM
	                                t_wish_base
                                WHERE
	                                activity_no = @activityNo
	                                AND is_deleted = 0
	                                AND is_online = 1
	                                {strWhere}
                                ORDER BY id;";
            var wishList = await _jiakeRepository.SQLQueryAsync<WishListResponse>(strSql, new { activityNo });

            //再查询会员已经许愿的愿望
            strSql = $@"SELECT
	                        wish_no AS wishNo,
	                        `status`
                        FROM
	                        t_make_wish
                        WHERE
	                        member_id = {memberId}
	                        AND activity_no = @activityNo; ";
            var memberWishes = await _jiakeRepository.SQLQueryAsync<(string, int)>(strSql, new { activityNo });
            //有达成的愿望就返回空吧
            if (memberWishes.Any(x => x.Item2 == 1 || x.Item2 == 4))
            {
                return (new List<WishListResponse>(), 0);
            }

            // 商户库存
            strSql = "SELECT SUM(IFNULL(wish_stock,1)) `value`,wish_no `key` FROM t_wish GROUP BY wish_no;";
            var mchStock = await _jiakeRepository.SQLQueryAsync<KeyValuePair<string, int>>(strSql);
            var ret = wishList.Where(p => mchStock.Any(x => x.Key == p.wishNo && x.Value > 0));

            if (memberWishes.Count() > 0)
            {
                var memberWishNos = memberWishes.Select(x => x.Item1).ToList();
                //排除掉会员已许愿的愿望
                ret = ret.Where(x => !memberWishNos.Contains(x.wishNo));
            }
            //分页
            return (ret.Skip(request.pageSize * (request.pageIndex - 1)).Take(request.pageSize), ret.Count());
        }

        /// <summary>
        /// 选择愿望时的其他愿望显示不显示，规则是提交过一次其他愿望就不再显示
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <returns>是否提交过其他愿望 1提交过 0未提交过</returns>
        public async Task<int> IsHaveOtherWishAsync(string activityNo, int memberId)
        {
            string strSql = $@"SELECT COUNT(*) FROM t_other_wish WHERE activity_no=@activityNo AND member_id={memberId};";
            var OtherWishCount = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql, new { activityNo });
            return OtherWishCount == 0 ? 0 : 1;
        }

        /// <summary>
        /// 愿望收集
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<(int, string)> SubmitWishAsync(string activityNo, int memberId, string content)
        {
            //判断是否已经提交过其他愿望
            var OtherWishCount = await this.IsHaveOtherWishAsync(activityNo, memberId);
            if (OtherWishCount > 0)
            {
                return (0, "兔兔已经收集过您想要的愿望了~");
            }
            string strSql = $@"INSERT INTO t_other_wish(activity_no,member_id,wish_content,created_time)
	                            VALUES (@activityNo,@memberId,@content,NOW());SELECT @@IDENTITY;";
            var id = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(strSql, new { activityNo, memberId, content });

            return (id > 0 ? 1 : 0, "OK");
        }

        /// <summary>
        /// 愿望详情
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        public async Task<WishDetailResponse> GetWishDetailAsync(string activityNo, string wishNo, int gender)
        {
            string picColumn = "wish_pic";
            if (gender == 2)
            {
                picColumn = "wish_pic2";
            }
            string strSql = $@"SELECT
	                                id,
	                                wish_name AS `name`,
	                                {picColumn} AS pic,
	                                wish_unit AS unit,
	                                remark,
	                                rules AS ruleStr
                                FROM
	                                t_wish_base
                                WHERE
	                                activity_no = @activityNo
	                                AND wish_no = @wishNo;";
            var ret = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<WishDetailResponse>(strSql, new { activityNo, wishNo });
            return ret;
        }

        /*****************************************分隔符*************************************************/

        /// <summary>
        /// 获取愿望助力人数
        /// </summary>
        /// <param name="WishId"></param>
        /// <returns></returns>
        public async Task<int> GetBoostCountByWishIdAsync(int WishId)
        {
            var sql = @"select count(*) from t_help_wish where make_wish_id=@WishId
                        UNION ALL
                        SELECT 5 FROM t_make_wish WHERE id=@WishId AND is_power=1";
            return (await _jiakeRepository.SQLQueryAsync<int>(sql, new { WishId })).Sum();
        }

        /// <summary>
        /// 问题反馈
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="activityNo"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<int> SaveProblemAsync(int memberId, string activityNo, string content)
        {
            var sql =
                @"insert into t_problem(activity_no,member_id,content,created_time)
                values(@activityNo,@memberId,@content,now());select @@identity;";
            return await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql, new { memberId, activityNo, content });
        }

        /// <summary>
        /// 兑现愿望--必须是达成的愿望才能兑现
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        public async Task<(int, int, string)> TakeWishAsync(int memberId, string mchNo)
        {
            // 查找用户在该商户下的愿望
            var sql = "SELECT id,exchange_status FROM t_make_wish where mch_no=@mchNo and member_id=@memberId and `status` in (1,4);";
            var makeWish = await _jiakeRepository.SQLQueryAsync<(int, int)>(sql, new { memberId, mchNo });
            if (!makeWish.Any() || !makeWish.Any(p => p.Item2 == 0))
            {
                return (2, makeWish.FirstOrDefault(p => p.Item2 == 1).Item1, "您在本店没有要兑现的愿望");
            }
            var makeWishId = makeWish.FirstOrDefault(p => p.Item2 == 0).Item1;
            sql = "update t_make_wish set exchange_status=1,exchange_time=now() where id = @makeWishId and member_id=@memberId and status in (1,4) and exchange_status=0;";
            var rows = await _jiakeRepository.SQLExecute(sql, new { memberId, makeWishId });
            if (rows > 0)
            {
                sql = "select count(*) from t_make_wish where mch_no=@mchNo and exchange_status=1";
                var count = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql, new { mchNo });
                sql = "update t_merchant set fake_wish_complated=fake_wish_complated+@count WHERE mch_no=@mchNo;";
                count = count < 3 ? count : 3;
                await _jiakeRepository.SQLExecute(sql, new { mchNo, count = count * 8 });
            }
            return (rows, makeWishId, rows > 0 ? "成功" : "失败");
        }

        /// <summary>
        /// 获取愿望档位规则
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>

        public async Task<WishRuleItem[]> GetWishRulesByWishNoAsync(string wishNo)
        {
            var ruleStr = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<string>(
                "SELECT rules FROM t_wish_base WHERE wish_no=@wishNo", new { wishNo });
            if (string.IsNullOrEmpty(ruleStr))
            {
                return new WishRuleItem[0];
            }
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<WishRuleItem[]>(ruleStr);
            return data;
        }

        /// <summary>
        /// 达成愿望
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        public async Task<(int, string, int)> ComplateWishAsync(int memberId, int makeWishId, string mchNo)
        {
            int isSend = 0;
            var boostCount = await GetBoostCountByWishIdAsync(makeWishId);
            if (boostCount == 0)
            {
                return (0, "不符合达成条件", isSend);
            }
            var sql = @"SELECT a.mch_no,b.wish_no,a.user_count,a.`status`,exchange_status,b.rules FROM t_make_wish a
                        LEFT JOIN t_wish_base b on a.wish_no= b.wish_no
                        WHERE a.id =@makeWishId;";
            // 获取愿望的状态相关信息
            var (oldMchNo, wishNo, targetCount, status, exStatus, ruleStr) = await _jiakeRepository
                .SQLQueryFirstOrDefaultAsync<(string, string, int, int, int, string)>(sql, new { makeWishId });

            // 未达成的 或 许愿中的并且无档位的 不可达成;
            if (status == 2 || status == 3 || exStatus == 1)
            {
                return (0, "禁止操作", isSend);
            }
            // 新、旧商户相同，直接返回成功
            if (oldMchNo == mchNo)
            {
                return (1, "成功", isSend);
            }

            //如果是null,代表首次达成，发消息
            if (string.IsNullOrEmpty(oldMchNo) && status == 0)
            {
                isSend = 1;
            }

            var colPart = ""; bool canComplate = true;// 是否可以达成愿望
            /*无档位的必须等待被动达成再选择商户。有档位的满足任一档位后即可手动达成*/
            if (status == 0 && !string.IsNullOrEmpty(ruleStr))// 许愿中
            {
                var rules = Newtonsoft.Json.JsonConvert.DeserializeObject<WishRuleItem[]>(ruleStr);
                canComplate = rules.Any(p => boostCount >= p.n);
                colPart = string.Format(",`status`={0},complated_time=now(),updated_time = now()", boostCount > targetCount ? 1 : 4);
            }
            if (!canComplate)
            {
                return (0, "不符合达成条件", isSend);
            }

            Func<int, string> leftHandler = (v) =>
            {
                var l = 4 - v.ToString().Length;
                var r = new StringBuilder();
                for (int i = 0; i < l; i++)
                {
                    r.Append("0");
                }
                return r.AppendFormat("{0}", v).ToString();
            };
            // 兑换码
            var code = string.Format("WJHY{0}{1}", mchNo, wishNo, leftHandler(makeWishId));
            var execSql = new StringBuilder();

            //商户不同 按照商户编号与愿望编号归还库存
            if (!string.IsNullOrEmpty(oldMchNo) && oldMchNo != mchNo)
            {
                execSql.AppendFormat("update t_wish set wish_stock=wish_stock+1 where mch_no='{0}' and wish_no='{1}';", oldMchNo, wishNo);
            }
            // 核减新商户库存
            execSql.AppendFormat("update t_wish set wish_stock=wish_stock-1 where mch_no='{0}' and wish_no='{1}';", mchNo, wishNo);
            // 更新许愿信息
            execSql.AppendFormat("update t_make_wish set code=@code,mch_no=@mchNo{0} where id = @makeWishId;", colPart);
            execSql.AppendFormat("update t_make_wish set `status`= 2,updated_time = now() where id<> @makeWishId and member_id= @memberId;");

            try
            {
                var msg = "ok";
                var re = 1;
                using (var unit = _unitOfWorkFactory.Create())
                {
                    if (await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(
                        "SELECT ifnull(wish_stock,1) FROM t_wish WHERE mch_no = @mchNo AND wish_no=@wishNo", new { mchNo, wishNo }) > 0)
                    {
                        await _jiakeRepository.SQLExecute(execSql.ToString(), new { mchNo, makeWishId, code, memberId });
                        unit.SaveChanges();
                    }
                    else
                    {
                        msg = "库存不足，请更换商户";
                        re = 0;
                    }
                    return (re, msg, isSend);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ComplateWishAsync");
                return (0, "操作失败", isSend);
            }
        }

        /// <summary>
        /// 获取门店列表
        /// </summary>
        /// <param name="makeWishId"></param>
        /// <param name="lo"></param>
        /// <param name="la"></param>
        /// <returns></returns>
        public async Task<(IEnumerable<MchItem>, int)> GetMchListAsync(MchListQuery query)
        {
            var makeWish = await GetMakeWishAsync(0, query.wishId);
            if (makeWish == null)
            {
                return (new MchItem[0], 0);
            }
            var sql = "select max(max_user_count) from t_wish where wish_no=@wishNo AND is_deleted=0";
            var maxUserCount = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql, new { makeWish.wishNo });

            sql = @"SELECT DISTINCT c.id,c.mch_no mchNo,c.mch_name `name`,mch_tel tel,mch_pic pic,
                        fake_wish_complated wishComplated,mch_latitude latitude,mch_longitude longitude
                        FROM t_make_wish  b
                        INNER JOIN t_wish a on a.wish_no=b.wish_no AND a.is_deleted=0
                        INNER JOIN t_merchant c on c.mch_no=a.mch_no and c.is_deleted=0
                        WHERE (wish_stock>0 or wish_stock is null) AND b.id=@wishId and max_user_count>=@boosts;";
            var data = await _jiakeRepository.SQLQueryAsync<MchItem>(sql, new
            {
                query.wishId,
                boosts = maxUserCount > 0 && makeWish.boosts > maxUserCount ? maxUserCount : makeWish.boosts
            });
            if (!data.Any())
            {
                return (data, 0);
            }
            foreach (var item in data)
            {
                if (query.lo > 0 && query.la > 0)
                {
                    item.shopDistance = Math.Round(CoordDispose.GetDistance(new Degree(query.lo, query.la), new Degree(item.longitude, item.latitude)) / 1000, 1);
                }
                else
                    item.shopDistance = 999999;
            }
            var total = data.Count();

            // 只有第一个按最近距离，其余按次数
            if (query.lo > 0 && query.la > 0)
            {
                var fistData = data.OrderBy(p => p.shopDistance).FirstOrDefault();
                foreach (var i in data)
                {
                    if (i.id == fistData.id)
                    {
                        i.shopDistance = 0;
                    }
                    else
                    {
                        i.shopDistance = 999999;
                    }
                }
            }

            data = data.OrderBy(p => p.shopDistance).ThenByDescending(p => p.wishComplated).ThenBy(p => p.id)
                .Skip((query.pageIndex - 1) * query.pageSize).Take(query.pageSize);

            if (query.pageIndex == 1 && query.la > 0 && query.lo > 0)
            {
                data.FirstOrDefault().isShorted = true;
            }
            return (data, total);
        }

        /// <summary>
        /// 助力列表
        /// </summary>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<string>> GetBoostListAsync(int makeWishId)
        {
            var sql =
                @"SELECT b.nick_name,d.wish_name,d.single_power,wish_unit,wish_type FROM t_help_wish a
                LEFT JOIN t_member b on b.id=a.member_id
                LEFT JOIN t_make_wish c on c.id=a.make_wish_id
                LEFT JOIN t_wish_base d on d.wish_no=c.wish_no
                WHERE a.make_wish_id=@makeWishId ORDER BY a.created_time desc,a.id desc";

            var data = await _jiakeRepository.SQLQueryAsync<(string, string, double, string, int)>(sql, new { makeWishId });
            if (data.Any(p => p.Item5 == 1))
            {
                return data.Select(p => string.Format("好友{0}已帮你完成{1} {2}{3}", HttpUtility.UrlDecode(p.Item1), p.Item2, p.Item3, p.Item4));
            }
            return data.Select(p => string.Format("好友{0}已帮你完成{1}助力", HttpUtility.UrlDecode(p.Item1), p.Item2));
        }

        /// <summary>
        /// 获取会员愿望值
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<int> GetWishValueAsync(int memberId)
        {
            var sql = @"SELECT count(*),1 FROM t_help_wish WHERE member_id = @memberId
                        UNION ALL
                        SELECT count(*),2 FROM t_make_wish WHERE member_id = @memberId AND is_power =1;";
            var data = await _jiakeRepository.SQLQueryAsync<(int, int)>(sql, new { memberId });
            return data.Any(p => p.Item1 > 0 && p.Item2 == 1) && !data.Any(p => p.Item1 > 0 && p.Item2 == 2) ? 5 : 0;
        }

        /// <summary>
        /// 获取愿望许愿人数
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<int> GetWishMakersAsync(string wishNo)
        {
            var sql = @"SELECT count(*)*7 FROM t_make_wish WHERE wish_no=@wishNo
                        UNION ALL
                        SELECT DATEDIFF(NOW(),start_time)*48 FROM t_activity;";
            var data = await _jiakeRepository.SQLQueryAsync<int>(sql, new { wishNo });

            var ret = data.Sum();

            return ret < 0 ? 0 : ret;
        }

        #region 查看愿望

        /// <summary>
        /// 获取愿望商户
        /// </summary>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        public async Task<MakeWishMchInfo> GetMakeWishMchInfoAsync(int makeWishId)
        {
            var sql = @"SELECT mch_pic pic,mch_tel tel,mch_address address,mch_name `name`,mch_longitude lo,mch_latitude la FROM t_make_wish a
                        INNER JOIN t_merchant b on a.mch_no=b.mch_no WHERE a.id =@makeWishId";
            var data = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<MakeWishMchInfo>(sql, new { makeWishId });
            return data;
        }

        /// <summary>
        /// 查看愿望
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        public async Task<ViewWishDetail> GetMakeWishAsync(int memberId, int makeWishId)
        {
            var sql = @"SELECT a.id,case a.for_people WHEN 1 THEN b.wish_pic ELSE b.wish_pic2 END pic,
                        b.wish_unit unit,b.wish_name `name`,b.remark,
                        b.rules ruleStr,a.user_count userCount,a.`status`,a.exchange_status exStatus,
                        a.member_id memberId,b.wish_no wishNo,a.for_people gender FROM t_make_wish a
                        INNER JOIN t_wish_base b on a.wish_no=b.wish_no WHERE a.id =@makeWishId";
            var data = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<ViewWishDetail>(sql, new { makeWishId });
            if (data == null)
            {
                return data;
            }
            data.mchInfo = await GetMakeWishMchInfoAsync(makeWishId);
            data.boosts = await GetBoostCountByWishIdAsync(makeWishId);
            if (memberId == data.memberId)
            {
                data.isWishMaker = 1;
                data.scores = await GetWishValueAsync(memberId);
            }
            else if (memberId > 0)
            {
                data.boostStatus = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(
                    "SELECT count(id) FROM t_help_wish WHERE make_wish_id =@makeWishId and member_id=@memberId;", new { makeWishId, memberId }) > 0 ? 1 : 0;
            }

            data.ResetRule();
            data.SetProcess();
            data.SetReceiveStatus(data.boosts);
            return data;
        }

        #endregion 查看愿望

        /// <summary>
        /// 获取会员助力次数
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<int> GetMemberBoostCountAsync(int memberId)
        {
            var sql = @"SELECT count(*) FROM t_help_wish WHERE member_id = @memberId;";
            var data = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql, new { memberId });
            return data;
        }

        /// <summary>
        /// 帮TA助力
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        public async Task<(int, string, int, int)> BoostWishAsync(string activityNo, int memberId, int makeWishId)
        {
            int mpArrive = 0; //是否达成 1 达成 0 未达成
            int mpBoost = 0; //是否还差2人达成 1 差2人 0 不差
            var makeWish = await GetMakeWishAsync(0, makeWishId);
            if (makeWish == null || makeWish.rules == null)
            {
                return (0, "信息错误", mpArrive, mpBoost);
            }
            if (makeWish.memberId == memberId)
            {
                return (0, "不能给自己助力哦", mpArrive, mpBoost);
            }
            var rule = makeWish.rules.LastOrDefault();
            if (makeWish.status == 1 || makeWish.status == 4 || makeWish.boosts >= rule.n)
            {
                return (0, "该心愿已达成", mpArrive, mpBoost);
            }
            var sql = new StringBuilder();
            if (makeWish.boosts + 1 == rule.n)
            {
                sql.Append("update t_make_wish set `status`=1,complated_time=now(),updated_time=now() where id = @makeWishId;");
                sql.Append("update t_make_wish set `status`=2,updated_time=now() where id <> @makeWishId and member_id=@wishMaker;");
                mpArrive = 1;
            }
            sql.Append(@"INSERT INTO t_help_wish(activity_no,make_wish_id,member_id,created_time)
                        VALUES(@activityNo,@makeWishId,@memberId,now());select @@identity;");
            using (var unit = _unitOfWorkFactory.Create())
            {
                await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql.ToString(), new { activityNo, memberId, makeWishId, wishMaker = makeWish.memberId });
                unit.SaveChanges();
            }

            if (makeWish.userCount - makeWish.boosts - 1 == 2)
            {
                mpBoost = 1;
            }

            return (1, "成功", mpArrive, mpBoost);
        }

        /// <summary>
        /// 获取活动时间
        /// </summary>
        /// <param name="activityNo"></param>
        /// <returns></returns>
        public async Task<DateTime?> GetOverTimeAsync(string activityNo)
        {
            if (overTime.HasValue)
            {
                return overTime;
            }
            var sql = "SELECT end_time FROM t_activity WHERE activity_no=@activityNo;";
            overTime = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<DateTime?>(sql, new { activityNo });
            return overTime;
        }

        /// <summary>
        /// 许愿
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="wishNo"></param>
        /// <param name="userCount"></param>
        /// <param name="activityNo"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public async Task<(int, string)> MakeWishAsync(int memberId, string wishNo, int userCount, string activityNo, int gender)
        {
            var msg = "成功";
            if (!await IsExistsAsync(activityNo, wishNo))
            {
                return (0, "该愿望不存在");
            }
            var myWishes = await _jiakeRepository.SQLQueryAsync<(string, int)>(
                @"SELECT wish_no,`status` FROM t_make_wish where member_id=@memberId;", new { memberId });
            // 有已达成的愿望 或者 已经许过该愿望了 退出
            if (myWishes.Any(p => p.Item2 == 1 || p.Item2 == 4))
            {
                return (0, "本次活动仅可参加一次哦");
            }
            if (myWishes.Any(p => p.Item1 == wishNo))
            {
                return (0, "您已许过该愿望");
            }
            var sql = @"INSERT INTO t_make_wish
                        (activity_no,wish_no,member_id,user_count,created_time,updated_time,over_time,`status`,for_people)
                        VALUES
                        (@activityNo,@wishNo,@memberId,@userCount,now(),now(),@overTime,0,@gender);
                        SELECT @@identity;";
            var id = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(sql,
                new { memberId, wishNo, userCount, activityNo, gender, overTime = await GetOverTimeAsync(activityNo) });
            return (id, msg);
        }

        /// <summary>
        /// 愿望加速
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="makeWishId"></param>
        /// <returns></returns>
        public async Task<(int, int, int)> AccelerateWishAsync(int memberId, int makeWishId)
        {
            int mpArrive = 0, mpBoost = 0;
            var myWish = await GetMakeWishAsync(memberId, makeWishId);
            if (myWish == null || myWish.scores == 0)
            {
                return (0, mpArrive, mpBoost);
            }
            string colPart = "", sql = "";
            var boosts = myWish.boosts + myWish.scores;

            //加速完成后会达到选择的目标档位
            if (boosts >= myWish.userCount)
            {
                mpArrive = 1;
            }
            else if (myWish.userCount - 2 == boosts)
            {
                mpBoost = 1;
            }
            //超越最高档位了，更新许愿信息
            if (boosts >= myWish.rules.LastOrDefault().n)
            {
                colPart = ",`status`=1,complated_time=now(),updated_time=NOW()";
                sql += "UPDATE t_make_wish SET `status` = 2,updated_time=NOW() WHERE member_id=@memberId and id<> @makeWishId;";
            }
            sql += $"UPDATE t_make_wish SET is_power = 1{colPart} WHERE id = @makeWishId;";
            var rows = 0;
            using (var unit = _unitOfWorkFactory.Create())
            {
                rows = await _jiakeRepository.SQLExecute(sql, new { makeWishId, memberId });
                unit.SaveChanges();
            }
            return (rows, mpArrive, mpBoost);
        }

        /// <summary>
        /// 愿望是否存在
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        public async Task<bool> IsExistsAsync(string activityNo, string wishNo)
        {
            return await _jiakeRepository.SQLQueryFirstOrDefaultAsync<int>(
                "SELECT count(id) FROM t_wish_base WHERE wish_no=@wishNo", new { wishNo }) > 0;
        }

        /// <summary>
        /// 会员愿望状态-登录接口使用
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<(int, int, string)> MemberWishStatusAsync(string activityNo, int memberId)
        {
            var sql = "select id,mch_no,status,exchange_status from t_make_wish where member_id=@memberId and activity_no=@activityNo";
            var data = await _jiakeRepository.SQLQueryAsync<(int, string, int, int)>(sql, new { activityNo, memberId });
            if (!data.Any())
            {
                return (0, 0, null);// 未参与
            }

            int wishId = 0, status = 2; string mchNo = null;
            // 已达成的
            var complateWishes = data.Where(p => p.Item3 == 1 || p.Item3 == 4);
            if (complateWishes.Any())
            {
                var wish = complateWishes.FirstOrDefault();// 只能有一个达成的愿望
                wishId = wish.Item1;
                mchNo = wish.Item2;
                if (wish.Item4 == 0)
                {
                    status = 1;//未核销
                }
            }
            return (wishId, status, mchNo);// 默认已核销或未达成
        }

        /// <summary>
        /// 获取愿望名称，openid,nick_name
        /// </summary>
        /// <param name="WishId"></param>
        /// <returns></returns>
        public async Task<(MPTemplateData, string)> GetWishOpenId(int makeWishId, string mPTemplateID, int memberId)
        {
            MPTemplateData data = null;

            string strSql = $@"SELECT
	                bas.wish_name,
                    bas.wish_unit,
                    mak.user_count,
                    bas.rules,
	                mem.openId,
	                mem.nick_name,
                    mem.is_send
                FROM
	                t_make_wish mak
	                LEFT JOIN t_wish_base bas ON bas.wish_no = mak.wish_no
	                LEFT JOIN t_member mem ON mem.id = mak.member_id
                WHERE
	                mak.id = {makeWishId};";
            var (wishName, wishUnit, userCount, rulesStr, openId, nickName, isSend) = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<(string, string, int, string, string, string, int)>(strSql);

            if (!string.IsNullOrEmpty(wishUnit) && wishUnit != "--")
            {
                var rules = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<WishRuleItem>>(rulesStr);
                var r = rules.Where(e => e.n == userCount).FirstOrDefault()?.r;
                if (r != null)
                {
                    wishName = string.Concat(wishName, r, wishUnit);
                }
            }

            if (mPTemplateID == MPTemplateID.mpBoost) //差2人达成提醒
            {
                data = new MPTemplateData(
                            "距离愿望达成仅差一步之遥，再去邀请好友帮你增加愿望值吧~",
                            $"{wishName}",
                            "还差2位好友助力",
                            $"{DateTime.Now.ToString("yyyy年MM月dd日")}",
                            "点击查看愿望详情哦~"
                            );
            }
            if (mPTemplateID == MPTemplateID.mpArrive) //愿望达成提醒
            {
                data = new MPTemplateData(
                                "您的愿望已经达成啦，快来把它实现吧！",
                                $"{wishName}",
                                $"{DateTime.Now.ToString("yyyy年MM月dd日")}",
                                "",
                                "点击实现愿望哟~"
                                );
            }
            if (mPTemplateID == MPTemplateID.mpReserve && isSend == 0) //预约愿望提醒
            {
                strSql = $@"SELECT openId FROM t_member WHERE id={memberId};";
                openId = await _jiakeRepository.SQLQueryFirstOrDefaultAsync<string>(strSql);
                if (!string.IsNullOrEmpty(openId))
                {
                    data = new MPTemplateData(
                                "亲爱的，恭喜您成功预约活动，活动开始后不要忘记参加哦！",
                                $"预约成功",
                                $"实现吧！2021愿望君！",
                                "许下你的愿望，兔兔帮你实现！感谢您的报名！",
                                ""
                                );
                }
            }
            return (data, openId);
        }

        /// <summary>
        /// 预约成功，更新发送记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public async Task<int> SaveMemberIsSend(int memberId)
        {
            string strSql = $@"UPDATE t_member
                SET is_send=1 WHERE id={memberId};";

            int resCode = await _jiakeRepository.SQLExecute(strSql);

            return resCode;
        }

        /// <summary>
        /// 1-可以正常核销 2-愿望已达成但未选择门店 3-没有愿望或没有达成的愿望 4-扫码门店与所选门店不符 5-已正常核销（不区分门店）
        /// </summary>
        /// <param name="activityNo"></param>
        /// <param name="memberId"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        public async Task<(int, int)> GetCheckoutWishAsync(string activityNo, int memberId, string mchNo)
        {
            int ret = 1;
            var sql = @"SELECT id,mch_no,exchange_status FROM t_make_wish
                        WHERE member_id =@memberId AND `status` in (1,4) AND activity_no=@activityNo";
            var data = await _jiakeRepository.SQLQueryAsync<(int, string, int)>(sql, new { activityNo, memberId });
            if (!data.Any())
            {
                return (3, 0);
            }
            // 数据有问题
            if (data.Count() > 1)
            {
                return (0, 0);
            }
            var wish = data.FirstOrDefault();
            if (wish.Item3 == 1)
            {
                ret = 5;
            }
            else if (wish.Item2 == null)
            {
                ret = 2;
            }
            else if (wish.Item2 != mchNo)
            {
                ret = 4;
            }
            return (ret, wish.Item1);
        }
    }
}
