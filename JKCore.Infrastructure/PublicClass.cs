using LazZiya.ImageResize;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace JKCore.Infrastructure
{
    /// <summary>
    ///PublicClass 的摘要说明
    /// </summary>
    public class PublicClass
    {
        public PublicClass()
        {
            //
            //TODO: 在此处添加构造函数逻辑
            //
        }

        #region ========加密========

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            Text = HttpUtility.UrlEncode(Text);
            return Encrypt(Text, "12345678");
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Encrypt(string Text, string sKey)
        {
            if (String.IsNullOrEmpty(Text))
                return "";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            des.Key = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        public static string MD5(string str)
        {
            //微软md5方法参考return FormsAuthentication.HashPasswordForStoringInConfigFile(str, "md5");
            byte[] b = Encoding.Default.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
                ret += b[i].ToString("X").PadLeft(2, '0');
            return ret;
        }

        public static string Encrypt(string Text, string sKey, string sIV)
        {
            if (String.IsNullOrEmpty(Text))
                return "";
            Text = HttpUtility.UrlEncode(Text);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = Encoding.GetEncoding("UTF-8").GetBytes(Text);
            des.Mode = CipherMode.CBC;
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            ret.ToString();
            return ret.ToString();
        }

        #endregion ========加密========

        #region ========解密========

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            return HttpUtility.UrlDecode(Decrypt(Text, "12345678"));
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Decrypt(string Text, string sKey)
        {
            if (String.IsNullOrEmpty(Text))
                return "";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        public static string Decrypt(string Text, string sKey, string sIV)
        {
            if (String.IsNullOrEmpty(Text))
                return "";
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray = new byte[Text.Length / 2];
            for (int x = 0; x < Text.Length / 2; x++)
            {
                int i = (Convert.ToInt32(Text.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            des.IV = ASCIIEncoding.ASCII.GetBytes(sIV);
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            return HttpUtility.UrlDecode(System.Text.Encoding.Default.GetString(ms.ToArray()));
        }

        #endregion ========解密========

        #region 时间戳转换

        /// <summary>
        /// 日期转换为时间戳（时间戳单位秒）
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public static long ConvertToTimeStamp(DateTime time)
        {
            DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(time.AddHours(-8) - Jan1st1970).TotalMilliseconds;
        }

        /// <summary>
        /// 时间戳转换为日期（时间戳单位秒）
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public static DateTime ConvertToDateTime(long timeStamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddSeconds(timeStamp).AddHours(8);
        }

        #endregion 时间戳转换

        #region 图片处理

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="file"></param>
        /// <param name="imagePath"></param>
        /// <param name="webRootPath"></param>
        /// <param name="filename"></param>
        /// <param name="useWatermark">是否使用水印，默认不启用</param>
        /// <param name="watermarkType">水印类型，txt：文字；img：图片水印</param>
        /// <param name="watermarkText">文字水印内容</param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> UploadFile(IFormFile file, string imagePath, string webRootPath, string filename = "",
                                                                            bool useWatermark = false, string watermarkType = "txt", string watermarkText = "")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = DateTime.Now.Ticks.ToString();
            }
            string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            string savePath = imagePath + filename + suffix;
            if (!Directory.Exists(webRootPath + imagePath))
            {
                Directory.CreateDirectory(webRootPath + imagePath);
            }

            using (FileStream fs = new FileStream(webRootPath + savePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);

                if (useWatermark)
                {
                    savePath = imagePath + "watermark/" + filename + suffix;
                    if (!Directory.Exists(webRootPath + imagePath + "watermark"))
                    {
                        Directory.CreateDirectory(webRootPath + imagePath + "watermark");
                    }
                    switch (watermarkType)
                    {
                        case "txt":    // 文字水印
                            {
                                using (var img = Image.FromStream(fs))
                                {
                                    var iOps = new TextWatermarkOptions
                                    {
                                        TextColor = Color.FromArgb(100, Color.White),
                                        Location = TargetSpot.TopLeft,
                                        FontName = "微软雅黑",
                                        FontStyle = FontStyle.Bold,
                                        FontSize = 12
                                    };
                                    img.AddTextWatermark(watermarkText, iOps)
                                        .SaveAs($@"{webRootPath}{imagePath}watermark/{filename}{suffix}");  // 保存水印
                                }
                            }
                            break;

                        case "img":
                            {
                                using (var img = Image.FromStream(fs))
                                {
                                    var iOps = new ImageWatermarkOptions
                                    {
                                        Location = TargetSpot.TopLeft
                                    };
                                    string watermarkPath = $"{ webRootPath }/Image/watermark.png";
                                    var waterImg = Image.FromFile(watermarkPath);
                                    var tmpImg = waterImg.Scale(img.Width / 2, img.Height / 2);
                                    var waterImgPath = $@"{webRootPath}{imagePath}watermark/{filename}{suffix}";
                                    img.AddImageWatermark(tmpImg, iOps)
                                        .SaveAs(waterImgPath);  // 保存水印
                                    waterImg.Dispose();
                                }
                            }
                            break;
                    }
                }
            }
            return savePath;
        }

        /// <summary>
        /// 上传图像-缩略图-重定义大小
        /// </summary>
        /// <param name="file"></param>
        /// <param name="imagePath"></param>
        /// <param name="webRootPath"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> UploadFile(IFormFile file, string imagePath, string webRootPath, int newWidth, int newHeight, string filename = "")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = DateTime.Now.Ticks.ToString();
            }
            string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            string savePath = imagePath + filename + suffix;
            if (!Directory.Exists(webRootPath + imagePath))
            {
                Directory.CreateDirectory(webRootPath + imagePath);
            }
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var img = Image.FromStream(stream))
                {
                    ThumbImg(img, webRootPath + savePath, 100, 100);
                }
            }
            return savePath;
        }

        /// <summary>
        /// 上传图像-缩略图-质量压缩
        /// </summary>
        /// <param name="file"></param>
        /// <param name="imagePath"></param>
        /// <param name="webRootPath"></param>
        /// <param name="level"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> UploadFile(IFormFile file, string imagePath, string webRootPath, long level, string filename = "")
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = DateTime.Now.Ticks.ToString();
            }
            string suffix = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            string savePath = imagePath + filename + suffix;
            if (!Directory.Exists(webRootPath + imagePath))
            {
                Directory.CreateDirectory(webRootPath + imagePath);
            }
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                Image img = Bitmap.FromStream(stream);
                using (var bitMap = new Bitmap(img))
                {
                    Compress(bitMap, 20, webRootPath + savePath);
                }
            }
            return savePath;
        }

        public static void CopyFile(string imagePath, string destpath)
        {
            if (File.Exists(imagePath))
                File.Copy(imagePath, destpath);
        }

        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        /// <summary>
        /// 制作缩略图
        /// </summary>
        /// <param name="original">图片对象</param>
        /// <param name="newFileName">新图路径</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        public static void ThumbImg(Image original, string newFileName, int maxWidth, int maxHeight)
        {
            Size newSize = ResizeImage(original.Width, original.Height, maxWidth, maxHeight);
            using (Image displayImage = new Bitmap(original, newSize))
            {
                try
                {
                    displayImage.Save(newFileName);
                }
                finally
                {
                    original.Dispose();
                }
            }
        }

        /// <summary>
        /// 计算新尺寸
        /// </summary>
        /// <param name="width">原始宽度</param>
        /// <param name="height">原始高度</param>
        /// <param name="maxWidth">最大新宽度</param>
        /// <param name="maxHeight">最大新高度</param>
        /// <returns></returns>
        private static Size ResizeImage(int width, int height, int maxWidth, int maxHeight)
        {
            if (maxWidth <= 0)
                maxWidth = width;
            if (maxHeight <= 0)
                maxHeight = height;
            decimal MAX_WIDTH = maxWidth;
            decimal MAX_HEIGHT = maxHeight;
            decimal ASPECT_RATIO = MAX_WIDTH / MAX_HEIGHT;

            int newWidth, newHeight;
            decimal originalWidth = width;
            decimal originalHeight = height;

            if (originalWidth > MAX_WIDTH || originalHeight > MAX_HEIGHT)
            {
                decimal factor;
                if (originalWidth / originalHeight > ASPECT_RATIO)
                {
                    factor = originalWidth / MAX_WIDTH;
                    newWidth = Convert.ToInt32(originalWidth / factor);
                    newHeight = Convert.ToInt32(originalHeight / factor);
                }
                else
                {
                    factor = originalHeight / MAX_HEIGHT;
                    newWidth = Convert.ToInt32(originalWidth / factor);
                    newHeight = Convert.ToInt32(originalHeight / factor);
                }
            }
            else
            {
                newWidth = width;
                newHeight = height;
            }
            return new Size(newWidth, newHeight);
        }

        /// <summary>
        /// 压缩相关
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns></returns>
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }

            return null;
        }

        /// <summary>
        /// 图片压缩
        /// </summary>
        /// <param name="srcBitmap"></param>
        /// <param name="level"></param>
        /// <param name="fileName"></param>
        public static void Compress(Bitmap srcBitmap, long level, string fileName)
        {
            ImageCodecInfo myImageCodecInfo;
            System.Drawing.Imaging.Encoder myEncoder;
            EncoderParameter myEncoderParameter;
            EncoderParameters myEncoderParameters;
            myImageCodecInfo = GetEncoderInfo("image/jpeg");

            myEncoder = System.Drawing.Imaging.Encoder.Quality;
            myEncoderParameters = new EncoderParameters(1);
            myEncoderParameter = new EncoderParameter(myEncoder, level);
            myEncoderParameters.Param[0] = myEncoderParameter;
            srcBitmap.Save(fileName, myImageCodecInfo, myEncoderParameters);

            srcBitmap.Dispose();
        }

        #endregion 图片处理

        /// <summary>
        /// 生成6位随机密码
        /// </summary>
        /// <returns></returns>
        public static string GetRandom()
        {
            Random rnd = new Random();
            //length是你需要几个随机数
            string str = "";//这里随便定义了一个string类型你可以int数组
            for (int i = 0; i < 1; i++)
            {
                int n = rnd.Next(100000, 999999);
                //n就是你要的随机数，如果你要5位的就将上面改成(10000,99999),6位：(100000,999999)
                str += n;//如果是数组这里需要 str。add（n）；
            }
            return str;
        }

        /// <summary>
        /// 获取前三位code
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetWaterBarCode(string lastCode)
        {
            string code = "";
            //最后一个取货码
            var end3Code = Convert.ToInt32(lastCode);
            end3Code += 1;
            if (end3Code < 10)
            {
                code = "000" + end3Code;
            }
            else if (end3Code < 100)
            {
                code = "00" + end3Code;
            }
            else if (end3Code < 1000)
            {
                code = "0" + end3Code;
            }
            else if (end3Code < 10000)
            {
                code = end3Code.ToString();
            }
            else
            {
                code = "0001";
            }
            return code;
        }

        public static string GetCashFlowNo()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss") + GetRandom();
        }

        /// <summary>
        /// 数字转换成周几
        /// </summary>
        /// <param name="week"></param>
        /// <returns></returns>
        public static string GetWeekStr(int week)
        {
            string str = "一,二,三,四,五,六,日";
            return "周" + str.Split(',')[week - 1];
        }

        public static string GetWeekStrExtend(int week)
        {
            string str = "日,一,二,三,四,五,六";
            return str.Split(',')[week];
        }

        public static string GetTodayTomorrowWeek(int week)
        {
            int dtWeek = (int)DateTime.Now.DayOfWeek;

            if ((dtWeek == 0 ? 7 : dtWeek) == week)
            {
                return "今日";
            }
            else if (dtWeek + 1 == week)
            {
                return "明日";
            }

            string str = "一,二,三,四,五,六,日";
            return "周" + str.Split(',')[week - 1];
        }

        public static string DayOfWeek()
        {
            var week = "";
            var dt = DateTime.Today.DayOfWeek.ToString();
            switch (dt)
            {
                case "Monday":
                    week = "星期一";
                    break;

                case "Tuesday":
                    week = "星期二";
                    break;

                case "Wednesday":
                    week = "星期三";
                    break;

                case "Thursday":
                    week = "星期四";
                    break;

                case "Friday":
                    week = "星期五";
                    break;

                case "Saturday":
                    week = "星期六";
                    break;

                case "Sunday":
                    week = "星期日";
                    break;
            }
            return week;
        }

        public static string DayOfWeek(DateTime date, bool prefix = false)
        {
            var label = "星期";
            if (prefix)
            {
                label = "周";
            }
            string[] weekdays = { "日", "一", "二", "三", "四", "五", "六" };
            string week = $"{label}{weekdays[Convert.ToInt32(date.DayOfWeek)]}";

            return week;
        }

        /// <summary>
        /// 两个时间的天数之差（整数天）
        /// </summary>
        /// <param name="end"></param>
        /// <param name="beg"></param>
        /// <returns></returns>
        public static int DateTimeDiffDays(DateTime? end, DateTime? beg)
        {
            if (end == null || beg == null)
                return 0;
            return (DateTime.Parse(end.Value.ToString("yyyy-MM-dd")) - DateTime.Parse(beg.Value.ToString("yyyy-MM-dd"))).Days;
        }

        /// <summary>
        /// 获取当前是一年中的第几周
        /// </summary>
        /// <returns></returns>
        public static int GetWeekOfYear()
        {
            GregorianCalendar gc = new GregorianCalendar();

            int weekOfYear = gc.GetWeekOfYear(DateTime.Now, System.Globalization.CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);

            return weekOfYear;
        }

        /// <summary>
        /// 获取本周 周一与周日的日期（不含时分秒）
        /// </summary>
        /// <returns></returns>
        public static (DateTime, DateTime) MondayAndSunday()
        {
            return MondayAndSunday(DateTime.Now);
        }

        /// <summary>
        /// 获取指定日期所在周 周一与周日的日期（不含时分秒）
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public static (DateTime, DateTime) MondayAndSunday(DateTime today)
        {
            DateTime start, end;
            var DayOfTheWeek = today.DayOfWeek;
            //calc week date
            if (DayOfTheWeek == System.DayOfWeek.Sunday)
            {
                start = today.AddDays(-6);
            }
            else
            {
                start = today.AddDays(1).AddDays(-(int)DayOfTheWeek);
            }
            end = start.AddDays(6);
            return (start, end);
        }

        /// <summary>
        /// 分割时间字符,不验证格式
        /// 2020=01-01@2020-01-01 结果：2020-01-01和2020-01-01
        /// 2020-01-01 结果：2020-01-01和时间最大值
        /// </summary>
        /// <param name="datestr"></param>
        /// <returns></returns>
        public static Tuple<bool, List<string>> SplitDate(string datestr)
        {
            var times = datestr.Split('@');
            if (times.Length > 2 || times.Length <= 0)
            {
                return new Tuple<bool, List<string>>(false, new List<string>()); ;
            }
            var list = new List<string>();
            list.Add(times[0]);
            if (times.Length == 2)
                list.Add(times[1]);
            else
                list.Add(DateTime.MaxValue.ToString());

            return new Tuple<bool, List<string>>(true, list);
        }

        public static string CutApiUrl(string url)
        {
            int l = url.LastIndexOf("/");

            if (l > 0)
            {
                return url.Substring(0, l + 1)
                    .Replace("http://app.api.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://app.api.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://app.api.official:5000", "https://c.jiake365.com")
                    .Replace("http://platformapp.api.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://platformapp.api.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://app.api.v1.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://app.api.v1.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://app.api.v2.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://app.api.v2.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://app.api.v3.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://app.api.v3.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://platformapp.api.v1.test:5000", "https://dev-h5.365jiake.com")
                    .Replace("http://platformapp.api.v1.release:5000", "https://beta-h5.365jiake.com")
                    .Replace("http://activity.api.test:5000", "http://11.365jiake.cn");
            }
            else
            {
                return url;
            }
        }

        #region 异业合作商品码

        /// <summary>
        /// 生成3位随机码
        /// </summary>
        /// <returns></returns>
        public static string Cooperate3Random()
        {
            Random rnd = new Random();
            string code = rnd.Next(1, 999).ToString();
            if (code.Length == 2)
            {
                code = "0" + code;
            }
            else if (code.Length == 1)
            {
                code = "00" + code;
            }
            return code;
        }

        /// <summary>
        /// 补齐5位码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string Cooperate5Repair(string code)
        {
            if (code.Length == 4)
            {
                code = "0" + code;
            }
            else if (code.Length == 3)
            {
                code = "00" + code;
            }
            else if (code.Length == 2)
            {
                code = "000" + code;
            }
            else if (code.Length == 1)
            {
                code = "0000" + code;
            }
            return code;
        }

        #endregion 异业合作商品码

        #region 下载Excel

        /// <summary>
        /// 转换成流
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="columnNames"></param>
        /// <param name="outOfColumn"></param>
        /// <param name="sheetName"></param>
        /// <param name="title"></param>
        /// <param name="isProtected"></param>
        /// <returns></returns>
        public static Byte[] GetByteToExportExcel<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumn, string sheetName = "Sheet1", string title = "", int isProtected = 0)
        {
            using (var fs = new MemoryStream())
            {
                using (var package = CreateExcelPackage(datas, columnNames, outOfColumn, sheetName, title, isProtected))
                {
                    package.SaveAs(fs);
                    return fs.ToArray();
                }
            }
        }

        /// <summary>
        /// 生成Excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="columnNames"></param>
        /// <param name="outOfColumns"></param>
        /// <param name="sheetName"></param>
        /// <param name="title"></param>
        /// <param name="isProtected"></param>
        /// <returns></returns>
        private static ExcelPackage CreateExcelPackage<T>(List<T> datas, Dictionary<string, string> columnNames, List<string> outOfColumns, string sheetName = "Sheet1", string title = "", int isProtected = 0)
        {
            var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            if (isProtected == 1)
            {
                worksheet.Protection.IsProtected = true;//设置是否进行锁定
                worksheet.Protection.SetPassword("yunxi");//设置密码
                worksheet.Protection.AllowAutoFilter = false;//下面是一些锁定时权限的设置
                worksheet.Protection.AllowDeleteColumns = false;
                worksheet.Protection.AllowDeleteRows = false;
                worksheet.Protection.AllowEditScenarios = false;
                worksheet.Protection.AllowEditObject = false;
                worksheet.Protection.AllowFormatCells = false;
                worksheet.Protection.AllowFormatColumns = false;
                worksheet.Protection.AllowFormatRows = false;
                worksheet.Protection.AllowInsertColumns = false;
                worksheet.Protection.AllowInsertHyperlinks = false;
                worksheet.Protection.AllowInsertRows = false;
                worksheet.Protection.AllowPivotTables = false;
                worksheet.Protection.AllowSelectLockedCells = false;
                worksheet.Protection.AllowSelectUnlockedCells = false;
                worksheet.Protection.AllowSort = false;
            }

            var titleRow = 0;
            if (!string.IsNullOrWhiteSpace(title))
            {
                titleRow = 1;
                worksheet.Cells[1, 1, 1, columnNames.Count()].Merge = true;//合并单元格
                worksheet.Cells[1, 1].Value = title;
                worksheet.Cells.Style.WrapText = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;//水平居中
                worksheet.Cells[1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;//垂直居中
                worksheet.Row(1).Height = 30;//设置行高
                worksheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小
            }

            //获取要反射的属性,加载首行
            Type myType = typeof(T);
            List<PropertyInfo> myPro = new List<PropertyInfo>();
            int i = 1;
            foreach (string key in columnNames.Keys)
            {
                PropertyInfo p = myType.GetProperty(key);
                myPro.Add(p);

                var colDataType = "";//列字段类型
                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    colDataType = p.PropertyType.GetGenericArguments()[0].FullName;
                }
                else
                {
                    colDataType = p.PropertyType.FullName;
                }
                switch (colDataType)
                {
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                        worksheet.Cells[1, i, ExcelPackage.MaxRows, i].Style.Numberformat.Format = "0";
                        break;

                    case "System.Decimal":
                    case "System.Single":
                    case "System.Double":
                        worksheet.Cells[1, i, ExcelPackage.MaxRows, i].Style.Numberformat.Format = "0.00";
                        break;

                    case "System.DateTime":
                        worksheet.Cells[1, i, ExcelPackage.MaxRows, i].Style.Numberformat.Format = "yyyy-MM-dd HH:mm:ss";
                        break;

                    default:
                        worksheet.Cells[1, i, ExcelPackage.MaxRows, i].Style.Numberformat.Format = "@";
                        break;
                }

                worksheet.Cells[1 + titleRow, i].Value = columnNames[key];
                i++;
            }

            int row = 2 + titleRow;
            foreach (T data in datas)
            {
                int column = 1;
                foreach (PropertyInfo p in myPro.Where(info => !outOfColumns.Contains(info.Name)))
                {
                    worksheet.Cells[row, column].Value = p == null ? "" : p.GetValue(data, null);
                    column++;
                }
                row++;
            }
            return package;
        }

        #endregion 下载Excel

        public static string CutApiUrl(HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }

        /// <summary>
        /// 生成4位随机码
        /// </summary>
        /// <returns></returns>
        public static string GetYYTCode(string mchNo, string goodsNo)
        {
            Random rnd = new Random();
            string code = rnd.Next(1, 9999).ToString();
            if (code.Length == 3)
            {
                code = "0" + code;
            }
            else if (code.Length == 2)
            {
                code = "00" + code;
            }
            else if (code.Length == 1)
            {
                code = "000" + code;
            }

            return string.Concat("YYT", mchNo, goodsNo, code);
        }

        /// <summary>
        /// 文本解码
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string TxtDecode(string txt)
        {
            return HttpUtility.HtmlDecode(HttpUtility.UrlDecode(txt));
        }
    }
}
