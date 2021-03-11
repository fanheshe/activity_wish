using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Activity.Interfaces.Application;
using JKCore.Infrastructure;
using Newtonsoft.Json.Linq;
using JKCore.Activity.Api.Common;
using Activity.Interfaces.Application.Model;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using Senparc.Weixin;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.IO.Compression;
using System.Drawing.Imaging;
using System.Drawing;
using System.Web;

namespace JKCore.Activity.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OperatController : ControllerBase
    {
        private readonly IOperatApplication _operatApplication;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string _wxOpenAppId = Config.SenparcWeixinSetting.WxOpenAppId;//与微信小程序后台的AppId设置保持一致，区分大小写。

        public OperatController(IOperatApplication operatApplication, IWebHostEnvironment hostingEnvironment)
        {
            _operatApplication = operatApplication;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 愿望列表
        /// </summary>
        /// <param name="wishName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> WishList(string wishName, int pageIndex = 1, int pageSize = 10)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.WishList(activityNo, wishName, pageIndex, pageSize);

            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 愿望上下架
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="isOnline"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ChangeStatus([FromBody] JObject jObject)
        {
            if (!jObject.TryGetValue("wishNo", out JToken wishNoJToken))
            {
                return BadRequest("参数无效 ！");
            }
            string wishNo = wishNoJToken.Value<string>();

            if (!jObject.TryGetValue("isOnline", out JToken isOnlineJToken))
            {
                return BadRequest("参数无效 ！");
            }
            int isOnline = isOnlineJToken.Value<int>();

            string activityNo = this.GetActivityNo();

            int resCode = await _operatApplication.ChangeStatus(activityNo, wishNo, isOnline);

            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 商户列表
        /// </summary>
        /// <param name="wishNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MchList(string wishNo, int pageIndex = 1, int pageSize = 10)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.MchList(activityNo, wishNo, pageIndex, pageSize);
            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 添加商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <param name="wishTotal"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddMch([FromBody] JObject jObject)
        {
            if (!jObject.TryGetValue("wishNo", out JToken wishNoJToken))
            {
                return BadRequest("参数无效 ！");
            }
            string wishNo = wishNoJToken.Value<string>();

            if (!jObject.TryGetValue("mchNo", out JToken mchNoJToken))
            {
                return BadRequest("参数无效 ！");
            }
            string mchNo = mchNoJToken.Value<string>();

            if (!jObject.TryGetValue("wishTotal", out JToken wishTotalJToken))
            {
                return BadRequest("参数无效 ！");
            }
            int wishTotal = wishTotalJToken.Value<int>();

            string activityNo = this.GetActivityNo();

            var (resCode, msg) = await _operatApplication.AddMch(activityNo, wishNo, mchNo, wishTotal);

            if (!string.IsNullOrEmpty(msg))
            {
                return BadRequest(msg);
            }

            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 删除商户
        /// </summary>
        /// <param name="wishNo"></param>
        /// <param name="mchNo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeletedMch([FromBody] JObject jObject)
        {
            if (!jObject.TryGetValue("wishNo", out JToken wishNoJToken))
            {
                return BadRequest("参数无效 ！");
            }
            string wishNo = wishNoJToken.Value<string>();

            if (!jObject.TryGetValue("mchNo", out JToken mchNoJToken))
            {
                return BadRequest("参数无效 ！");
            }
            string mchNo = mchNoJToken.Value<string>();

            string activityNo = this.GetActivityNo();

            int resCode = await _operatApplication.DeletedMch(activityNo, wishNo, mchNo);

            return this.MyOK(new { resCode });
        }

        /// <summary>
        /// 许愿列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> MakeWishList([FromQuery] RequestMakeWishInfo request)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.MakeWishList(activityNo, request);

            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 好友助力列表
        /// </summary>
        /// <param name="wishId"></param>
        /// <param name="tel"></param>
        /// <param name="inviteTel"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> HelpList(int wishId, string tel, string inviteTel, int pageIndex = 1, int pageSize = 10)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.HelpList(wishId, activityNo, tel, inviteTel, pageIndex, pageSize);

            return this.MyOK(new { dataList, total });
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
        [HttpGet]
        public async Task<IActionResult> OtherWishList(string content, DateTime? startTime, DateTime? endTime, int pageIndex = 1, int pageSize = 10)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.OtherWishList(activityNo, content, startTime, endTime, pageIndex, pageSize);

            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 导出许愿反馈列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadOtherWishList(string content, DateTime? startTime, DateTime? endTime, int pageIndex = 1, int pageSize = 10000)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.OtherWishList(activityNo, content, startTime, endTime, pageIndex, pageSize);

            if (!dataList.Any())
            {
                return BadRequest("没有可导出的数据");
            }

            string excelName = $@"许愿反馈信息-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls";

            var columns = new Dictionary<string, string>() {
                { "nickName","昵称"},
                { "tel","手机号"},
                { "createdTime","反馈时间"},
                { "content","反馈内容"}
            };
            var fs = PublicClass.GetByteToExportExcel(dataList.ToList(), columns, new List<string>());
            return File(fs, "application/vnd.android.package-archive", excelName);
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
        [HttpGet]
        public async Task<IActionResult> ProblemList(string content, DateTime? startTime, DateTime? endTime, int pageIndex = 1, int pageSize = 10)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.ProblemList(activityNo, content, startTime, endTime, pageIndex, pageSize);

            return this.MyOK(new { dataList, total });
        }

        /// <summary>
        /// 导出客服问题列表
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadProblemList(string content, DateTime? startTime, DateTime? endTime, int pageIndex = 1, int pageSize = 10000)
        {
            string activityNo = this.GetActivityNo();

            var (dataList, total) = await _operatApplication.ProblemList(activityNo, content, startTime, endTime, pageIndex, pageSize);

            if (!dataList.Any())
            {
                return BadRequest("没有可导出的数据");
            }

            string excelName = $@"客服问题汇总-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xls";

            var columns = new Dictionary<string, string>() {
                { "nickName","昵称"},
                { "tel","手机号"},
                { "createdTime","提交时间"},
                { "content","提交问题"}
            };
            var fs = PublicClass.GetByteToExportExcel(dataList.ToList(), columns, new List<string>());
            return File(fs, "application/vnd.android.package-archive", excelName);
        }

        /// <summary>
        /// 下载商户核销码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadMchQRCode()
        {
            string activityNo = this.GetActivityNo();
            var (mchList, total) = await _operatApplication.MchList(activityNo, "", 1, 1000);
            if (total <= 0)
            {
                return BadRequest("未找到商户！");
            }

            string dateName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string filePath = $"{_hostingEnvironment.WebRootPath}/Image/Wish/{dateName}/";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            foreach (var data in mchList)
            {
                string path = $@"pages/conversion/index?mchNo={data.mchNo}";

                await WxAppApi.GetWxaCodeAsync(_wxOpenAppId, string.Concat(filePath, data.mchName, ".jpeg"), path);
            }

            string zipPath = $"/File/Wish/";
            string zipPathName = $"{zipPath}{dateName}.zip";
            string zipPathDir = _hostingEnvironment.WebRootPath + zipPath;
            if (!Directory.Exists(zipPathDir))
            {
                Directory.CreateDirectory(zipPathDir);
            }
            try
            {
                ZipFile.CreateFromDirectory(filePath, $"{_hostingEnvironment.WebRootPath}{zipPathName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("压缩错误 ！");
            }

            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }

            return File(zipPathName, "application/zip", $"商户核销码-{dateName}.zip");
        }

        [HttpGet]
        public async Task<IActionResult> GetQRCode(string path)
        {
            //var memberId = this.GetMemberId();
            ////Console.WriteLine("memberId=={1} path1:{0}", path, memberId);
            //var stream = new MemoryStream();
            //path = HttpUtility.UrlDecode(path);
            ////Console.WriteLine("memberId=={1} path2:{0}", path, memberId);
            //await WxAppApi.GetWxaCodeAsync(_wxOpenAppId, stream, path);
            ////必须将流的当前位置置0，否则将引发异常
            ////如果不设置为0，则流的当前位置在流的末端，然后读流就会从末端开始读取，就会引发无效操作异常System.InvalidOperationException
            ////System.InvalidOperationException: Response Content-Length mismatch: too few bytes written (0 of xxxx)
            //stream.Position = 0;
            //return new FileStreamResult(stream, "image/gif");

            path = HttpUtility.UrlDecode(path);
            using (var ms = new MemoryStream())
            {
                await WxAppApi.GetWxaCodeAsync(_wxOpenAppId, ms, path);

                return File(ms.GetBuffer(), "image/jpeg");
            }
        }
    }
}