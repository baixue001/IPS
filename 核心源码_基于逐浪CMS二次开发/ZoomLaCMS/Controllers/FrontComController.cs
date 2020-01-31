using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ZoomLaCMS.Ctrl;
using ZoomLa.SQLDAL;
using ZoomLa.Common;

namespace ZoomLaCMS.Controllers
{
    public class FrontComController : Ctrl_User
    {
        public int LoginCount
        {
            get
            {
                return DataConvert.CLng(HttpContext.Session.GetInt32("ValidateCount"));
            }
            set
            {
                HttpContext.Session.SetInt32("ValidateCount",value);
            }
        }
        public void ViewHistory()
        {

        }
        public IActionResult Promo()
        {
            string PStr = GetParam("p");
            string PUName = GetParam("pu");
            int PUserID = DataConvert.CLng(EncryptHelper.DesDecrypt(PStr));
            if (PUserID < 1 && string.IsNullOrEmpty(PUName)) { return WriteErr("无用户信息"); }
            if (PUserID > 0)
            {
                mu = buser.SelReturnModel(PUserID);
            }
            else if (!string.IsNullOrEmpty(PUName))
            {
                mu = buser.GetUserByName(PUName);
            }
            //if (mu.UserID < 1) { function.WriteErrMsg("推广用户不存在");return; }
            //if (mu.UserID > 1) { Response.Cookies["UserState2"]["ParentUserID"] = mu.UserID.ToString(); }
            return Redirect("/User/Register?ParentUserID=" + mu.UserID);
        }
        public IActionResult OrderExample() { return View(); }
        //简洁用户登录
        public IActionResult Login()
        {
            string uname = Request.Form["TxtUserName"];
            string upwd = Request.Form["TxtPassword"];
            ViewBag.err = "";
            if (string.IsNullOrEmpty(uname + upwd))
            {

            }
            else
            {
                if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(upwd))
                {
                    ViewBag.err = "用户名与密码不能为空";
                }
                else
                {
                    mu = buser.GetUserByName(uname, StringHelper.MD5(upwd));
                    if (mu.IsNull) { ViewBag.err = "用户名或密码错误"; }
                    else { buser.SetLoginState(mu); }
                }
            }
            return View(mu);
        }
        //AJAX用户登录页
        public IActionResult Login_Ajax()
        {
            ViewBag.LoginCount = LoginCount;
            return View(mu);
        }
        public void CallCounter()
        {
            //string ZType = GetParam("ztype");
            //string InfoID = GetParam("id");
            //string InfoTitle = SafeSC.GetRequest("title");
            //M_Com_VisitCount visitMod = new M_Com_VisitCount();
            //visitMod.IP = IPScaner.GetUserIP();
            //visitMod.UserID = buser.GetLogin().UserID;
            //visitMod.OSVersion = VisitCounter.User.Agent(2);
            //if (Request.UrlReferrer != null)
            //{
            //    visitMod.Source = Request.UrlReferrer.ToString();//访问源
            //}
            //visitMod.BrowerVersion = DeviceHelper.GetBrower().ToString();
            //visitMod.Device = VisitCounter.User.Agent(3); //设备
            //visitMod.ZType = ZType;
            //visitMod.InfoID = InfoID;
            //visitMod.InfoTitle = InfoTitle;
            //B_Com_VisitCount.Insert(visitMod);
        }
        public string css(string id)
        {
            //Response.ContentType = "text/css";
            //string css = B_Sys_CSSManage.SelTopCSS(id);
            //return css;
            return "";
        }
    }
}