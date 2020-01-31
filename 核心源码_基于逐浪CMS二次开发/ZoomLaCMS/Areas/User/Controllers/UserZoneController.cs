using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class UserZoneController : Ctrl_User
    {
        B_User_Follow fwBll = new B_User_Follow();
        B_User_FriendApply faBll = new B_User_FriendApply();
        B_User_Friend friendBll = new B_User_Friend();
        B_User_BlogStyle bsBll = new B_User_BlogStyle();
        public void Index()
        {
            Response.Redirect("/User/UserZone/MyZonePage");
        }
        public void Default() { Response.Redirect("/User/UserZone/MyZonePage"); }
        public IActionResult MySubscription()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult FollowList()
        {
            int ztype = DataConverter.CLng(RequestEx["type"]);
            PageSetting setting = null;
            if (ztype == 1)//关注我的
            {
                setting = fwBll.SelByTUser_SPage(CPage, PSize, mu.UserID, RequestEx["skey"]);
            }
            else
            {
                setting = fwBll.SelByUser_SPage(CPage, PSize, mu.UserID, RequestEx["skey"]);
            }
            ViewBag.ztype = ztype;
            return View(setting);
        }
        //取消关注
        public int Follow_Del(string id)
        {
            fwBll.DelByIDS(id);
            return 1;
        }
        #region 好友
        public IActionResult FriendApply()
        {
            string action = RequestEx["action"] ?? "";
            PageSetting setting = null;
            switch (action)
            {
                case "send"://我发送的好友申请
                    setting = faBll.SelMySendApply_SPage(CPage, PSize, mu.UserID);
                    break;
                default://我收到的未处理的好友申请
                    setting = faBll.SelMyReceApply_SPage(CPage, PSize, mu.UserID);
                    break;
            }
            ViewBag.action = action;
            return View(setting);
        }
        public IActionResult FriendList()
        {
            PageSetting setting = friendBll.SelMyFriend_SPage(CPage, PSize, mu.UserID, RequestEx["skey"]);
            return View(setting);
        }
        public PartialViewResult Friend_Data()
        {
            PageSetting setting = friendBll.SelMyFriend_SPage(CPage, PSize, mu.UserID, RequestEx["skey"]);
            return PartialView("FriendList_List", setting);
        }
        public IActionResult UserQuestFriend() { return View(); }
        public IActionResult QueryUser()
        {
            string action = RequestEx["action"] ?? "";
            int sex = 0;
            string username = "";
            int userid = 0;
            switch (action)
            {
                case "vague":
                    sex = DataConverter.CLng(RequestEx["sex"]);
                    ViewBag.where = "sex=" + sex;
                    break;
                case "username":
                    username = RequestEx["username"];
                    ViewBag.where = "username=" + username;
                    break;
                case "uid":
                    userid = DataConverter.CLng(RequestEx["userid"]);
                    ViewBag.where = "userid=" + userid;
                    break;
            }
            PageSetting setting = buser.SelPage(CPage, PSize,new F_User() { });
            return View(setting);
        }
        public PartialViewResult QueryUser_Data()
        {
            int sex = DataConverter.CLng(RequestEx["sex"]);
            string username = RequestEx["username"];
            int userid = DataConverter.CLng(RequestEx["userid"]);
            PageSetting setting = buser.SelPage(CPage, PSize, new F_User() { });
            return PartialView("QueryUser_List", setting);
        }
        //删除好友
        public int Friend_Del(string id)
        {
            int tuid = DataConverter.CLng(id);
            friendBll.DelFriend(mu.UserID, tuid);
            return 1;
        }
        //同意好友申请
        public int FriendApply_Agree(string id)
        {
            int mid = DataConverter.CLng(id);
            M_User_FriendApply faMod = faBll.SelReturnModel(mid);
            faMod.ZStatus = (int)ZLEnum.ConStatus.Audited;
            faBll.UpdateByID(faMod);
            friendBll.AddFriendByApply(faMod);
            return Success;
        }
        //拒绝好友申请
        public int FriendApply_Reject(string id)
        {
            int mid = DataConverter.CLng(id);
            M_User_FriendApply faMod = faBll.SelReturnModel(mid);
            faMod.ZStatus = (int)ZLEnum.ConStatus.Reject;
            faBll.UpdateByID(faMod);
            return 1;
        }
        public PartialViewResult FriendApplyRece_Data()
        {
            return PartialView("FriendApply_ReceList", faBll.SelMyReceApply_SPage(CPage, PSize, mu.UserID));
        }
        public PartialViewResult FriendApplySend_Data()
        {
            return PartialView("FriendApply_SendList", faBll.SelMySendApply_SPage(CPage, PSize, mu.UserID));
        }
        #endregion
        public IActionResult MyZonePage()
        {
            PageSetting setting = bsBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("MyZonePage_List", setting); }
            if (mu.State != 2) { ViewBag.showtype = 1; }
            else if (setting.itemCount < 1) { ViewBag.showtype = 2; }
            else
            {
                if (mu.PageID < 1)
                {
                    ViewBag.sname = "还没有选定模板";
                }
                else
                {
                    M_User_BlogStyle bsMod = bsBll.SelReturnModel(mu.PageID);
                    if (bsMod != null)
                    {
                        ViewBag.sname = bsMod.StyleName + " <a href='/User/Space/SpaceManage?ID=" + mu.UserID + "' target='_blank' class='btn btn-xs btn-info'>访问页面</a>";
                    }
                }
            }
            return View(setting);
        }
        public IActionResult MyZonePage_Apply()
        {
            B_User.UpdateField("PageID", RequestEx["ID"], mu.UserID);
            return WriteOK("操作成功", "MyZonePage");
        }
    }
}
