using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class MessageController : Ctrl_User
    {
        B_Message msgBll = new B_Message();
        public void Index()
        {
            Response.Redirect("Message"); 
        }
        public IActionResult Message()
        {
            F_Message filter = new F_Message()
            {
                title = RequestEx["skey_t"],
                uid = mu.UserID,
                filter = "rece"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax())
            {
                return PartialView("Message_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //新建|修改邮件,草稿
        public IActionResult MessageSend()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0)
            {
                msgMod = msgBll.SelReturnModel(Mid);
                if (msgMod.Sender != mu.UserID) { return WriteErr("你无权修改该邮件"); }
                if (msgMod.Savedata == 0) { return WriteErr("邮件已发送,不可修改"); }
            }
            else
            {
                M_UserInfo tmu = new M_UserInfo(true);
                if (DataConverter.CLng(RequestEx["uid"]) > 0)
                {
                    tmu = buser.SelReturnModel(DataConverter.CLng(RequestEx["uid"]));
                }
                else if (!string.IsNullOrEmpty(RequestEx["name"]))
                {
                    tmu = buser.GetUserByName(HttpUtility.UrlDecode(RequestEx["name"]));
                }
                if (!tmu.IsNull) { msgMod.Incept = tmu.UserID.ToString(); }

                if (!string.IsNullOrEmpty(RequestEx["content"])) { msgMod.Content = HttpUtility.UrlDecode(RequestEx["content"]); }
                if (!string.IsNullOrEmpty(RequestEx["title"])) { msgMod.Title = HttpUtility.UrlDecode(RequestEx["title"]); }
            }
            return View(msgMod);
        }
        //阅读邮件
        public IActionResult MessageRead()
        {
            M_Message msgMod = msgBll.SelReturnModel(Mid);
            if (msgMod == null) { return WriteErr("邮件不存在"); }
            if (msgMod.Sender == mu.UserID||
                msgMod.Incept.Contains("," + mu.UserID + ",") ||
                msgMod.CCUser.Contains("," + mu.UserID + ",")) { }
            else { return WriteErr("你无权阅读该邮件"); }
            return View(msgMod);
        }
        //草稿箱
        public IActionResult MessageDraftbox()
        {
            F_Message filter = new F_Message()
            {
                //title = RequestEx["skey_t"],
                uid = mu.UserID,
                filter = "draft"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax())
            {
                return PartialView("Message_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //发件箱
        public IActionResult MessageOutbox()
        {
            F_Message filter = new F_Message()
            {
                title = RequestEx["skey_t"],
                uid = mu.UserID,
                filter = "send"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax())
            {
                return PartialView("MessageOutbox_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //回收站
        public IActionResult MessageRecycle()
        {
            F_Message filter = new F_Message()
            {
                title = RequestEx["skey_t"],
                uid = mu.UserID,
                filter = "recycle"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax())
            {
                return PartialView("MessageRecycle_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        public IActionResult Mobile() { return View(); }
        //------------------------------
        [HttpPost]
        
        public IActionResult Message_Add()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0) { msgMod = msgBll.SelReturnModel(Mid); }
            msgMod.Savedata = 0;
            msgMod.status = 1;
            FillMsgModel(msgMod);
            if (msgMod.MsgID > 0)
            {
                msgBll.UpdateByID(msgMod);
            }
            else { msgMod.Sender = mu.UserID;msgMod.UserName = mu.UserName; msgBll.GetInsert(msgMod); }

            return WriteOK("发送成功", "MessageOutbox");
        }
        [HttpPost]
        
        public IActionResult Message_Draft()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0) { msgMod = msgBll.SelReturnModel(Mid); }
            msgMod.Sender = mu.UserID;
            msgMod.Savedata = 1;
            msgMod.status = 0;
            FillMsgModel(msgMod);
            if (msgMod.MsgID > 0)
            {
                msgBll.UpdateByID(msgMod);
            }
            else { msgMod.Sender = mu.UserID;msgMod.UserName = mu.UserName; msgBll.GetInsert(msgMod); }
            return WriteOK("存为草稿成功", "MessageDraftbox");
        }
        [HttpPost]
        public int Message_Del(string ids)
        {
            msgBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        [HttpPost]
        public int Message_RealDel(string ids)
        {
         //msgBll.ReCoverByIDS
            msgBll.RealDelByIDS(ids, mu.UserID);
            return Success;
        }
        [HttpPost]
        public int Message_Recovery(string ids)
        {
         //msgBll.ReCoverByIDS
            msgBll.ReFromRecycle(ids, mu.UserID);
            return Success;
        }

        private M_Message FillMsgModel(M_Message msgMod)
        {
            msgMod.Title = HttpUtility.HtmlEncode(RequestEx["title_t"]);
            msgMod.Content = RequestEx["content_t"];
            msgMod.Incept = RequestEx["refer_hid"];
            msgMod.CCUser = RequestEx["ccuser_hid"];
            msgMod.Attachment = RequestEx["Attach_Hid"];
            return msgMod;
        }

    }
}
