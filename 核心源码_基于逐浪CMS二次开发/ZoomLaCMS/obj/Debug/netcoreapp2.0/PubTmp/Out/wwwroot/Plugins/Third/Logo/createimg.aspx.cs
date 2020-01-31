
using Newtonsoft.Json;
using Svg;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using ZoomLa.BLL.AdSystem;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.LOGO
{
    public partial class createimg : System.Web.UI.Page
    {
        //*字体安装后,目标机器需要重启下电脑,否则字体有可能不生效
        //1,DrawString画出的字体,边框颜色与字体颜色不同
        //答:g.Clear(Color.White); 使用与背景一致的颜色,透明色会有异色描边的情况
        B_Logo_Icon iconBll = new B_Logo_Icon();
        //-----------------------------
        public string Action { get { return DataConvert.CStr(Request.QueryString["action"]); } }
        protected void Page_Load(object sender, EventArgs e)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            switch (Action)
            {
                case "text":
                    {
                        FontPack model = new FontPack().FromJson(Request["model"]);
                        if (string.IsNullOrEmpty(model.text)) { throw new Exception("文字内容不能为空"); }
                        int bmpLen = 0;
                        int size = GetFontSize(model.text, ref bmpLen);//自动计算出文字大小
                        //------------------------------------
                        Bitmap empty = new Bitmap(bmpLen, (int)(size * 1.5));//创建图片背景
                        Graphics g = Graphics.FromImage(empty);
                        //g.Clear(model.ColorHx16toRGB(model.bkcolor));//清除画面，填充背景
                        g.Clear(Color.FromArgb(0));
                        Font mFont = new Font(model.family, size, FontStyle.Regular);
                        g.DrawString(model.text, mFont,
                           new SolidBrush(model.ColorHx16toRGB(model.color)),//Color.FromArgb(100,255,255,255)
                           new Point(0, 0));
                        empty.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    break;
                case "logo":
                default:
                    {
                        //int LogoID = DataConvert.CLng(Request.QueryString["LogoID"]);
                        //string svgPath = Server.MapPath(iconBll.PlugDir + "assets/icons/" + LogoID + ".svg");
                        //SvgDocument svg = SvgDocument.Open(svgPath);
                        //Bitmap bmp = svg.Draw();
                        //System.Drawing.Image empty = ImgHelper.ReadImgToMS(iconBll.PlugDir + "assets/empty.png");
                        //Graphics g = Graphics.FromImage(empty);
                        ////SizeF textSize = g.MeasureString(CompName, new Font(""));
                        //g.DrawImage(bmp, new Point() { X = 95, Y = 40 });//400,400,商标也固定大小
                        //                                                 //添加文字
                        //Font mFont = new Font(SelFont("m"), GetFontSize(CompName), FontStyle.Regular);
                        //Font sFont = new Font(SelFont("s"), (GetFontSize(SubTitle) / 2), FontStyle.Regular);
                        //g.DrawString(CompName, mFont,
                        //    new SolidBrush(Color.FromArgb(100, 0, 0, 0)),
                        //    GetPosition(empty, g.MeasureString(CompName, mFont), 265));
                        //g.DrawString(SubTitle, sFont,
                        //    new SolidBrush(Color.FromArgb(100, 102, 102, 102)),
                        //    GetPosition(empty, g.MeasureString(SubTitle, sFont), 305 + (int)mFont.Size));
                        //empty.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    break;
            }

            //------------
            Response.Cache.SetNoStore();
            Response.ClearContent();
            Response.ContentType = "image/gif";
            Response.BinaryWrite(ms.ToArray());
        }
        private Point GetPosition(Image image, SizeF textSize, int posy)
        {
            int x = (image.Width - (int)textSize.Width) / 2 + 5;
            return new Point(x, posy);
        }
        //根据字符数计算出Size,最多十个字符
        private int GetFontSize(string text, ref int mapLen)//400
        {
            //六空格==三英文==两汉字 (1,2,3),以空格作为基准单位,每个英文占两空格,每个汉字占三空格位
            int size = 0; mapLen = 0;
            if (text.Length <= 4) { size = 45; }
            else if (text.Length <= 6) { size = 35; }
            else { size = 25; }
            foreach (char t in text.ToCharArray())
            {
                if (t == ' ') { mapLen += size / 3; }
                else if ((t >= 0x4e00 && t <= 0x9fbb)) { mapLen += (int)(size * 1.5); }
                else
                {
                    //数字,英文,标点符号
                    //length += 2;
                    mapLen += size;
                }
            }
            return size;
        }
        //----------------
        private string SelFont(string type)
        {
            string font = Request["font"];
            if (string.IsNullOrEmpty(font))
            {
                if (type == "m") { font = "逐浪拉勾艺黑体";  }
                else { font = "逐浪拉勾艺黑体"; }
            }
            return font;
        }
        //-------------------------------------------------
        //  bkcolor: "#000", color: "#fff", text: "", size: "", family: "", addon: ""
        public class FontPack
        {
            public string bkcolor = "";
            public string color = "";
            public string text = "";
            public int size = 12;
            public string family = "";
            public string addon = "";
            //字体生成的方向  水平|垂直
            public string direction = "horizontal";//horizontal|vertical
            public FontPack FromJson(string json)
            {
                FontPack model = new FontPack();
                if (string.IsNullOrEmpty(json)) { return model; }
                json = HttpUtility.UrlDecode(json);
                model = JsonConvert.DeserializeObject<FontPack>(json);
                //------------------
                if (string.IsNullOrEmpty(model.bkcolor)) { model.bkcolor = ""; }
                if (string.IsNullOrEmpty(model.color)) { model.color = "#fff"; }
                if (model.size < 1) { model.size = 12; }
                return model;
            }
            public System.Drawing.Color ColorHx16toRGB(string strHxColor)
            {
                if (string.IsNullOrEmpty(strHxColor))
                {
                    //return System.Drawing.Color.FromArgb(0, 0, 0);//设为黑色
                    return System.Drawing.Color.FromArgb(0);//使用透明色
                }
                else
                {
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(System.Int32.Parse(strHxColor.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier), System.Int32.Parse(strHxColor.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier), System.Int32.Parse(strHxColor.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier));
                    return color;
                }
            }
            /// <summary>
            /// [颜色：RGB转成16进制]
            /// </summary>
            /// <param name="R">红 int</param>
            /// <param name="G">绿 int</param>
            /// <param name="B">蓝 int</param>
            /// <returns></returns>
            public string ColorRGBtoHx16(int R, int G, int B)
            {
                return System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.FromArgb(R, G, B));
            }
        }
    }
}