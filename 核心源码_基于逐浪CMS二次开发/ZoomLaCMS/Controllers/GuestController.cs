using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class GuestController : Ctrl_User
    {
        B_GuestBookCate cateBll = new B_GuestBookCate();
        B_GuestBook guestBll = new B_GuestBook();
        public IActionResult Default()
        {
            int CateID = DataConverter.CLng(RequestEx["cateid"]);
            string Skey = RequestEx["skey"];
            M_GuestBookCate cateMod = cateBll.SelReturnModel(CateID);
            if (cateMod == null) { cateMod = new M_GuestBookCate() { CateID = 0, CateName = "留言信息", NeedLog = 0 }; }
            ViewBag.cateMod = cateMod;
            ViewBag.cateDt = cateBll.SelByGuest();
            ViewBag.needlog = cateMod.NeedLog;
            ViewBag.mu = buser.GetLogin();
            PageSetting setting = B_GuestBook.SelPage(CPage, PSize, new F_Guest()
            {
                parentId = 0,
                skey = Skey,
                cateIds = cateMod.CateID.ToString(),
                uids = mu.UserID.ToString(),
                onlyAudit = true
            });
            return View(setting);
        }
        public PartialViewResult Cate_Data()
        {
            int CateID = DataConverter.CLng(RequestEx["cateid"]);
            PageSetting setting = B_GuestBook.SelPage(CPage, PSize, new F_Guest()
            {
                parentId = 0,
                skey = RequestEx["skey"],
                cateIds = CateID.ToString(),
                uids = mu.UserID.ToString(),
                onlyAudit = true
            });
            return PartialView("Default_List", setting);
        }
        public IActionResult Add()
        {
            if (!ZoomlaSecurityCenter.VCodeCheck(RequestEx["VCode_hid"], RequestEx["VCode"]))
            {
                 return WriteErr("验证码不正确", Request.RawUrl());
            }
            int CateID = DataConverter.CLng(RequestEx["Cate"]);
            M_GuestBook info = new M_GuestBook();
            M_GuestBookCate cateMod = cateBll.SelReturnModel(CateID);
            //不允许匿名登录,必须登录才能发表留言
            if (cateMod.NeedLog == 1)
            {
                if (!mu.IsNull)
                {
                    info.UserID = mu.UserID;
                }
                else
                {
                    return Redirect("/User/Login");
                }
            }
            else if (buser.CheckLogin())
            {
                info.UserID = buser.GetLogin().UserID;
            }
            info.CateID = CateID;
            //是否开启审核
            info.Status = cateMod.Status == 1 ? 0 : 1;
            info.ParentID = 0;
            info.Title = HttpUtility.HtmlEncode(Request.Form["Title"]);
            info.TContent = RequestEx["Content"];
            info.IP =IPScaner.GetUserIP(HttpContext);
            guestBll.AddTips(info);
            if (cateMod.Status == 1)
            {
                if (cateMod.IsShowUnaudit == 1)
                {
                    return WriteOK("您的留言已提交，请等待系统审核", "/Guest/Default?CateID=" + CateID);
                }
                else
                {
                    return WriteOK("您的留言已提交，通过系统审核后会出现在开放列表中", "/Guest/Default?CateID=" + CateID);
                }
            }
            else
            {
                return WriteOK("留言成功", "/Guest/Default?CateID=" + CateID);
            }
        }
        public IActionResult GuestShow()
        {
            int GID = DataConverter.CLng(RequestEx["GID"]);
            if (GID < 1) {  return WriteErr("没有传入留言ID");  }
            M_GuestBook info = guestBll.SelReturnModel(GID);
            M_UserInfo mu = buser.GetLogin();
            if (info.IsNull || info.ParentID > 0 || info.Status == -1) {  return WriteErr("留言信息不存在!"); }
            if (info.Status == 0 && info.UserID != mu.UserID) {  return WriteErr("该留言未通过审核,无法查看详情"); }
            M_GuestBookCate cateMod = cateBll.SelReturnModel(info.CateID);
            ViewBag.mu = mu;
            ViewBag.cateMod = cateMod;
            ViewBag.GTitle = info.Title;
            ViewBag.cateDt = cateBll.SelByGuest();
            PageSetting setting = B_GuestBook.SelPage(CPage, 20, new F_Guest() { gid = GID });
            return View(setting);
        }
        public PartialViewResult GuestShow_Data()
        {
            int GID = DataConverter.CLng(RequestEx["GID"]);
            PageSetting setting = B_GuestBook.SelPage(CPage, 20, new F_Guest() { gid = GID });
            return PartialView(setting);
        }
        public IActionResult AddReply()
        {
            int GID = DataConverter.CLng(RequestEx["GID"]);
            if (!ZoomlaSecurityCenter.VCodeCheck(RequestEx["VCode_hid"], RequestEx["VCode"]))
            {
               return WriteErr("验证码不正确", Request.RawUrl()); 
            }
            M_GuestBook pinfo = guestBll.SelReturnModel(GID);
            M_GuestBookCate cateMod = cateBll.SelReturnModel(pinfo.CateID);
            M_GuestBook info = new M_GuestBook();
            M_UserInfo mu = buser.GetLogin();
            info.UserID = mu.UserID;
            info.ParentID = GID;
            info.Status = cateMod.Status == 1 ? 0 : 1;
            info.Title = "[会员回复]";
            info.CateID = pinfo.CateID;
            info.TContent = Request.Form["Content"];
            //info.Status = SiteConfig.SiteOption.OpenAudit > 0 ? 0 : 1;
            info.IP =IPScaner.GetUserIP(HttpContext);
            guestBll.AddTips(info);
            if (info.Status == 1) { return WriteOK("回复成功", "GuestShow?Gid=" + GID); }
            else { return WriteOK("您的回复已提交，请等待后系统审核", "GuestShow?Gid=" + GID);  }
        }
    }
}