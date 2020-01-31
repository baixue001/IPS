using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Common;

namespace ZoomLa.BLL.Helper
{
    /*
    1.根据需求引入相应的支持文件(dll)等
    2.调用方法生成

    */
    public class FileConver
    {
        /// <summary>
        /// 使用itext7 html2pdf组件转换
        /// </summary>
        /// <param name="htmlPath">html文件路径</param>
        /// <param name="pdfPath">PDF文件输出路径</param>
        public void HtmlToPdf(string htmlPath, string pdfPath)
        {
            /*
             * 1.支持<img/>图片组件(必须以传入文件信息的方式)
             *   * //html中的图片必须与html在同一目录,示例:<img src="icons.jpg" style="width:100%;" />
             *   * //也可直接使用base64格式,示例:<img src="base64" />
             * 2.已增加中文支持(可直接复制font目录下的字体文件)
             */
            //ConverterProperties props = new ConverterProperties();
            //FontProgram font = FontProgramFactory.CreateFont(@"D:\Code\ZoomLaCMS\ZoomLa.WebSite\Tools\MSYHL.TTC,1");
            //DefaultFontProvider defaultFontProvider = new DefaultFontProvider(false, false, false);
            //defaultFontProvider.AddFont(font);
            //props.SetFontProvider(defaultFontProvider);
            ////必须使用FileInfo,否则图片无法载入
            //FileInfo pdfFile = new FileInfo(function.VToP(pdfPath));
            //FileInfo htmlFile = new FileInfo(function.VToP(htmlPath));
            //HtmlConverter.ConvertToPdf(htmlFile, pdfFile, props);

        }
        /// <summary>
        /// 使用magicka.net组件转换
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <param name="imgPath"></param>
        public void PdfToImage(string pdfPath, string imgPath)
        {
            //using (FileStream fs = new FileStream(function.VToP(pdfPath), FileMode.Open))
            //{
            //    byte[] PDFbytes = IOHelper.StreamToBytes(fs);
            //    //设置dll文件的目录
            //    string dlllib = AppDomain.CurrentDomain.BaseDirectory + "bin/lib";
            //    MagickNET.SetGhostscriptDirectory(dlllib);
            //    MagickReadSettings setting = new MagickReadSettings();
            //    // Settings the density to 300 dpi will create an image with a better quality
            //    setting.Density = new Density(100);
            //    setting.BackgroundColor = MagickColor.FromRgb(255, 255, 255);
            //    setting.FillColor = MagickColor.FromRgb(0, 0, 0);
            //    setting.StrokeColor = MagickColor.FromRgb(0, 0, 0);
            //    // setting.Format = MagickFormat.Png;
            //    using (MagickImageCollection images = new MagickImageCollection())
            //    {
            //        // 读取二进制数组中的文件
            //        images.Read(PDFbytes, setting);
            //        using (MagickImage vertical = images.AppendVertically())
            //        {
            //            string path = function.VToP(imgPath);
            //            vertical.Write(path);
            //            byte[] ReusltByte = File.ReadAllBytes(path);
            //        }
            //    }
            //}

        }
        public void HtmlToImage(string html, string imgPath)
        {
            //html文件与图片放同一目录
            string htmlPath = imgPath.Split('.')[0] + ".html";
            string pdfPath = imgPath.Split('.')[0] + ".pdf";
            SafeSC.WriteFile(htmlPath, html);
            HtmlToPdf(htmlPath, pdfPath);
            PdfToImage(pdfPath, imgPath);
        }
        /*
            string html = SafeSC.ReadFileStr("/Tools/test.html";);
            HttpHelper.DownFile("https://thirdqq.qlogo.cn/g?b=sdk&k=3CNhGDhLgcv9Ob1UZua4VQ&s=100&t=1483321856", "/tools/face.jpg");
            html = html.Replace("@userFace", "face.jpg");
            new FileConver().HtmlToImage(html,"/tools/result.jpg");
         */
    }
}
