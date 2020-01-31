using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.Common;

namespace ZoomLa.Extend
{
    /// <summary>
    /// 加载Html模板,命名按 TLP_模块_功能的方式
    /// </summary>
    public class ExTlp
    {
        public const string tlpDir = "/Extend/Common/TLP/";
        /// <summary>
        /// 用于加载radio等模板
        /// </summary>
        public static string LoadTlp(string name, string[] names, string[] values)
        {
            string tlpPath = tlpDir + name + ".html";
            string html = SafeSC.ReadFileStr(tlpPath);
            for (int i = 0; i < names.Length; i++)
            {
                html = html.Replace("@" + names[i], values[i]);
            }
            return html;
        }
        public static string ShowImagesView(string imgs)
        {
            string html = "";
            string tlp = "<img src=\"{0}\" class=\"imgView\" style='max-width:100px;max-height:100px;margin-right:5px;margin-bottom:5px;'/>";
            string[] imgArr = imgs.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string img in imgArr)
            {
                html += string.Format(tlp, img);
            }
            return "<div class='imagesView_wrap'>" + html + "</div>";
        }
        /// <summary>
        /// 手机下使用,可支持预览
        /// </summary>
        /// <param name="imgStr">图片1|图片2</param>
        /// <returns></returns>
        public static string ShowMobileImageView(object imgStr)
        {

            //if (imgStr.ToString().StartsWith("[")) { imgStr = ExHelper.JsonToImgs(imgStr.ToString()); }
            //<div class="ps-gallery" data-pswp-uid="1">
            //<figure>
            //    <div class="img-dv"><a href="{0}" data-size="1920x1080"><img src="{0}"></a></div>
            //    <figcaption style="display:none;">{1}</figcaption>
            //</figure>
            string html = "", imgTlp = "", imgWrap = "";
            imgWrap = "<div class=\"ps-gallery\" data-pswp-uid=\"" + function.GetRandomString(6, 2) + "\">{0}</div>";
            imgTlp = "<figure>";
            imgTlp += "<div class='img-dv'><a href='{0}' data-size='x'><img src='{0}'></a></div>";
            imgTlp += "<figcaption style=\"display:none;\"></figcaption>";
            imgTlp += "</figure>";
            string[] imgs = imgStr.ToString().Split('|');
            foreach (string img in imgs)
            {
                if (string.IsNullOrEmpty(img)) { continue; }
                html += string.Format(imgTlp, img);
            }
            return string.Format(imgWrap, html);
        }
        /// <summary>
        /// 移动端下显示的标题
        /// </summary>
        public static string ShowMobiletTitle(string title)
        {
            return LoadTlp("M_Title", new string[] { "title" }, new string[] { title });
        }
    }
}
