using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JKCore.Infrastructure
{
    public class OperatController : ControllerBase
    {
        /// <summary>
        /// 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserId()
        {
            var Claims = HttpContext.User.Claims;
            return Convert.ToInt32(Claims.FirstOrDefault(x => x.Type == "UserId")?.Value);
        }
        /// <summary>
        /// 获取用户姓名
        /// </summary>
        /// <returns></returns>
        public string GetRealName()
        {
            var Claims = HttpContext.User.Claims.ToList();
            return Claims.FirstOrDefault(x => x.Type == "RealName")?.Value;
        }
        /// <summary>
        /// 获取失效时间
        /// </summary>
        /// <returns></returns>
        public long GetExp()
        {
            var Claims = HttpContext.User.Claims.ToList();
            return Convert.ToInt64(Claims.FirstOrDefault(x => x.Type == "exp")?.Value);
        }
        /// <summary>
        /// 获取token
        /// </summary>
        /// <returns></returns>
        public string GetAccessToken()
        {
            return HttpContext.Request.Headers.FirstOrDefault(a => a.Key.ToLower() == "authorization").Value;
        }
    }
}
