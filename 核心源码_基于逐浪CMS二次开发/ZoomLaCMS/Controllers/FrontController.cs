using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.BLL.SYS;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;
using ZoomLaCMS.Models;

namespace ZoomLaCMS.Controllers
{
    public class FrontController : Ctrl_Front
    {
        B_Content conBll = new B_Content();
        B_Node nodeBll = new B_Node();
        B_Model modBll = new B_Model();
        B_Product proBll = new B_Product();
        M_Node nodeMod = new M_Node();
        public IActionResult ErrToClient(string msg)
        {
            return WriteErr(msg);
        }
        public IActionResult Index() { return RedirectPermanent("Default"); }
        //首页
        public IActionResult Default()
        {
            //M_UserInfo mu = buser.SelReturnModel(1);
            //buser.SetLoginState(mu);

            //M_AdminInfo adminMod = B_Admin.GetAdminByAdminId(1);
            //B_Admin.SetLoginState(HttpContext, adminMod);

            ////return Redirect("/Plat/Blog/Default");
            //// return Redirect("/Admin/Template/LabelSql?LabelName=" + HttpUtility.UrlEncode("通用网站地图_基础子类"));
            //return Redirect("/Admin/Default");


            if (SiteConfig.SiteInfo.IsInstalled == false) { return RedirectToAction("Index", "Install"); }
            //--------------
            string vpath = SiteConfig.SiteOption.TemplateDir + "/" + SiteConfig.SiteOption.IndexTemplate.TrimStart('/');
            string TemplateDir = function.VToP(vpath);
            string fileex = B_Node.GetFileEx(DataConverter.CLng(SiteConfig.SiteOption.IndexEx));
            if (FileSystemObject.IsExist(function.VToP("/index" + fileex), FsoMethod.File)) { Response.Redirect("index" + fileex); }
            if (!FileSystemObject.IsExist(TemplateDir, FsoMethod.File))
            {
                return WriteErr("[产生错误的可能原因：(" + vpath + ")不存在或未开放!]");
            }
            else
            {
                string readfile = FileSystemObject.ReadFile(TemplateDir);
                string IndexHtml = bll.CreateHtml(readfile);
                return HtmlToClient(IndexHtml);
            }
        }
        #region 内容
        public IActionResult Content()
        {
            M_CommonData ItemInfo = null;
            //M_UserInfo mu = buser.GetLogin();
            
            string title = GetParam("ID");
            if (ItemID > 0)
            {
                ItemInfo = conBll.GetCommonData(ItemID);
                nodeMod = nodeBll.SelReturnModel(ItemInfo.NodeID);
                if (nodeMod == null || nodeMod.IsNull) { return ErrToClient("[产生错误的可能原因：(" + ItemInfo.NodeID + ")节点不存在!]"); }
                if (nodeMod.ItemOpenTypeTrue.Equals("cn")) { return ErrToClient("[产生错误的可能原因：内容信息不存在或未开放!调用方法：/Item/标题]"); }
            }
            else if (!string.IsNullOrEmpty(title))
            {
                ItemInfo = conBll.SelModelByTitle(title);
                nodeMod = nodeBll.SelReturnModel(ItemInfo.NodeID);
                if (nodeMod == null || nodeMod.IsNull) { return ErrToClient("[产生错误的可能原因：(" + ItemInfo.NodeID + ")节点不存在!]"); }
                if (!nodeMod.ItemOpenTypeTrue.Equals("cn")) { return ErrToClient("[产生错误的可能原因：内容信息不存在或未开放!调用方法：/Item/内容GID]"); }
            }
            else { return ErrToClient("[产生错误的可能原因：内容信息不存在或未开放!调用方法：/Item/内容GID]"); }
            if (ItemInfo == null || ItemInfo.IsNull) { return ErrToClient("[产生错误的可能原因：[" + ItemID + "]内容信息不存在或未开放!]"); }
            //访问权限
            if (!string.IsNullOrEmpty(ItemInfo.UGroupAuth) && ("," + ItemInfo.UGroupAuth + ",").IndexOf("," + mu.GroupID + ",") < 0)
            {
                return WriteErr("你无权访问该内容");
            }
            M_ModelInfo modelinfo = modBll.SelReturnModel(ItemInfo.ModelID);
            //-----------------------------------------------------
            if (modelinfo == null || modelinfo.IsNull) { return ErrToClient("[产生错误的可能原因：(" + ItemInfo.ModelID + ")模型不存在!]"); }
            if (ItemInfo.Status == (int)ZLEnum.ConStatus.Recycle) { return ErrToClient("[当前信息已删除，您无法浏览!]"); }
            else if (ItemInfo.Status == (int)ZLEnum.ConStatus.UnAudit) { return ErrToClient("[当前信息待审核状态，您无法浏览!]"); }
            else if (ItemInfo.Status != (int)ZLEnum.ConStatus.Audited) { return ErrToClient("[当前信息未通过审核，您无法浏览!]"); }


            //-------------------------End;
            //自定义模板>节点模板
            string TemplateDir = ItemInfo.Template;
            if (string.IsNullOrEmpty(TemplateDir))
            {
                TemplateDir = nodeBll.GetModelTemplate(ItemInfo.NodeID, ItemInfo.ModelID);
            }
            if (string.IsNullOrEmpty(TemplateDir))
            {
                return WriteErr("该内容所属模型未指定模板");
            }
            try
            {
                return HtmlToClient(Content_TemplateToHtml(TemplateDir));
            }
            catch(Exception) { return WriteErr("模板["+ TemplateDir + "]文件不存在"); }
        }
        private string Content_TemplateToHtml(string TemplateDir)
        {
            string Templatestrstr = FileSystemObject.ReadFile(function.VToP(SiteConfig.SiteOption.TemplateDir + "/" + TemplateDir));
            string ContentHtml = bll.CreateHtml(Templatestrstr, CPage, ItemID, "0");//Templatestrstr:模板页面字符串,页码,该文章ID
            /* --------------------判断是否分页 并做处理------------------------------------------------*/
            if (!string.IsNullOrEmpty(ContentHtml))
            {
                string infoContent = ""; //进行处理的内容字段
                string pagelabel = "";
                string infotmp = "";
                #region 分页符分页
                string pattern = @"{\#PageCode}([\s\S])*?{\/\#PageCode}";  //查找要分页的内容
                if (Regex.IsMatch(ContentHtml, pattern, RegexOptions.IgnoreCase))
                {
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
                        }
                        else   //进行内容分页处理
                        {
                            //文件名
                            string file1 = "Content?ID=" + ItemID.ToString();
                            //取分页标签处理结果 返回字符串数组 根据数组元素个数生成几页 
                            string ilbl = pagelabel.Replace("{ZL.Page ", "").Replace("/}", "").Replace(" ", ",");
                            string lblContent = "";
                            IList<string> ContentArr = new List<string>();

                            if (string.IsNullOrEmpty(ilbl))
                            {
                                lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                ContentArr = bll.GetContentPage(infoContent);
                            }
                            else
                            {
                                string[] paArr = ilbl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (paArr.Length == 0)
                                {
                                    lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    ContentArr = bll.GetContentPage(infoContent);
                                }
                                else
                                {
                                    string lblname = paArr[0].Split(new char[] { '=' })[1].Replace("\"", "");
                                    B_Label blbl = new B_Label();
                                    lblContent = blbl.GetLabelXML(lblname).Content;
                                    if (string.IsNullOrEmpty(lblContent))
                                    {
                                        lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    }
                                    ContentArr = bll.GetContentPage(infoContent);
                                }
                            }
                            if (ContentArr.Count > 0) //存在分页数据
                            {
                                string curCPage = GetParam("cpage");
                                bool isAll = !(string.IsNullOrEmpty(curCPage)) && curCPage.Equals("0");
                                if (isAll)//必须明确传值,才显示全部
                                {
                                    ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                                    ContentHtml = ContentHtml.Replace("[PageCode/]", "");
                                }
                                else
                                {
                                    int _cpage = PageHelper.GetCPage(CPage, 1, ContentArr.Count) - 1;
                                    ContentHtml = ContentHtml.Replace(infotmp, ContentArr[_cpage]);
                                    ContentHtml = ContentHtml.Replace("{#Content}", "").Replace("{/#Content}", "");
                                }
                                ContentHtml = ContentHtml.Replace(pagelabel, bll.GetPage(lblContent, ItemID, CPage, ContentArr.Count, ContentArr.Count));//输出分页
                            }
                            else
                            {
                                ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                                ContentHtml = ContentHtml.Replace(pagelabel, "");
                            }
                        }
                    }
                    else  //没有分页标签
                    {
                        //如果设定了分页内容字段 将该字段内容的分页标志清除
                        if (!string.IsNullOrEmpty(infoContent))
                            ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                    }
                }
                #endregion
                #region  查找要分页的内容
                pattern = @"{\#Content}([\s\S])*?{\/\#Content}";
                if (Regex.IsMatch(ContentHtml, pattern, RegexOptions.IgnoreCase))
                {
                    infoContent = Regex.Match(ContentHtml, pattern, RegexOptions.IgnoreCase).Value;
                    infotmp = infoContent;
                    infoContent = infoContent.Replace("{#Content}", "").Replace("{/#Content}", "");
                    //查找分页标签
                    bool isPage = false;
                    string pattern1 = @"{ZL\.Page([\s\S])*?\/}";
                    if (Regex.IsMatch(ContentHtml, pattern1, RegexOptions.IgnoreCase))
                    {
                        pagelabel = Regex.Match(ContentHtml, pattern1, RegexOptions.IgnoreCase).Value;
                        isPage = true;
                    }
                    if (isPage)//包含分页
                    {
                        if (string.IsNullOrEmpty(infoContent)) //没有设定要分页的字段内容
                        {
                            ContentHtml = ContentHtml.Replace(pagelabel, "");
                        }
                        else   //进行内容分页处理
                        {
                            //文件名
                            string file1 = "Content?ID=" + ItemID.ToString();
                            //取分页标签处理结果 返回字符串数组 根据数组元素个数生成几页 
                            string ilbl = pagelabel.Replace("{ZL.Page ", "").Replace("/}", "").Replace(" ", ",");
                            string lblContent = "";
                            int NumPerPage = 500;
                            IList<string> ContentArr = new List<string>();

                            if (string.IsNullOrEmpty(ilbl))
                            {
                                lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                            }
                            else
                            {
                                string[] paArr = ilbl.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                if (paArr.Length == 0)
                                {
                                    lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                }
                                else
                                {
                                    string lblname = paArr[0].Split(new char[] { '=' })[1].Replace("\"", "");
                                    if (paArr.Length > 1)
                                    {
                                        NumPerPage = DataConverter.CLng(paArr[1].Split(new char[] { '=' })[1].Replace("\"", ""));
                                    }
                                    B_Label blbl = new B_Label();
                                    lblContent = blbl.GetLabelXML(lblname).Content;
                                    if (string.IsNullOrEmpty(lblContent))
                                    {
                                        lblContent = "{loop}<a href=\"{$pageurl/}\">{$pageid/}</a>$$$<b>[{$pageid/}]</b>{/loop}"; //默认格式的分页导航
                                    }
                                    ContentArr = bll.GetContentPage(infoContent, NumPerPage);
                                }
                            }
                            if (ContentArr.Count > 0) //存在分页数据
                            {
                                int _cpage = PageHelper.GetCPage(CPage, 0, ContentArr.Count - 1);
                                ContentHtml = ContentHtml.Replace(infotmp, ContentArr[_cpage]);
                                ContentHtml = ContentHtml.Replace(pagelabel, bll.GetPage(lblContent, ItemID, CPage, ContentArr.Count, NumPerPage));
                            }
                            else
                            {
                                ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                                ContentHtml = ContentHtml.Replace(pagelabel, "");
                            }
                        }
                    }
                    else//没有分页标签
                    {
                        //如果设定了分页内容字段 将该字段内容的分页标志清除
                        if (!string.IsNullOrEmpty(infoContent))
                            ContentHtml = ContentHtml.Replace(infotmp, infoContent);
                    }
                }
                #endregion
            }
            //替换默认分页标签
            string patterns = @"{ZL\.Page([\s\S])*?\/}";
            string pagelabels = Regex.Match(ContentHtml, patterns, RegexOptions.IgnoreCase).Value;
            if (!string.IsNullOrEmpty(pagelabels)) { ContentHtml = ContentHtml.Replace(pagelabels, ""); }
            //if (nodeMod.SafeGuard == 1 && File.Exists(Server.MapPath("/js/Guard.js"))) { ContentHtml = ContentHtml + SafeSC.ReadFileStr("/js/Guard.js"); }
            //if (SiteConfig.SiteOption.IsSensitivity == 1) { ContentHtml = B_Sensitivity.Process(ContentHtml); }
            return ContentHtml;
        }
        #endregion
        #region 商城
        public IActionResult ShopContent()
        {
            if (ItemID < 1)  return WriteErr("未指定商品ID");
            M_Product proMod = proBll.GetproductByid(ItemID);
            try { OrderCommon.ProductCheck(proMod); } catch (Exception ex) { return WriteErr(ex.Message); }
            M_ModelInfo modelinfo = modBll.SelReturnModel(proMod.ModelID);
            proMod.AllClickNum = proMod.AllClickNum + 1;
            proBll.UpdateByID(proMod);
            string TempNode = nodeBll.GetModelTemplate(proMod.Nodeid, proMod.ModelID);
            string TemplateDir = "";
            if (!string.IsNullOrEmpty(proMod.ModeTemplate))//个性模板
            {
                TemplateDir = proMod.ModeTemplate;
            }
            else if (!string.IsNullOrEmpty(TempNode))
            {
                TemplateDir = TempNode;
            }
            else
            {
                TemplateDir = "/" + modelinfo.ContentModule;
            }
            string path = CheckTemplateUrl(TemplateDir, ref err);
            if (!string.IsNullOrEmpty(err)) { return WriteErr(err); }

            string Templatestr = FileSystemObject.ReadFile(path);
            string ContentHtml = bll.CreateHtml(Templatestr, 0, ItemID, 0);
            return HtmlToClient(ContentHtml);
        }
        #endregion
        #region 店铺
        #endregion
        public IActionResult NodePage() {
            if (ItemID < 1) { return ErrToClient("[产生错误的可能原因：栏目ID不能为空!]"); }
            nodeMod = nodeBll.SelReturnModel(ItemID);
            if (nodeMod == null || nodeMod.IsNull) { return ErrToClient("[产生错误的可能原因：栏目ID您访问的节点信息不存在!]"); }
            if (string.IsNullOrEmpty(nodeMod.ListTemplateFile)) { return ErrToClient("[产生错误的可能原因：该节点未指定热门信息模板!]"); }
            //if (!CheckNodeViewAuth(nodeMod,mu, ref err)) { return WriteErr(err, SafeSC.RawUrl); }
            return HtmlToClient(TemplateToHtml(nodeMod.ListTemplateFile));
        }
        public IActionResult NodeNews()
        {
            if (ItemID < 1) { return ErrToClient("[未指定栏目ID]"); }
            nodeMod = nodeBll.GetNodeXML(ItemID);
            if (nodeMod.IsNull) { ErrToClient("[产生错误的可能原因：您访问的节点信息不存在!]"); }
            // if (!CheckNodeViewAuth(nodeMod,mu, ref err)) { return WriteErr(err, SafeSC.RawUrl); }
            return HtmlToClient(TemplateToHtml(nodeMod.LastinfoTemplate));
        }
        public IActionResult NodeHot() {
            if (ItemID < 1) { return ErrToClient("[产生错误的可能原因：栏目ID不能为空!]"); }
            M_Node nodeMod = nodeBll.SelReturnModel(ItemID);
            if (nodeMod == null || nodeMod.IsNull) { return ErrToClient("[产生错误的可能原因：栏目ID您访问的节点信息不存在!]"); }
            // if (!CheckNodeViewAuth(nodeMod,mu, ref err)) { return WriteErr(err, SafeSC.RawUrl); }
            return HtmlToClient(TemplateToHtml(nodeMod.HotinfoTemplate));
        }
        public IActionResult NodeElite() {
            if (ItemID < 1) { return ErrToClient("[产生错误的可能原因：栏目ID不能为空!]"); }
            M_Node nodeMod = nodeBll.SelReturnModel(ItemID);
            if (nodeMod == null || nodeMod.IsNull) { return ErrToClient("[产生错误的可能原因：栏目ID您访问的节点信息不存在!]"); }
            //if (!CheckNodeViewAuth(nodeMod,mu, ref err)) { return WriteErr(err, SafeSC.RawUrl); }
           return HtmlToClient(TemplateToHtml(nodeMod.ProposeTemplate));
        }
        //ColumnList
        public IActionResult ColumnList() {
            if (ItemID < 1) { return ErrToClient("[产生错误的可能原因：没有指定栏目ID]"); }
            M_Node nodeinfo = nodeBll.GetNodeXML(ItemID);
            if (nodeinfo.IsNull) { return ErrToClient("产生错误的可能原因：您访问的栏目不存在"); }
            string TemplateDir = "";
            if (string.IsNullOrEmpty(nodeinfo.IndexTemplate)) { TemplateDir = nodeinfo.ListTemplateFile; }
            else { TemplateDir = nodeinfo.IndexTemplate; }
            if (string.IsNullOrEmpty(TemplateDir))
            {
                return ErrToClient("产生错误的可能原因：节点不存在或未绑定模型模板");
            }
            else
            {
                return HtmlToClient(TemplateToHtml(TemplateDir));
            }
        }
        //返回物理路径
        private string CheckTemplateUrl(string templateUrl, ref string err)
        {
            string tlpDir = SiteConfig.SiteOption.TemplateDir;
            if (string.IsNullOrEmpty(templateUrl)) { err = "未指定模板"; return ""; }
            if (!templateUrl.StartsWith(tlpDir)) { templateUrl = tlpDir + "/" + templateUrl.TrimStart('/'); }
            string path = function.VToP(templateUrl);
            if (!System.IO.File.Exists(path)) { err = "模板文件[" + templateUrl + "]不存在"; return ""; }
            return path;

        }
    }
}