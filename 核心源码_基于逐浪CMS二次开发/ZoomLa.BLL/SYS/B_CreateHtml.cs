using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.SYS;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
namespace ZoomLa.BLL
{
    public class AsyncModel
    {
        public object func;
        public object result;
    }
    public class ContextHelper
    {
        public static string RawUrl(HttpRequest request)
        {
            return new StringBuilder()
                 .Append(request.PathBase)
                 .Append(request.Path)
                 .Append(request.QueryString)
                 .ToString();
        }
        public static string FullUrl(HttpRequest request)
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
    }
    public class B_CreateHtml
    {
        private B_Content bcontent = new B_Content();
        private B_CreateShopHtml shophtml = null;
        //private B_DataSource dsBll = new B_DataSource();
        private B_User buser = null;
        private B_Model bmodel = new B_Model();
        private B_ModelField fieldBll = new B_ModelField();
        private B_Node nodeBll = new B_Node();
        //private B_FTP ftpBll = new B_FTP();
        private RegexHelper regexHelper = new RegexHelper();
        private static string ManageDir = string.IsNullOrEmpty(SiteConfig.SiteOption.ManageDir) ? "/manage/" : "/" + SiteConfig.SiteOption.ManageDir + "/";
        private static string UploadDir = SiteConfig.SiteOption.UploadDir;
        // http://win05:86/tools/example?id=2&name=test
        private string fullUrl = "";

        // /tools/example?id=2&name=test
        private string rawUrl = "";

        private string m_UrlReferrer = "";
        private HttpContext CurrentReq = null;
        private delegate string GetContentFunc(int Cpage, int InfoID, string urltype, string Result, HttpContext current);
        public string UrlReferrer
        {
            set
            {
                if (string.IsNullOrEmpty(m_UrlReferrer))
                    m_UrlReferrer = value;
            }
            get
            {
                if (!string.IsNullOrEmpty(m_UrlReferrer))
                {
                    return m_UrlReferrer;
                }
                else
                {
                    return "";
                }
            }
        }
        private string urlAuthority = SiteConfig.SiteInfo.SiteUrl.Replace("http://", "").Replace("https://", "");
        #region 消息
        private const string Err_Lack_Field = "[ERR:({0})标签没有指定要查询的字段]";
        private const string Err_Lack_TbName = "[ERR:({0})标签没有指定要查询的数据表]";
        private const string Err_Lack_Param = "[ERR:({0})标签缺少参数]";
        private const string Err_LabelError = "[ERR:({0})标签解析错误]";//SQL语句报错等
        private const string Err_Param_Error = "[ERR:({0})的参数不正确,{1}]";
        #endregion
        //异步,多线程
        public B_CreateHtml(HttpContext context)
        {
            try
            {
                LabelCache.Clear();
                CurrentReq = context;
                rawUrl = ContextHelper.RawUrl(context.Request);
                fullUrl = ContextHelper.FullUrl(context.Request);
                shophtml = new B_CreateShopHtml(context, rawUrl);
                buser = new B_User(CurrentReq);
            }
            catch (Exception ex)
            {
                WriteLog("B_CreateHtml(HttpRequest r)", "", ex.Message);
            }
        }
        /// <summary>
        /// 将模板内容转换成Html代码(普通页使用)
        /// </summary>
        /// <param name="TemplateContent">模板Html</param>
        /// <param name="Cpage">当前页,只有SQL语句拼接时才使用了其,其他的方法只负责传值</param>
        /// <param name="urltype">用于分页,PageCode中的PageUrl</param>
        /// <returns>解析后的模板Html</returns>
        public string CreateHtml(string TemplateContent, int Cpage = 1, int InfoID = 0, string urltype = "0")
        {
            string Result = TemplateContent;
            try
            {
                this.UrlReferrer = urlAuthority;
                if (!CheckisTrue(Result)) { return Result; }
                //if (Result.Contains("{Split/}"))
                //{
                //    //先处理数据源标签,避免分段后无法完成替换
                //    Result = GetContent_Source(Cpage, InfoID, Result);
                //    string[] temp = Regex.Split(Result, "{Split/}");
                //    List<AsyncModel> list = new List<AsyncModel>();
                //    for (int i = 0; i < temp.Length; i++)
                //    {
                //        AsyncModel model = new AsyncModel();
                //        GetContentFunc func = GetContent;
                //        model.result = func.BeginInvoke(Cpage, InfoID, urltype, temp[i], CurrentReq, null, null);
                //        model.func = func;
                //        list.Add(model);
                //    }
                //    Result = "";
                //    foreach (AsyncModel model in list)
                //    {
                //        Result += ((GetContentFunc)model.func).EndInvoke(model.result as System.IAsyncResult);
                //    }
                //}
                //else
                //{
                   Result = GetContent(Cpage, InfoID, urltype, Result);
                //}
                Result = Result.Replace("{hmw:cs}", "");
                Result = Result.Replace("{hmw:js}", "");
                Result = Result.Replace("{hmw:jsloop}", "");
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog("CreateHtml()", "", ex.Message);
                return Result;
            }
        }
        /// <summary>
        /// 将模板内容转换成Html代码(黄页使用)
        /// </summary>
        /// <param name="pagenum">黄页的PageID</param>
        public string CreateHtml(string TemplateContent, int Cpage, int InfoID, int pagenum)
        {
            string Result = TemplateContent;
            try
            {
                if (CheckisTrue(Result))
                {
                    Result = GetContent(Cpage, InfoID, pagenum.ToString(), Result);//PageConvert
                    Result = Result.Replace("{hmw:cs}", ""); //去掉判断临时标签:)
                    Result = Result.Replace("{hmw:js}", "");
                    Result = Result.Replace("{hmw:jsloop}", "");
                }
                return Result;
            }
            catch (Exception ex)
            {
                WriteLog("CreateHtml By One", "", ex.Message);
                return Result;
            }
        }
        /// <summary>
        /// 转换标签
        /// </summary>
        /// <param name="Cpage"></param>
        /// <param name="InfoID"></param>
        /// <param name="urltype"></param>
        /// <param name="Result"></param>
        /// <param name="current">当前上下文</param>
        /// <returns></returns>
        private string GetContent(int Cpage, int InfoID, string urltype, string Result, HttpContext current = null)
        {
            if (current != null)
            {
                CurrentReq = current;
                buser = new B_User(current);
                shophtml.CurrentReq = current;
            }
            string mat = "";
            string pattern = @"{\$([\s\S])*?\/}";
            Result = shophtml.CreateShopHtml(Result);
            // 正则表达式查找数据源标签
            Result = GetContent_Source(Cpage, InfoID, Result);
            #region 正则表达式查找自定义标签
            //pattern = @"{ZL\.Label([\s\S])*?\/}";
            //flag = false;
            //do
            //{
            //    flag = false;
            //    MatchCollection matchs = Regex.Matches(Result, pattern, RegexOptions.IgnoreCase);
            //    foreach (Match match in matchs)//自定义
            //    {
            //        Result = shophtml.CreateShopHtml(Result);
            //        mat = match.Value;
            //        Result = Result.Replace(mat, ContentLabelProc(mat, Cpage, InfoID, urltype));
            //        flag = true;
            //    }
            //}
            //while (flag);
            do
            {
                mat = GetLabelFromHtml(Result);
                if (string.IsNullOrWhiteSpace(mat)) { break; }
                Result = shophtml.CreateShopHtml(Result);
                Result = Result.Replace(mat, ContentLabelProc(mat, Cpage, InfoID, urltype));
            } while (true);
            #endregion
            #region 替换系统标签
            {
                pattern = @"{\$([\s\S])*?\/}";
                MatchCollection matchs = Regex.Matches(Result, pattern, RegexOptions.IgnoreCase);
                if (matchs.Count > 0)
                {
                    foreach (Match match in matchs)
                    {
                        Result = shophtml.CreateShopHtml(Result);
                        mat = match.Value;
                        Result = Result.Replace(mat, SysLabelProc(mat));
                    }
                }
            }
            #endregion
            Result = Deal_FunLabel(Result);
            //再使用一次,避免遗漏
            Result = shophtml.CreateShopHtml(Result);
            return Result;
        }
        //转换标签_数据源
        private string GetContent_Source(int Cpage, int InfoID, string Result)
        {
            string pattern = @"{ZL\.Source([\s\S])*?\/}";
            bool flag = false;
            do
            {
                flag = false;
                MatchCollection matchs = Regex.Matches(Result, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matchs)//数据源
                {
                    Result = shophtml.CreateShopHtml(Result);
                    string mat = match.Value;
                    Result = ContentSourceLabelProc(mat, Cpage, InfoID, Result);
                    flag = true;
                }
            }
            while (flag);
            return Result;
        }
        #region 数据源标签
        /// <summary>
        /// 数据源标签处理
        /// </summary>
        private string ContentSourceLabelProc(string ilabel, int Cpage, int InfoID, int pagenum, string Result)
        {
            string sqllabelcontent = "";
            M_Label label = GetLabelXML(ilabel);
            if (!string.IsNullOrEmpty(label.ErrorMsg)) { Result = Result.Replace(ilabel, label.ErrorMsg); return Result; }
            try
            {
                if (label.LabelType == 1)//静态标签
                {
                    sqllabelcontent = Getjude(Cpage, InfoID, label);
                    return sqllabelcontent;
                }
                else if (string.IsNullOrEmpty(label.LabelField))
                {
                    return string.Format(Err_Lack_Field, label.LabelName);
                }
                else if (string.IsNullOrEmpty(label.LabelTable))
                {
                    return string.Format(Err_Lack_TbName, label.LabelName);
                }
                label.Content = Getjude(Cpage, InfoID, label);
                M_Label mylabel = LabelParamFunc(label, ilabel, InfoID.ToString());//处理其中的@参数
                string sqlCount = mylabel.LabelCount;
                string sqlField = mylabel.LabelField;
                string sqlTable = mylabel.LabelTable;
                string sqlWhere = mylabel.LabelWhere;
                string sqlOrder = mylabel.LabelOrder;
                string temp = Result;
                #region 让查询支持扩展标签
                if (CheckisTrue(sqlTable))
                {
                    sqlTable = shophtml.CreateShopHtml(sqlTable);
                    sqlTable = GetContent(0, 0, "0", sqlTable);
                }

                if (CheckisTrue(sqlWhere))
                {
                    sqlWhere = shophtml.CreateShopHtml(sqlWhere);
                    sqlWhere = GetContent(0, 0, "0", sqlWhere);
                }
                if (CheckisTrue(sqlCount))
                {
                    sqlCount = shophtml.CreateShopHtml(sqlCount);
                    sqlCount = GetContent(0, 0, "0", sqlCount);
                }
                if (CheckisTrue(sqlOrder))
                {
                    sqlOrder = shophtml.CreateShopHtml(sqlOrder);
                    sqlOrder = GetContent(0, 0, "0", sqlOrder);
                }
                #endregion
                DataTable dt = SelByLabel(mylabel, DataConverter.CLng(sqlCount), sqlTable, sqlField, sqlWhere, sqlOrder).dt;
                temp = temp.Replace(ilabel, "");
                string htmlcode = GetHtmlSource(temp, label.LabelName, dt);
                if (htmlcode.IndexOf("{SField") > -1)
                {
                    htmlcode = GetHtmlSource(htmlcode, label.LabelName, dt); //GetSourceField(result, dr1, LabelName);
                }
                return htmlcode;
            }
            catch (Exception ex) { WriteLog("ContentSourceLabelProc", ilabel, ex.Message); return string.Format(Err_LabelError, label.LabelName); }
        }
        /// <summary>
        /// 处理数据源标签(create中进此)[main]
        /// </summary>
        /// <param name="ilabel">标签名</param>
        /// <param name="Cpage">当前页,无则为0</param>
        /// <param name="InfoID">内容GeneralID</param>
        /// <param name="Result">待解析的模板</param>
        private string ContentSourceLabelProc(string ilabel, int Cpage, int InfoID, string Result)
        {
            string sqllabelcontent = "";
            M_Label label = GetLabelXML(ilabel);
            if (!string.IsNullOrEmpty(label.ErrorMsg)) { Result = Result.Replace(ilabel, label.ErrorMsg); return Result; }
            try
            {
                if (label.LabelType == 1)//静态标签
                {
                    sqllabelcontent = Getjude(Cpage, InfoID, label);
                    return sqllabelcontent;
                }
                else if (string.IsNullOrEmpty(label.LabelField))
                {
                    return string.Format(Err_Lack_Field, label.LabelName);
                }
                else if (string.IsNullOrEmpty(label.LabelTable))
                {
                    return string.Format(Err_Lack_TbName, label.LabelName);
                }
                label.Content = Getjude(Cpage, InfoID, label);
                M_Label mylabel = LabelParamFunc(label, ilabel, InfoID.ToString());//处理其中的@参数
                string sqlCount = mylabel.LabelCount;
                string sqlField = mylabel.LabelField;
                string sqlTable = mylabel.LabelTable;
                string sqlWhere = mylabel.LabelWhere;
                string sqlOrder = mylabel.LabelOrder;
                string temp = Result;
                #region 让查询支持扩展标签

                if (CheckisTrue(sqlTable))
                {
                    sqlTable = shophtml.CreateShopHtml(sqlTable);
                    sqlTable = GetContent(0, 0, "0", sqlTable);
                }
                if (CheckisTrue(sqlWhere))
                {
                    sqlWhere = shophtml.CreateShopHtml(sqlWhere);
                    sqlWhere = GetContent(0, 0, "0", sqlWhere);
                }
                if (CheckisTrue(sqlCount))
                {
                    sqlCount = shophtml.CreateShopHtml(sqlCount);
                    sqlCount = GetContent(0, 0, "0", sqlCount);
                }
                if (CheckisTrue(sqlOrder))
                {
                    sqlOrder = shophtml.CreateShopHtml(sqlOrder);
                    sqlOrder = GetContent(0, 0, "0", sqlOrder);
                }

                #endregion
                DataTable dt = SelByLabel(mylabel, DataConverter.CLng(sqlCount), sqlTable, sqlField, sqlWhere, sqlOrder).dt;
                temp = temp.Replace(ilabel, "");//替换掉数据源标签
                string htmlcode = GetHtmlSource(temp, label.LabelName, dt);//获取DT,在此处理
                if (htmlcode.IndexOf("{SField") > -1)
                {
                    htmlcode = GetHtmlSource(htmlcode, label.LabelName, dt); //GetSourceField(result, dr1, LabelName);  
                }
                return htmlcode;
            }
            catch (Exception ex) { WriteLog("ContentSourceLabelProc_2", ilabel, ex.Message); return string.Format(Err_LabelError, label.LabelName); }
        }
        /// <summary>
        /// 将数据源中的字段替换标签中的字段
        /// </summary>
        /// <param name="content">模板内容</param>
        /// <param name="LabelName">标签名称</param>
        /// <param name="dt">数据表</param>
        /// <returns></returns>
        private string GetHtmlSource(string content, string LabelName, DataTable dt)
        {
            string result = content;
            StringBuilder sb = new StringBuilder();
            string pattern = @"{Repeate}([\s\S])*?{/Repeate}";
            string temp = "";
            //有循环列表，换算循环列表部分
            if (Regex.IsMatch(result, pattern, RegexOptions.IgnoreCase))
            {
                temp = Regex.Match(result, pattern, RegexOptions.IgnoreCase).Value;
                temp = temp.Replace("{Repeate}", "").Replace("{/Repeate}", "");
                foreach (DataRow dr in dt.Rows)
                {
                    string s = temp;
                    string pattern1 = @"{SField([\s\S])*?/}";
                    MatchCollection matchs = Regex.Matches(s, pattern1, RegexOptions.IgnoreCase);
                    foreach (Match match in matchs)
                    {
                        s = s.Replace(match.Value, GetSourceField(match.Value, dr, LabelName));
                    }
                    sb.Append(s);
                }
                result = result.Replace(temp, sb.ToString()).Replace("{Repeate}", "").Replace("{/Repeate}", "");
            }

            string pattern2 = @"{SField([\s\S])*?/}";
            MatchCollection matchs1 = Regex.Matches(result, pattern2, RegexOptions.IgnoreCase);
            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr1 = dt.Rows[0];
                foreach (Match match1 in matchs1)
                {
                    result = result.Replace(match1.Value, GetSourceField(match1.Value, dr1, LabelName));
                }
            }
            else
            {
                foreach (Match match1 in matchs1)
                {
                    result = result.Replace(match1.Value, "[暂无记录]");
                }
            }
            if (dt != null)
                dt.Dispose();
            return result;
        }
        /// <summary>
        /// 解析引用了数据源标签页面的字段(未优化)
        /// </summary>
        private string GetSourceField(string ilabel, DataRow dr, string LabelName)
        {
            string re = ilabel;
            string lbl = ilabel;
            string onlbl = ilabel;
            string relbl = "";
            string[] sourcelable = lbl.Split(new string[] { "{SField" }, StringSplitOptions.RemoveEmptyEntries);
            if (sourcelable.Length > 1)
            {
                lbl = "{SField" + sourcelable[sourcelable.Length - 1];
                relbl = "{SField" + sourcelable[sourcelable.Length - 1];
            }

            lbl = lbl.Replace("{SField ", "").Replace("/}", "").Replace(" ", ",");

            string[] paArr = lbl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            string lblname = paArr[0].Split(new char[] { '=' })[1].Replace("\"", "");
            string fname = paArr[1].Split(new char[] { '=' })[1].Replace("\"", "");
            string type = "0";
            try
            {
                if (paArr.Length > 2)
                {
                    type = paArr[2].Split(new char[] { '=' })[1].Replace("\"", "");

                    type = GetContent(0, 0, "0", type);
                }


                if (lblname == LabelName)
                {
                    string filevalue = dr[fname] is DBNull ? "" : dr[fname].ToString();//字段值赋值
                    re = filevalue;//获得数据源的值
                }

                if (sourcelable.Length > 1)
                {
                    re = onlbl.Replace(relbl, re);
                }
                else
                {
                    if (type == "1")
                    {
                        re = "{#Content}" + re + "{/#Content}";
                    }

                    if (type == "2")
                    {
                        re = "{#PageCode}" + re + "{/#PageCode}";
                    }
                }

            }
            catch
            {
                return "";
            }
            dr = null;
            return re;
        }
        #endregion
        #region 分页标签与相关
        /// <summary>
        /// 依据字符数切割内容,返回切割后的内容数组
        /// </summary>
        /// <param name="Content">要处理的内容</param>
        /// <param name="NumperPage">每页字符数</param>
        /// <returns>数组，每个元素代表某页显示的内容</returns>
        public IList<string> GetContentPage(string Content, int NumperPage)
        {
            string tmpContent = Content;
            int length = Content.Length;
            int PageNum = 0;
            IList<string> ContentList = new List<string>();
            string pageContent = ""; //每页内容存储变量
            if ((NumperPage > 0) && (length > NumperPage))
            {
                PageNum = length / NumperPage;
                if (PageNum * NumperPage < length)
                    PageNum++;
                int beginPoint = 0; //每页内容运算开始位置
                int endPosition = 0; //每页内容运算结束位置   
                int i = 0;

                while (!string.IsNullOrEmpty(tmpContent)) //遍历运算每页的内容
                {
                    i++;
                    if (endPosition > 0)
                    {
                        beginPoint = endPosition + 1;
                    }
                    endPosition = GetHtmlPosition(Content, beginPoint, NumperPage);

                    if (beginPoint + NumperPage < length && endPosition < length)
                    {
                        pageContent = Content.Substring(beginPoint, endPosition - beginPoint);
                        tmpContent = tmpContent.Remove(0, endPosition - beginPoint);
                    }
                    else
                    {
                        pageContent = Content.Substring(beginPoint);
                        tmpContent = "";
                    }
                    if (!string.IsNullOrEmpty(pageContent))
                        ContentList.Add(pageContent);
                }
            }
            else
            {
                ContentList.Add(Content);
            }

            return ContentList;
        }
        /// <summary>
        /// 依据分页符切割内容,返回切割后的内容数组
        /// </summary>
        /// <param name="Content">要处理的内容</param>
        /// <returns>数组，每个元素代表某页显示的内容</returns>
        public IList<string> GetContentPage(string Content)
        {
            string tmpContent = Content;
            int length = Content.Length;
            IList<string> ContentList = new List<string>();
            string[] Contentarr = tmpContent.Split(new string[] { "[PageCode/]" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Contentarr.Length; i++)
            {
                if (!string.IsNullOrEmpty(Contentarr[i]))
                {
                    ContentList.Add(Contentarr[i]);
                }
            }
            return ContentList;
        }
        /// <summary>
        /// HTML生成内容分页导航
        /// </summary>        
        /// <param name="fname">文件名,不含扩展</param>
        /// <param name="fileex">扩展名 html|shtml|htm</param>
        public string GetPage(string content, string fname, string fileex, int cpage, int pageCount, int psize)
        {
            //只有一页的时候，不显示分页导航
            if (pageCount <= 1) { return ""; }
            string currentUrl = ""; //当前url
            string firsturl = "";     //首页url
            string prvurl = "";     //前一页url
            string nexturl = "";    //后一页url
            string endurl = "";     //末页url
            string allurl = "";//显示全部
            cpage = GetCPage(cpage, pageCount);
            //前一页页码
            int prvid = (cpage == 1) ? 1 : cpage - 1;
            //下一页页码
            int nextid = (cpage + 1) >= pageCount ? pageCount : cpage + 1;
            //当前url
            if (cpage == 1)
                currentUrl = fname + fileex;
            else
                currentUrl = fname + "_" + cpage + fileex;
            //首页url
            firsturl = fname + fileex;
            //前一页url
            if (prvid == 1)
                prvurl = fname + fileex;
            else
                prvurl = fname + "_" + prvid + fileex;
            //后一页url
            nexturl = fname + "_" + nextid + fileex;
            //末页url
            endurl = fname + "_" + pageCount + fileex;
            allurl = fname + "_0" + fileex;
            content = content.Replace("{totalrecord/}", pageCount.ToString()); //替换总页数变量(需要上层传下记录,否则不好获取)
            content = content.Replace("{totalpage/}", pageCount.ToString()); //替换总页数变量
            content = content.Replace("{pagesize/}", psize.ToString()); //替换每页记录数
            content = content.Replace("{currenturl/}", currentUrl); //替换当前页url
            content = content.Replace("{firsturl/}", firsturl); //替换第一页url
            content = content.Replace("{prvurl/}", prvurl); //替换上一页url
            content = content.Replace("{nexturl/}", nexturl); //替换下一页url
            content = content.Replace("{endurl/}", endurl); //替换末页url
            content = content.Replace("{allurl/}", allurl); //显示全部
            content = content.Replace("{currentpage/}", cpage.ToString()); //替换当前页码
            content = content.Replace("{prvpage/}", prvid.ToString()); //替换上一页页码
            content = content.Replace("{nextpage/}", nextid.ToString()); //替换下一页页码
            content = content.Replace("{endpage/}", pageCount.ToString()); //替换末页页码

            #region 查找替换循环页处理
            //string pattern = @"{loop([\s\S])*?{/loop}";
            //string temp = "";
            ////有循环列表，换算循环列表部分
            //if (Regex.IsMatch(Content, pattern, RegexOptions.IgnoreCase))
            //{
            //    temp = Regex.Match(Content, pattern, RegexOptions.IgnoreCase).Value;//temp为循环模板
            //    string s = temp;
            //    temp = temp.Replace("{loop", "").Replace("{/loop}", "");
            //    int range = 0;

            //    //指定了范围
            //    if (temp.IndexOf("range=") >= 0)
            //    {
            //        temp = temp.Remove(temp.IndexOf("range="), 6);
            //        int pos = temp.IndexOf("}");
            //        string rng = temp.Substring(0, pos);
            //        rng = rng.Replace("'", "").Replace(" ", "");
            //        range = DataConverter.CLng(rng);
            //        temp = temp.Substring(pos + 1);
            //    }
            //    else
            //    {
            //        temp = temp.Substring(1);
            //    }

            //    string CommonStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[0]; //普通页样式
            //    string CurrentStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];//当前页样式
            //    StringBuilder sb = new StringBuilder();
            //    string pvalue = temp;
            //    if (range > 0) //指定循环范围
            //    {
            //        if (CPage - range > 1) //当前页循环范围在第一页后
            //        {
            //            pvalue = CommonStyle.Replace("{pageid/}", "1");
            //            pvalue = pvalue.Replace("{pageurl/}", firsturl);
            //            sb.Append(pvalue);
            //            if (CPage - range > 2)
            //                sb.Append("<li><a href='javascript:;'>...</a></li>");
            //            for (int i = CPage - range; i <= CPage; i++)
            //            {
            //                if (i == CPage)
            //                {
            //                    pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", currentUrl);
            //                }
            //                else
            //                {
            //                    pvalue = CommonStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + i.ToString() + fileex);
            //                }
            //                sb.Append(pvalue + " ");
            //            }
            //        }
            //        else
            //        {
            //            for (int i = 1; i <= CPage; i++)
            //            {
            //                if (i == CPage)
            //                {
            //                    pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", currentUrl);
            //                }
            //                else
            //                {
            //                    pvalue = CommonStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + i.ToString() + fileex);
            //                }
            //                sb.Append(pvalue + " ");
            //            }
            //        }
            //        if (CPage + range >= endid) //当前页码加循环范围超出末页页码
            //        {
            //            for (int k = CPage + 1; k <= endid; k++)
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", k.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + k.ToString() + fileex);

            //                sb.Append(pvalue + " ");
            //            }
            //        }
            //        else
            //        {
            //            for (int k = CPage + 1; k <= CPage + range; k++)
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", k.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + k.ToString() + fileex);

            //                sb.Append(pvalue + " ");
            //            }
            //            if (CPage + range < endid - 1)
            //            {
            //                sb.Append("<li><a href='javascript:;'>...</a></li>");
            //            }
            //            pvalue = temp;
            //            pvalue = CommonStyle.Replace("{pageid/}", endid.ToString());
            //            pvalue = pvalue.Replace("{pageurl/}", endurl);

            //            sb.Append(pvalue + " ");
            //        }
            //    }
            //    else //没有指定循环范围
            //    {
            //        //pvalue=<a href="{pageurl/}" class="p{pageid/}"><span>{pageid/}</span></a>$$$<a href="/html/ttxw/663.html"  class="p{pageid/}"><span>1</span></a>
            //        for (int t = 1; t <= countpage; t++)
            //        {
            //            if (CPage == t)
            //            {
            //                pvalue = CurrentStyle.Replace("{pageid/}", t.ToString());
            //                if (CPage == 1)
            //                {
            //                    pvalue = pvalue.Replace("{pageurl/}", firsturl);
            //                }
            //                else
            //                {
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + t.ToString() + fileex);
            //                }
            //            }
            //            else
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", t.ToString());
            //                if (t == 1)
            //                {
            //                    pvalue = pvalue.Replace("{pageurl/}", firsturl);
            //                }
            //                else
            //                {
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + "_" + t.ToString() + fileex);
            //                }
            //            }
            //            pvalue = pvalue.Replace("{allurl/}", allurl);
            //            sb.Append(pvalue + " ");
            //        }
            //    }
            //    Content = Content.Replace(s, sb.ToString());
            //}
            #endregion
            //静态页生成时无参数需要处理,默认为空
            content = PageLoop(cpage, pageCount, content, fname + "_", fileex, "", firsturl, currentUrl);
            return content;
        }
        /// <summary>
        /// ASPX生成内容分页导航
        /// </summary>
        /// <param name="content">需要处理的Html+JS内容</param>
        /// <param name="InfoID">内容GeneralID</param>
        /// <param name="psize">无用,内容分页不需要其,也无记录条数</param>
        public string GetPage(string content, int infoID, int cpage, int pageCount, int psize)
        {
            //只有一页的时候，不显示分页导航
            if (pageCount < 1) { return ""; }
            string currentUrl = ""; //当前url
            string firsturl = "";     //首页url
            string prvurl = "";     //前一页url
            string nexturl = "";    //后一页url
            string endurl = "";     //末页url
            string allurl = "";//显示全部
            cpage = GetCPage(cpage, pageCount);
            //前一页页码
            int prvid = (cpage <= 1) ? 1 : cpage - 1;
            //下一页页码
            int nextid = (cpage + 1) >= pageCount ? pageCount : cpage + 1;
            //------------------------------------------------------------------
            string pageName = fullUrl;//取页面名
            string[] strArray = fullUrl.Split('/');
            if (strArray.Length > 0) { pageName = strArray[strArray.Length - 1]; }
            string query = "";
            if (fullUrl.Contains("?")) { query = "?" + fullUrl.Split('?')[1]; }
            // pageName= /Item/4  
            if (Regex.IsMatch(pageName, "_([\\d]+)", RegexOptions.IgnoreCase))//内容页
            {
                pageName = Regex.Replace(pageName, "_([\\d]+).*", "_", RegexOptions.IgnoreCase);
            }
            else if (Regex.IsMatch(pageName, "([\\d]+)", RegexOptions.IgnoreCase))
            {
                pageName = Regex.Replace(pageName, ".*", "_", RegexOptions.IgnoreCase);
            }
            //当前url
            if (cpage <= 1)
                currentUrl = pageName + "1";
            else
                currentUrl = pageName + cpage + "" + query;
            firsturl = pageName + "1" + query;
            //前一页url
            if (prvid == 1)
                prvurl = pageName + "1" + query;
            else
                prvurl = pageName + prvid + "" + query;
            //后一页url
            nexturl = pageName + nextid + "" + query;
            //末页url
            endurl = pageName + pageCount + "" + query;
            allurl = pageName + "0" + "" + query;

            content = content.Replace("{totalrecord/}", pageCount.ToString()); //替换总页数变量
            content = content.Replace("{totalpage/}", pageCount.ToString()); //替换总页数变量
            content = content.Replace("{pagesize/}", psize.ToString()); //替换每页记录数
            content = content.Replace("{currenturl/}", currentUrl); //替换当前页url
            content = content.Replace("{firsturl/}", firsturl); //替换第一页url
            content = content.Replace("{prvurl/}", prvurl); //替换上一页url
            content = content.Replace("{nexturl/}", nexturl); //替换下一页url
            content = content.Replace("{endurl/}", endurl); //替换末页url
            content = content.Replace("{allurl/}", allurl); //显示全部
            content = content.Replace("{currentpage/}", cpage.ToString()); //替换当前页码
            content = content.Replace("{prvpage/}", prvid.ToString()); //替换上一页页码
            content = content.Replace("{nextpage/}", nextid.ToString()); //替换下一页页码
            content = content.Replace("{endpage/}", pageCount.ToString()); //替换末页页码

            #region 查找替换循环页处理
            //string pattern = @"{loop([\s\S])*?{/loop}";
            //string temp = "";
            ////有循环列表，换算循环列表部分
            //if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
            //{
            //    temp = Regex.Match(content, pattern, RegexOptions.IgnoreCase).Value;
            //    string s = temp;
            //    temp = temp.Replace("{loop", "").Replace("{/loop}", "");
            //    int range = 0;
            //    //指定了范围
            //    if (temp.IndexOf("range=") >= 0)
            //    {
            //        temp = temp.Remove(temp.IndexOf("range="), 6);
            //        int pos = temp.IndexOf("}");
            //        string rng = temp.Substring(0, pos);

            //        rng = rng.Replace("\\", "");
            //        rng = rng.Replace("'", "").Replace(" ", "");

            //        range = DataConverter.CLng(rng);
            //        temp = temp.Substring(pos + 1);
            //    }
            //    else
            //    {
            //        temp = temp.Substring(1);
            //    }
            //    string CommonStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[0]; //普通页样式
            //    string CurrentStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];//当前页样式
            //    StringBuilder sb = new StringBuilder();
            //    string pvalue = temp;

            //    if (range > 0) //指定循环范围
            //    {
            //        if (CPage - range > 1) //当前页循环范围在第一页后
            //        {
            //            pvalue = CommonStyle.Replace("{pageid/}", "1");
            //            pvalue = pvalue.Replace("{pageurl/}", firsturl);

            //            sb.Append(pvalue);
            //            if (CPage - range > 2)
            //                sb.Append("<li><a href='javascript:;'>...</a></li>");
            //            for (int i = CPage - range; i <= CPage; i++)
            //            {
            //                if (i == CPage)
            //                {
            //                    pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", currentUrl);
            //                }
            //                else
            //                {
            //                    pvalue = CommonStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + i.ToString() + "" + aaa);
            //                }
            //                sb.Append(pvalue + "\r\n");
            //            }
            //        }
            //        else
            //        {
            //            for (int i = 1; i <= CPage; i++)
            //            {
            //                pvalue = temp;
            //                if (i == CPage)
            //                {
            //                    pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", currentUrl);
            //                }
            //                else
            //                {
            //                    pvalue = CommonStyle.Replace("{pageid/}", i.ToString());
            //                    pvalue = pvalue.Replace("{pageurl/}", urlstr + i.ToString() + "" + aaa);
            //                }
            //                sb.Append(pvalue + " ");
            //            }
            //        }
            //        if (CPage + range >= endid) //当前页码加循环范围超出末页页码
            //        {
            //            for (int k = CPage + 1; k <= endid; k++)
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", k.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", urlstr + k.ToString() + "" + aaa);
            //                sb.Append(pvalue + " ");
            //            }
            //        }
            //        else
            //        {
            //            for (int k = CPage + 1; k <= CPage + range; k++)
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", k.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", urlstr + k.ToString() + "" + aaa);

            //                sb.Append(pvalue + " ");
            //            }
            //            if (CPage + range < endid - 1)
            //            {
            //                sb.Append("<li><a href='javascript:;'>...</a></li>");
            //            }
            //            pvalue = CommonStyle.Replace("{pageid/}", endid.ToString());
            //            pvalue = pvalue.Replace("{pageurl/}", endurl);
            //        }
            //    }
            //    else //没有指定循环范围
            //    {
            //        for (int t = 1; t <= countpage; t++)
            //        {
            //            if (CPage == t)
            //            {
            //                pvalue = CurrentStyle.Replace("{pageid/}", t.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", currentUrl);
            //            }
            //            else
            //            {
            //                pvalue = CommonStyle.Replace("{pageid/}", t.ToString());
            //                pvalue = pvalue.Replace("{pageurl/}", urlstr + t.ToString() + "" + aaa);
            //            }
            //            sb.Append(pvalue + " ");
            //        }
            //    }
            //    content = content.Replace(s, sb.ToString());
            //}
            #endregion
            content = PageLoop(cpage, pageCount, content, pageName, "", query, firsturl, currentUrl);
            return content;
        }
        /// <summary>
        /// 生成列表页分页标签[分页标签]
        /// /Class_4/Default
        /// </summary>
        /// <param name="iLabel">分页标签名</param>
        /// <param name="PageSize">每页记录数</param>
        /// <param name="CPage">当前页码</param>
        /// <param name="ReCount">总记录数</param>
        /// <param name="pageUrl">页面Url不含扩展名 扩展名由生成状态参数决定</param>
        /// <returns>生成后的分页代码</returns>
        private string GetPageCode(string iLabel, int psize, int cpage, int itemCount, int infoID, string pageUrl)
        {
            string LabelContent = "";
            if (iLabel == "{ZL.Page/}")//默认中文分页
            {
                LabelContent = "<table id=\"pageDiv\" totalPage=\"{totalpage/}\" style=\"width:100%;border:none;\" width=\"100%\" align=\"center\">"
                                + "<tbody>"
                                + "<tr>"
                                + "<td valign=\"bottom\" align=\"center\" nowrap=\"nowrap\" style=\"width: 40%;\">总共<b>{totalrecord/}</b>条记录&nbsp;&nbsp;当前页面:<strong><font color=\"red\">{currentpage/}</font></strong>/共<strong>{totalpage/}</strong>页&nbsp; 每页<strong><font color=\"red\">{pagesize/}</font></strong>条记录&nbsp;"
                                + "&nbsp;<a href=\"{firsturl/}\">首页</a> "
                                + "&nbsp;<a href=\"{prvurl/}\">上一页</a>"
                                + "&nbsp;<a href=\"{nexturl/}\">下一页</a>"
                                + "&nbsp;<a href=\"{endurl/}\">尾页</a>"
                                + "</td>"
                                + "</tr>"
                                + "</tbody>"
                                + "</table>";
            }
            else if (iLabel == "{ZL.PageEn/}")//默认英文分页
            {
                LabelContent = "<table id=\"pageDiv\" totalPage=\"{totalpage/}\" style=\"width:100%;border:none;\" width=\"100%\" align=\"center\">"
                + "<tbody>"
                + "<tr>"
                + "<td valign=\"bottom\" align=\"center\" nowrap=\"nowrap\" style=\"width: 40%;\">Total<b>{totalrecord/}</b>Record&nbsp;&nbsp;CPage:<strong><font color=\"red\">{currentpage/}</font></strong>/<strong>{totalpage/}</strong>&nbsp; Per Page<strong><font color=\"red\">{pagesize/}</font></strong>Record&nbsp;"
                + "&nbsp;<a href=\"{firsturl/}\">First</a> "
                + "&nbsp;<a href=\"{prvurl/}\">Pre</a>"
                + "&nbsp;<a href=\"{nexturl/}\">Next</a>"
                + "&nbsp;<a href=\"{endurl/}\">End</a>"
                + "</td>"
                + "</tr>"
                + "</tbody>"
                + "</table>";
            }
            else//自定义分页读分页标签内容
            {
                M_Label label = GetLabelXML(iLabel);
                if (!string.IsNullOrEmpty(label.ErrorMsg)) { return label.ErrorMsg; }
                if (label.LabelType != 5) { return "[ERR:(" + label.LabelName + ")类型不正确,LableType标识错误]"; }
                LabelContent = Getjude(cpage, infoID, label);
            }
            //--------------------
            string currentUrl = ""; //当前url
            string firsturl = "";     //首页url
            string prvurl = "";     //前一页url
            string nexturl = "";    //后一页url
            string endurl = "";     //末页url
            //--------------------页面Size
            int countpage = PageHelper.GetPageCount(itemCount, psize);
            cpage = GetCPage(cpage, countpage);
            //前一页页码
            int prvid = (cpage == 1) ? 1 : cpage - 1;
            //下一页页码
            int nextid = (cpage + 1) >= countpage ? countpage : cpage + 1;
            //--------------------页面参数
            string pageName = GetPageName(fullUrl);//取页面名
            //pageName=
            string query = "";//参数
            if (fullUrl.Contains("?"))
            {
                query = "?" + rawUrl.Split('?')[1];
                query = query.TrimEnd('&');
            }
            #region 替换Url
            //Class_100/Default
            if (Regex.IsMatch(pageName, "[D,d]efault_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[D,d]efault_([\\d]+)", "Default_");
            }
            else if (Regex.IsMatch(pageName, "[D,d]efault"))
            {
                pageName = Regex.Replace(pageName, "[D,d]efault", "Default_");
            }
            if (Regex.IsMatch(pageName, "[N,n]ode[H,h]ot_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[H,h]ot_([\\d]+)", "NodeHot_");
            }
            else if (Regex.IsMatch(pageName, "[N,n]ode[H,h]ot"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[H,h]ot", "NodeHot_");
            }
            if (Regex.IsMatch(pageName, "[N,n]ode[N,n]ews_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[N,n]ews_([\\d]+)", "NodeNews_");
            }
            else if (Regex.IsMatch(pageName, "[N,n]ode[N,n]ews"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[N,n]ews", "NodeNews_");
            }

            if (Regex.IsMatch(pageName, "[N,n]ode[E,e]lite_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[E,e]lite_([\\d]+)", "NodeElite_");
            }
            else if (Regex.IsMatch(pageName, "[N,n]ode[E,e]lite"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[E,e]lite", "NodeElite_");
            }
            if (Regex.IsMatch(pageName, "[N,n]ode[P,p]age_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[P,p]age_([\\d]+)", "NodePage_");
            }
            else if (Regex.IsMatch(pageName, "[N,n]ode[P,p]age"))
            {
                pageName = Regex.Replace(pageName, "[N,n]ode[P,p]age", "NodePage_");
            }
            if (Regex.IsMatch(pageName, "[P,p]age[L,l]ist_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[P,p]age[L,l]ist_([\\d]+)", "PageList_");
            }
            else if (Regex.IsMatch(pageName, "[P,p]age[L,l]ist"))
            {
                pageName = Regex.Replace(pageName, "[P,p]age[L,l]ist", "PageList_");
            }
            else if (Regex.IsMatch(pageName, "[L,l]ist_([\\d]+)"))
            {
                pageName = Regex.Replace(pageName, "[L,l]ist_([\\d]+)", "List_");
            }
            else if (Regex.IsMatch(pageName, "[L,l]ist"))
            {
                pageName = Regex.Replace(pageName, "[L,l]ist", "List_");
            }
            pageName = Regex.Replace(pageName, ".*", "");//PageList_
            #endregion
            //当前url
            currentUrl = pageName + cpage.ToString() + "" + query;//这里添加下,否则跳转无参
            //首页url
            firsturl = pageName + "1" + query;
            //前一页url
            prvurl = pageName + prvid.ToString() + "" + query;
            //后一页url
            nexturl = pageName + nextid.ToString() + "" + query;
            //末页url
            endurl = pageName + countpage + "" + query;
            //------------------------替换占位符
            LabelContent = LabelContent.Replace("{totalrecord/}", itemCount.ToString()); //替换总页数变量
            LabelContent = LabelContent.Replace("{totalpage/}", countpage.ToString()); //替换总页数变量
            LabelContent = LabelContent.Replace("{pagesize/}", psize.ToString()); //替换每页记录数
            LabelContent = LabelContent.Replace("{currenturl/}", currentUrl); //替换当前页url
            LabelContent = LabelContent.Replace("{firsturl/}", firsturl); //替换第一页url
            LabelContent = LabelContent.Replace("{prvurl/}", prvurl); //替换上一页url
            LabelContent = LabelContent.Replace("{nexturl/}", nexturl); //替换下一页url
            LabelContent = LabelContent.Replace("{endurl/}", endurl); //替换末页url
            LabelContent = LabelContent.Replace("{currentpage/}", cpage.ToString()); //替换当前页码
            LabelContent = LabelContent.Replace("{prvpage/}", prvid.ToString()); //替换上一页页码
            LabelContent = LabelContent.Replace("{nextpage/}", nextid.ToString()); //替换下一页页码
            LabelContent = LabelContent.Replace("{endpage/}", countpage.ToString()); //替换末页页码
            //查找替换循环页处理
            LabelContent = PageLoop(cpage, countpage, LabelContent, pageName, "", query, firsturl, currentUrl);
            return LabelContent;
        }
        /// <summary>
        /// 有循环列表,换算循环列表部分
        /// (@"{loop([\s\S])*?{/loop}")
        /// </summary>
        /// <param name="LabelContent">需要处理分页标签Html+JS内容</param>
        /// <param name="CPage">当前第几页</param>
        /// <param name="countpage">总共多少页</param>
        /// <param name="query">URL参数(带?号)</param>
        /// <param name="firsturl">首页链接</param>
        /// <param name="currentUrl">当前页链接</param>
        private string PageLoop(int cpage, int pageCount, string labelContent, string pageName, string ext, string query, string firsturl, string currentUrl)
        {
            string pattern = @"{loop([\s\S])*?{/loop}";
            string temp = "";
            if (Regex.IsMatch(labelContent, pattern, RegexOptions.IgnoreCase))
            {
                temp = Regex.Match(labelContent, pattern, RegexOptions.IgnoreCase).Value;
                string backup = temp;
                temp = temp.Replace("{loop", "").Replace("{/loop}", "");
                int range = 0;
                //指定了范围,则取出range的值,然后以此循环
                if (temp.IndexOf("range=") >= 0)
                {
                    temp = temp.Remove(temp.IndexOf("range="), 6);// \'显示半径\'}<a href="{pageurl/}" >{pageid/}</a>$$$<a href="ColumnList?NodeID=76&&p=1">1</a>
                    int pos = temp.IndexOf("}");
                    string rng = temp.Substring(0, pos);
                    rng = rng.Replace("'", "").Replace(" ", "");
                    rng = rng.Replace(@"\", "");
                    if (rng.IndexOf("$0") > -1)
                    {
                        rng = rng.Replace("$0", "");
                    }
                    range = DataConverter.CLng(rng);

                    temp = temp.Substring(pos + 1);
                }
                else
                {
                    temp = temp.Substring(1);
                }
                string CommonStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[0]; //普通页样式<a href="{pageurl/}" >{pageid/}</a>
                string CurrentStyle = temp.Split(new string[] { "$$$" }, StringSplitOptions.None)[1];//当前页样式<a href="ColumnList?NodeID=76&&p=1">1</a>
                StringBuilder sb = new StringBuilder();
                string pvalue = "";

                if (range > 0) //指定循环范围
                {
                    if (cpage - range > 1) //当前页循环范围在第一页后
                    {
                        pvalue = CommonStyle.Replace("{pageid/}", "1");
                        pvalue = pvalue.Replace("{pageurl/}", firsturl);

                        sb.Append(pvalue + " ");

                        if (cpage - range > 2)
                        {
                            sb.Append("<li><a href='javascript:;'>...</a></li>");
                        }

                        for (int i = cpage - range; i <= cpage; i++)
                        {
                            if (i == cpage)
                            {
                                pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
                                pvalue = pvalue.Replace("{pageurl/}", currentUrl);
                            }
                            else
                            {
                                pvalue = CommonStyle.Replace("{pageid/}", i.ToString()).Replace("{pageurl/}", pageName + i + ext + query);
                            }
                            sb.Append(pvalue + " ");
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= cpage; i++)
                        {
                            pvalue = temp;
                            if (i == cpage)
                            {
                                pvalue = CurrentStyle.Replace("{pageid/}", i.ToString());
                                pvalue = pvalue.Replace("{pageurl/}", currentUrl);
                            }
                            else
                            {
                                pvalue = CommonStyle.Replace("{pageid/}", i.ToString()).Replace("{pageurl/}", pageName + i + ext + query);
                            }
                            sb.Append(pvalue + " ");
                        }
                    }

                    if (cpage < pageCount) //当前页小于末页
                    {
                        if (cpage + range >= pageCount) //当前页码加循环范围超出末页页码
                        {
                            for (int k = cpage + 1; k <= pageCount; k++)
                            {
                                pvalue = CommonStyle.Replace("{pageid/}", k.ToString()).Replace("{pageurl/}", pageName + k + ext + query);
                                sb.Append(pvalue + " ");
                            }
                        }
                        else
                        {
                            for (int k = cpage + 1; k <= cpage + range; k++)
                            {
                                pvalue = CommonStyle.Replace("{pageid/}", k.ToString()).Replace("{pageurl/}", pageName + k + ext + query);
                                sb.Append(pvalue + " ");
                            }
                            if (cpage + range < pageCount - 1)
                            {
                                sb.Append("<li><a href='javascript:;'>...</a></li>");
                            }
                            pvalue = CommonStyle.Replace("{pageid/}", pageCount.ToString()).Replace("{pageurl/}", pageName + pageCount + ext + query);
                            sb.Append(pvalue + " ");
                        }
                    }
                }
                else //没有指定循环范围
                {
                    for (int i = 1; i <= pageCount; i++)
                    {
                        pvalue = temp;
                        if (i == cpage)
                        {
                            pvalue = CurrentStyle.Replace("{pageid/}", i.ToString()).Replace("{pageurl/}", currentUrl);
                        }
                        else if (i == 1)
                        {
                            //如果是首页,则特殊处理 /item/82.html
                            pvalue = CommonStyle.Replace("{pageid/}", i.ToString()).Replace("{pageurl/}", pageName.TrimEnd('_') + ext);
                        }
                        else
                        {
                            pvalue = CommonStyle.Replace("{pageid/}", i.ToString()).Replace("{pageurl/}", pageName + i + ext);
                        }
                        sb.Append(pvalue + " ");
                    }
                }
                labelContent = labelContent.Replace(backup, sb.ToString());
            }
            return labelContent;
        }
        /// <summary>
        /// 从1开始,不能大于pageCount
        /// </summary>
        private int GetCPage(int cpage, int pageCount)
        {
            if (pageCount < 1) { pageCount = 1; }//如无数据,则也显示1页,从1开始
            if (cpage <= 1) { cpage = 1; }
            if (cpage > pageCount) { cpage = pageCount; }
            return cpage;
        }
        /// <summary>
        /// 根据传入的URL,获取页面名
        /// http://1th.cn/Class_4/Default
        /// PageList_1
        /// </summary>
        private string GetPageName(string strRaw)
        {
            strRaw = strRaw.ToLower().Split('?')[0];
            string pageName = "";
            if (strRaw.Contains("://"))
            {
                strRaw = Regex.Split(strRaw.ToLower(), "://", RegexOptions.IgnoreCase)[1];
            }
            int start = strRaw.LastIndexOf('/') + 1;
            pageName = strRaw.Substring(start, (strRaw.Length - start));
            return pageName;
        }
        #endregion
        #region 动态标签
        /// <summary>
        /// 根据Label中的条件筛选数据
        /// </summary>
        private static PageSetting SelByLabel(M_Label labelMod, int sqlCount, string sqlTable, string sqlField, string sqlWhere, string sqlOrder, int cpage = 1)
        {
            if (sqlCount < 1) { sqlCount = 10000; }
            SqlBase db = DBCenter.DB;
            PageSetting config = new PageSetting() { cpage = cpage, psize = sqlCount, pageMethod = "row" };

            config.fields = sqlField;
            config.order = sqlOrder;
            config.where = sqlWhere;
            config.T1Alias = config.t1;//兼容MYSQL
            config.T2Alias = string.IsNullOrEmpty(config.t2) ? "" : config.t2;
            if (!string.IsNullOrEmpty(labelMod.DataSourceType) && labelMod.DataSourceType.StartsWith("{"))
            {
                JObject jobj = JsonConvert.DeserializeObject<JObject>(labelMod.DataSourceType);
                db = B_DataSource.GetDSByType(jobj["ds_m"].ToString());
            }
            B_Label.GetT1AndT2(sqlTable, ref config.t1, ref config.t2);
            if (sqlTable.ToUpper().Contains(" ON "))
            {
                config.join = B_Label.GetJoinType(sqlTable);
                config.on = Regex.Split(sqlTable, " on ", RegexOptions.IgnoreCase)[1];
                config.T2Alias = config.t2;
            }
            config.T1Alias = config.t1;
            try
            {
                //排序条件为空,且字段字符串非*,则取第一个字段升序
                //if (string.IsNullOrEmpty(sqlOrder))
                //{
                //    if (!sqlField.Equals("*")) { sqlOrder = sqlField.Split(',')[0] + " ASC"; }
                //}
                if (string.IsNullOrEmpty(sqlOrder)) //无Order排序则不分页
                {
                    config.dt = db.SelTop(config);
                }
                else
                {
                    config.pageMethod = "row";
                    config.dt = db.SelPage(config);
                }
                return config;
            }
            catch (Exception ex) { throw new Exception("SelByLabel:报错:" + ex.Message); }
        }
        /// <summary>
        /// 处理标签取得换算后的标签Html的代码,动态标签,2,4均在此处理,3为数据源标签 [main]
        /// </summary>
        private string ContentLabelProc(string ilabel, int Cpage, int InfoID, string urltype)
        {
            if (!string.IsNullOrEmpty(LabelCache.GetLabel(ilabel)))
            {
                return LabelCache.GetLabel(ilabel);
            }
            string sqllabelcontent = "";
            M_Label label = GetLabelXML(ilabel);
            try
            {
                Hashtable hb = GetParam(ilabel);
                if (!string.IsNullOrEmpty(label.ErrorMsg)) { return label.ErrorMsg; }
                if (label.LabelType == 1)//静态标签
                {
                    sqllabelcontent = Getjude(Cpage, InfoID, label);
                    return sqllabelcontent;
                }
                else if (string.IsNullOrEmpty(label.LabelField))
                {
                    return string.Format(Err_Lack_Field, label.LabelName);
                }
                else if (string.IsNullOrEmpty(label.LabelTable))
                {
                    return string.Format(Err_Lack_TbName, label.LabelName);
                }
                //-------------------------------------------------
                label.Content = Getjude(Cpage, InfoID, label);
                M_Label mylabel = LabelParamFunc(label, ilabel, InfoID.ToString());//处理其中的@参数
                string sqlCount = mylabel.LabelCount;
                string sqlField = mylabel.LabelField;
                string sqlTable = mylabel.LabelTable;
                string sqlWhere = mylabel.LabelWhere;
                string sqlOrder = mylabel.LabelOrder;
                string returntxt = "";//解析标签后的结果Html
                #region 让查询支持扩展标签
                if (CheckisTrue(sqlTable))
                {
                    sqlTable = shophtml.CreateShopHtml(sqlTable);
                    sqlTable = GetContent(Cpage, InfoID, urltype, sqlTable);
                }
                if (CheckisTrue(sqlWhere))
                {
                    sqlWhere = shophtml.CreateShopHtml(sqlWhere);
                    sqlWhere = GetContent(Cpage, InfoID, urltype, sqlWhere);
                }
                if (CheckisTrue(sqlCount))
                {
                    sqlCount = shophtml.CreateShopHtml(sqlCount);
                    sqlCount = GetContent(Cpage, InfoID, urltype, sqlCount);
                }
                if (CheckisTrue(sqlOrder))
                {
                    sqlOrder = shophtml.CreateShopHtml(sqlOrder);
                    sqlOrder = GetContent(0, 0, "0", sqlOrder);
                }
                #endregion
                DataTable dt = new DataTable();
                if (label.LabelType == 2)//动态标签,Type为2
                {
                    #region 动态标签
                    dt = SelByLabel(mylabel, DataConvert.CLng(sqlCount), sqlTable, sqlField, sqlWhere, sqlOrder).dt;
                    //如果无数据,则加载无数据模板
                    if (dt.Rows.Count < 1 && !string.IsNullOrEmpty(mylabel.EmptyContent))
                    {
                        mylabel.Content = mylabel.EmptyContent;
                    }
                    foreach (DictionaryEntry de in hb)//处理参数
                    {
                        mylabel.Content = mylabel.Content.Replace("@" + de.Key.ToString(), de.Value.ToString());
                    }
                    //Coffee
                    if (mylabel.Content.Contains("{Repeate"))//在这里处理ZL.Label中的Repeater,不跳入GetHtmlContent
                    {
                        if (mylabel.Content.Contains("{ZL:Json}")) { mylabel.Content = mylabel.Content.Replace("{ZL:Json}", JsonConvert.SerializeObject(dt)); }
                        mylabel.Content = Deal_Repeate(mylabel, dt);
                    }
                    else if (dt.Rows.Count > 0)
                    {
                        mylabel.Content = For_Field(mylabel.Content, dt, 0);
                        mylabel.Content = GetHtmlContent(ilabel, mylabel.Content, dt, mylabel);
                    }
                    else
                    {
                        //没有数据非Repeater的情况下,将所有字段占位符替换
                        mylabel.Content = For_Field(mylabel.Content, dt, 0);
                    }
                    returntxt = mylabel.Content;
                    LabelCache.AddLabel(ilabel, returntxt);
                    #endregion
                }
                else//4在此处理,Order不能为空，4:分页动态标签
                {
                    #region 带分页的动态标签
                    PageSetting config = SelByLabel(mylabel, DataConvert.CLng(sqlCount), sqlTable, sqlField, sqlWhere, sqlOrder, Cpage);
                    dt = config.dt;
                    int Count = config.itemCount;
                    if (mylabel.Content.Contains("{ZL:Json}")) { mylabel.Content = mylabel.Content.Replace("{ZL:Json}", JsonConvert.SerializeObject(dt)); }
                    if (mylabel.Content.Contains("{Repeate") && dt.Rows.Count > 0)//在这里处理ZL.Label中的Repeater,不跳入GetHtmlContent
                    {
                        //生成分页导航代码
                        string pattern = @"{ZL\.Page([\s\S])*?\/}";
                        MatchCollection matchs = Regex.Matches(mylabel.Content, pattern, RegexOptions.IgnoreCase);
                        //栏目列表,labelcontent=解析后的分页模板,为空则不输出内容
                        foreach (Match match in matchs)
                        {
                            mylabel.Content = mylabel.Content.Replace(match.Value, GetPageCode(match.Value, DataConverter.CLng(sqlCount), Cpage, Count, InfoID, urltype));
                        }
                        foreach (DictionaryEntry de in hb)//处理Repeater中的参数
                        {
                            mylabel.Content = mylabel.Content.Replace("@" + de.Key.ToString(), de.Value.ToString());
                        }
                        returntxt = Deal_Repeate(mylabel, dt);
                    }
                    mylabel.Content = returntxt;
                    returntxt = GetHtmlContent(ilabel, mylabel.Content, dt, mylabel);//解析其余标签
                    #endregion
                }
                return returntxt;
            }
            catch (Exception ex) { WriteLog("ContentLabelProc1", ilabel, ex.Message); return string.Format(Err_LabelError, label.LabelName); }
        }
        /// <summary>
        /// 将数据集中的数据替换标签中的字段
        /// </summary>
        private string GetHtmlContent(string ilabel, string content, DataTable dt, M_Label label)
        {
            if (!string.IsNullOrEmpty(LabelCache.GetLabel(ilabel)))
            {
                return LabelCache.GetLabel(ilabel);
            }
            string result = content;
            StringBuilder sb = new StringBuilder("");
            string pattern2 = @"{Field=([\s\S])*?/}";
            MatchCollection matchs1 = Regex.Matches(result, pattern2, RegexOptions.IgnoreCase);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow dr1 = dt.Rows[0];
                foreach (Match match1 in matchs1)
                {
                    #region 查询是否为下载专用字段
                    string filevalue = dr1[GetFileName(match1.Value)].ToString();//字段值赋值
                    string Filedname = GetFileName(match1.Value);//字段名
                    #region 查询模型表
                    string selecttabale = label.LabelField;
                    IList<string> selectmodelfield = new List<string>();
                    IList<string> selectmodelfieldall = new List<string>();
                    M_ModelField mofillinfo = new M_ModelField();

                    #region 指定模型字段的查询
                    if (label.LabelType != 1)
                    {
                        try
                        {
                            Regex regexObj = new Regex("ZL_[a-zA-Z0-9]*_[a-zA-Z0-9]*.[a-zA-Z0-9]*");//查询指定字段查询
                            Match matchResults = regexObj.Match(selecttabale);
                            while (matchResults.Success)
                            {
                                if (selectmodelfield.IndexOf(matchResults.Value) == -1)
                                {
                                    string[] valuearr = null;
                                    if (matchResults.Value.IndexOf(".") > -1)
                                    {
                                        valuearr = matchResults.Value.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                                        if (valuearr.Length == 2)//只加字段
                                        {
                                            selectmodelfield.Add(matchResults.Value);
                                        }
                                    }
                                }
                                matchResults = matchResults.NextMatch();
                            }
                        }
                        catch
                        {
                            //Syntax error in the regular expression
                        }
                    }
                    #endregion
                    string newtablename = "";
                    int modeid = 0;

                    if (selectmodelfield.Count > 0)//符合多表
                    {
                        #region 指定字段查询
                        for (int c = 0; c < selectmodelfield.Count; c++)//遍历泛型
                        {
                            string allstr = selectmodelfield[c].ToString();
                            string[] strarr = null;
                            if (allstr.IndexOf(".") > 0)//多表查询
                            {
                                string tablename = "";
                                strarr = allstr.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                if (strarr.Length > 1)
                                {
                                    if (strarr[1].ToString().ToLower() == Filedname.ToLower())//确定目标
                                    {
                                        tablename = strarr[0].ToString();
                                        M_ModelInfo modelMod = bmodel.SelModelByTbName(tablename);
                                        if (modelMod != null)
                                        {
                                            newtablename = tablename;
                                            modeid = DataConverter.CLng(modelMod.ModelID);
                                            mofillinfo = fieldBll.GetModelByFieldName(modeid, Filedname);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    else//无数据
                    {

                        if (label.LabelTable.IndexOf("ZL_") > -1)
                        {
                            string[] arr = label.LabelTable.Split(new string[] { "ZL_" }, StringSplitOptions.None);

                            if (arr.Length == 2)//单表
                            {
                                string tablesname = label.LabelTable;

                                if (tablesname.IndexOf('.') > -1)
                                {
                                    string[] a1 = tablesname.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                    tablesname = a1[a1.Length - 1];
                                }
                                M_ModelInfo modelMod = bmodel.SelModelByTbName(tablesname);
                                if (modelMod != null)
                                {
                                    newtablename = tablesname;
                                    modeid = DataConverter.CLng(modelMod.ModelID);
                                    mofillinfo = fieldBll.GetModelByFieldName(modeid, Filedname);
                                }
                            }
                            else//多表，使用*查询
                            {
                                try
                                {
                                    Regex regexObj = new Regex(@"ZL_[a-zA-Z0-9]*_[a-zA-Z0-9]*");
                                    Match matchResults = regexObj.Match(label.LabelTable);
                                    while (matchResults.Success)
                                    {
                                        // matched text: matchResults.Value
                                        string tablesname = matchResults.Value;

                                        M_ModelInfo modelMod = bmodel.SelModelByTbName(tablesname);
                                        if (modelMod != null)
                                        {
                                            newtablename = tablesname;
                                            modeid = DataConverter.CLng(modelMod.ModelID);
                                            mofillinfo = fieldBll.GetModelByFieldName(modeid, Filedname);
                                        }
                                        matchResults = matchResults.NextMatch();
                                    }
                                }
                                catch
                                {
                                    // Syntax error in the regular expression
                                }

                            }
                        }
                    }
                    #endregion
                    string newfilevalue2 = filevalue;
                    #endregion
                    result = result.Replace(match1.Value, newfilevalue2);
                }
            }
            else //没有数据
            {
                foreach (Match match1 in matchs1)
                {
                    result = result.Replace(match1.Value, "[ERR:没有数据]");
                }
            }
            LabelCache.AddLabel(ilabel, result);
            return result;
        }
        #endregion
        #region 扩展函数,系统标签,即B_Fun中的标签
        //系统标签,站点名,Logo等
        private static string SysLabelProc(string syslabel)
        {
            //处理系统标签获得函数后的内容
            string lbl = syslabel.Replace("{$", "").Replace("/}", "");
            switch (lbl)
            {
                case "SiteName":
                    return SiteConfig.SiteInfo.SiteName;
                case "SiteURL":
                    return SiteConfig.SiteInfo.SiteUrl;
                case "SiteTitle":
                    return SiteConfig.SiteInfo.SiteTitle;
                case "MetaKeywords":
                    return "<meta name=\"Keywords\" content=\"" + SiteConfig.SiteInfo.MetaKeywords + "\">";
                case "MetaDescription":
                    return "<meta name=\"Description\" content=\"" + SiteConfig.SiteInfo.MetaDescription + "\">";
                case "LogoUrl":
                    return SiteConfig.SiteInfo.LogoUrl;
                case "Banner":
                    return SiteConfig.SiteInfo.BannerUrl;
                case "CompName":
                    return SiteConfig.SiteInfo.CompanyName;
                case "Webmaster":
                    return SiteConfig.SiteInfo.Webmaster;
                case "WebmasterEmail":
                    return SiteConfig.SiteInfo.WebmasterEmail;
                case "Copyright":
                    return SiteConfig.SiteInfo.Copyright;
                case "UploadDir":
                    return UploadDir + "/";
                case "ManageDir":
                    return ManageDir;
                case "CssDir"://风格路径
                    return SiteConfig.SiteOption.CssDir + "/";
                case "StylePath"://默认风格
                    return SiteConfig.SiteOption.StylePath;
                case "JS"://默认风格
                    return SiteConfig.SiteOption.JS;
                case "LogoAdmin":
                    return SiteConfig.SiteInfo.LogoAdmin;
                case "AdDir":
                    return SiteConfig.SiteOption.AdvertisementDir;
                case "PWAMainfirst":
                    return SiteConfig.SiteOption.PWA_Content;
                default:
                    return "[ERR:未定义的系统标签(" + lbl + ")请检查标签名是否正确]";

            }
        }
        // 扩展标签,用于处理B_Fun中的标签
        private string FunLabelProc(string funlabel, int InfoID, string urltype)
        {
            //处理扩展函数标签获得换算后的内容
            string lbl = funlabel.Replace("{ZL:", "").Replace("/}", "");
            string funcname = lbl.Substring(0, lbl.IndexOf("("));
            string funparam = lbl.Substring(lbl.IndexOf("(") + 1);
            funparam = funparam.Substring(0, funparam.Length - 1);
            try
            {
                #region 扩展标签处理
                switch (funcname)
                {
                    case "GetImg":
                        {
                            string url = "/Common/Label/FontToImg?txt=" + funparam;
                            return "<img src='" + url + "' class='fontimg' />";
                        }
                    case "SplitDown":
                        string[] paramArr = funparam.Split(',');//下载地址,下载名,打开方式
                        if (string.IsNullOrEmpty(funparam)) { return "[暂无下载]"; }
                        if (string.IsNullOrEmpty(paramArr[0])) { return "[未指定下载地址]"; }
                        string[] downurls = paramArr[0].Split('$');
                        string fname = paramArr.Length > 1 ? paramArr[1] : "", openMode = paramArr.Length > 2 ? paramArr[2] : "_blank";
                        if (!string.IsNullOrEmpty(openMode)) { openMode = "target=\"" + openMode + "\""; }
                        string tlp = "<a href=\"{0}\" {1} >{2}</a>", liTlp = "<li><a href=\"{0}\" {1} >{2}</a></li>";
                        //官方下载|/down/zoomla!cms1.2.rar$CSDN分流下载|http://download.csdn.net/source/729427$一特分流下载|http://www.1th.cn/News/develop/gxkfcszj_ZoomlazlCMS12bfb.html
                        string lis = "";
                        for (int i = 0; i < downurls.Length; i++)
                        {
                            if (!downurls[i].Contains("|")) { return "[下载地址格式不正确]"; }
                            string name = downurls[i].Split('|')[0];
                            string url = GetDownUrl(downurls[i].Split('|')[1]);
                            if (!string.IsNullOrWhiteSpace(fname)) { name = fname + (i + 1); }
                            if (downurls.Length == 1)
                            {
                                return string.Format(tlp, url, openMode, name);
                            }
                            else
                            {
                                lis += string.Format(liTlp, url, openMode, name);
                            }
                        }
                        return lis;
                    //分解关键字，LIKE 查询
                    case "GetKeyWord":
                        if (string.IsNullOrEmpty(funparam)) { return "1=1"; }
                        return GetKeyWord(funparam);
                    //根据节点ID获取节点列表或节点首页链接
                    case "GetNodeUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeUrl");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetNodeUrl节点ID参数必须是数字]";
                            }
                        }
                        return GetNodeLinkUrl(DataConverter.CLng(funparam));
                    case "UeditorOL":
                        return UeditorOL(funparam);
                    case "MagazinePicCount":
                        return MagazinePicCount(funparam);
                    case "CreateLi":
                        return CreateLi(funparam);
                    case "Calc":
                        try
                        {
                            string exp = funparam.Split(',')[0];//表达式
                            string denum = "f" + (funparam.Split(',').Length > 1 ? funparam.Split(',')[1] : "2");//小数位,默认5位
                            return Convert.ToDouble(new DataTable().Compute(exp, null)).ToString(denum);
                        }
                        catch (Exception ex) { return ex.Message; }
                    case "StarLabel"://动态化静态标签
                        return StarLabel(funparam);
                    //安全保护
                    case "Guard":
                        if (File.Exists(function.VToP("/JS/Guard.js")))
                        {
                            return SafeSC.ReadFileStr("/JS/Guard.js");
                        }
                        else return "";
                    //根据节点ID获得列表链接
                    case "GetNodeListUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeListUrl");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetNodeListUrl节点ID参数必须是数字]";
                            }
                        }
                        return GetNodeListPath(DataConverter.CLng(funparam));
                    //获得专题分类列表
                    case "GetSpecialList":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetSpecialList");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetSpecialList节点ID参数必须是数字]";
                            }
                        }
                        return GetSpecialList(DataConverter.CLng(funparam));
                    //获得专题列表页
                    case "GetSpecialPage":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetSpecialPage");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetSpecialPage节点ID参数必须是数字]";
                            }
                        }
                        return GetSpecialPage(DataConverter.CLng(funparam));
                    //根据内容ID获取内容页链接
                    case "GetInfoUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetInfoUrl");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetInfoUrl内容ID参数必须是数字]";
                            }
                        }
                        return GetInfoPath(DataConverter.CLng(funparam));
                    case "LinkName":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "LinkName");
                        }
                        return funparam;
                    //根据商品ID获取内容页链接
                    case "GetShopUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetShopUrl");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetShopUrl内容ID参数必须是数字]";
                            }
                        }
                        return GetShopPath(DataConverter.CLng(funparam));
                    case "GetLastinfo":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, funcname);
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetLastinfo节点ID参数必须是数字]";
                            }
                        }
                        return GetLastinfos(DataConverter.CLng(funparam));
                    //case "Font":
                    //    {
                    //        string fontName = GetParamFromArr(funparam, 0).Trim();
                    //        if (string.IsNullOrEmpty(fontName)) { return string.Format(Err_Lack_Param, funcname); }
                    //        DataRow dr = XMLDB.SelReturnModel(XMLDB.ZLFont, "name", fontName);
                    //        if (dr == null) { return "[字体(" + fontName + ")不存在]"; }
                    //        else
                    //        {
                    //            var html = "<style type=\"text/css\">"
                    //                + "@import url(https://code.z01.com/Zfont.css);"
                    //                + "." + dr["css"] + "{font-family:'" + dr["css"] + "';}"
                    //                + "</style>";
                    //            return html;
                    //        }
                    //    }
                    case "GetHotinfo":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetHotinfo");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetHotinfo节点ID参数必须是数字]";
                            }
                        }
                        return GetHotinfos(DataConverter.CLng(funparam));
                    case "GetProposeinfo":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetProposeinfo");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetProposeinfo节点ID参数必须是数字]";
                            }
                        }
                        return GetProposeinfos(DataConverter.CLng(funparam));
                    case "GetNodeOpen":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeOpen");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetNodeOpen节点ID参数必须是数字]";
                            }
                        }
                        return GetNodeOpen(DataConverter.CLng(funparam));

                    case "GetNodeItemOpen":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeItemOpen");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetNodeItemOpen节点ID参数必须是数字]";
                            }
                        }
                        return GetNodeItemOpen(DataConverter.CLng(funparam));
                    case "GetNodeCustom":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeCustom");
                        }
                        else
                        {
                            if (funparam.IndexOf(",") < 0)
                            {
                                return string.Format(Err_Lack_Param, "GetNodeCustom");
                            }
                            else
                            {
                                string nodeid = funparam.Split(new char[] { ',' })[0];
                                string num = funparam.Split(new char[] { ',' })[1];
                                if (!DataValidator.IsNumber(nodeid) || !DataValidator.IsNumber(num))
                                {
                                    return "[ERR:GetNodeCustom节点ID参数和自设内容序号必须是数字]";
                                }
                                return GetCustom(DataConverter.CLng(nodeid), DataConverter.CLng(num));
                            }
                        }

                    //根据节点ID获取该节点下内容页链接打开方式
                    case "GetInfoOpen":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetInfoOpen");
                        }
                        else
                        {
                            if (!DataValidator.IsNumber(funparam))
                            {
                                return "[ERR:GetInfoOpen节点ID参数必须是数字]";
                            }
                        }
                        return GetInfoOpen(DataConverter.CLng(funparam));
                    //获取当前用户名
                    case "GetuserName":
                        if (buser.CheckLogin())
                        {
                            return buser.GetLogin().UserName;
                        }
                        else
                        {
                            return "";
                        }
                    case "GetuserID"://获取当前用户ID
                        if (buser.CheckLogin())
                        {
                            return buser.GetLogin().UserID.ToString();
                        }
                        else
                        {
                            return "";
                        }
                    case "TimeNow":
                    case "DateNow":
                    case "DateAndTime"://当前日期时间
                        {
                            if (string.IsNullOrEmpty(funparam)) { return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"); }
                            else { return DateTime.Now.ToString(funparam); }
                        }
                    //当前季节
                    case "Season"://季节
                        string nowseason = "";
                        int nowmonth = DateTime.Now.Month;
                        if (nowmonth >= 3 && nowmonth <= 5)
                        {
                            nowseason = "春";
                        }
                        else if (nowmonth >= 6 && nowmonth <= 8)
                        {
                            nowseason = "夏";
                        }
                        else if (nowmonth >= 9 && nowmonth <= 11)
                        {
                            nowseason = "秋";
                        }
                        else if (nowmonth >= 12 && nowmonth <= 2)
                        {
                            nowseason = "冬";
                        }
                        return nowseason;
                    //case "SolarTerms"://节气
                    //    return Season.ChineseTwentyFourDay(DateTime.Now);
                    case "ConverToWeek"://将日期时间转换成星期几
                        {
                            //Regex r = new Regex("(?<=(=\"))[.\\s\\S]*?(?=(\"))", RegexOptions.IgnoreCase);
                            funparam = funparam.Replace(")", "").Trim(' ');
                            string[] s = funparam.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (s.Length < 2)
                            {
                                if (s[0].Equals("中文"))
                                    return GetChineseWeek(DateTime.Now.DayOfWeek.ToString());
                                else
                                    return DateTime.Now.DayOfWeek.ToString();

                            }
                            else
                            {
                                DateTime dt = DateTime.Parse(s[0]);
                                if (s[1].Equals("中文"))
                                    return GetChineseWeek(dt.DayOfWeek.ToString());//返回中文
                                else
                                    return dt.DayOfWeek.ToString();//返回英文
                            }
                        }
                    case "FormatDate": //将日期时间格式化成指定模式
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "FormatDate");
                        }
                        else if (funparam.IndexOf(",") < 0)
                        {
                            return string.Format(Err_Lack_Param, "FormatDate");
                        }
                        else
                        {
                            try
                            {
                                string objdate = funparam.Split(',')[0];
                                string objfor = funparam.Split(',')[1];
                                if (string.IsNullOrEmpty(objdate)) { objdate = DateTime.Now.ToString(); }
                                if (!string.IsNullOrEmpty(objfor))
                                {
                                    return Convert.ToDateTime(objdate).ToString(objfor);
                                }
                                else { return objdate; }
                            }
                            catch
                            {
                                return string.Format(Err_Lack_Param, "FormatDate");
                            }
                        }
                    case "ChrLen":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "ChrLen");
                        }
                        else
                        {
                            return StringHelper.SubStringLength(funparam).ToString();
                        }
                    case "Len":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "Len");
                        }
                        else
                        {
                            return funparam.Length.ToString();
                        }
                    case "Replace":
                        {
                            //内容和被替换内容中可能有逗号(是否以双逗号隔开)
                            if (string.IsNullOrEmpty(funparam)) { return string.Format(Err_Lack_Param, "Replace"); }
                            if (funparam.IndexOf(",") < 0) { return string.Format(Err_Lack_Param, "Replace"); }
                            string[] pos = funparam.Split(new string[] { "," }, StringSplitOptions.None);
                            if (pos.Length != 3) { return string.Format(Err_LabelError, "Replace"); }
                            if (string.IsNullOrEmpty(pos[0])) { return ""; }
                            if (string.IsNullOrEmpty(pos[1])) { return pos[0]; }
                            return pos[0].Replace(pos[1], pos[2]);
                        }
                    //将超过一定长度的字符串截取以'...'结尾的字符串
                    case "CutText":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "CutText");
                        }
                        else
                        {
                            if (funparam.LastIndexOf(",") < 0)
                            {
                                return string.Format(Err_Lack_Param, "CutText");
                            }
                            else
                            {
                                try
                                {
                                    int pos = funparam.LastIndexOf(",");
                                    string ex = "";
                                    if (pos < funparam.Length)
                                        ex = funparam.Substring(pos + 1);
                                    else
                                        ex = "";
                                    funparam = funparam.Substring(0, pos);
                                    pos = funparam.LastIndexOf(",");
                                    string input = funparam.Substring(0, pos);

                                    int len = DataConverter.CLng(funparam.Substring(pos + 1));
                                    return CutText(input, len, ex);
                                }
                                catch
                                {
                                    return "[ERR:CutText字符串格式化参数错误]";
                                }
                            }
                        }
                    case "ByteFileDown":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return "[暂无下载]";
                        }
                        else
                        {
                            string Gid = "";
                            string Modelid = "";
                            string FileName = "";
                            if (funparam.IndexOf(",") >= 0)
                            {
                                Gid = funparam.Split(new char[] { ',' })[0];
                                Modelid = funparam.Split(new char[] { ',' })[1];
                                FileName = funparam.Split(new char[] { ',' })[2];
                            }
                            else
                            {
                                Gid = funparam;
                                Modelid = "";
                                FileName = "";
                            }
                            if (string.IsNullOrEmpty(Gid) || string.IsNullOrEmpty(Modelid) || string.IsNullOrEmpty(FileName))
                            {
                                return "[暂无下载]";
                            }

                            else
                            {
                                return "/manage/Content/ShowPic?type=1&Gid=" + Gid + "&ModeID=" + Modelid + "&FileName=" + FileName;
                            }

                        }
                    case "GetQCUrl":
                        string[] arr = funparam.Split(',');
                        if (arr.Length < 2)
                        {
                            return string.Format(Err_Lack_Param, "GetQCUrl");
                        }
                        return string.Format("'/Common/Common.ashx?url={0}' width='{1}px'", arr[0], arr[1]);
                    case "GetQRurl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return Err_Lack_Param;
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new char[] { ',' });

                            if (paraArr.Length < 2)
                            {
                                return Err_Lack_Param;
                            }
                            else
                            {
                                string strs = paraArr[0].ToString();
                                int num1 = DataConverter.CLng(paraArr[1]);
                                string str = "https://chart.googleapis.com/chart?cht=qr&chs=" + num1 + "x" + num1 + "&choe=UTF-8&chld=L|4&chl=" + strs;
                                return str;
                            }
                        }
                    case "GetConPic":
                        if (string.IsNullOrEmpty(funparam))
                            return string.Format(Err_Lack_Param, "GetConPic");
                        string[] parama = funparam.Split(',');
                        SafeSC.CheckDataEx(parama[0], parama[2]);
                        string pk = parama[0].ToLower().Equals("zl_commonmodel") ? "GeneralID" : "ID";
                        try
                        {
                            object content = DBCenter.ExecuteScala(parama[0], parama[2], pk + "=" + parama[1]);
                            int count = DataConvert.CLng(parama[3]);
                            if (content == DBNull.Value || string.IsNullOrEmpty(content.ToString()) || !content.ToString().Contains("<img")) return "";
                            string result = "";
                            foreach (string url in regexHelper.GetImgUrl(content.ToString(), count))
                            {
                                result += url + "|";
                            }
                            return result.Trim('|');
                        }
                        catch (Exception)
                        {
                            return string.Format(Err_Lack_Param, "GetConPic");
                        }

                    case "SplitPicUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "SplitPicUrl");
                        }
                        else
                        {
                            //<ul>{ZL:SplitPicUrl(我是提示|demo/01.jpg$我是提示2|demo/02.jpg,perfix,suffix)/}</ul>
                            //前缀和后缀可以省略
                            string prefix = funparam.Split(',').Length > 1 ? funparam.Split(',')[1] : "";
                            string suffix = funparam.Split(',').Length > 2 ? funparam.Split(',')[2] : "";
                            funparam = funparam.Split(',')[0];
                            string[] imgurl = funparam.Split(new char[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
                            StringBuilder sb = new StringBuilder();
                            for (int i = 0; i < imgurl.Length; i++)
                            {
                                string title = imgurl[i].Split('|')[0];
                                string url = prefix + function.GetImgUrl(imgurl[i].Split('|')[1]) + suffix;
                                sb.Append("<li><img src='" + url + "' title ='" + title + "' /></li>");
                            }
                            return sb.ToString();
                        }
                    case "GetPicUrlCount":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return "0";
                        }
                        else
                        {
                            string[] downurl = funparam.Split(new char[] { '$' });
                            return downurl.Length.ToString();
                        }
                    case "RepWord":
                        {
                            return "";
                        }
                    case "PreInfoID":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "PreInfoID");
                        }
                        else
                        {
                            int infoid = DataConverter.CLng(funparam);

                            return bcontent.GetPreID(infoid).ToString();
                        }
                    case "NextInfoID":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "NextInfoID");
                        }
                        else
                        {
                            int infoid = DataConverter.CLng(funparam);

                            return bcontent.GetNextID(infoid).ToString();
                        }
                    case "GetPicUrl":
                        return function.GetImgUrl(funparam);
                    case "RemoveHtmlTag":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "RemoveHtmlTag");
                        }
                        {
                            string[] paraArr = new string[] { };
                            if (funparam.Contains("|||")) { paraArr = Regex.Split(funparam, Regex.Escape("|||")); }
                            else { paraArr = funparam.Split(','); }
                            if (paraArr.Length < 2) { return string.Format(Err_Lack_Param, "RemoveHtmlTag"); }
                            int len = DataConverter.CLng(paraArr[1]);//取出位置
                            string result = StringHelper.StripHtml(paraArr[0], len);
                            return result;
                        }
                    case "GetMod":
                        if (string.IsNullOrEmpty(funparam) || funparam.Split(',').Length < 2)
                        {
                            return string.Format(Err_Lack_Param, "GetMod");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(',');
                            int num = DataConverter.CLng(paraArr[0]);
                            int num1 = DataConverter.CLng(paraArr[1]);
                            if (num1 == 0)
                                return "[ERR:GetMod除数不能为0]";
                            if (num <= num1)
                                return num.ToString();
                            return (num % num1).ToString();
                        }
                    case "GetRepeatstr":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetRepeatstr");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new char[] { ',' });
                            if (paraArr.Length < 2)
                            {
                                return string.Format(Err_Lack_Param, "GetRepeatstr");
                            }
                            else
                            {
                                string num = paraArr[0].ToString();
                                int num1 = DataConverter.CLng(paraArr[1]);
                                if (num1 < 0)
                                    return "[ERR:GetRepeatstr位数不能小于0]";
                                string strnum = num.ToString();
                                for (int i = 0; i < num1 - 1; i++)
                                {
                                    strnum = strnum + num;
                                }
                                return strnum.ToString();
                            }
                        }
                    case "SimilarInfo":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "SimilarInfo");
                        }
                        else
                        {
                            string re = "";
                            string[] paraArr = funparam.Split(new char[] { ',' });
                            if (paraArr.Length < 2)
                            {
                                return "[ERR:SimilarInfo缺少要处理的参数]";
                            }
                            else
                            {
                                int infoid = DataConverter.CLng(paraArr[0]);
                                int num = DataConverter.CLng(paraArr[1]);
                                try
                                {
                                    re = GetSimilarInfo(infoid, num);
                                }
                                catch (Exception e)
                                {
                                    return "[ERR:SimilarInfo" + e.Message + "]";
                                }
                            }
                            return re;
                        }
                    case "Right":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "Right");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new char[] { ',' });
                            if (paraArr.Length < 2)
                            {
                                return string.Format(Err_Lack_Param, "Right");
                            }
                            else
                            {
                                string strs = paraArr[0].ToString();

                                int num1 = DataConverter.CLng(paraArr[1]);

                                if (num1 < 0)
                                {
                                    return "[ERR:Right位数不能小于0]";
                                }

                                string returnvalue = BaseClass.Right(strs, num1);
                                return returnvalue.ToString();
                            }
                        }
                    case "Left":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "Left");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new char[] { ',' });
                            if (paraArr.Length < 2)
                            {
                                return string.Format(Err_Lack_Param, "Left");
                            }
                            else
                            {
                                string strs = paraArr[0].ToString();

                                int num1 = DataConverter.CLng(paraArr[1]);

                                if (num1 < 0)
                                {
                                    return "[ERR:Left位数不能小于0]";
                                }

                                string returnvalue = BaseClass.Left(strs, num1);
                                return returnvalue.ToString();
                            }
                        }
                    case "SplitWord":
                        #region
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "SplitWord");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new string[] { "," }, StringSplitOptions.None);
                            if (paraArr.Length < 3)
                            {
                                return string.Format(Err_Lack_Param, "SplitWord");
                            }
                            else
                            {
                                string spstr = paraArr[0].ToString();//处理的字符串
                                string splitstr = paraArr[1].ToString();//截取的标识符
                                int strats = DataConverter.CLng(paraArr[2].ToString());//取出位置
                                if (spstr != "")
                                {
                                    if (spstr.IndexOf("" + splitstr + "") > -1)
                                    {
                                        string[] spstrarr = spstr.Split(new string[] { "" + splitstr + "" }, StringSplitOptions.None);
                                        if (strats <= spstrarr.Length)
                                        {
                                            return spstrarr[strats];
                                        }
                                        else
                                        {
                                            return spstr;
                                        }
                                    }
                                    else
                                    {
                                        return spstr;
                                    }
                                }
                            }
                            return "";
                        }
                    #endregion
                    case "Big5":
                        string retuntxt = "<A class=Channel href=\"javascript:void(0)\" name=\"StranLink\" id=\"StranLink\">繁体中文</A> <SCRIPT language=javascript src=\"/js/gb_big5.js\"></SCRIPT>";
                        return retuntxt;
                    case "HTML5":
                        string retunt = "<!--[if IE]>\n\t<script src=\"/js/html5.js\"></script>\n<![endif]-->";
                        return retunt;
                    case "Boot3":
                    case "Boot":
                        {
                            string html = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n";
                            html += "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n";
                            html += "<meta name=\"renderer\" content=\"webkit\">\n";
                            html += "<link href=\"/dist/css/bootstrap.min.css\" rel=\"stylesheet\"/>\n";
                            html += "<!--[if lt IE 9]>\n\t";
                            html += "<script src=\"https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js\"></script>\n";
                            html += "<script src=\"https://oss.maxcdn.com/libs/respond.js/1.3.0/respond.min.js\"></script>\n";
                            html += "<![endif]-->\n";
                            html += "<script src=\"/JS/jquery-1.11.1.min.js\" ></script>\n";
                            html += "<script src=\"/dist/js/bootstrap.min.js\"></script>\n";
                            html += "<link href=\"/dist/css/zico.min.css\" rel=\"stylesheet\"/>";
                            return html;
                        }
                    case "Boot4":
                        {
                            string html = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n";
                            html += "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n";
                            //html += "<meta name=\"renderer\" content=\"webkit\">\n";
                            html += "<meta name=\"renderer\" content=\"webkit|ie-comp|ie-stand\">\n";
                            html += "<link href=\"/dist/css/bootstrap4.min.css\" rel=\"stylesheet\"/>\n";
                            html += "<link href=\"/dist/css/zico.min.css\" rel=\"stylesheet\"/>\n";
                            html += "<script src=\"/JS/jquery.min.js\" ></script>\n";
                            html += "<script src=\"/dist/js/popper.min.js\"></script>\n";
                            html += "<script src=\"/dist/js/bootstrap4.min.js\"></script>";
                            return html;
                        }
                    case "BootZico":
                        {
                            string html = "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\n";
                            html += "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\n";
                            html += "<meta name=\"renderer\" content=\"webkit|ie-comp|ie-stand\">\n";
                            html += "<link href=\"/dist/css/bootstrap4.min.css\" rel=\"stylesheet\"/>\n";
                            html += "<script src=\"/JS/jquery.min.js\" ></script>\n";
                            html += "<script src=\"/dist/js/popper.min.js\"></script>\n";
                            html += "<script src=\"/dist/js/bootstrap4.min.js\"></script>\n";
                            html += "<script src=\"/dist/js/zico.min.js\"></script>";
                            return html;
                        }

                    case "IPprovince"://输出当前IP所在省份，非国内IP则输出归国信息
                        {
                            string ip = string.IsNullOrEmpty(funparam) ? IPScaner.GetUserIP(CurrentReq) : funparam;
                            return IPScaner.IPLocation(ip, 2);
                        }
                    case "IPcity"://输出当前IP所在城市，非国内IP则输出详细描述
                        {
                            string ip = string.IsNullOrEmpty(funparam) ? IPScaner.GetUserIP(CurrentReq) : funparam;
                            return IPScaner.IPLocation(ip, 3);
                        }
                    case "IPall"://输出当前IP的完整描述
                        {
                            string ip = string.IsNullOrEmpty(funparam) ? IPScaner.GetUserIP(CurrentReq) : funparam;
                            return IPScaner.IPLocation(ip, 4);
                        }
                    case "IPAdd"://输出当前IP地址
                        return IPScaner.GetUserIP(CurrentReq);
                    case "GetSplit":
                        #region
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetSplit");
                        }
                        else
                        {//GetSplit(字符(str),截取标识符(str),开始位置(0),结束位置(n),重复开始(str),重复中间(str),重负结束(str))
                            string[] paraArr = funparam.Split(new string[] { "," }, StringSplitOptions.None);
                            if (paraArr.Length < 7)
                            {
                                return string.Format(Err_Lack_Param, "GetSplit");
                            }
                            else
                            {
                                string spstr = paraArr[0].ToString();//处理的字符串
                                string splitstr = paraArr[1].ToString();//截取的标识符
                                int strats = DataConverter.CLng(paraArr[2].ToString());//开始位置
                                int ends = DataConverter.CLng(paraArr[3].ToString());//结束位置
                                string restr = paraArr[4].ToString();//重复开始字符串
                                string midstr = paraArr[5].ToString();//重复中间字符串
                                string endstr = paraArr[6].ToString();//重复结束字符串
                                string returnstr = "";
                                string[] tempstrd = spstr.Split(new string[] { "" + splitstr + "" }, StringSplitOptions.RemoveEmptyEntries);

                                if (tempstrd.Length < strats)
                                {
                                    strats = tempstrd.Length;
                                }

                                if (ends > tempstrd.Length)
                                {
                                    ends = tempstrd.Length;
                                }

                                for (int si = strats; si < ends; si++)
                                {
                                    if (si < ends - 1)
                                    {
                                        returnstr = returnstr + tempstrd[si] + midstr;
                                    }
                                    else
                                    {
                                        returnstr = returnstr + tempstrd[si];
                                    }
                                }
                                returnstr = restr + returnstr + endstr;
                                return returnstr.ToString();
                            }
                        }
                    #endregion
                    case "Panoramic":
                        #region
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "Panoramic");
                        }
                        else
                        {//GetSplit(字符(str),截取标识符(str),开始位置(0),结束位置(n),重复开始(str),重复中间(str),重负结束(str))
                            string[] paraArr = funparam.Split(new string[] { "," }, StringSplitOptions.None);
                            if (paraArr.Length < 3)
                            {
                                return string.Format(Err_Lack_Param, "Panoramic");
                            }
                            else
                            {
                                string p_id = paraArr[0].ToString();//全景ID
                                string p_width = paraArr[1].ToString();//宽度
                                string p_height = paraArr[2].ToString();//高度
                                StringBuilder returnbulid = new StringBuilder();
                                returnbulid.AppendLine("<object classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\"");
                                returnbulid.AppendLine("id=\"Show3Dmodel" + p_id + "\" width=\"" + p_width + "\" height=\"" + p_height + "\"");
                                returnbulid.AppendLine("codebase=\"http://fpdownload.macromedia.com/get/flashplayer/current/swflash.cab\">");
                                returnbulid.AppendLine("<param name=\"movie\" value=\"/Panoramic3D/Flex3d.swf?id=" + p_id + "\" />");
                                returnbulid.AppendLine("<param name=\"quality\" value=\"high\" />");
                                returnbulid.AppendLine("<param name=\"bgcolor\" value=\"#869ca7\" />");
                                returnbulid.AppendLine("<param name=\"allowScriptAccess\" value=\"sameDomain\" />");
                                returnbulid.AppendLine("<param name=\"allowFullScreen\" value=\"true\" />");
                                returnbulid.AppendLine("<embed src=\"/Panoramic3D/Flex3d.swf?id=" + p_id + "\" quality=\"high\" bgcolor=\"#869ca7\"");
                                returnbulid.AppendLine("width=\"" + p_width + "\" height=\"" + p_height + "\" name=\"Show3Dmodel\" align=\"middle\"");
                                returnbulid.AppendLine("play=\"true\"");
                                returnbulid.AppendLine("loop=\"false\"");
                                returnbulid.AppendLine("quality=\"high\"");
                                returnbulid.AppendLine("allowScriptAccess=\"sameDomain\"");
                                returnbulid.AppendLine("allowFullScreen=\"true\"");
                                returnbulid.AppendLine("type=\"application/x-shockwave-flash\"");
                                returnbulid.AppendLine("pluginspage=\"http://www.adobe.com/go/getflashplayer\">");
                                returnbulid.AppendLine("</embed>");
                                returnbulid.AppendLine("</object>");
                                return returnbulid.ToString();
                            }
                        }
                    #endregion
                    case "GetNodeLinkUrl":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetNodeLinkUrl");
                        }
                        else
                        {
                            int Nodeid = DataConverter.CLng(funparam);
                            return GetNodeLinkUrl(Nodeid);
                        }
                    case "Random":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "Random");
                        }
                        else
                        {
                            int length = DataConvert.CLng(funparam) == 0 ? 6 : Convert.ToInt32(funparam);
                            return function.GetRandomString(length, 2);
                        }
                    case "CityNameByCode"://根据编号获取省市县的名字
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "CityNameByCode");
                        }
                        break;
                    //case "CopyRight":
                    //    #region
                    //    {
                    //        //传入内容ID,与版权模板
                    //        int gid = DataConvert.CLng(GetParamFromArr(funparam, 0));
                    //        string name = GetParamFromArr(funparam, 1);
                    //        string html = "";
                    //        //如果未配置版权信息,则不操作
                    //        DataTable crdt = new B_Content_CR().SelByGidToDT(gid);
                    //        if (crdt.Rows.Count > 0)
                    //        {
                    //            name = string.IsNullOrEmpty(name) ? "default" : name;
                    //            html = SafeSC.ReadFileStr("/Common/Label/CR/" + name + ".tlp");
                    //            foreach (DataColumn dc in crdt.Columns)
                    //            {
                    //                html = html.Replace("@" + dc.ColumnName, DataConvert.CStr(crdt.Rows[0][dc.ColumnName]));
                    //            }
                    //        }
                    //        return "<div id=\"CopyRight\">" + html + "</div>";
                    //    }
                    #endregion
                    case "GetCSS"://从数据库中获取指定名称的CSS
                        {
                            //string name = GetParamFromArr(funparam, 0);
                            //string mode = GetParamFromArr(funparam, 1);
                            //string css = ZoomLa.BLL.Sys.B_Sys_CSSManage.SelTopCSS(name);
                            //switch (mode)
                            //{
                            //    case "0":
                            //        return "<link href=\"/com/css/" + name + "\" rel=\"stylesheet\" />";
                            //    default:
                            //        return "<style type=\"text/css\">/*From Zcss*/" + css.Replace(@"\n", "") + "</style>";
                            //}
                            return "";
                        }
                    //case "WXJSSign":
                    //    {
                    //        //https://mp.weixin.qq.com/wiki?t=resource/res_main&id=mp1421141115
                    //        //wx.config({{ZL:WXJSSign(APPID,)/},jsApiList:[]});
                    //        int appid = DataConvert.CLng(GetParamFromArr(funparam, 0));
                    //        string url = GetParamFromArr(funparam, 1);//Request.Url.AbsoluteUri
                    //        if (string.IsNullOrEmpty(url)) { url = CurrentReq.Request.Url.AbsoluteUri; }
                    //        WxAPI api = WxAPI.Code_Get(appid);
                    //        string wxappid = api.AppId.APPID;
                    //        string timeStamp = WxAPI.HP_GetTimeStamp();
                    //        string sign = api.JSAPI_GetSign(api.JSAPITicket, WxAPI.nonce, timeStamp, url);
                    //        return "appId: \"" + api.AppId.APPID + "\",timestamp:\"" + timeStamp + "\" , nonceStr:\"" + WxAPI.nonce + "\", signature:\"" + sign + "\"";
                    //    }
                    case "SiteInfo"://获取站点基本配置中的信息
                        #region
                        {
                            string name = GetParamFromArr(funparam, 0);
                            string result = "";
                            SiteInfo info = SiteConfig.SiteInfo;
                            switch (name)
                            {
                                case "网站名称":
                                    result = info.SiteName;
                                    break;
                                case "网站标题":
                                    result = info.SiteTitle;
                                    break;
                                case "网站地址":
                                    result = info.SiteUrl;
                                    break;
                                case "LOGO地址":
                                    result = info.LogoUrl;
                                    break;
                                case "官方二维码":
                                    result = info.BannerUrl;
                                    break;
                                case "公司名称":
                                    result = info.CompanyName;
                                    break;
                                case "站长姓名":
                                    result = info.Webmaster;
                                    break;
                                case "站长邮箱":
                                    result = info.WebmasterEmail;
                                    break;
                                case "站长手机":
                                    result = info.MasterPhone;
                                    break;
                                default:
                                    result = string.Format(Err_Param_Error, funcname, "(" + name + ")不存在");
                                    break;
                            }
                            return result;
                        }
                    #endregion
                    case "GetMoney":
                        if (string.IsNullOrEmpty(funparam))
                        {
                            return string.Format(Err_Lack_Param, "GetMoney");
                        }
                        else
                        {
                            string[] paraArr = funparam.Split(new char[] { ',' });
                            if (paraArr.Length < 2)
                            {
                                return string.Format(Err_Lack_Param, "GetMoney");
                            }
                            else
                            {
                                double num = DataConverter.CDouble(paraArr[0]);
                                int num1 = DataConverter.CLng(paraArr[1]);
                                int type = DataConverter.CLng(paraArr[2]);
                                if (num1 < 0)
                                    return "[ERR:GetMoney位数不能小于0]";
                                num = Math.Round(num, num1);


                                string strnum = num.ToString();
                                string[] str = strnum.Split(new char[] { '.' });
                                if (strnum.IndexOf(".") == -1)
                                {
                                    if (num1 > 0)
                                    {
                                        strnum = strnum + ".";

                                    }

                                    for (int i = 0; i <= num1 - 1; i++)
                                    {

                                        strnum = strnum + "0";
                                    }


                                }
                                else
                                {
                                    if (type == 1)
                                    {
                                        string[] ts = strnum.Split(new char[] { '.' });
                                        if (ts[1].Length < num1)
                                        {
                                            for (int i = 0; i < num1 - ts[1].Length; i++)
                                            {
                                                strnum = strnum + "0";
                                            }
                                            //strnum = strnum + "0";
                                        }
                                    }
                                    else
                                    {
                                        strnum = DataConverter.CLng(strnum).ToString();
                                    }
                                }
                                return strnum.ToString();
                            }
                        }
                }
                return "[ERR:(" + funcname + ")不可识别的扩展函数标签]";
                #endregion
            }
            catch (Exception ex)
            {
                WriteLog("FunLabelProc", funlabel, ex.Message);
                return "[ERR:(" + funcname + ")解析失败]";
            }
        }
        // 用于从,号切割的数组中取出参数
        private string GetParamFromArr(string funparam, int index)
        {
            string[] arr = (funparam ?? "").Split(',');
            return arr.Length > index ? arr[index] : "";
        }
        /// <summary>
        /// 返回配置的SiteUrl,结尾无/
        /// </summary>
        private string GetUrl()
        {
            return "";
            //return SiteConfig.SiteInfo.SiteUrl;
        }
        // 获得最新信息链接
        private string GetLastinfos(int nodeid)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(nodeid);
            string linkurl = "";

            string fileEx = B_Node.GetFileEx(nodeinfo.LastinfoPageEx);
            if (FileSystemObject.IsExist(SiteConfig.SiteMapath() + SiteConfig.SiteOption.GeneratedDirectory + @"\" + nodeinfo.NodeDir + @"\NodeNews" + fileEx, FsoMethod.File))
            {
                linkurl = "/";

                if (SiteConfig.SiteOption.GeneratedDirectory != "")
                {
                    linkurl += SiteConfig.SiteOption.GeneratedDirectory + "/";
                }
                if (nodeinfo.NodeDir != "")
                {
                    linkurl += nodeinfo.NodeDir + "/";
                }

                linkurl += "NodeNews" + fileEx;
            }
            else
            {
                if (GetUrl().IndexOf("http://") > -1)
                {
                    if (GetUrl().Substring(GetUrl().Length - 1, 1) != "/")
                    {
                        linkurl = GetUrl() + "/Class_" + nodeinfo.NodeID.ToString() + "/NodeNews";
                    }
                    else
                    {
                        linkurl = GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeNews";
                    }

                }
                else
                {
                    linkurl = "/" + GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeNews";
                }

            }
            return linkurl;
        }
        // 获得热门信息链接
        private string GetHotinfos(int nodeid)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(nodeid);
            string linkurl = "";

            string fileEx = B_Node.GetFileEx(nodeinfo.HotinfoPageEx);
            if (FileSystemObject.IsExist(SiteConfig.SiteMapath() + SiteConfig.SiteOption.GeneratedDirectory + @"\" + nodeinfo.NodeDir + @"\NodeHot" + fileEx, FsoMethod.File))
            {
                linkurl = "/";
                if (SiteConfig.SiteOption.GeneratedDirectory != "")
                {
                    linkurl += SiteConfig.SiteOption.GeneratedDirectory + "/";
                }
                if (nodeinfo.NodeDir != "")
                {
                    linkurl += nodeinfo.NodeDir + "/";
                }

                linkurl += "NodeHot" + fileEx;
            }
            else
            {
                if (GetUrl().IndexOf("http://") > -1)
                {
                    if (GetUrl().Substring(GetUrl().Length - 1, 1) != "/")
                    {
                        linkurl = GetUrl() + "/Class_" + nodeinfo.NodeID.ToString() + "/NodeHot";
                    }
                    else
                    {
                        linkurl = GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeHot";
                    }

                }
                else
                {
                    linkurl = "/" + GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeHot";
                }

            }
            return linkurl;
        }
        // 获得推荐信息链接
        private string GetProposeinfos(int nodeid)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(nodeid);
            string linkurl = "";

            string fileEx = B_Node.GetFileEx(nodeinfo.ProposePageEx);
            if (FileSystemObject.IsExist(SiteConfig.SiteMapath() + SiteConfig.SiteOption.GeneratedDirectory + @"\" + nodeinfo.NodeDir + @"\NodeElite" + fileEx, FsoMethod.File))
            {
                linkurl = "/";

                if (SiteConfig.SiteOption.GeneratedDirectory != "")
                {
                    linkurl += SiteConfig.SiteOption.GeneratedDirectory + "/";
                }
                if (nodeinfo.NodeDir != "")
                {
                    linkurl += nodeinfo.NodeDir + "/";
                }

                linkurl += "NodeElite" + fileEx;
            }
            else
            {
                if (GetUrl().IndexOf("http://") > -1)
                {
                    if (GetUrl().Substring(GetUrl().Length - 1, 1) != "/")
                    {
                        linkurl = GetUrl() + "/Class_" + nodeinfo.NodeID.ToString() + "/NodeElite";
                    }
                    else
                    {
                        linkurl = GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeElite";
                    }

                }
                else
                {
                    linkurl = "/" + GetUrl() + "Class_" + nodeinfo.NodeID.ToString() + "/NodeElite";
                }

            }
            return linkurl;
            //return GetUrl() + "NodeElite?NodeID=" + nodeid.ToString();
        }
        // 相关内容
        private string GetSimilarInfo(int infoid, int num)
        {
            StringBuilder sb = new StringBuilder();
            string key = bcontent.GetCommonData(infoid).TagKey;
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }
            else
            {
                string[] keyarr = key.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                string keysql = "";
                for (int i = 0; i < keyarr.Length; i++)
                {
                    if (string.IsNullOrEmpty(keysql))
                        keysql = "(TagKey like '" + keyarr[i] + "'";
                    else
                        keysql = keysql + " or TagKey like '" + keyarr[i] + "'";
                }
            }
            return sb.ToString();
        }
        //切割文本字符串
        private string CutText(string input, int len, string ex)
        {
            try
            {
                if (DataSecurity.Len(input) <= len)
                    return input;
                else
                {
                    return DataSecurity.GetSubString(input, len) + ex;
                }
            }
            catch
            {
                return "";
            }
        }
        private string GetCustom(int NodeID, int Num)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(NodeID);
            if (Num < 1)
                Num = 1;
            string Custom = nodeinfo.Custom;
            if (!string.IsNullOrEmpty(Custom))
            {
                string[] CustArr = Custom.Split(new string[] { "{SplitCustom}" }, StringSplitOptions.RemoveEmptyEntries);
                if (CustArr.Length > 0)
                {
                    if (Num <= CustArr.Length)
                    {
                        return CustArr[Num - 1];
                    }
                    else
                        return "";
                }
                else
                    return "";
            }
            return "";
        }
        // 根据节点ID获取该节点下内容页链接打开方式
        private string GetInfoOpen(int NodeID)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(NodeID);
            return nodeinfo.ItemOpenType ? "_blank" : "_self";
        }
        // 根据节点ID获取节点链接打开方式
        private string GetNodeOpen(int NodeID)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(NodeID);
            return nodeinfo.OpenTypeTrue;
        }
        // 根据节点ID获取内容链接打开方式
        private string GetNodeItemOpen(int NodeID)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(NodeID);
            return nodeinfo.ItemOpenType ? "_blank" : "_self";
        }
        // 获得当前连接地址
        private string GetNodeLinkUrl(int NodeID)
        {
            M_Node nodeinfo = nodeBll.SelReturnModel(NodeID);
            string linkurl = "";
            if (nodeinfo.NodeType == 3)
            {
                if (nodeinfo.NodeUrl.IndexOf("http://") > -1 || nodeinfo.NodeUrl.IndexOf("https://") > -1)
                {
                    linkurl = nodeinfo.NodeUrl;
                }
                else
                {
                    linkurl = GetUrl() + nodeinfo.NodeUrl;
                }
            }
            else
            {
                string fileEx = B_Node.GetFileEx(nodeinfo.ListPageHtmlEx);
                //----------------------------
                string ppath = SiteConfig.SiteMapath() + SiteConfig.SiteOption.GeneratedDirectory + @"\";
                M_Node pnodeMod = nodeBll.SelReturnModel(nodeinfo.ParentID);
                while (pnodeMod != null && pnodeMod.NodeID != 0)
                {
                    ppath += pnodeMod.NodeDir + "\\";
                    pnodeMod = nodeBll.SelReturnModel(pnodeMod.ParentID);
                }
                ppath += nodeinfo.NodeDir + "\\";
                ppath += "index" + fileEx;
                if (File.Exists(ppath))
                {
                    linkurl = function.PToV(ppath);
                }
                else
                {
                    if (GetUrl().IndexOf("http://") > -1)
                    {
                        if (GetUrl().Substring(GetUrl().Length - 1, 1) != "/")
                        {
                            linkurl = GetUrl() + "/Class_" + nodeinfo.NodeID + "/Default";
                        }
                        else
                        {
                            linkurl = GetUrl() + "Class_" + nodeinfo.NodeID + "/Default";
                        }
                    }
                    else
                    {
                        linkurl = "/" + GetUrl() + "Class_" + nodeinfo.NodeID + "/Default";
                    }
                }
            }
            return linkurl;
        }
        // 根据内容ID获取内容页链接
        private string GetInfoPath(int gid)
        {
            M_CommonData cinfo = this.bcontent.GetCommonData(gid);
            if (cinfo == null) { return ""; }
            if (cinfo.IsCreate == 1 && !string.IsNullOrEmpty(cinfo.HtmlLink) && File.Exists(function.VToP(cinfo.HtmlLink)))
            {
                return GetUrl() + "/" + (cinfo.HtmlLink.TrimStart('/'));
            }
            else
            {
                return GetUrl() + "/Item/" + gid + "";
            }
        }
        // 根据内容ID获取商品页链接
        private string GetShopPath(int p)
        {
            return GetUrl() + "/Shop/" + p + "";
        }
        //获取关键词查询语句
        private string GetKeyWord(string keyword, string field = "Tagkey")
        {
            string sql = " ";
            string[] keys = keyword.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in keys)
            {
                sql += " " + field + " LIKE '%" + s + "%' OR";
            }
            sql = sql.TrimEnd("OR".ToCharArray()) + " ";
            return sql;
        }
        // 百度编辑器大纲
        private string UeditorOL(string id)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(function.VToP(@"\Config\LabelSupport.xml"));
            XmlNode node = xmlDoc.SelectSingleNode("/DataSet/Table[LabelName='UeditorOL']");
            string nodeText = node.SelectSingleNode("LabelContent").InnerText;
            if (!string.IsNullOrEmpty(id))
            {
                nodeText = nodeText.Replace("$SID", id);
            }
            else
            {
                nodeText = nodeText.Replace("$SID", "txt_content");
            }
            return nodeText;
        }
        // 获取杂志图片数量
        private string MagazinePicCount(string id)
        {
            return "";
        }
        //评星控制支持,用于替换静态标签中的值,也可以用于动态化静态标签
        private string StarLabel(string param) //标签,值
        {
            string label = "{" + param.Split(',')[0] + "/}";
            string value = param.Split(',')[1];
            string s = CreateHtml(label, 0, 0, 0);
            s = s.Replace("{PubContentid/}", value);
            return s;
            //return "";
        }
        //{ZL:CreateLi(a,黑色:/item/1:/uploadfiles/1/222.jpg|红色:/item/1:/uploadfiles/1/222.jpg|紫色:/item/1:/uploadfiles/1/222.jpg)/}
        //<li class=”a1”><img src=”/uploadfiles/1/222.jpg” alt=”黑色” /><span>黑色</span></li>
        //<li class=”a2”><img src=”/uploadfiles/1/222.jpg” alt=”红色” /><span>红色</span></li>
        // 根据输入信息,返回li或其他格式信息,参数内容以:小切割,|大切割,用于58代卖商城
        private string CreateLi(string param)
        {
            string result = "", lihtml = "<li class='@css'><a href='@url'><img src='@imgurl' alt='@color' /><span>@color</span></a></li>";
            string css = param.Split(',')[0];
            string[] liarr = param.Split(',')[1].Split('|');
            for (int i = 0; i < liarr.Length; i++)
            {
                string color = liarr[i].Split(':')[0];
                string url = liarr[i].Split(':')[1];
                string imgurl = liarr[i].Split(':')[2];
                result += lihtml.Replace("@css", css + i).Replace("@url", url).Replace("@imgurl", imgurl).Replace("@color", color);
            }
            return result;
        }
        //将获取到的周转为中文
        private string GetChineseWeek(string week)
        {
            string result = "";
            switch (week)
            {
                case "Monday":
                    result = "星期一";
                    break;
                case "Tuesday":
                    result = "星期二";
                    break;
                case "Wednesday":
                    result = "星期三";
                    break;
                case "Thursday":
                    result = "星期四";
                    break;
                case "Friday":
                    result = "星期五";
                    break;
                case "Saturday":
                    result = "星期六";
                    break;
                case "Sunday":
                    result = "星期天";
                    break;
                default:
                    result = "无法转换";
                    break;
            }
            return result;
        }
        // 根据节点ID获取节点列表页
        private string GetNodeListPath(int NodeID)
        {
            M_Node nodeinfo = this.nodeBll.SelReturnModel(NodeID);
            if (!string.IsNullOrEmpty(nodeinfo.NodeUrl) && nodeinfo.ListPageHtmlEx < 3 && FileSystemObject.IsExist(SiteConfig.SiteInfo.SiteUrl + nodeinfo.NodeUrl, FsoMethod.File))
                return GetUrl() + nodeinfo.NodeUrl;
            else
                return GetUrl() + "/Class_" + NodeID + "/NodePage";
        }
        // 获得专题分类列表
        private string GetSpecialList(int funparam)
        {
            return GetUrl() + "/Special_" + funparam + "/List";
        }
        //专题列表页
        private string GetSpecialPage(int funparam)
        {
            return GetUrl() + "/Special_" + funparam + "/Default";
        }
     
        #region 新标签
        /// <summary>
        /// 处理Repeater(一个标签中可包含多个Repate,但如果是嵌套请使用动态标签)
        /// </summary>
        /// <param name="labelMod">已处理好参数的Label</param>
        /// <param name="dt">用于循环的数据DataTable</param>
        /// <returns>Html</returns>
        private string Deal_Repeate(M_Label labelMod, DataTable dt)
        {
            string content = labelMod.Content;
            StringBuilder sb = new StringBuilder("");
            List<L_StringInfo> list = GetStrBySE(ref content, "{Repeate}", "{/Repeate}");
            if (list.Count < 1) { return content; }
            foreach (L_StringInfo model in list)
            {
                //先处理其中的缓存标签,将其数据取出,处理Repeater中的ZL.Label，将数据准备好
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string result = For_Field(model.content_nose, dt, i);
                    result = result.Replace("{ZL:jsq}", (i + 1).ToString());
                    result = Deal_FunLabel(result);
                    sb.Append(result);
                }
                content = content.Replace("[" + model.flag + "]", sb.ToString());
                sb.Clear();
            }
            return content;
        }
        //替换扩展函数
        private string Deal_FunLabel(string tlp)
        {
            //{ZL:GetConPic2({ZL:GetConPic()/})/}
            string pattern = @"\({ZL:([\s\S])*?\/}\)"; //替换扩展函数
            string mat = "";
            MatchCollection matchs = Regex.Matches(tlp, pattern, RegexOptions.IgnoreCase); //字符串,正则公式，附加条件，此为忽略大小写
            foreach (Match match in matchs)
            {
                mat = match.Value.Substring(1, match.Value.Length - 2);
                tlp = tlp.Replace(mat, FunLabelProc(mat, 0, ""));
            }
            pattern = @"{ZL:([\s\S])*?\/}";
            matchs = Regex.Matches(tlp, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matchs)
            {
                mat = match.Value;
                tlp = tlp.Replace(mat, FunLabelProc(mat, 0, ""));
            }
            return tlp;
        }
        //替换For循环中的字段
        private static string For_Field(string temp, DataTable dt, int row)
        {
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string cname = dt.Columns[i].ColumnName;
                    //Oracle DT取出的为全大写,需要忽略大小写
                    temp = Regex.Replace(temp, "{Field=\"" + cname + "\"/}", DataConvert.CStr(dt.Rows[row][cname]), RegexOptions.IgnoreCase);
                    //temp = temp.Replace("{Field=\"" + cname + "\"/}", dt.Rows[row][cname].ToString());
                }
            }
            else
            {
                //无数据情况下,直接将占位符替換
                foreach (DataColumn dc in dt.Columns)
                {
                    temp = Regex.Replace(temp, "{Field=\"" + dc.ColumnName + "\"/}", "", RegexOptions.IgnoreCase);
                }
            }
            return temp;
        }
        //新版For标签,不启用
        //private static string Deal_For(M_Label labelMod, DataTable dt)
        //{
        //    string content = labelMod.Content;
        //    StringBuilder sb = new StringBuilder(""); //{For Filter="UserID<0" Exp="i=0;1<10;i+1" }<div>sfsfsf</div>{/For}
        //    string pattern = @"{For([\s\S])*}([\s\S])*?({/For})";
        //    if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase))
        //    {
        //        //读取参数,确定循环条件
        //        string p1 = @"(?<={For)([\s\S])*?(})";
        //        string mylabel = Regex.Match(content, p1, RegexOptions.IgnoreCase).Value.Replace("}", "");// Filter="UserID<0" Exp="0;<;10;+;1" //代表数据表中字段的起始位与结束位
        //        string[] param = mylabel.Split(' ');
        //        string filter = GetParam(content, "Filter");
        //        string[] exp = GetParam(content, "Exp").Split(new char[] { ';' }, StringSplitOptions.None);
        //        dt.DefaultView.RowFilter = filter;
        //        DataTable mydt = dt.DefaultView.ToTable();

        //        //获取循环内容
        //        string p2 = @"(?<={For([\s\S])*})([\s\S])*?({/For})";//取模板正则
        //        string temp = Regex.Match(content, p2, RegexOptions.IgnoreCase).Value.Replace("{/For}", "");
        //        int j = 0, v = 0;//移

        //        //获取for循环条件,如果exp为空,则输出mydt中全部数据
        //        if (exp.Length < 2)//如为空,则输出dt中所有数据,为空情况下该值为1
        //        {
        //            exp = ("0;<;" + mydt.Rows.Count + ";+;1").Split(new char[] { ';' }, StringSplitOptions.None);
        //        }
        //        j = Convert.ToInt32(exp[2]);//循环条件,每次循环需要增加或减少的数值
        //        v = Convert.ToInt32(exp[4]);
        //        switch (exp[1])
        //        {
        //            case "<":
        //                for (int i = Convert.ToInt32(exp[0]); i < mydt.Rows.Count && i < j; )
        //                {
        //                    sb.Append(For_Field(temp, mydt, i));
        //                    i = For_DealOP(exp[3], i, v);
        //                }
        //                break;
        //            case ">":
        //                for (int i = Convert.ToInt32(exp[0]); i < mydt.Rows.Count && i > j; )
        //                {
        //                    sb.Append(For_Field(temp, mydt, i));
        //                    i = For_DealOP(exp[3], i, v);
        //                }
        //                break;
        //            case "<=":
        //                for (int i = Convert.ToInt32(exp[0]); i < mydt.Rows.Count && i <= j; )
        //                {
        //                    sb.Append(For_Field(temp, mydt, i));
        //                    i = For_DealOP(exp[3], i, v);
        //                }
        //                break;
        //            case ">=":
        //                for (int i = Convert.ToInt32(exp[0]); i < mydt.Rows.Count && i >= j; )
        //                {
        //                    sb.Append(For_Field(temp, mydt, i));
        //                    i = For_DealOP(exp[3], i, v);
        //                }
        //                break;
        //        }//Switch(exp[1]) end;
        //    }//For End;
        //    return sb.ToString();
        //}
        //private int For_DealOP(string op, int i, int v)
        //{
        //    switch (op)
        //    {
        //        case "-":
        //            i = i - v;
        //            break;
        //        case "+":
        //            i = i + v;
        //            break;
        //    }
        //    return i;
        //}
        #endregion
        #region 辅助工具方法 
        /// <summary>
        /// 清除掉{table1}.dbo|{table2}.dbo之类的字符
        /// </summary>
        private static string ClearTableHolder(string text)
        {
            text = text.Replace("{table1}.dbo.", "").Replace("{table2}.dbo.", "");
            return text;
        }
        /// <summary>
        /// 解析处理下载地址,看是本站还是外站
        /// </summary>
        private static string GetDownUrl(string url)
        {
            return "";
        }
        private static void WriteLog(string func, string label, string message, string remind = "")
        {
            string tlp = "来源：B_CreateHtml,方法名：{0},标签：{1},报错：{2},备注：{3}";
            string msg = string.Format(tlp, func, label, message, remind);
            //ZLLog.L(ZLEnum.Log.labelex, string.Format(tlp, func, label, message, remind));
            //throw new Exception(msg);
        }
        //从标签中获取指定参数,如不存在,则返回空
        private static string GetFileName(string regFiled)
        {
            return regFiled.Replace("{Field=", "").Replace("/}", "").Replace("\"", "");
        }
        /// <summary>
        /// 根据参数名,从标签中正则匹配,并将值返回
        /// </summary>
        private static string GetParam(string label, string pname)
        {
            //以{或空格开头=,中间最少要有一个内容，且以"空格或"/}结尾
            string reg = "(?<=([\\s]|{)" + pname + @"="")([\s\S]){0,}?((""[\s]|""/}))".Replace("/}", "");
            return Regex.Match(label, reg).Value.Replace("\"", "");
        }
        /// <summary>
        /// 将标签的参数分解
        /// </summary>
        private static Hashtable GetParam(string ilabel)
        {
            ilabel = ilabel.Replace("{ZL.Label ", "");
            ilabel = ilabel.Replace("{ZL.Source ", "");
            ilabel = ilabel.Replace("{ZL.Page ", "");
            ilabel = ilabel.Replace("/}", "").Replace(" ", ",");


            Hashtable hb = new Hashtable();
            string[] paArr = ilabel.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string key = "";
            string value = "";
            //id="测试动态1",NodeID="",
            for (int i = 0; i < paArr.Length; i++)
            {
                try
                {
                    //由col-2:引起
                    if (!paArr[i].Contains("=")) continue;
                    key = paArr[i].Split(new char[] { '=' })[0];
                    value = paArr[i].Split(new char[] { '=' })[1].Replace("\"", "");
                    hb.Add(key, value);
                }
                catch (Exception ex) { WriteLog("GetParam", ilabel, ex.Message, paArr[i]); }
            }
            if (hb.Contains("id") && !string.IsNullOrEmpty(hb["id"].ToString())) { hb["id"] = hb["id"].ToString().Replace(" ", ""); }
            return hb;
        }
        /// <summary>
        /// 传入需要解析的字符串,获取标签
        /// </summary>
        private static M_Label GetLabelXML(string ilabel)
        {
            M_Label labelMod = new M_Label();
            Hashtable hb = GetParam(ilabel);
            if (!hb.Contains("id") || string.IsNullOrEmpty(hb["id"].ToString()))
            {
                labelMod.ErrorMsg = "[ERR:缺少ID标识符]";
            }
            labelMod = new B_Label().GetLabelXML(hb["id"].ToString());
            if (labelMod.IsNull) { labelMod.ErrorMsg = "[ERR:(" + hb["id"].ToString() + ")标签不存在]"; }
            return labelMod;
        }
        /// <summary>
        /// 处理标签中的参数,用于ZL.Label,数据源等
        /// </summary>
        /// <param name="labelMod">标签模型</param>
        /// <param name="ilabel">页面上放置的标签</param>
        /// <param name="InfoID">无则填0或1</param>
        /// <returns>已替换完@参数的标签模型</returns>
        private static M_Label LabelParamFunc(M_Label label, string ilabel, string InfoID)
        {
            Hashtable hb = GetParam(ilabel);
            M_Label rmod = (M_Label)label.Clone();//返回的结果
            rmod.LabelField = ClearTableHolder(rmod.LabelField);
            rmod.LabelTable = ClearTableHolder(rmod.LabelTable);
            rmod.LabelWhere = ClearTableHolder(rmod.LabelWhere);
            rmod.LabelOrder = ClearTableHolder(rmod.LabelOrder);
            if (string.IsNullOrEmpty(label.Param)) { return rmod; }
            //--------------------------------------------------------
            string[] pa = label.Param.Split(new char[] { '|' });
            for (int i = 0; i < pa.Length; i++)
            {
                string paname = pa[i].Split(new char[] { ',' })[0];
                if (DataConverter.CLng(pa[i].Split(new char[] { ',' })[2]) == 2)//页面参数。可能生成时使用线程会出现问题，线程不支持页面参数
                {
                    rmod.Content = rmod.Content.Replace("@" + paname, InfoID);
                    rmod.EmptyContent = rmod.EmptyContent.Replace("@" + paname, InfoID);
                    rmod.LabelField = rmod.LabelField.Replace("@" + paname, InfoID);
                    rmod.LabelWhere = rmod.LabelWhere.Replace("@" + paname, InfoID);
                    rmod.LabelCount = rmod.LabelCount.Replace("@" + paname, InfoID);
                    rmod.LabelOrder = rmod.LabelOrder.Replace("@" + paname, InfoID);
                }
                else//普通参数
                {
                    string wheretxt = "";
                    if (ilabel.IndexOf(paname + "=\"") > -1)
                    {
                        string[] t2 = ilabel.Split(new string[] { paname + "=\"" }, StringSplitOptions.None);
                        string[] t3 = t2[1].Split(new string[] { "\"" }, StringSplitOptions.None);
                        wheretxt = t3[0];
                    }
                    else
                    {
                        foreach (DictionaryEntry de in hb)
                        {
                            if (de.Key.Equals(paname)) { wheretxt = de.Value.ToString(); break; }
                        }
                    }
                    if (BaseClass.Right(wheretxt, 1) == ",")
                    {
                        wheretxt = BaseClass.Left(wheretxt, wheretxt.Length - 1);
                    }
                    if (string.IsNullOrEmpty(wheretxt))//无匹配置,则读取默认值
                    {
                        wheretxt = pa[i].Split(new char[] { ',' })[1].ToString();
                        if (wheretxt.Equals("默认值")) { wheretxt = ""; }
                    }
                    rmod.LabelTable = rmod.LabelTable.Replace("@" + paname, wheretxt);
                    rmod.Content = rmod.Content.Replace("@" + paname, wheretxt);
                    rmod.EmptyContent = rmod.EmptyContent.Replace("@" + paname, InfoID);
                    rmod.LabelField = rmod.LabelField.Replace("@" + paname, wheretxt);
                    rmod.LabelWhere = rmod.LabelWhere.Replace("@" + paname, wheretxt);
                    rmod.LabelCount = rmod.LabelCount.Replace("@" + paname, wheretxt);
                    rmod.LabelOrder = rmod.LabelOrder.Replace("@" + paname, wheretxt);
                    //label.ProceParam = label.ProceParam.Replace("@" + paname, wheretxt);//存储过程参数
                }
            }
            return rmod;
        }
        /// <summary>
        /// 判断是否存在标签
        /// </summary>
        /// <param name="Result">包含标签的字符串,即页面或标签内容</param>
        private static bool CheckisTrue(string Result)
        {
            if (string.IsNullOrEmpty(Result)) { return false; }
            string[] pattern = new string[7];
            bool iss = false;
            pattern[0] = @"{ZL\.Source([\s\S])*?\/}";
            pattern[1] = @"{ZL\.Label([\s\S])*?\/}";
            pattern[2] = @"{\$([\s\S])*?\/}";
            pattern[3] = @"{ZL:([\s\S])*?\/}";
            pattern[4] = @"\{Pub.Load_[\s\S]*?/}";
            pattern[5] = @"\{\$[\s\S]*?\}";
            pattern[6] = @"\{\$[\s\S]*?\$\}";
            for (int i = 0; i < pattern.Length; i++)
            {
                MatchCollection matchs = Regex.Matches(Result, pattern[i], RegexOptions.IgnoreCase);
                if (matchs.Count > 0) { iss = true; break; }
            }
            return iss;
        }
        /// <summary>
        /// 标签IF Else判断功能,用于ZL.Label
        /// </summary>
        /// <param name="Cpage">页码</param>
        /// <param name="InfoID"></param>
        /// <param name="label">标签模型</param>
        /// <returns></returns>
        private string Getjude(int Cpage, int InfoID, M_Label label)
        {
            if (label == null || label.IsOpen == 0) { return label.Content; }

            string sqllabelcontent = "";
            //变量占位符
            string tempValue = GetContent(Cpage, InfoID, "0", label.Valueroot);
            //要对比的变量占位符(可为常量)
            string tempModelvalue = GetContent(Cpage, InfoID, "0", label.Modelvalue);
            switch (label.Modeltypeinfo)
            {
                case "参数判断":
                    #region
                    switch (label.setroot)
                    {
                        case "大于":
                            if (DataConverter.CLng(tempValue) > DataConverter.CLng(tempModelvalue))
                            {
                                sqllabelcontent = label.Content;
                            }
                            else
                            {
                                sqllabelcontent = label.FalseContent;
                            }
                            break;
                        case "等于":
                            if (tempValue == tempModelvalue)
                            {
                                sqllabelcontent = label.Content;
                            }
                            else
                            {
                                sqllabelcontent = label.FalseContent;
                            }
                            break;
                        case "小于":
                            if (DataConverter.CLng(tempValue) < DataConverter.CLng(tempModelvalue))
                            {
                                sqllabelcontent = label.Content;
                            }
                            else
                            {
                                sqllabelcontent = label.FalseContent;
                            }
                            break;
                        case "不等于":
                            if (tempValue != tempModelvalue)
                            {
                                sqllabelcontent = label.Content;
                            }
                            else
                            {
                                sqllabelcontent = label.FalseContent;
                            }
                            break;
                    }
                    #endregion
                    break;
                case "用户登录判断":
                    if (buser.CheckLogin())
                    {
                        sqllabelcontent = label.Content;
                    }
                    else
                    {
                        sqllabelcontent = label.FalseContent;
                    }
                    break;
                default:
                    sqllabelcontent = label.Content;
                    break;
            }
            return sqllabelcontent;
        }
        /// <summary>
        /// [disuse]从指定位置开始截取指定长度后的位置（如果其中包含不完整html，则将完整html包含进去重新计算完整html结束位置）
        /// </summary>
        private static int GetHtmlPosition(string content, int beginPoint, int Num)
        {
            string pattern = @"<p>([\s\S])*?</p>|<img([\s\S])*?/>|<li>([\s\S])*?</li>";
            int endPos = beginPoint + Num - 1;
            //string tmp = content.Substring(beginPoint, endPos - beginPoint);

            int RePos = beginPoint;
            int matchlen = 0;
            MatchCollection matchs = new Regex(pattern, RegexOptions.IgnoreCase).Matches(content, beginPoint);

            if (matchs.Count < 1)
            {
                return endPos;
            }
            else
            {
                for (int i = 0; i < matchs.Count; i++)
                {
                    matchlen = matchs[i].Index + matchs[i].Length;
                    if (RePos >= endPos)
                    {
                        return RePos;
                    }
                    else
                    {
                        if (matchlen >= endPos)
                        {
                            return matchlen;
                        }
                        else
                        {
                            RePos = matchlen + 1;
                        }
                    }
                }
            }
            return endPos;
        }
        /// <summary>
        /// 返回字符串数组,不包含起始与结束字符
        /// </summary>
        private static List<L_StringInfo> GetStrBySE(ref string content, string s, string e)
        {
            List<L_StringInfo> list = new List<L_StringInfo>();
            if (StrHelper.StrNullCheck(content, s, e) || content.IndexOf(s) < 0 || content.IndexOf(e) < 0) { return list; }
            while (content.Contains(s) && content.Contains(e))
            {
                L_StringInfo model = new L_StringInfo();
                model.char_start = s;
                model.char_end = e;
                model.start = content.IndexOf(s);
                model.len = content.IndexOf(e) + model.char_end.Length - model.start;//起始与结束位等
                model.content = content.Substring(model.start, model.len);
                list.Add(model);
                content = content.Remove(model.start, model.len);
                content = content.Insert(model.start, "[" + model.flag + "]");
            }
            return list;
        }
        /// <summary>
        /// 从html中获取动态标签(暂时只用于动态标签)
        /// </summary>
        private static string GetLabelFromHtml(string html)
        {
            //170920:{ZL.Label id="test" Gid="{ZL.Label id="获取组ID" Test="{ZL.Label id="Test标签" /}" /}" /}   OK
            //180425:{ZL.Label id="根据会员ID输出会员名" Uid="{Field="NodeID"/}" /}-->{ZL.Label id="根据会员ID输出会员名" Uid="{Field="NodeID"/}
            try
            {
                string start = "{ZL.Label ";//{Field
                string end = "/}";
                if (string.IsNullOrEmpty(html) || html.Length < (start.Length + end.Length)) { return ""; }
                int sIndex = html.IndexOf(start);
                int eIndex = html.IndexOf(end, sIndex + start.Length);
                if (sIndex < 0 || eIndex < 0) { return ""; }
                eIndex += end.Length;
                //是否包含子标签,如是则优先解析子标签
                string label = html.Substring(sIndex, eIndex - sIndex);
                int lastIndex = label.LastIndexOf(start);
                if (lastIndex > 0)
                {
                    //return GetLabelFromHtml(label.Substring(lastIndex, label.Length - lastIndex));
                    label = label.Substring(lastIndex, label.Length - lastIndex);
                }

                return label;
            }
            catch (Exception ex) { WriteLog("GetLabelFromHtml()", "", ex.Message); return ""; }
        }
        #endregion
        //切割出的字符串信息,用于{Repeater}
        private class L_StringInfo
        {
            public L_StringInfo() { flag = function.GetRandomString(8); }
            /// <summary>
            /// 起始符的位置(相对整个字符串)
            /// </summary>
            public int start = 0;
            /// <summary>
            /// 结束符的位置(相对整个字符串)
            /// </summary>
            public int end { get { return start + len; } }
            /// <summary>
            /// 字符串的长度
            /// </summary>
            public int len = 0;
            public string char_start = "";
            public string char_end = "";
            /// <summary>
            /// 包含起始与结束符号的内容
            /// </summary>
            public string content = "";
            public string flag = "";
            public string content_nose { get { return content.Substring(char_start.Length, content.Length - char_start.Length - char_end.Length); } }
        }
    }
}