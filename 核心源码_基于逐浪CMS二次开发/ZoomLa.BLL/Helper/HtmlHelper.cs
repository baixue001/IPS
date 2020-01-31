
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

using ZoomLa.Common;
using System.Text.RegularExpressions;
using ZoomLa.Model.Collect;
using System.Web;
/*
* Html相关,基于HtmlParser
*/
namespace ZoomLa.BLL.Helper
{
    public class HtmlHelper
    {
        RegexHelper regHelper = new RegexHelper();
        public string vdir = "/UploadFiles/DownFile/";
        public string GetHtmlFromSite_NoEx(string url)
        {
            try
            {
                return GetHtmlFromSite(url);
            }
            catch (Exception ex) { return ex.Message; }
        }
        public string GetHtmlFromSite(string url)
        {
            //获取或设置用于对向   internet   资源的请求进行身份验证的网络凭据。（可有可无） 
            //wb.credentials=credentialcache.defaultcredentials;  
            url = url.Trim();//勿修改Url避免大小写敏感
            if (string.IsNullOrEmpty(url)) { throw new Exception("链接地址为空"); }
            if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://")) { throw new Exception(url + "并非有效的Http地址链接"); }
            try
            {
                WebClient wb = new WebClient();
                byte[] htmlData = wb.DownloadData(url);
                string result = Encoding.UTF8.GetString(htmlData);
                string head = regHelper.GetValueBySE(result, "<head>", "</head>").ToLower();
                List<string> metas = RegexHelper.GetValuesByWhere(head,"<meta",">");
                foreach (string meta in metas)
                {
                    if (meta.Contains("charset=gb2312") || meta.Contains("charset=\"gb2312\"") || meta.Contains("charset=gbk") || meta.Contains("charset=\"gbk\""))
                    {
                        result = Encoding.Default.GetString(htmlData);
                        break;
                    }
                }
                return result;
            }
            catch (Exception ex) { throw new Exception(url + ":" + ex.Message); }
        }
        //根据各种筛选条件,获取到需要的元素,后其看是否改为全Filter
        public string GetByFilter(string html, FilterModel model)//OR与AND都只能同时接受两个
        {
            string result = "";
            return result;
        }
        /// <summary>
        /// 替换其中的中文符号,如左右括号等
        /// </summary>
        public string ReplaceChinaChar(string article)
        {
            article = article.Replace("&#65288;", "(").Replace("&#65289;", ")").Replace("&#65292;", ",").Replace("&#65306;", "：").Replace("&#65311;", "?").Replace("&#65281;", "!");
            return article;
        }
        /// <summary>
        /// 获取BodyHtml,去除Script
        /// </summary>
        public string GetBodyHtml(string html)
        {
            return "";
        }
        /// <summary>
        /// 将img图片路径转为网路完整的图片路径
        /// </summary>
        /// <param name="html">需要转换的内容</param>
        /// <param name="url">替换站点路径:http://www.z01.com</param>
        /// <returns></returns>
        public string ConvertImgUrl(string html, string url)
        {
            return "";
        }
        //获取标题
        public string GetTitle(string html)
        {
            return "";
        }
        /// <summary>
        /// 从Html中获取所有超链接,必须以<html>包裹,自动过滤javascript:;等无效Url
        /// </summary>
        /// <param name="html">需要筛选的Html代码</param>
        /// <param name="pre">链接前加</param>
        /// <param name="end">链接后加</param>
        /// <param name="charcontain">必须包含指定字符</param>
        /// <returns></returns>
        public string GetAllLink(string html, M_Collect_ListFilter model)
        {
            return "";
        }
       
        //下载并存为mht格式
        public string DownToMHT(string url, string vpath)
        {
            return "";
        }
        #region 获取指定网站的新闻信息,已固定
        //------------------
        //获取新浪的内容页,id=artibody
        //------------------
        public string GetSinaArticle(string html)
        {
            return "";
        }
        #endregion
    }
    public class FilterModel
    {
        /// <summary>
        /// Html标识, 不允许为空,可多选
        /// </summary>
        public string EType { get; set; }
        public string ID { get; set; }
        public string CSS { get; set; }
        /// <summary>
        /// 开始与结束字符串,最后筛选队列
        /// </summary>
        public string Start { get; set; }
        public string End { get; set; }
        /// <summary>
        /// 是否筛除js脚本
        /// </summary>
        public bool AllowScript { get; set; }
        /// <summary>
        /// 附加规则,只取第一张图片的值(图片值不为空,且必须符合规范)
        /// </summary>
        public bool Ruler_FirstImg { get; set; }
        /// <summary>
        /// 采集网站的路径,可以使用目标网址,用于补全目标资源相对路径地址
        /// </summary>
        public string BaseUrl = "";
    }
    //使用内置的IE浏览 器完成操作
    public class IEBrowHelper
    {
        Bitmap m_Bitmap;
        private string html, stype = "img";
        string m_Url;
        int m_BrowserWidth, m_BrowserHeight, m_ThumbnailWidth, m_ThumbnailHeight;
        public IEBrowHelper() { }
        public IEBrowHelper(string Url, int BrowserWidth, int BrowserHeight, int ThumbnailWidth, int ThumbnailHeight)
        {
            m_Url = Url;
            m_BrowserHeight = BrowserHeight;
            m_BrowserWidth = BrowserWidth;
            m_ThumbnailWidth = ThumbnailWidth;
            m_ThumbnailHeight = ThumbnailHeight;
        }
        public string GetHtmlFromSite(string url)
        {
            return "";
        }

    }
}
