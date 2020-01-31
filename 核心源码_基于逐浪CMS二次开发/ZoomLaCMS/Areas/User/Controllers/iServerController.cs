using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class iServerController : Ctrl_User
    {
        B_IServer isBll = new B_IServer();
        B_IServerReply repBll = new B_IServerReply();
        public void Index()
        {
            Response.Redirect("FiServer"); 
        }
        // 回答其他用户的提问,仅限于被@
        public IActionResult ISAnswer()
        {
            string title = RequestEx["title"];
            PageSetting setting = isBll.SelPage(CPage, PSize, new F_IServer()
            {
                ccuser = mu.UserID.ToString(),
                title = RequestEx["title"],
                state = RequestEx["state"]
            });
            if (Request.IsAjax())
            {
                return PartialView("ISAnswer_List",setting);
            }
            return View(setting);
        }
        public IActionResult FiServer()
        {
            //订单ID,有问必答与订单绑定
            int OrderID = DataConverter.CLng(RequestEx["orderid"]);
            int Type = DataConverter.CLng(RequestEx["type"],-100);
            string typeStr = "";
            if (Type == -100) { typeStr = ""; }
            else { typeStr = isBll.TypeArr[Type]; }
            string state = isBll.GetStatus(DataConverter.CLng(RequestEx["num"],-100));
            //----------------------------------
            PageSetting config = isBll.SelPage(CPage, PSize, new F_IServer()
            {
                uid = mu.UserID,
                state = state,
                type = typeStr,
                title = RequestEx["skey_t"]
            });
            if (Request.IsAjax()) { return PartialView("FiServer_List",config); }
            ViewBag.allnum = isBll.getiServerNum("", mu.UserID, typeStr, OrderID);
            ViewBag.treatnum = isBll.getiServerNum("处理中", mu.UserID, typeStr, OrderID);
            ViewBag.nrslvnum = isBll.getiServerNum("未解决", mu.UserID, typeStr, OrderID);
            ViewBag.rslvnum = isBll.getiServerNum("已解决", mu.UserID, typeStr, OrderID);
            ViewBag.socknum = isBll.getiServerNum("已锁定", mu.UserID, typeStr, OrderID);
            ViewBag.typedt = isBll.GetSeachUserIdType(mu.UserID);
            return View(config);
        }
        public IActionResult AddQuestion()
        {
            return View();
        }
        
        public IActionResult Question_Add()
        {
            M_IServer isMod = new M_IServer();
            isMod.UserId = mu.UserID;
            isMod.UserName = mu.UserName;
            isMod.Title = RequestEx["title_t"];
            isMod.Content = RequestEx["txtContent"];
            isMod.Priority = RequestEx["Priority"];
            isMod.Type = RequestEx["Type"];
            isMod.Root = "网页表单";
            isMod.State = "未解决";
            if (SafeSC.CheckIDS(RequestEx["CCUser_Hid"]))
            {
                isMod.CCUser = RequestEx["CCUser_Hid"];
            }
            isMod.RequestTime = DataConverter.CDate(RequestEx["mydate_t"]);
            if (!string.IsNullOrEmpty(RequestEx["OrderID"])) { isMod.OrderType = DataConverter.CLng(RequestEx["OrderID"]); }
            isMod.Path = RequestEx["attach_hid"];
            isMod.QuestionId = isBll.Insert(isMod);
            if (isMod.QuestionId > 0)
            {
                return WriteOK("提交成功", "FiServer?OrderID=" + isMod.OrderType); 
            }
            else
            {
                return WriteErr("提交失败-可能是由于系统未开放功能所致"); 
            }
        }
        //仅用于用户操作
        [HttpPost]
        public string IServer_API()
        {
            string action = RequestEx["action"] ?? "";
            M_IServer isMod = isBll.SelReturnModel(Mid);
            if (isMod == null) { return ""; }
            if (isMod.UserId != mu.UserID) { return "你无权操作该内容"; }
            switch (action)
            {
                //case "state":
                //    {
                //        int state = DataConverter.CLng(RequestEx["state"]);
                //        isBll.UpdateState(Mid,state);
                //    }
                //    break;
                case "solve":
                    {
                        isMod.State = isBll.GetStatus(3);
                        isMod.SolveTime = DateTime.Now;
                        isBll.UpdateByID(isMod);
                    }
                    break;
                case "close":
                    {
                        isBll.UpdateState(Mid, -1);
                    }
                    break;
                case "udel"://用户删除
                    {
                        isBll.DeleteById(Mid);
                    }
                    break;
            }
            return Success.ToString();
        }
        public IActionResult FiServerInfo()
        {
            string Menu = RequestEx["menu"];
            string Path = RequestEx["filepath"];
            if (Menu.Equals("filedown") && !string.IsNullOrEmpty(Path))
            {
      
            }
            M_IServer serverMod = isBll.SelReturnModel(Mid);
            if (serverMod == null) { return WriteErr("问题不存在"); return null; }
            //回复列表
            ViewBag.replydt = repBll.SeachById(serverMod.QuestionId);
            //更新已读状态
            repBll.GetUpdataState(1, serverMod.QuestionId);
            return View(serverMod);
        }
        public IActionResult SelectiServer()
        {
            string skey = DataConverter.CStr(RequestEx["strTitle"]);
            int Type = DataConverter.CLng(RequestEx["type"]);
            int OrderID = DataConverter.CLng(RequestEx["orderid"]);
            string state = isBll.GetStatus(DataConverter.CLng(RequestEx["num"]));
            F_IServer filter = new F_IServer()
            {
                uid = mu.UserID,
                title = skey,
                state = state,
                type = isBll.TypeArr[Type],
                oid = OrderID
            };
            //回答问题,不需要进行用户筛选
            if (DataConverter.CStr(RequestEx["filter"]).Equals("answer"))
            {
                filter.uid = -100;
                filter.ccuser = mu.UserID.ToString();
            }
            PageSetting setting = isBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest()) { return PartialView("SelectServer_List", setting); }
            ViewBag.typedt = isBll.GetSeachUserIdType(mu.UserID);
            return View(setting);
        }
        #region 回复信息
        public IActionResult ISReplyAdd()
        {
            M_IServerReply repMod = new M_IServerReply();
            if (Mid > 0)
            {
                repMod = repBll.SelReturnModel(Mid);
                if (repMod.UserId != mu.UserID) { return WriteErr("你无权修改该回复");return null; }
            }
            else
            {
                repMod.QuestionId = DataConverter.CLng(RequestEx["Qid"]);
            }
            return View(repMod);
        }
        
        public ContentResult QuestionReply_Add()
        {
            int Qid = DataConverter.CLng(RequestEx["Qid"]);
            M_IServerReply repMod = new M_IServerReply();
            if (Mid > 0)
            {
                repMod = repBll.SelReturnModel(Mid);
                if (repMod.UserId != mu.UserID) { return Content("你无权修改该回复");  }
            }
            repMod.UserId = mu.UserID;
            repMod.UserName = mu.UserName;
            repMod.Title = RequestEx["Title_T"];
            repMod.Content = RequestEx["Content_T"];
            repMod.Path = RequestEx["Attach_Hid"];
            if (repMod.Id > 0)
            {
                repBll.UpdateByID(repMod);
            }
            else
            {
                repMod.QuestionId = Qid;
                repBll.Add(repMod);
            }
            return Content("<script>parent.location=parent.location;</script>");
        }
        #endregion
    }
}
