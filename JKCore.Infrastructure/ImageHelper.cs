using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Shapes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;

namespace JKCore.Infrastructure
{
    public class ImageHelper
    {

        #region  验证码长度(默认6个验证码的长度)    
        int length = 4;
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        #endregion
        #region 验证码字体大小(为了显示扭曲效果，默认40像素，可以自行修改)
        int fontSize = 40;
        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }
        #endregion
        #region 边框补(默认1像素)    
        int padding = 2;
        public int Padding
        {
            get { return padding; }
            set { padding = value; }
        }
        #endregion
        #region 自定义背景色(默认白色)    
        Rgba32 backgroundColor = Rgba32.White;
        public Rgba32 BackgroundColor
        {
            get { return backgroundColor; }
            set { backgroundColor = value; }
        }
        #endregion
        #region 是否输出燥点(默认不输出)    
        bool chaos = true;
        public bool Chaos
        {
            get { return chaos; }
            set { chaos = value; }
        }
        #endregion
        #region 输出燥点的颜色(默认灰色)    
        Color chaosColor = Color.LightGray;
        public Color ChaosColor
        {
            get { return chaosColor; }
            set { chaosColor = value; }
        }
        #endregion
        #region 自定义随机颜色数组    
        //Color[] colors = { Color.Black, Color.Red, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple }
        //public Color[] Colors
        //{
        //    get { return colors; }
        //    set { colors = value; }
        //}
        Rgba32[] rgbas = { Rgba32.Black, Rgba32.Red, Rgba32.DarkBlue, Rgba32.Green, Rgba32.Orange, Rgba32.Brown, Rgba32.DarkCyan, Rgba32.Purple };
        public Rgba32[] Rgbas
        {
            get { return rgbas; }
            set { rgbas = value; }
        }
        #endregion

        #region 自定义字体数组    
        string[] fonts = { "Arial", "Georgia" };
        public string[] Fonts
        {
            get { return fonts; }
            set { fonts = value; }
        }
        #endregion
        public byte[] GetValidCodeByte(string fontPath,string code)
        {

            int fSize = FontSize;
            int fWidth = fSize + Padding;

            int imageWidth = (int)(code.Length * fWidth) + 4 + Padding * 2;
            int imageHeight = fSize * 2 + Padding;

            var bb = default(byte[]);
            try
            {
                var dianWith = 1; //点宽度
                //var xx_space = 10;  //点与点之间x坐标间隔
                //var yy_space = 5;    //y坐标间隔
                var wenZiLen = code.Length;  //文字长度
                //var maxX = imageWidth / wenZiLen; //每个文字最大x宽度
                //var prevWenZiX = 0; //前面一个文字的x坐标
                //var size = 16;//字体大小
                List<SixLabors.Fonts.Font> myfonts = new List<SixLabors.Fonts.Font>();
                foreach (string fontName in fonts)
                {
                    //var install_Family = new FontCollection().Install($@"C:\MyWork\ziti\{fontName}.TTF");
                    //var install_Family = new FontCollection().Find("Arial");
                    myfonts.Add(new SixLabors.Fonts.Font(new FontCollection().Install($"{fontPath}{fontName}.TTF"), 50, SixLabors.Fonts.FontStyle.Bold));
                }
                //点坐标
                var listPath = new List<IPath>();

                Random rand = new Random();
                //给背景添加随机生成的燥点    
                if (this.Chaos)
                {
                    //Pen pen = new Pen(ChaosColor, 0);
                    int c = (Length) * 10*2;

                    for (int i = 0; i < c; i++)
                    {
                        int x = rand.Next(imageWidth);
                        int y = rand.Next(imageHeight);

                        var position = new Vector2(x, y);
                        var linerLine = new LinearLineSegment(position, position);
                        var shapesPath = new SixLabors.Shapes.Path(linerLine);
                        listPath.Add(shapesPath);
                    }
                }

                int left = 0, top = 0, top1 = 1, top2 = 1;
                int n1 = (imageHeight - FontSize - Padding * 2);
                int n2 = n1 / 4;
                top1 = n2;
                top2 = n2 * 2;
                //画图
                using (Image<Rgba32> image = new Image<Rgba32>(imageWidth, imageHeight))   //画布大小
                {
                    image.Mutate(x =>
                    {
                        var imgProc = x;
                        int cindex, findex;

                        //随机字体和颜色的验证码字符    
                        for (int i = 0; i < wenZiLen; i++)
                        {
                            cindex = rand.Next(Rgbas.Length);
                            findex = rand.Next(Fonts.Length);

                            var font = myfonts[findex];
                            var rgba = Rgbas[cindex];
                            //当前的要输出的字
                            var nowWenZi = code.Substring(i, 1);

                            //文字坐标
                            var wenXY = new Vector2();
                            if (i % 2 == 1)
                            {
                                top = top2;
                            }
                            else
                            {
                                top = top1;
                            }

                            wenXY.X = i * fWidth;
                            wenXY.Y = top;
                            //prevWenZiX = Convert.ToInt32(Math.Floor(wenXY.X)) + fSize * 2;
                            imgProc.DrawText(nowWenZi, font, rgba, wenXY);
                        }

                        //逐个画字
                        //for (int i = 0; i < wenZiLen; i++)
                        // {
                        //     cindex = rand.Next(Rgbas.Length);
                        //     findex = rand.Next(Fonts.Length);

                        //     var font = myfonts[findex];
                        //     var rgba = Rgbas[cindex];
                        //     //当前的要输出的字
                        //     var nowWenZi = content.Substring(i, 1);

                        //     //文字坐标
                        //     var wenXY = new Vector2();
                        //     var maxXX = prevWenZiX + (maxX - fSize);
                        //     wenXY.X = new Random().Next(prevWenZiX, maxXX);
                        //     wenXY.Y = new Random().Next(0, imageHeight - fSize);

                        //     prevWenZiX = Convert.ToInt32(Math.Floor(wenXY.X)) + fSize;

                        //     //画字
                        //     imgProc.DrawText(nowWenZi, font, rgba, wenXY);
                        //     //imgProc.DrawText(nowWenZi, font, i % 2 > 0 ? Rgba32.HotPink : Rgba32.Red, wenXY,TextGraphicsOptions.Default);
                        // }

                        //画点 
                        imgProc.BackgroundColor(Rgba32.WhiteSmoke).   //画布背景
                                     Draw(
                                     SixLabors.ImageSharp.Processing.Pens.Dot(Rgba32.HotPink, dianWith),   //大小
                                     new SixLabors.Shapes.PathCollection(listPath)  //坐标集合
                                 ).DrawLines(Rgba32.Red, //字体颜色
                            2,   //字体大小
                            new SixLabors.Primitives.PointF[]{
                                    new Vector2(10, 10),
                                    new Vector2(imageWidth, imageHeight)
                            }).DrawLines(Rgba32.Black, //字体颜色
                            2,   //字体大小
                            new SixLabors.Primitives.PointF[]{
                                    new Vector2(10, imageHeight),
                                    new Vector2(imageWidth, 10)
                            }); //两点一线坐标);

                    });
                    using (MemoryStream stream = new MemoryStream())
                    {
                        image.SaveAsPng(stream);
                        bb = stream.GetBuffer();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return bb;
        }
       
    }
}
