﻿using System;
using System.Data;
using ZoomLa.Common;
using ZoomLa.Components;
using System.Web;
using ZoomLa.Model;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading;
using System.Xml;
using ZoomLa.BLL.CreateJS;
using System.IO;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.BLL.Content;
using Microsoft.AspNetCore.Http;
using ZoomLa.Model.Content;

namespace ZoomLa.BLL
{
    public class B_Create
    {
        B_Model bmodel = new B_Model();
        B_Node nodeBll = new B_Node();
        B_Content conBll = new B_Content();
        B_CreateHtml bll;
        B_User buser = new B_User();
        public B_Create(HttpContext r)
        {
            bll = new B_CreateHtml(r);
        }
        //------------------------------Tools
        private void ReplaceHref(ref string contentHtml, int pageCount, string FieldExtstr, string pagename = "index") //用于支持生成列表页,CreateNodePage
        {
            contentHtml = Regex.Replace(contentHtml, "CreateHtml1.aspx", pagename + FieldExtstr, RegexOptions.IgnoreCase);
            contentHtml = Regex.Replace(contentHtml, "EditContent1.aspx", pagename + FieldExtstr, RegexOptions.IgnoreCase);
            for (int i = 2; i <= pageCount; i++)
            {
                contentHtml = Regex.Replace(contentHtml, "CreateHtml" + i + ".aspx", pagename + "_" + i + FieldExtstr, RegexOptions.IgnoreCase);
                contentHtml = Regex.Replace(contentHtml, "EditContent" + i + ".aspx", pagename + "_" + i + FieldExtstr, RegexOptions.IgnoreCase);
            }
        }
        //从Html中取出页码计数
        private int GetPageCount(string html)
        {
            int pcount = 0;
            var regexObj = new Regex("id=\"pageDiv\" totalPage=\"\\d+\"", RegexOptions.IgnoreCase);
            var matchResult = regexObj.Match(html);
            if (matchResult.Success)
            {
                //pageType = 1;//BootStrap数字类型分页
                Regex regexObj2 = new Regex(@"\d+");
                Match matchResult2 = regexObj2.Match(matchResult.Value);
                if (matchResult2.Success)
                {
                    pcount = DataConverter.CLng(matchResult2.Value);
                }
            }
            return pcount < 1 ? 1 : pcount;
        }
        // 获得扩展名
        private string GetFileEx(int ContentFileEx)
        {
            return B_Node.GetFileEx(ContentFileEx);
        }
        /// <summary>
        /// 返回模板文件的物理路径
        /// </summary>
        /// <param name="vpath">节点模板URL</param>
        private string GetTemplateUrl(string vpath)
        {
            if (string.IsNullOrEmpty(vpath) || string.IsNullOrEmpty(SiteConfig.SiteOption.TemplateDir)) { return ""; }
            string result = SiteConfig.SiteOption.TemplateDir + "/" + vpath;
            result = result.Replace("\\", "/").Replace("//", "/");
            return function.VToP(result);
        }
        /// <summary>
        /// 获取指定节点的父目录,用于设定存放html页面的路径
        /// </summary>
        /// <param name="NodeID">起始NodeID</param>
        private string GetParentDir(int NodeID)
        {
            string NodeDir = "";
            M_Node nodeinfo = nodeBll.SelReturnModel(NodeID);
            if (!nodeinfo.IsNull)
            {
                if (nodeinfo.ParentID > 0 && nodeinfo.HtmlPosition > 0)
                {
                    NodeDir = GetParentDir(nodeinfo.ParentID) + "/" + nodeinfo.NodeDir;
                }
                else
                {
                    NodeDir = "/" + nodeinfo.NodeDir;
                }
            }
            return NodeDir;
        }
        /// <summary>
        /// 根据配置计算出html应该放置在哪个目录下(虚拟路径)
        /// </summary>
        private string GetNodeDir(M_Node nodeinfo)
        {
            string NodeDir = string.IsNullOrEmpty(nodeinfo.NodeDir) ? "" : "/" + nodeinfo.NodeDir.TrimStart('/');
            if (nodeinfo.HtmlPosition > 0 && nodeinfo.ParentID > 0)
            {
                if (nodeinfo.NodeType == 1)
                    NodeDir = GetParentDir(nodeinfo.ParentID) + NodeDir;
                else
                    NodeDir = GetParentDir(nodeinfo.ParentID);
            }
            NodeDir = NodeDir.TrimEnd('/') + '/';
            return NodeDir;
        }
        /// <summary>
        /// 节点是否存了栏目的index地址,如未存则更新,存了则继承
        /// shtml页面如无法打开,需要在mime中添加映射,同于html
        /// </summary>
        private string GetInfoFile(M_Node nodeinfo, string pageName)
        {
            string InfoFile = "";
            string NodeDir = GetNodeDir(nodeinfo);
            switch (pageName)
            {
                case "index":
                    {
                        //为空或以aspx结尾则重新处理
                        if (string.IsNullOrEmpty(nodeinfo.NodeUrl) || nodeinfo.NodeUrl.ToLower().Contains(".aspx"))
                        {
                            if (nodeinfo.NodeType == 1) //栏目节点
                            {
                                InfoFile = NodeDir + "/" + pageName;//index listpage
                            }
                            else //单页
                            {
                                InfoFile = NodeDir.TrimEnd('/');//不去除则会 404/.html
                            }
                            InfoFile = InfoFile + GetFileEx(nodeinfo.ListPageHtmlEx);
                            DBCenter.UpdateSQL("ZL_Node", "NodeUrl=@InfoFile", "NodeID=" + nodeinfo.NodeID,
                                new List<SqlParameter>() { new SqlParameter("InfoFile", InfoFile) });
                        }
                        else
                        {
                            InfoFile = nodeinfo.NodeUrl;
                        }
                    }
                    break;
                case "listpage":
                    {
                        if (string.IsNullOrEmpty(nodeinfo.NodeListUrl) || nodeinfo.NodeListUrl.ToLower().Contains(".aspx"))
                        {
                            if (nodeinfo.NodeType == 1) //栏目节点
                            {
                                InfoFile = NodeDir + "/" + pageName;//index listpage
                            }
                            else //单页
                            {
                                InfoFile = NodeDir.TrimEnd('/');//不去除则会 404/.html
                            }
                            InfoFile = InfoFile + GetFileEx(nodeinfo.ListPageEx);
                            DBCenter.UpdateSQL("ZL_Node", "NodeListUrl=@InfoFile", "NodeID=" + nodeinfo.NodeID,
                                new List<SqlParameter>() { new SqlParameter("InfoFile", InfoFile) });
                        }
                        else
                        {
                            InfoFile = nodeinfo.NodeListUrl;
                        }
                    }
                    break;
            }
            string gdir = "/" + SiteConfig.SiteOption.GeneratedDirectory.TrimStart('/');
            InfoFile = function.VToP(gdir + InfoFile);
            return InfoFile;
        }
        //------------------------------Tools End; 
        /// <summary>
        /// 用于处理分页
        /// </summary>
        private string GetPagenurl(ref string contents)
        {
            Regex regexObj = new Regex("[\"|\'][-A-Z0-9+&@#/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|][\"\']", RegexOptions.IgnoreCase);
            Match matchResults = regexObj.Match(contents);
            string gd = SiteConfig.SiteOption.GeneratedDirectory;
            if (!gd.StartsWith("/"))
            {
                gd = "/" + gd;
            }
            string GeneratedDirectory = gd;//生成路径
            int maxpagenum = 0;
            int thisnodeid = 0;

            while (matchResults.Success)
            {
                if (matchResults.ToString().IndexOf(".aspx") > -1)
                {
                    if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                    {
                        string stringlist = matchResults.ToString();
                        string slist = stringlist;
                        contents = GetPagenurl(ref contents, matchResults, GeneratedDirectory, ref maxpagenum, ref thisnodeid, ref stringlist);
                    }
                }
                matchResults = matchResults.NextMatch();
            }
            return contents;
        }
        private string GetPagenurl(ref string contents, Match matchResults, string GeneratedDirectory, ref int maxpagenum, ref int thisnodeid, ref string stringlist)
        {
            if (stringlist.IndexOf("&p=") > -1)//处理分页内容
            {
                string[] stringlistarr = stringlist.Split(new string[] { "&p=" }, StringSplitOptions.None);
                stringlist = stringlistarr[0];
                int infoid = GetInfoID(stringlist, "nodeid=");
                M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                thisnodeid = nodeinfo.NodeID;
                string NodeUrl = nodeinfo.NodeUrl;
                int pagenum = 0;
                if (matchResults.ToString().IndexOf("&p=") > -1)//处理分页内容
                {
                    string pageurl = matchResults.ToString();
                    pageurl = pageurl.Replace("'", "");
                    string[] pageurlarr = pageurl.Split(new string[] { "&p=" }, StringSplitOptions.None);

                    if (pageurlarr.Length > 0)
                    {
                        if (pageurlarr[1].IndexOf("&") > -1)
                        {
                            string[] pageurlarrarr = pageurlarr[1].Split(new string[] { "&" }, StringSplitOptions.None);
                            pagenum = DataConverter.CLng(pageurlarrarr[0]);
                        }
                        else
                        {
                            pagenum = DataConverter.CLng(pageurlarr[1]);
                        }
                    }

                    if (pagenum > maxpagenum) { maxpagenum = pagenum; }

                    string filename = "_" + infoid.ToString() + "_" + pagenum.ToString();
                    if (pagenum == 1) { filename = ""; }
                    if (!string.IsNullOrEmpty(NodeUrl))
                    {
                        if (NodeUrl.IndexOf(".") > -1)
                        {
                            string[] nodearr = NodeUrl.Split(new string[] { "." }, StringSplitOptions.None);
                            NodeUrl = nodearr[0].ToString() + filename + "." + nodearr[1].ToString();
                        }
                    }
                    string aspxpage = matchResults.ToString();
                    aspxpage = aspxpage.Replace("'", "");
                    aspxpage = "/" + aspxpage;//动态地址
                    string htmlpage = "/" + GeneratedDirectory + NodeUrl;//静态地址
                    contents = contents.Replace(matchResults.ToString(), htmlpage);
                }
            }
            return contents;
        }
        /// <summary>
        /// 生成首页
        /// </summary>
        /// <param name="IndexDir">模板地址</param>
        /// <param name="indexex">后缀名</param>
        /// <param name="Dir">首页存放地址</param>
        public void CreateIndex(string IndexDir, string indexex, string Dir)
        {
            if (FileSystemObject.IsExist(IndexDir, FsoMethod.File))
            {
                try
                {
                    string IndexHtml = bll.CreateHtml(FileSystemObject.ReadFile(IndexDir), 0, 0, "0");
                    IndexHtml = Gethtmlpageurl(IndexHtml);
                    #region 查找ColumnList
                    StringCollection resultList = new StringCollection();
                    Regex regexObj = new Regex(@"ColumnList\.aspx\?NodeID=[\w]*", RegexOptions.Multiline);
                    Match matchResult = regexObj.Match(IndexHtml);
                    while (matchResult.Success)
                    {
                        string MatchValue = matchResult.Value;
                        try
                        {
                            Regex newregexObj = new Regex("[0-9]*", RegexOptions.IgnorePatternWhitespace);
                            Match matchResultstext = newregexObj.Match(MatchValue);

                            IList<int> nodelist = new List<int>();
                            while (matchResultstext.Success)
                            {
                                if (matchResultstext.ToString() != "")
                                {
                                    int nodeid = DataConverter.CLng(matchResultstext.ToString());
                                    if (nodelist.IndexOf(nodeid) == -1)
                                    {
                                        nodelist.Add(nodeid);
                                    }
                                }
                                matchResultstext = matchResultstext.NextMatch();
                            }
                            if (nodelist.Count > 0)
                            {
                                for (int nid = 0; nid < nodelist.Count; nid++)
                                {
                                    int NodeID = DataConverter.CLng(nodelist[nid]);
                                    M_Node ninfo = nodeBll.SelReturnModel(NodeID);

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("生成首页错误", ex.Message);
                        }
                        matchResult = matchResult.NextMatch();
                    }
                    #endregion
                    string indexPath = function.VToP(Dir + "\\index" + indexex);
                    FileSystemObject.WriteFile(indexPath, IndexHtml);
                    B_Release.AddResult("生成首页", indexPath);
                }
                catch (Exception ex)
                {
                    WriteLog("生成首页", ex.Message);
                }
            }
            else
            {
                B_Release.AddResult("首页生成失败——[" + IndexDir + "]模板文件不存在");
            }
        }
        /// <summary>
        /// 生成栏目节点||单页的首页
        /// </summary>
        /// <param name="NodeID">节点ID</param>        
        public void CreateNodePage(int NodeID)
        {
            M_Node nodeinfo = nodeBll.SelReturnModel(NodeID);
            string TemplateUrl = GetTemplateUrl(nodeinfo.IndexTemplate);
            if (string.IsNullOrEmpty(nodeinfo.IndexTemplate)) { B_Release.AddResult(nodeinfo.NodeName + "未指定栏目首页模板"); }
            else if (nodeinfo.ListPageHtmlEx == 3) { B_Release.AddResult(nodeinfo.NodeName + "--栏目节点首页系统设置为动态生成略过"); }
            else if (!File.Exists(TemplateUrl)) { B_Release.AddResult(nodeinfo.NodeName + "--栏目节点首页的模板[" + nodeinfo.IndexTemplate + "]不存在"); }
            else
            {
                string InfoExt = GetFileEx(nodeinfo.ListPageHtmlEx);
                // D:\Code\MVC\ZoomLa.WebSite\html\news\index.shtml
                string InfoFile = GetInfoFile(nodeinfo, "index");
                //-------------------------------------------------------------
                string Tempcontent = FileSystemObject.ReadFile(TemplateUrl);
                string ContentHtml = bll.CreateHtml(Tempcontent, 0, NodeID, "1");
                //判断包含多少个分页
                StringCollection resultList = new StringCollection();
                int Pagecount = GetPageCount(ContentHtml);//总页数                     
                #region 首页分页
                ContentHtml = ContentHtml.Replace("当前页面:<strong><font color=red>0</font></strong>/共", "当前页面:<strong><font color=red>1</font></strong>/共");
                ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>首页</[a,A]>", "首页", RegexOptions.RightToLeft);
                ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>上一页</[a,A]>", "上一页", RegexOptions.RightToLeft);
                if (Pagecount > 1)
                {
                    ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>下一页</[a,A]>", "<a href='index_2" + InfoExt + "'>下一页</a>", RegexOptions.RightToLeft);
                    ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>尾页</[a,A]>", "<a href='index_" + Pagecount + InfoExt + "'>尾页</a>", RegexOptions.RightToLeft);
                }
                else
                {
                    ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>下一页</[a,A]>", "下一页", RegexOptions.RightToLeft);
                    ContentHtml = Regex.Replace(ContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>尾页</[a,A]>", "尾页", RegexOptions.RightToLeft);
                }
                ReplaceHref(ref ContentHtml, Pagecount, InfoExt);

                FileSystemObject.WriteFile(InfoFile, ContentHtml);
                string indexUrl = function.PToV(InfoFile.Replace("//", "/"));
                B_Release.AddResult("栏目节点[" + nodeinfo.NodeName + ",共" + Pagecount + "页]<a href='" + indexUrl + "' target='_blank'>" + indexUrl + "</a>");
                #endregion
                #region 循环生成分页
                if (Pagecount > 1)//从第二页开始,第一页在上面已经生成
                {
                    int counts = 1;
                    for (int c = 1; c < Pagecount; c++)
                    {
                        counts += 1;
                        LabelCache.Clear();
                        string PageContentHtml = bll.CreateHtml(Tempcontent, counts, NodeID, "1");
                        PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>首页</[a,A]>", "<a href='index" + InfoExt + "'>首页</a>", RegexOptions.RightToLeft);
                        if (counts == 2)
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>上一页</[a,A]>", "<a href='index" + InfoExt + "'>上一页</a>", RegexOptions.RightToLeft);
                        }
                        else
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>上一页</[a,A]>", "<a href='index_" + (counts - 1).ToString() + InfoExt + "'>上一页</a>", RegexOptions.RightToLeft);
                        }
                        if (counts < Pagecount)
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>下一页</[a,A]>", "<a href='index_" + (counts + 1).ToString() + InfoExt + "'>下一页</a>", RegexOptions.RightToLeft);
                        }
                        else
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>下一页</[a,A]>", "下一页", RegexOptions.RightToLeft);
                        }
                        if (counts < Pagecount)
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>尾页</[a,A]>", "<a href='index_" + Pagecount + InfoExt + "'>尾页</a>", RegexOptions.RightToLeft);
                        }
                        else
                        {
                            PageContentHtml = Regex.Replace(PageContentHtml, "<[a,A] [h,H][r,R][e,E][f,F]='.*?'>尾页</[a,A]>", "尾页", RegexOptions.RightToLeft);
                        }
                        ReplaceHref(ref PageContentHtml, Pagecount, InfoExt);
                        //将.html replace to _count.html
                        string ppath = InfoFile.Replace(InfoExt, "_" + counts + InfoExt).Replace("//", "/");
                        string vpath = function.PToV(ppath);
                        FileSystemObject.WriteFile(ppath, PageContentHtml);
                        B_Release.AddResult("栏目节点[" + nodeinfo.NodeName + ",共" + Pagecount + "页][分页" + counts + "]<a href='" + vpath + "' target='_blank'>" + vpath + "</a>");
                    }
                }
                #endregion
            }
        }
        /// <summary>
        /// 创建指定内容ID的静态页面
        /// </summary>
        /// <param name="InfoID">内容信息ID</param>
        /// <param name="check">是检测节点或内容是否允许生成静态</param>
        public void CreateInfo(int InfoID)
        {
            B_Label labelBll = new B_Label();
            M_CommonData mdata = conBll.GetCommonData(InfoID);
            if (mdata == null || mdata.GeneralID < 1) { B_Release.AddResult("内容[" + InfoID + "]不存在:"); return; }
            int NodeID = mdata.NodeID, ModelID = mdata.ModelID;
            M_Node nodeinfo = nodeBll.SelReturnModel(NodeID);
            string TemplateDir = SiteConfig.SiteOption.TemplateDir + "/";
            string GeneratedDirectory = SiteConfig.SiteOption.GeneratedDirectory;

            //获取模板地址以及静态文件名
            if (!string.IsNullOrEmpty(mdata.Template))
            {
                TemplateDir = TemplateDir + mdata.Template;
            }
            else
            {
                if (nodeBll.IsExistTemplate(NodeID, ModelID))
                {
                    TemplateDir = TemplateDir + nodeBll.GetModelTemplate(NodeID, ModelID);
                }
                else
                {
                    TemplateDir = TemplateDir + bmodel.SelReturnModel(ModelID).ContentModule;
                }
            }
            if (string.IsNullOrEmpty(TemplateDir))
            {
                B_Release.AddResult("未指定内容页模板[" + nodeinfo.NodeName + "]:"); return;
            }
            TemplateDir = function.VToP(TemplateDir);
            if (!FileSystemObject.IsExist(TemplateDir, FsoMethod.File))
            {
                B_Release.AddResult("内容页生成略过[" + nodeinfo.NodeName + "]:内容页模板不存在"); return;
            }
            if (nodeinfo.ContentFileEx == 3)
            {
                B_Release.AddResult("内容页生成略过[" + nodeinfo.NodeName + "]:[" + mdata.Title + "]"); return;
            }
            //-----------------------------------检测End-----------------------------------//
            //根据模板创建分页HTML
            string ContentHtml = bll.CreateHtml(FileSystemObject.ReadFile(TemplateDir), 0, InfoID, "0");
            //设定内容页文件名
            int InfoFileRule = nodeinfo.ContentPageHtmlRule;
            string InfoFile = "";
            string fileex = GetFileEx(nodeinfo.ContentFileEx);
            // /html/news/
            string NodeDir = GetNodeDir(nodeinfo);
            NodeDir = "/" + GeneratedDirectory.TrimStart('/') + NodeDir;
            //内容页文件命名规则
            switch (InfoFileRule)
            {
                case 0:
                    InfoFile = NodeDir + mdata.CreateTime.ToString("yyyy/MM/dd") + "/" + InfoID;
                    break;
                case 1:
                    InfoFile = NodeDir + mdata.CreateTime.ToString("yyyy-MM") + "/" + InfoID;
                    break;
                case 2:
                    InfoFile = NodeDir + InfoID;
                    break;
                case 3:
                    InfoFile = NodeDir + mdata.CreateTime.ToString("yyyy-MM") + "/" + mdata.Title;
                    break;
            }
            //加上文件扩展名
            string htmlLink = (InfoFile + fileex).Replace("//", "/");
            string FileLink = htmlLink;

            /* --------------------判断是否分页并做处理------------------------------------------------*/
            string infoContent = ""; //需要进行处理的内容HTML
            string infotmp = "";//处理后的内容HTML
            string pagelabel = "";//匹配到的分页标签
            string pattern = @"{\#PageCode}([\s\S])*?{\/\#PageCode}";  //手动分页入此
            if (Regex.IsMatch(ContentHtml, pattern, RegexOptions.IgnoreCase))
            {
                #region 自定义分页
                infoContent = Regex.Match(ContentHtml, pattern, RegexOptions.IgnoreCase).Value;
                infotmp = infoContent;
                infoContent = infoContent.Replace("{#PageCode}", "").Replace("{/#PageCode}", "");

                //查找分页标签
                bool isPage = false;
                string pattern1 = @"{ZL\.Page([\s\S])*?\/}";
                if (Regex.IsMatch(ContentHtml, pattern1, RegexOptions.IgnoreCase))
                {
                    pagelabel = Regex.Match(ContentHtml, pattern1, RegexOptions.IgnoreCase).Value;
                    isPage = true;
                }
                if (isPage)
                {
                    if (string.IsNullOrEmpty(infoContent)) //没有设定要分页的字段内容
                    {
                        ContentHtml = ContentHtml.Replace(pagelabel, "");
                        ContentHtml = Gethtmlpageurl(ContentHtml);

                        FileLink = function.VToP(FileLink);
                        B_Release.AddResult("生成内容页", FileLink);
                        FileSystemObject.WriteFile(FileLink, ContentHtml);
                        conBll.UpdateCreate(InfoID, htmlLink);
                    }
                    else//手动分页的内容分页处理
                    {
                        //文件名
                        string fname = htmlLink.Remove(htmlLink.LastIndexOf("."));
                        //取分页标签处理结果 返回字符串数组 根据数组元素个数生成几页 
                        string ilbl = pagelabel.Replace("{ZL.Page ", "").Replace("/}", "").Replace("\" ", "\",").Replace(" ", "");
                        string lblContent = "";
                        int NumPerPage = 500;//默认500
                        IList<string> ContentArr = new List<string>();
                        if (string.IsNullOrEmpty(ilbl))
                        {
                            lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                            //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                            ContentArr = bll.GetContentPage(infoContent);
                        }
                        else
                        {
                            string[] paArr = ilbl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (paArr.Length == 0)
                            {
                                lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                ContentArr = bll.GetContentPage(infoContent);

                            }
                            else
                            {
                                string lblname = paArr[0].Split(new char[] { '=' })[1].Replace("\"", "");
                                if (paArr.Length > 1)
                                {
                                    NumPerPage = DataConverter.CLng(paArr[1].Split(new char[] { '=' })[1].Replace("\"", ""));
                                }
                                lblContent = labelBll.GetLabelXML(lblname).Content;
                                if (string.IsNullOrEmpty(lblContent))
                                {
                                    lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                }
                                //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                ContentArr = bll.GetContentPage(infoContent);

                            }
                        }
                        string Content1 = "";//当前分页的内容
                        string allContent = "";
                        for (int i = 1; i <= ContentArr.Count; i++)//手动分页输出至本地
                        {
                            Content1 = ContentHtml.Replace(infotmp, ContentArr[i - 1]);
                            Content1 = Content1.Replace(pagelabel, bll.GetPage(lblContent, fname, fileex, i, ContentArr.Count, NumPerPage));
                            if (i == 1)
                            {
                                FileLink = fname + fileex;
                            }
                            else
                            {
                                FileLink = fname + "_" + i.ToString() + fileex;
                            }
                            FileLink = function.VToP(FileLink);
                            Content1 = Gethtmlpageurl(Content1);
                            Content1 = Content1.Replace("{#Content}", "").Replace("{/#Content}", "").Replace("{#PageCode}", "").Replace("{/#PageCode}", "");
                            B_Release.AddResult("生成内容页", FileLink);
                            FileSystemObject.WriteFile(FileLink, Content1);
                            if (i == 1) { conBll.UpdateCreate(InfoID, htmlLink); }
                            allContent += ContentArr[i - 1];
                        }
                        //分页，显示全按,内容功能
                        Content1 = ContentHtml.Replace(infotmp, allContent);
                        Content1 = Content1.Replace(pagelabel, bll.GetPage(lblContent, fname, fileex, 0, ContentArr.Count, NumPerPage));
                        FileLink = function.VToP(fname + "_0" + fileex);
                        FileSystemObject.WriteFile(FileLink, Content1);//D:\Zoomla6x\ZoomLa.WebSite\html\ttxw\53_3.html:
                        ContentArr.Clear();
                    }
                }
                else  //没有分页标签
                {
                    //如果设定了分页内容字段 将该字段内容的分页标志清除
                    if (!string.IsNullOrEmpty(infoContent))
                    {
                        ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                    }
                    ContentHtml = Gethtmlpageurl(ContentHtml);
                    FileLink = function.VToP(htmlLink);
                    B_Release.AddResult("生成内容页", FileLink);
                    FileSystemObject.WriteFile(FileLink, ContentHtml);
                    conBll.UpdateCreate(InfoID, htmlLink);
                }
                #endregion
            }
            else
            {
                //string pattern = @"<$Content>([\s\S])*?</$Content>";  //查找要分页的内容
                #region 内容分页,已用的很少
                pattern = @"{#Content}([\s\S])*?{/#Content}";
                if (Regex.IsMatch(ContentHtml, pattern, RegexOptions.IgnoreCase))
                {
                    //自动分页,进此
                    infoContent = Regex.Match(ContentHtml, pattern, RegexOptions.IgnoreCase).Value;
                    infotmp = infoContent;
                    //infoContent = infoContent.Replace("<$Content>", "").Replace("</$Content>", "");
                    infoContent = infoContent.Replace("{#Content}", "").Replace("{/#Content}", "");

                    //查找分页标签
                    bool isPage = false;
                    string pattern1 = @"{ZL\.Page([\s\S])*?\/}";
                    if (Regex.IsMatch(ContentHtml, pattern1, RegexOptions.IgnoreCase))
                    {
                        pagelabel = Regex.Match(ContentHtml, pattern1, RegexOptions.IgnoreCase).Value;
                        isPage = true;
                    }
                    if (isPage)//带{ZL.Page/}进此
                    {
                        if (string.IsNullOrEmpty(infoContent)) //没有设定要分页的字段内容
                        {
                            ContentHtml = ContentHtml.Replace(pagelabel, "");
                            ContentHtml = Gethtmlpageurl(ContentHtml);

                            FileLink = function.VToP(FileLink);
                            B_Release.AddResult("生成内容页", FileLink);
                            FileSystemObject.WriteFile(FileLink, ContentHtml);
                            conBll.UpdateCreate(InfoID, htmlLink);
                        }//(自定义标签暂未入此)
                        else //进行内容分页处理,自动分页标签处理,内容页面的生成与创建html
                        {
                            //文件名
                            string file1 = htmlLink;
                            file1 = file1.Remove(file1.LastIndexOf("."));
                            //取分页标签处理结果 返回字符串数组 根据数组元素个数生成几页 
                            string ilbl = pagelabel.Replace("{ZL.Page ", "").Replace("/}", "").Replace(" ", ",");
                            string lblContent = "";
                            int NumPerPage = 500;
                            IList<string> ContentArr = new List<string>();

                            //填充ContentArr,ContentArr存解析完成后的内容
                            if (string.IsNullOrEmpty(ilbl))
                            {
                                lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                ContentArr = bll.GetContentPage(infoContent);
                            }
                            else
                            {
                                string[] paArr = ilbl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (paArr.Length == 0)
                                {
                                    lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                    ContentArr = bll.GetContentPage(infoContent);
                                }
                                else
                                {
                                    //对{ZL.Page/}进行处理
                                    string lblname = paArr[0].Split(new char[] { '=' })[1].Replace("\"", "");
                                    if (paArr.Length > 1)
                                    {
                                        NumPerPage = DataConverter.CLng(paArr[1].Split(new char[] { '=' })[1].Replace("\"", ""));
                                    }
                                    lblContent = labelBll.GetLabelXML(lblname).Content;
                                    if (string.IsNullOrEmpty(lblContent))
                                    {
                                        lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    }
                                    //ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                    ContentArr = bll.GetContentPage(infoContent);
                                }
                            }
                            string Content1 = "";
                            string allContent = "";
                            for (int i = 1; i <= ContentArr.Count; i++)//自动分页输出至本地
                            {
                                Content1 = ContentHtml.Replace(infotmp, ContentArr[i - 1]);
                                Content1 = ContentHtml.Replace(pagelabel, bll.GetPage(lblContent, file1, fileex, i, ContentArr.Count, NumPerPage));
                                if (i == 1)
                                {
                                    FileLink = file1 + fileex;
                                }
                                else
                                {
                                    FileLink = file1 + "_" + i.ToString() + fileex;
                                }
                                FileLink = function.VToP(FileLink);
                                Content1 = Gethtmlpageurl(Content1);

                                Content1 = Content1.Replace("{#Content}", "").Replace("{/#Content}", "");
                                B_Release.AddResult("生成内容页", FileLink);
                                FileSystemObject.WriteFile(FileLink, Content1);
                                if (i == 1)
                                {
                                    conBll.UpdateCreate(InfoID, htmlLink);
                                }
                                allContent += ContentArr[i - 1];
                            }
                            //分页，显示全部内容功能
                            Content1 = ContentHtml.Replace(infotmp, allContent);
                            Content1 = ContentHtml.Replace(pagelabel, bll.GetPage(lblContent, file1, fileex, 0, ContentArr.Count, NumPerPage));
                            FileLink = function.VToP(file1 + "_0" + fileex);
                            FileSystemObject.WriteFile(FileLink, Content1);//D:\Zoomla6x\ZoomLa.WebSite\html\ttxw\53_3.html:
                            ContentArr.Clear();
                        }//else end;
                    }
                }
                else  //没有分页标签
                {
                    //如果设定了分页内容字段 将该字段内容的分页标志清除
                    if (!string.IsNullOrEmpty(infoContent))
                    {
                        ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                    }
                    ContentHtml = Gethtmlpageurl(ContentHtml);
                    FileLink = function.VToP(htmlLink);
                    B_Release.AddResult("生成内容页", FileLink);
                    //替换掉分页标签占位符
                    Regex regexObj = new Regex(@"\{ZL\.Page([\s\S])*?/\}", RegexOptions.IgnoreCase);
                    ContentHtml = regexObj.Replace(ContentHtml, "");
                    FileSystemObject.WriteFile(FileLink, ContentHtml);
                    conBll.UpdateCreate(InfoID, htmlLink);
                }
                #endregion
            }
        }
        /// <summary>
        /// 创建列表页
        /// </summary>
        /// <param name="NodeID">节点ID</param>
        public void CreateList(int NodeID)
        {
            M_Node nodeinfo = nodeBll.SelReturnModel(NodeID);
            CreateList(nodeinfo);
        }
        public void CreateList(M_Node nodeinfo)
        {
            try
            {
                string TemplateUrl = GetTemplateUrl(nodeinfo.ListTemplateFile);
                if (nodeinfo.ListPageHtmlEx == 3) { B_Release.AddResult(nodeinfo.NodeName + "--列表页系统设置为动态生成略过"); }
                else if (string.IsNullOrEmpty(TemplateUrl)) { B_Release.AddResult(nodeinfo.NodeName + "--未指定列表页模板"); }
                else if (!File.Exists(TemplateUrl)) { B_Release.AddResult(nodeinfo.NodeName + "[" + nodeinfo.ListTemplateFile + "]--模板文件不存在"); }
                else
                {
                    string Tempcontent = FileSystemObject.ReadFile(TemplateUrl);
                    string ContentHtml = bll.CreateHtml(Tempcontent, 0, nodeinfo.NodeID, "0");
                    //判断包含多少个分页
                    StringCollection resultList = new StringCollection();

                    int Pagecount = GetPageCount(ContentHtml);
                    ContentHtml = Gethtmlpageurl(ContentHtml);
                    string InfoFile = GetInfoFile(nodeinfo, "listpage");
                    string FieldExtstr = GetFileEx(DataConverter.CLng(nodeinfo.ListPageHtmlEx));
                    //进行循环生成分页
                    if (Pagecount > 1)
                    {
                        int counts = 1;
                        for (int c = 1; c < Pagecount; c++)
                        {
                            counts += 1;
                            LabelCache.Clear();
                            string PageContentHtml = bll.CreateHtml(Tempcontent, counts, nodeinfo.NodeID, "1");
                            B_Release.AddResult("列表页[分页" + counts + "]" + InfoFile.Replace(FieldExtstr, "_" + counts.ToString() + FieldExtstr));
                            PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>首页</a>", "<a href='listpage" + FieldExtstr + "'>首页</a>", RegexOptions.IgnoreCase);
                            if (counts == 2)
                            {
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>上一页</a>", "<a href='listpage" + FieldExtstr + "'>上一页</a>", RegexOptions.IgnoreCase);
                            }
                            else
                            {
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>上一页</a>", "<a href='listpage_" + (counts - 1).ToString() + FieldExtstr + "'>上一页</a>", RegexOptions.IgnoreCase);
                            }

                            if (counts < Pagecount)
                            {
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>下一页</a>", "<a href='listpage_" + (counts + 1).ToString() + FieldExtstr + "'>下一页</a>", RegexOptions.IgnoreCase);
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>尾页</a>", "<a href='listpage_" + Pagecount + FieldExtstr + "'>尾页</a>", RegexOptions.IgnoreCase);
                            }
                            else
                            {
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>下一页</a>", "下一页", RegexOptions.IgnoreCase);
                                PageContentHtml = Regex.Replace(PageContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>尾页</a>", "尾页", RegexOptions.IgnoreCase);
                            }
                            ReplaceHref(ref PageContentHtml, Pagecount, FieldExtstr, "listpage");
                            FileSystemObject.WriteFile(InfoFile.Replace(FieldExtstr, "_" + counts.ToString() + FieldExtstr), PageContentHtml);
                            PageContentHtml = "";
                        }
                    }//end

                    //更新列表首页分页地址进行替换
                    ContentHtml = ContentHtml.Replace("当前页面:<strong><font color=red>0</font></strong>/共", "当前页面:<strong><font color=red>1</font></strong>/共");
                    ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>首页</a>", "首页", RegexOptions.IgnoreCase);
                    ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>上一页</a>", "上一页", RegexOptions.IgnoreCase);
                    if (Pagecount > 1)
                    {
                        ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>下一页</a>", "<a href='listpage_2" + FieldExtstr + "'>下一页</a>", RegexOptions.IgnoreCase);
                        ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>尾页</a>", "<a href='listpage_" + Pagecount + FieldExtstr + "'>尾页</a>", RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>下一页</a>", "下一页", RegexOptions.IgnoreCase);
                        ContentHtml = Regex.Replace(ContentHtml, "<a href='(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|!:,.;]*[-A-Z0-9+&@#/%=~_|]'>尾页</a>", "尾页", RegexOptions.IgnoreCase);
                    }
                    ReplaceHref(ref ContentHtml, Pagecount, FieldExtstr, "listpage");
                    FileSystemObject.WriteFile(InfoFile, ContentHtml);
                    B_Release.AddResult("生成列表页[" + nodeinfo.NodeName + "]", function.VToP(InfoFile));
                }
            }
            catch (Exception ex) { WriteLog("生成发布--CreateList", ex.Message); }
        }
        /// <summary>
        /// 生成静态转换页面地址
        /// </summary>
        public string Gethtmlpageurl(string contents)
        {
            try
            {
                Regex regexObj = new Regex("[\"|\'][-A-Z0-9+&@#/%?=~_|!:,.;]*[A-Z0-9+&@#/%=~_|][\"\']", RegexOptions.IgnoreCase);
                Match matchResults = regexObj.Match(contents);
                string gd = SiteConfig.SiteOption.GeneratedDirectory;
                if (!gd.StartsWith("/"))
                {
                    gd = "/" + gd;
                }
                string GeneratedDirectory = gd;//生成路径
                int maxpagenum = 0;
                int thisnodeid = 0;

                while (matchResults.Success)
                {
                    if (matchResults.ToString().IndexOf(".aspx") > -1)
                    {
                        if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                        {
                            string stringlist = matchResults.ToString();
                            string slist = stringlist;
                            contents = GetPagenurl(ref contents, matchResults, GeneratedDirectory, ref maxpagenum, ref thisnodeid, ref stringlist);
                        }
                    }
                    matchResults = matchResults.NextMatch();
                }

                while (matchResults.Success)
                {

                    if (matchResults.ToString().IndexOf(".aspx") > -1)
                    {
                        if (matchResults.ToString().IndexOf("Content.aspx") > -1)
                        {
                            string stringlist = matchResults.ToString();
                            int infoid = GetInfoID(stringlist, "itemid=");
                            B_Content cll = new B_Content();
                            M_CommonData CommonData = cll.GetCommonData(infoid);
                            int nodeid = CommonData.NodeID;
                            M_Node nodeinfolist = nodeBll.SelReturnModel(nodeid);
                            string HtmlLink = CommonData.HtmlLink;

                            if (!string.IsNullOrEmpty(HtmlLink))
                            {
                                HtmlLink = HtmlLink.Replace("//", "/");
                            }

                            if (!string.IsNullOrEmpty(HtmlLink))
                            {
                                if (nodeinfolist.HtmlPosition == 0)
                                {
                                    stringlist = "../.." + HtmlLink;// stringlist.Replace("Content.aspx", "Content.html");
                                }
                                else
                                {
                                    stringlist = "../.." + GeneratedDirectory + HtmlLink;// stringlist.Replace("Content.aspx", "Content.html");
                                }

                                if (FileSystemObject.IsExist(function.VToP(stringlist), FsoMethod.File))
                                {
                                    contents = contents.Replace(matchResults.ToString(), stringlist);
                                }
                            }
                        }

                        if (matchResults.ToString().IndexOf("http://") > -1)
                        {
                            if (matchResults.ToString().IndexOf(SiteConfig.SiteInfo.SiteUrl) > -1)
                            {
                                #region 处理分页
                                if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                                {
                                    string stringlist = matchResults.ToString();
                                    string slist = stringlist;

                                    if (stringlist.IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string[] stringlistarr = stringlist.Split(new string[] { "&p=" }, StringSplitOptions.None);
                                        stringlist = stringlistarr[0];
                                    }
                                    int infoid = GetInfoID(stringlist, "nodeid=");
                                    M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                                    string NodeUrl = nodeinfo.NodeUrl;
                                    int pagenum = 0;

                                    if (matchResults.ToString().IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string pageurl = matchResults.ToString();
                                        pageurl = pageurl.Replace("'", "");
                                        string[] pageurlarr = pageurl.Split(new string[] { "&p=" }, StringSplitOptions.None);

                                        if (pageurlarr.Length > 0)
                                        {
                                            if (pageurlarr[1].IndexOf("&") > -1)
                                            {
                                                string[] pageurlarrarr = pageurlarr[1].Split(new string[] { "&" }, StringSplitOptions.None);
                                                pagenum = DataConverter.CLng(pageurlarrarr[0]);
                                            }
                                            else
                                            {
                                                pagenum = DataConverter.CLng(pageurlarr[1]);
                                            }
                                        }
                                        if (pagenum > maxpagenum) { maxpagenum = pagenum; }
                                        string filename = "_" + infoid.ToString() + "_" + pagenum.ToString();
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl.IndexOf(".") > -1)
                                            {
                                                string[] nodearr = NodeUrl.Split(new string[] { "." }, StringSplitOptions.None);
                                                NodeUrl = nodearr[0].ToString() + filename + "." + nodearr[1].ToString();
                                            }
                                        }

                                        string aspxpage = matchResults.ToString();
                                        aspxpage = aspxpage.Replace("'", "");

                                        aspxpage = "/" + aspxpage;//动态地址
                                        string htmlpage = "/" + GeneratedDirectory + NodeUrl;//静态地址
                                        contents = contents.Replace(matchResults.ToString(), htmlpage);
                                    }


                                    if (!string.IsNullOrEmpty(nodeinfo.IndexTemplate))
                                    {
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl != "")
                                            {
                                                string tempNodeurl = "../../" + GeneratedDirectory + NodeUrl;
                                                if (nodeinfo.HtmlPosition == 0)
                                                {
                                                    tempNodeurl = "../../" + NodeUrl;
                                                }
                                                if (FileSystemObject.IsExist(function.VToP(tempNodeurl), FsoMethod.File))
                                                {
                                                    contents = contents.Replace(matchResults.ToString(), "/" + GeneratedDirectory + NodeUrl);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else if (matchResults.ToString().IndexOf("http://127.0.0.1") > -1)
                            {
                                #region 处理分页
                                if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                                {
                                    string stringlist = matchResults.ToString();
                                    string slist = stringlist;

                                    if (stringlist.IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string[] stringlistarr = stringlist.Split(new string[] { "&p=" }, StringSplitOptions.None);
                                        stringlist = stringlistarr[0];
                                    }
                                    int infoid = GetInfoID(stringlist, "nodeid=");
                                    M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                                    string NodeUrl = nodeinfo.NodeUrl;
                                    int pagenum = 0;

                                    if (matchResults.ToString().IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string pageurl = matchResults.ToString();
                                        pageurl = pageurl.Replace("'", "");
                                        string[] pageurlarr = pageurl.Split(new string[] { "&p=" }, StringSplitOptions.None);

                                        if (pageurlarr.Length > 0)
                                        {
                                            if (pageurlarr[1].IndexOf("&") > -1)
                                            {
                                                string[] pageurlarrarr = pageurlarr[1].Split(new string[] { "&" }, StringSplitOptions.None);
                                                pagenum = DataConverter.CLng(pageurlarrarr[0]);
                                            }
                                            else
                                            {
                                                pagenum = DataConverter.CLng(pageurlarr[1]);
                                            }
                                        }
                                        if (pagenum > maxpagenum) { maxpagenum = pagenum; }
                                        string filename = "_" + infoid.ToString() + "_" + pagenum.ToString();
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl.IndexOf(".") > -1)
                                            {
                                                string[] nodearr = NodeUrl.Split(new string[] { "." }, StringSplitOptions.None);
                                                NodeUrl = nodearr[0].ToString() + filename + "." + nodearr[1].ToString();
                                            }
                                        }

                                        string aspxpage = matchResults.ToString();
                                        aspxpage = aspxpage.Replace("'", "");

                                        aspxpage = "/" + aspxpage;//动态地址
                                        string htmlpage = "/" + GeneratedDirectory + NodeUrl;//静态地址
                                        contents = contents.Replace(matchResults.ToString(), htmlpage);
                                    }


                                    if (!string.IsNullOrEmpty(nodeinfo.IndexTemplate))
                                    {
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl != "")
                                            {
                                                string tempNodeurl = "../../" + GeneratedDirectory + NodeUrl;
                                                if (nodeinfo.HtmlPosition == 0)
                                                {
                                                    tempNodeurl = "../../" + NodeUrl;
                                                }
                                                if (FileSystemObject.IsExist(function.VToP(tempNodeurl), FsoMethod.File))
                                                {
                                                    contents = contents.Replace(matchResults.ToString(), "/" + GeneratedDirectory + NodeUrl);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            else if (matchResults.ToString().IndexOf("http://localhost") > -1)
                            {
                                #region 处理分页
                                if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                                {
                                    string stringlist = matchResults.ToString();
                                    string slist = stringlist;

                                    if (stringlist.IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string[] stringlistarr = stringlist.Split(new string[] { "&p=" }, StringSplitOptions.None);
                                        stringlist = stringlistarr[0];
                                    }
                                    int infoid = GetInfoID(stringlist, "nodeid=");
                                    M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                                    string NodeUrl = nodeinfo.NodeUrl;
                                    int pagenum = 0;

                                    if (matchResults.ToString().IndexOf("&p=") > -1)//处理分页内容
                                    {
                                        string pageurl = matchResults.ToString();
                                        pageurl = pageurl.Replace("'", "");
                                        string[] pageurlarr = pageurl.Split(new string[] { "&p=" }, StringSplitOptions.None);

                                        if (pageurlarr.Length > 0)
                                        {
                                            if (pageurlarr[1].IndexOf("&") > -1)
                                            {
                                                string[] pageurlarrarr = pageurlarr[1].Split(new string[] { "&" }, StringSplitOptions.None);
                                                pagenum = DataConverter.CLng(pageurlarrarr[0]);
                                            }
                                            else
                                            {
                                                pagenum = DataConverter.CLng(pageurlarr[1]);
                                            }
                                        }
                                        if (pagenum > maxpagenum) { maxpagenum = pagenum; }
                                        string filename = "_" + infoid.ToString() + "_" + pagenum.ToString();
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl.IndexOf(".") > -1)
                                            {
                                                string[] nodearr = NodeUrl.Split(new string[] { "." }, StringSplitOptions.None);
                                                NodeUrl = nodearr[0].ToString() + filename + "." + nodearr[1].ToString();
                                            }
                                        }

                                        string aspxpage = matchResults.ToString();
                                        aspxpage = aspxpage.Replace("'", "");

                                        aspxpage = "/" + aspxpage;//动态地址
                                        string htmlpage = "/" + GeneratedDirectory + NodeUrl;//静态地址
                                        contents = contents.Replace(matchResults.ToString(), htmlpage);
                                    }


                                    if (!string.IsNullOrEmpty(nodeinfo.IndexTemplate))
                                    {
                                        if (!string.IsNullOrEmpty(NodeUrl))
                                        {
                                            if (NodeUrl != "")
                                            {
                                                string tempNodeurl = "../../" + GeneratedDirectory + NodeUrl;
                                                if (nodeinfo.HtmlPosition == 0)
                                                {
                                                    tempNodeurl = "../../" + NodeUrl;
                                                }
                                                if (FileSystemObject.IsExist(function.VToP(tempNodeurl), FsoMethod.File))
                                                {
                                                    contents = contents.Replace(matchResults.ToString(), "/" + GeneratedDirectory + NodeUrl);
                                                }
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else
                        {
                            #region 处理分页
                            if (matchResults.ToString().IndexOf("ColumnList.aspx") > -1)
                            {
                                string stringlist = matchResults.ToString();
                                string slist = stringlist;

                                if (stringlist.IndexOf("&p=") > -1)//处理分页内容
                                {
                                    string[] stringlistarr = stringlist.Split(new string[] { "&p=" }, StringSplitOptions.None);
                                    stringlist = stringlistarr[0];
                                }
                                int infoid = GetInfoID(stringlist, "nodeid=");
                                M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                                string NodeUrl = nodeinfo.NodeUrl;
                                int pagenum = 0;

                                if (matchResults.ToString().IndexOf("&p=") > -1)//处理分页内容
                                {
                                    string pageurl = matchResults.ToString();
                                    pageurl = pageurl.Replace("'", "");
                                    string[] pageurlarr = pageurl.Split(new string[] { "&p=" }, StringSplitOptions.None);

                                    if (pageurlarr.Length > 0)
                                    {
                                        if (pageurlarr[1].IndexOf("&") > -1)
                                        {
                                            string[] pageurlarrarr = pageurlarr[1].Split(new string[] { "&" }, StringSplitOptions.None);
                                            pagenum = DataConverter.CLng(pageurlarrarr[0]);
                                        }
                                        else
                                        {
                                            pagenum = DataConverter.CLng(pageurlarr[1]);
                                        }
                                    }
                                    if (pagenum > maxpagenum) { maxpagenum = pagenum; }
                                    string filename = "_" + infoid.ToString() + "_" + pagenum.ToString();
                                    if (!string.IsNullOrEmpty(NodeUrl))
                                    {
                                        if (NodeUrl.IndexOf(".") > -1)
                                        {
                                            string[] nodearr = NodeUrl.Split(new string[] { "." }, StringSplitOptions.None);
                                            NodeUrl = nodearr[0].ToString() + filename + "." + nodearr[1].ToString();
                                        }
                                    }

                                    string aspxpage = matchResults.ToString();
                                    aspxpage = aspxpage.Replace("'", "");

                                    aspxpage = "/" + aspxpage;//动态地址
                                    string htmlpage = "/" + GeneratedDirectory + NodeUrl;//静态地址
                                    contents = contents.Replace(matchResults.ToString(), htmlpage);
                                }


                                if (!string.IsNullOrEmpty(nodeinfo.IndexTemplate))
                                {
                                    if (!string.IsNullOrEmpty(NodeUrl))
                                    {
                                        if (NodeUrl != "")
                                        {
                                            string tempNodeurl = "../../" + GeneratedDirectory + NodeUrl;
                                            if (nodeinfo.HtmlPosition == 0)
                                            {
                                                tempNodeurl = "../../" + NodeUrl;
                                            }
                                            if (FileSystemObject.IsExist(function.VToP(tempNodeurl), FsoMethod.File))
                                            {
                                                contents = contents.Replace(matchResults.ToString(), "/" + GeneratedDirectory + NodeUrl);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        if (matchResults.ToString().IndexOf("NodePage.aspx") > -1)
                        {
                            string stringlist = matchResults.ToString();
                            int infoid = GetInfoID(stringlist, "nodeid=");
                            M_Node nodeinfo = nodeBll.SelReturnModel(infoid);
                            string NodeListUrl = nodeinfo.NodeListUrl;
                            if (!string.IsNullOrEmpty(nodeinfo.ListTemplateFile))
                            {
                                if (!string.IsNullOrEmpty(NodeListUrl))
                                {
                                    if (NodeListUrl != "")
                                    {
                                        string nodelisturl = "../.." + GeneratedDirectory + NodeListUrl;
                                        if (nodeinfo.HtmlPosition == 0)
                                        {
                                            nodelisturl = "../.." + NodeListUrl;
                                        }

                                        if (FileSystemObject.IsExist(function.VToP(nodelisturl), FsoMethod.File))
                                        {
                                            contents = contents.Replace(matchResults.ToString(), GeneratedDirectory + NodeListUrl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    matchResults = matchResults.NextMatch();
                }
                maxpagenum = 0;
                thisnodeid = 0;
            }
            catch (Exception) { }
            return contents;
        }
        /// <summary>
        /// 获得资料ID
        /// </summary>
        public int GetInfoID(string content, string itemlist)
        {
            int infosid = 0;
            if (content.IndexOf("?") > -1)
            {
                string[] contentarr = content.Split(new string[] { "?" }, StringSplitOptions.RemoveEmptyEntries);
                for (int c = 0; c < contentarr.Length; c++)
                {
                    string wenhao = contentarr[c].ToString().Replace("\"", ""); ;

                    if (wenhao.IndexOf("&") > -1)
                    {
                        string[] wenhaarr = wenhao.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int d = 0; d < wenhaarr.Length; d++)
                        {

                            if (wenhaarr[d].ToLower().IndexOf(itemlist) > -1)
                            {
                                infosid = DataConverter.CLng(wenhaarr[d].Replace(itemlist, ""));
                            }
                        }
                    }
                    else
                    {
                        if (wenhao.ToLower().IndexOf(itemlist) > -1)
                        {
                            infosid = DataConverter.CLng(wenhao.ToLower().Replace(itemlist, ""));
                        }
                    }
                }
            }
            return infosid;
        }

        #region  生成发布
        public int m_CreateCount;
        /// <summary>
        /// 发布选定的单页
        /// </summary>
        public void CreateSingleByID(string nodeids)
        {
            DataTable dt = nodeBll.GetCreateSingleByID(nodeids);
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                CreateNodePage(DataConverter.CLng(dr["NodeID"]));
            }
        }
        /// <summary>
        /// 发布所有单页
        /// </summary>
        public void CreateSingle()
        {
            DataTable dt = nodeBll.SelByType("2");
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                CreateNodePage(DataConverter.CLng(dr["NodeID"]));
            }
        }
        /// <summary>
        /// 发布选定的专题页
        /// </summary>
        /// <param name="specids">专题节点IDS</param>
        public void CreateSpecial(string specids)
        {
            try
            {
                string[] specArr = specids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                //生成专题内容
                for (int i = 0; i < specArr.Length; i++)
                {
                    DataTable dt = conBll.SelBySpecialID(specArr[i]);
                    m_CreateCount = dt.Rows.Count;
                    foreach (DataRow dr in dt.Rows)
                    {
                        CreateInfo(DataConverter.CLng(dr["GeneralID"]));
                    }
                }
                //生成专题列表页
                {
                    B_Spec spBll = new B_Spec();
                    foreach (string spec in specArr)
                    {
                        M_Spec spMod = spBll.SelReturnModel(DataConvert.CLng(spec));
                        CreateList(spMod.ToNode());
                    }
                }
            }
            catch (Exception ex) { B_Release.AddResult("专题--生成异常:" + ex.Message); }
        }
        /// <summary>
        /// 发布所有栏目
        /// </summary>
        public void CreateColumnAll()
        {
            DataTable dt = nodeBll.SelByType("1");
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                try
                {
                    CreateNodePage(DataConverter.CLng(dr["NodeID"]));
                    CreateList(DataConverter.CLng(dr["NodeID"]));
                }
                catch (Exception ex) { WriteLog("CreateColumnAll", ex.Message); }
            }
        }
        /// <summary>
        /// 发布选定的栏目页
        /// </summary>
        /// <param name="InfoId"></param>
        public void CreateColumnByID(string InfoId)
        {
            DataTable dt = nodeBll.SelByIDS(InfoId, "1,2");
            if (dt != null && dt.Rows.Count > 0)
            {
                m_CreateCount = dt.Rows.Count;
                try
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        CreateNodePage(Convert.ToInt32(dr["NodeID"]));
                        CreateList(Convert.ToInt32(dr["NodeID"]));
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("栏目生成", ex.Message);
                }
            }
        }
        /// <summary>
        /// 发布选定的多个内容页
        /// </summary>
        public void createann(string InfoId)
        {
            if (SafeSC.CheckIDS(InfoId))
            {
                DataTable dt = conBll.SelByIDS(InfoId);
                foreach (DataRow dr in dt.Rows)
                {
                    CreateInfo(DataConverter.CLng(dr["GeneralID"]));
                }
            }
        }
        /// <summary>
        /// 发布选定的内容页
        /// </summary>
        /// <param name="InfoId"></param>
        public void CreateContentColumn(int InfoId)
        {
            M_CommonData dt = conBll.GetCommonData(InfoId);
            m_CreateCount = 1;
            CreateInfo(DataConverter.CLng(dt.GeneralID));
        }
        /// <summary>
        /// 发布选定的栏目的内容页
        /// </summary>
        /// <param name="InfoId"></param>
        public void CreateInfoColumn(string InfoId)
        {
            //获取指定节点下的所有内容
            DataTable dt = conBll.GetCreateNodeList(InfoId);
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                CreateInfo(Convert.ToInt32(dr["GeneralID"]));
            }
        }
        /// <summary>
        /// 按日期发布内容页
        /// </summary>
        /// <param name="modelId"></param>
        public void CreateInfoDate(string InfoId)
        {
            DateTime ID1 = DataConverter.CDate(InfoId.Split(new char[] { ',' })[0]);
            DateTime ID2 = DataConverter.CDate(InfoId.Split(new char[] { ',' })[1]);
            DataTable dt = conBll.GetCreateDateList(ID1, ID2);
            m_CreateCount = dt.Rows.Count;
            int i = 1;
            foreach (DataRow dr in dt.Rows)
            {
                //---Coffee
                CreateInfo(DataConverter.CLng(dr["GeneralID"]));
                i++;
            }
            dt = dt.DefaultView.ToTable(true, "NodeID");//生成栏目列表
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                CreateNodePage(Convert.ToInt32(dt.Rows[j]["NodeID"]));
            }
        }
        /// <summary>
        /// 按日期发布内容页
        /// </summary>
        /// <param name="modelId"></param>
        public void CreateInfoDate(string InfoId, int NodeID)
        {
            DataTable newDT = new DataTable();
            newDT.Columns.Add(new DataColumn("NodeID", typeof(int)));
            newDT.Columns.Add(new DataColumn("NodeName", typeof(string)));
            DataRow dr = newDT.NewRow();
            dr[0] = NodeID;
            dr[1] = "首页";
            newDT.Rows.Add(dr);
            nodeBll.GetColumnList(NodeID, newDT);
            string str = "";
            for (int i = 0; i < newDT.Rows.Count; i++)
            {
                str += newDT.Rows[i]["NodeID"].ToString() + ",";
            }
            if (str.EndsWith(","))
            {
                str = str.Substring(0, str.Length - 1);
            }

            DateTime ID1 = DataConverter.CDate(InfoId.Split(new char[] { ',' })[0]);
            DateTime ID2 = DataConverter.CDate(InfoId.Split(new char[] { ',' })[1]);
            DataTable dt = conBll.GetCreateDateList(ID1, ID2);
            DataRow[] drarr = dt.Select(" NodeID in (" + str + ")");

            m_CreateCount = drarr.Length;
            foreach (DataRow tdr in drarr)
            {
                CreateInfo(DataConverter.CLng(tdr["GeneralID"]));
            }
        }
        /// <summary>
        /// 发布最新个数的内容页
        /// </summary>
        /// <param name="InfoId"></param>
        public void CreateLastInfoRecord(string InfoId)
        {
            DataTable dt = conBll.GetCreateCountList(DataConverter.CLng(InfoId));
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                CreateInfo(DataConverter.CLng(dr["GeneralID"]));
            }
        }
        /// <summary>
        /// 发布所有内容页
        /// </summary>
        public void CreateInfo()
        {
            DataTable dt = conBll.GetCreateAllList();
            m_CreateCount = dt.Rows.Count;
            foreach (DataRow dr in dt.Rows)
            {
                CreateInfo(DataConverter.CLng(dr["GeneralID"]));
            }
        }
        /// <summary>
        /// 发布指定节点的所有内容页
        /// </summary>
        public void CreateInfoByNodeID(int NodeID)
        {
            DataTable newDT = new DataTable();
            newDT.Columns.Add(new DataColumn("NodeID", typeof(int)));
            newDT.Columns.Add(new DataColumn("NodeName", typeof(string)));
            DataRow dr = newDT.NewRow();
            dr[0] = NodeID;
            dr[1] = "首页";
            newDT.Rows.Add(dr);
            nodeBll.GetColumnList(NodeID, newDT);
            string str = "";
            for (int i = 0; i < newDT.Rows.Count; i++)
            {
                str += newDT.Rows[i]["NodeID"].ToString() + ",";
            }
            if (str.EndsWith(","))
            {
                str = str.Substring(0, str.Length - 1);
            }
            CreateInfoColumn(str);
        }
        /// <summary>
        /// 发布主页
        /// </summary>
        public void CreatePageIndex()
        {
            m_CreateCount++;
            string IndexDir = "/" + SiteConfig.SiteOption.IndexTemplate.TrimStart('/');
            IndexDir = function.VToP(SiteConfig.SiteOption.TemplateDir + IndexDir);
            if (DataConverter.CLng(SiteConfig.SiteOption.IndexEx) < 3)
            {
                string indexex = GetFileEx(DataConverter.CLng(SiteConfig.SiteOption.IndexEx));
                if (!string.IsNullOrEmpty(IndexDir))
                {
                    CreateIndex(IndexDir, indexex, "/");
                }
                else
                {
                    B_Release.AddResult("未指定主页模板");
                }
            }
            else
            {
                B_Release.AddResult("主页--设置为动态生成略过");
            }
        }
        #endregion
        public void WriteLog(string type, string msg)
        {
            ZLLog.L(Model.ZLEnum.Log.labelex, type + ":" + msg);
        }
    }
}