using System;
using System.Data;
using System.Linq;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLaCMS.Ctrl;
using ZoomLa.BLL.Shop;
using ZoomLa.Model.Content;
using ZoomLa.BLL.Helper;

namespace ZoomLaCMS.Controllers
{
    public class CommentsController : Ctrl_User
    {
        B_Content conBll = new B_Content();
        B_Comment cmtBll = new B_Comment();
        B_Node nodeBll = new B_Node();
        B_KeyWord keyBll = new B_KeyWord();
        B_Product proBll = new B_Product();
        public int ItemID { get { return DataConverter.CLng(RequestEx["ID"]); } }
        public IActionResult CommentFor()
        {
            if (ItemID < 1) {return WriteErr("内容ID错误");  }
            PageSetting setting = cmtBll.SeachCommentByGid1(CPage, PSize, ItemID);
            if (Request.IsAjax())
            {
                return PartialView("CommentFor_List",setting);
            }
            //获取节点配置
            M_CommonData cdata = conBll.GetCommonData(ItemID);
            if (cdata == null) { return WriteErr("内容不存在");  }
            M_Node nodeMod = nodeBll.GetNodeXML(cdata.NodeID);
            if (nodeMod == null) { return WriteErr("节点不存在");  }
            ViewBag.nodeMod = nodeMod;
            ViewBag.mu = mu;
            return View(setting);
        }
        public string Comment_Reply()
        {
            if (!ZoomlaSecurityCenter.VCodeCheck(Request.Form["VCode_hid"], Request.Form["VCode"])) { return "-1"; }
            M_CommonData cdata = conBll.GetCommonData(ItemID);
            M_Node mnode = nodeBll.GetNodeXML(cdata.NodeID);
            M_Comment comment = new M_Comment();
            comment.GeneralID = ItemID;
            comment.UserID = mu.UserID;
            comment.Audited = true;
            comment.Pid = DataConverter.CLng(Request.Form["id"]);
            comment.Contents = Request.Form["content"];
            if (mnode.CommentType.Equals("2") && mu.IsNull) { return "-2"; }
            else if (string.IsNullOrEmpty(comment.Contents)) {return "-3"; }
            else { AddComment(comment); return "1"; }
        }
        //----------------------------------------------------------
        public IActionResult Keywords()
        {
            PageSetting setting = keyBll.SelPage(CPage, 9, RequestEx["skey"], DataConverter.CLng(RequestEx["pri"]));
            return View(setting);
        }
        public PartialViewResult Keyword_Data()
        {
            PageSetting setting = keyBll.SelPage(CPage, 9, RequestEx["skey"], DataConverter.CLng(RequestEx["pri"]));
            return PartialView("Keywords_List", setting);
        }
        public IActionResult KeywordContact()
        {
            string keyword = RequestEx["key"] ?? "";
            string type = RequestEx["type"] ?? "content";
            if (!string.IsNullOrEmpty(keyword)) { return WriteErr("未指定关键字"); }
            PageSetting setting = new PageSetting();
            switch (type)
            {
                case "content"://文章信息
                    setting = conBll.SelPageByTagkey(CPage, PSize, RequestEx["tagkey"], RequestEx["key"]);
                    ViewBag.action = "content";
                    break;
                default://商品信息
                    //setting = proBll.SelPageByKayword(CPage, PSize, RequestEx["tagkey"], RequestEx["key"]);
                    setting = proBll.SelPage(CPage, PSize, new Filter_Product()
                    {
                        skey = RequestEx["key"]
                    });
                    ViewBag.action = "commodity";
                    break; 
            }
            return View(setting);
        }
        public PartialViewResult Content_Data()
        {
            return PartialView("Contents_List", conBll.SelPageByTagkey(CPage, PSize, RequestEx["tagkey"], RequestEx["key"]));

        }
        public PartialViewResult Commoditys_Data()
        {
            return PartialView("Commoditys_List", proBll.SelPage(CPage, PSize, new Filter_Product()
            {
                skey = RequestEx["key"]
            }));
        }
        
        [HttpPost]
        public string Comment_API()
        {
            string action = RequestEx["action"];
            string value = "";
            string result = "";
            switch (action)
            {
                case "report"://举报
                    value = Request.Form["cid"];
                    cmtBll.ReportComment(Convert.ToInt32(value), mu.UserID);
                    result = Success.ToString();
                    break;
                case "support"://支持反对操作
                    value = Request.Form["flag"];
                    bool rflag = true;
                    bool flag = DataConverter.CLng(value) > 0;
                    if (buser.GetLogin().IsNull)
                        rflag = cmtBll.Support(Convert.ToInt32(Request.Form["id"]), flag ? 1 : 0, IPScaner.GetUserIP(HttpContext));
                    else
                        rflag = cmtBll.Support(Convert.ToInt32(Request.Form["id"]), flag ? 1 : 0,IPScaner.GetUserIP(HttpContext), mu.UserID, mu.UserName);
                    result = rflag ? Success.ToString() : Failed.ToString();
                    break;
                case "assist"://顶与踩
                    bool bl = true;
                    if (buser.GetLogin().IsNull)
                        bl = cmtBll.Support(0, Convert.ToInt32(Request.Form["value"]),IPScaner.GetUserIP(HttpContext), Convert.ToInt32(Request.Form["gid"]));
                    else
                        bl = cmtBll.Support(0, Convert.ToInt32(Request.Form["value"]),IPScaner.GetUserIP(HttpContext), mu.UserID, mu.UserName, Convert.ToInt32(Request.Form["gid"]));
                    result = bl ? "1" : "0";
                    break;
                case "reply"://回复
                    result = Comment_Reply();
                    break;
                case "sender"://发送评论
                    result = SendComm();
                    break;
                default:
                    throw new Exception(action + "不存在");
            }
            return result;
        }

        //-------------Tools
        private void AddComment(M_Comment cmtMod)
        {
            cmtBll.Add(cmtMod);
            if (SiteConfig.UserConfig.CommentRule > 0 && cmtMod.UserID > 0)//增加积分
            {
                buser.ChangeVirtualMoney(cmtMod.UserID, new M_UserExpHis()
                {
                    score = SiteConfig.UserConfig.CommentRule,
                    detail = "发表评论增加积分",
                    ScoreType = (int)M_UserExpHis.SType.Point
                });
            }
        }
        // 发表评论
        private string SendComm()
        {
            if (!ZoomlaSecurityCenter.VCodeCheck(Request.Form["VCode_hid"], Request.Form["VCode"])) { return "-1"; }
            //内容为空不允许发送
            if (string.IsNullOrEmpty(Request.Form["content"])) { return "-3"; }
            M_UserInfo mu = buser.GetLogin(false);
            M_Comment comment = new M_Comment();
            M_CommonData cdata = conBll.GetCommonData(ItemID);
            comment.GeneralID = ItemID;
            //是否开放评论
            if (cdata.IsComm == 0) { return  "-4"; }
            //节点是否开启评论权限
            M_Node nodeMod = nodeBll.SelReturnModel(cdata.NodeID);
            //需要登录,但用户未登录
            if (nodeMod.CommentType.Equals("2") && !buser.CheckLogin()) { return "-2"; }
            comment.UserID = mu.UserID;//支持一个支持匿名方法
            comment.Contents = Request.Form["content"];
            comment.Audited = true;
            //DataTable dts = cmtBll.SeachComment_ByGeneralID2(ItemID);
            //if (nodeMod.Purview != null && nodeMod.Purview != "")
            //{
            //    string Purview = nodeMod.Purview;
            //    DataTable AuitDT = nodeBll.GetNodeAuitDT(nodeMod.Purview);
            //    if (AuitDT == null && AuitDT.Rows.Count <= 0) { return "-4"; }
            //    DataRow auitdr = AuitDT.Rows[0];
            //    string forum_v = auitdr["forum"].ToString();
            //    if (string.IsNullOrEmpty(forum_v)) { return "-4"; }
            //    string[] forumarr = forum_v.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            //    //不允许评论
            //    if (!forumarr.Contains("1")) { return "-4"; }
            //    //不需要审核
            //    if (!forumarr.Contains("2")) { comment.Audited = true; }
            //    if (forumarr.Contains("3")) //一个文章只评论一次
            //    {
            //        if (cmtBll.SearchByUser(mu.UserID, cdata.NodeID).Rows.Count > 0) { return "-5"; }
            //    }
            //}
            AddComment(comment);
            return comment.Audited ? "2" : "1";
        }

    }
}
