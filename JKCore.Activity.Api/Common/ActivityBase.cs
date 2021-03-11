using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Activity.Api.Common
{
    public static class ActivityBase
    {
        /// <summary>
        /// 获取memberId
        /// </summary>
        /// <returns></returns>
        public static int GetMemberId(this ControllerBase controllerBase)
        {
            int.TryParse(controllerBase.HttpContext.Request.Headers.FirstOrDefault(a => a.Key.ToLower() == "memberid").Value, out int memberId);

            return memberId;
        }

        /// <summary>
        /// 获取ActivityNo（活动编号）
        /// </summary>
        /// <param name="controllerBase"></param>
        /// <returns></returns>
        public static string GetActivityNo(this ControllerBase controllerBase)
        {
            return "AC20210101";
        }
    }
}
