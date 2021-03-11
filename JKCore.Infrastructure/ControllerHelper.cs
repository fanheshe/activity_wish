using Microsoft.AspNetCore.Mvc;

namespace JKCore.Infrastructure
{
    public static class ControllerHelper
    {
        /// <summary>
        /// 返回结果
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object CreateResult(this ControllerBase controller, object data)
        {
            return new { data };
        }

        /// <summary>
        /// 自定义Ok
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static OkObjectResult MyOK(this ControllerBase controller, object data)
        {
            return controller.Ok(controller.CreateResult(data));
        }
    }
}
