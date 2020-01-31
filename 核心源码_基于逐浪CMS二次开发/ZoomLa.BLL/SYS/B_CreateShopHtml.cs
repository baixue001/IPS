using System;
using System.Data;
using System.Text.RegularExpressions;
using ZoomLa.Model;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using ZoomLa.SQLDAL;
using System.Text;
using System.Data.SqlClient;
using ZoomLa.Common;
using ZoomLa.Components;
using System.Globalization;
using System.Web;
using System.Collections.Generic;
using ZoomLa.BLL.Helper;
using Microsoft.AspNetCore.Http;
using ZoomLa.Safe;

namespace ZoomLa.BLL
{
    /// <summary>
    /// 将多用户商城及互动模块标签转换成HTML
    /// </summary>
    public class B_CreateShopHtml
    {
        public HttpContext CurrentReq = null;
        public string rawurl="";//Get从其中获取值
        public B_CreateShopHtml(HttpContext context, string raw)
        {
            CurrentReq = context;
            rawurl = raw;
        }

        /// <summary>
        /// 方法标签处理，用于黄页内容页等(需要优化)
        /// </summary>
        /// <param name="TemplateContent">模板Html</param>
        /// <returns>解析后的内容</returns>
        public string CreateShopHtml(string TemplateContent)
        {
            string Result = TemplateContent;
            try
            {
                //对指定标签，如GetRequest等进处理解析,
                Result = GetRequest(Result);
                //Result = Publable(Result);
                //Result = Pubinputlable(Result);
                Result = GetUrldecode(Result);
                Result = GetUrlencode(Result);
                //Result = OutToExcel(Result);//只处理{$GetExcel 标签
                //Result = OutToWord(Result);
                //Result = OutToImage(Result);
            }
            catch (Exception ex) { WriteLog("CreateShopHtml", "", ex.Message); }
            return Result;
        }
        /// <summary>
        /// 转换URL编码
        /// </summary>
        public string GetUrlencode(string str)
        {
            string pattern = @"\{\$GetUrlencode\([\s\S]*?\)\$\}";
            MatchCollection matchs = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
            for (int i = 0; i < matchs.Count; i++)
            {

                MatchCollection Smallmatchs = Regex.Matches(matchs[i].ToString(), pattern, RegexOptions.IgnoreCase);
                foreach (Match Smallmatch in Smallmatchs)
                {
                    string requesttxt = Smallmatch.ToString().Replace(@"{$GetUrlencode(", "").Replace(@")$}", "");
                    string requestvalue =HttpUtility.UrlEncode(requesttxt);
                    str = str.Replace(Smallmatch.Value, requestvalue);
                }
            }
            return str;
        }
        /// <summary>
        /// URL反编码
        /// </summary>
        /// <param name="str">模板Html</param>
        /// <returns></returns>
        public string GetUrldecode(string str)
        {
            string pattern = @"\{\$GetUrldecode\([\s\S]*?\)\$\}";
            MatchCollection matchs = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
            for (int i = 0; i < matchs.Count; i++)
            {

                MatchCollection Smallmatchs = Regex.Matches(matchs[i].ToString(), pattern, RegexOptions.IgnoreCase);
                foreach (Match Smallmatch in Smallmatchs)
                {
                    string requesttxt = Smallmatch.ToString().Replace(@"{$GetUrldecode(", "").Replace(@")$}", "");
                    string requestvalue = HttpUtility.UrlDecode(requesttxt);// System.Web.CurrentReq.Server.UrlDecode(requesttxt);
                    str = str.Replace(Smallmatch.Value, requestvalue);
                }
            }
            return str;
        }
        #region 自定义页面参数
        /// <summary>
        /// 获取GET提交
        /// </summary>
        /// <param name="html">模板html</param>
        /// <returns></returns>
        public string GetRequest(string html)
        {
            string pattern = @"\{\$GetRequest\([a-z0-9]*\)\$\}";//{$GetRequest(变量名)$}
            string url = rawurl.ToLower();
            string query = url.Contains("?") ? url.Split('?')[1] : "";
            MatchCollection matchs = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);
            for (int i = 0; i < matchs.Count; i++)
            {
                MatchCollection Smallmatchs = Regex.Matches(matchs[i].ToString(), pattern, RegexOptions.IgnoreCase);
                foreach (Match Smallmatch in Smallmatchs)
                {
                    string requesttxt = (Smallmatch.ToString().Replace(@"{$GetRequest(", "").Replace(@")$}", "") ?? "").ToLower();//变量名
                    string result = "";
                    try
                    {
                        //从query中取值
                        result = StrHelper.GetValFromUrl(url, requesttxt);
                        //如为空,则检测是否为路由页面 /Shop/1  /Item/1
                        if (string.IsNullOrEmpty(result) && (requesttxt.Equals("id") || requesttxt.Equals("itemid")))
                        {
                            result = GetIDVal(url);
                        }
                        // /class_1/default
                        if (string.IsNullOrEmpty(result) && requesttxt.Equals("nodeid"))
                        {
                            result = Regex.Split(url, Regex.Escape("class_"))[1].Split('/')[0];
                        }
                        result = SafeValue(result);
                        //ZLLog.L(url + "|" + result + "|" + Smallmatchs[0].Value);
                    }
                    catch (Exception ex) { ZLLog.L(Model.ZLEnum.Log.exception, ex.Message); }
                    result = HttpUtility.HtmlEncode(result);
                    if (!string.IsNullOrEmpty(result) && SafeSC.CheckData(result)) { result = ""; ZLLog.L(ZLEnum.Log.safe, "GetRequest:" + result); }
                    html = html.Replace(Smallmatch.ToString(), result);
                }
            }
            return html;
        }
        private string SafeValue(string result)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result = HttpUtility.UrlDecode(result);
                result = result.Replace(" ", "").Replace(".", "");
                if (SafeC.CheckData(result)) { result = ""; }
                result = HttpUtility.HtmlEncode(result);
                if (result.Length > 20) { result = ""; }
            }
            else { result = ""; }
            return result;
        }
        private string GetIDVal(string url)
        {
            string[] pages = "/item/,/shop/".Split(',');
            foreach (string page in pages)
            {
                if (url.StartsWith(page))
                {
                    // /Item/10   /Item/10_2   
                    url = Regex.Split(url, Regex.Escape(page))[1].Split('.')[0];//10||10_2-->10||10_2
                    if (url.Contains("_"))
                    {
                        return url.Split('_')[0];// realurl = "~/Content?ID=" + url.Split('_')[0] + "&CPage=" + url.Split('_')[1];
                    }
                    else
                    {
                        return url;
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 获取POST提交
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string PostRequest(string str)
        {
            string pattern = @"\{\$PostRequest\([a-z0-9]*\)\$\}";
            MatchCollection matchs = Regex.Matches(str, pattern, RegexOptions.IgnoreCase);
            for (int i = 0; i < matchs.Count; i++)
            {
                MatchCollection Smallmatchs = Regex.Matches(matchs[i].ToString(), pattern, RegexOptions.IgnoreCase);
                foreach (Match Smallmatch in Smallmatchs)
                {
                    string requesttxt = Smallmatch.ToString().Replace(@"{$PostRequest(", "").Replace(@")$}", "");
                    string result = "";
                    try
                    {
                        if (CurrentReq.Request.Form[requesttxt] != "")
                        {
                            result = CurrentReq.Request.Form[requesttxt];
                        }
                        else
                        {
                            result = CurrentReq.Request.Cookies[requesttxt].ToString();
                        }
                    }
                    catch { }
                    result = SafeValue(result);
                    str = str.Replace(Smallmatch.ToString(), result);
                }
            }
            return str;
        }

        #endregion
        private void WriteLog(string func, string label, string message, string remind = "")
        {
            string tlp = "来源：B_CreateShopHtml,方法名：{0},标签：{1},报错：{2},备注：{3}";
            ZLLog.L(ZLEnum.Log.labelex, string.Format(tlp, func, label, message, remind));
        }
    }
}