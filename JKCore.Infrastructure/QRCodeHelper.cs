using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using QRCoder;

namespace JKCore.Infrastructure
{
    public class QRCodeHelper
    {
        /// <summary>
        /// 生成网址二维码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Bitmap CreateWebUrl(string url)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(10);
            return qr;
        }

        public static Bitmap ScheduleIdQRCode(string url, string groupClassName, string groupScheduleDate, string coachName, string fieldName)
        {
            Bitmap image = new Bitmap(1086, 756);
            Graphics gh = Graphics.FromImage(image);
            gh.Clear(Color.White);

            FontFamily fontFamily = new FontFamily("Microsoft YaHei");
            string enSpace = "                                                            .";
            gh.DrawString($"课程名称：", new Font(fontFamily, 16), Brushes.Black, new PointF(100, 200));
            gh.DrawString($"{groupClassName}{enSpace}", new Font(fontFamily, 16, FontStyle.Bold), Brushes.Black, new PointF(205, 200));
            gh.DrawString($"上课时间：{groupScheduleDate}", new Font(fontFamily, 16), Brushes.Black, new PointF(100, 310));
            gh.DrawString($"教练：{coachName}{enSpace}", new Font(fontFamily, 16), Brushes.Black, new PointF(100, 420));
            gh.DrawString($"场地：{fieldName}{enSpace}", new Font(fontFamily, 16), Brushes.Black, new PointF(100, 530));

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(19);

            gh.DrawImage(qr, 450, 100);
            gh.Dispose();

            return image;
        }

        public static Bitmap TrainingQRCode(string url, string trainingName, string className, string trainingScheduleDate, string coachName, string fieldName)
        {
            string enSpace = "                                                            .";

            Bitmap image = new Bitmap(1086, 756);
            Graphics gh = Graphics.FromImage(image);
            gh.Clear(Color.White);
            gh.DrawString($"{trainingName}{enSpace}", new Font("Microsoft YaHei", 16, FontStyle.Bold), Brushes.Black, new PointF(100, 100));
            gh.DrawString($"课程名称：", new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(100, 200));
            gh.DrawString($"{className}{enSpace}", new Font("Microsoft YaHei", 16, FontStyle.Bold), Brushes.Black, new PointF(205, 200));
            gh.DrawString($"上课时间：", new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(100, 300));
            gh.DrawString(trainingScheduleDate, new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(100, 340));
            gh.DrawString($"教练：{coachName}{enSpace}", new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(100, 440));
            gh.DrawString($"场地：{fieldName}{enSpace}", new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(100, 540));

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(19);

            gh.DrawImage(qr, 360, 30);
            gh.Dispose();

            return image;
        }

        public static Bitmap TrainingCardQRCode(string url, string shopName)
        {
            Bitmap image = new Bitmap(1024, 768);
            Graphics gh = Graphics.FromImage(image);
            gh.Clear(Color.White);

            gh.DrawString($"{shopName}", new Font("Microsoft YaHei", 16), Brushes.Black, new PointF(470, 655));

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(19);

            gh.DrawImage(qr, 240, 30);
            gh.Dispose();

            return image;
        }
    }
}
