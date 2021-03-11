using Activity.Interfaces.Application.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Activity.Interfaces.Application
{
    public interface IOperatApplication
    {
        /// <summary>
        /// 愿望列表
        /// </summary>
        /// <param name="wishName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(IEnumerable<WishInfo>, int)> WishList(string activityNo, string wishName, int pageIndex, int pageSize);

        /// <summary>
        /// 愿望上下架
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="isOnline"></param>
        /// <returns></returns>
        Task<int> ChangeStatus(string activityNo, string wishNo, int isOnline);

        /// <summary>
        /// 商户列表
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        Task<(IEnumerable<MchInfo>, int)> MchList(string activityNo, string wishNo, int pageIndex, int pageSize);

        /// <summary>
        /// 添加商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <param name="wishTotal"></param>
        /// <returns></returns>
        Task<(int, string)> AddMch(string activityNo, string wishNo, string mchNo, int wishTotal);

        /// <summary>
        /// 删除商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        Task<int> DeletedMch(string activityNo, string wishNo, string mchNo);

        /// <summary>
        /// 许愿列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<(IEnumerable<MakeWishInfo>, int)> MakeWishList(string activityNo, RequestMakeWishInfo request);

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
        Task<(IEnumerable<HelpInfo>, int)> HelpList(int wishId, string activityNo, string tel, string inviteTel, int pageIndex, int pageSize);

        /// <summary>
        /// 许愿反馈列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(IEnumerable<ProblemInfo>, int)> OtherWishList(string activityNo, string content, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize);

        /// <summary>
        /// 客服问题列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<(IEnumerable<ProblemInfo>, int)> ProblemList(string activityNo, string content, DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize);
    }
}
