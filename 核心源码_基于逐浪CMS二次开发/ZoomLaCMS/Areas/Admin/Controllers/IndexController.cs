using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using ZoomLa.Model;
using ZoomLa.BLL;
using System.Security.Claims;
using ZoomLaCMS.Ctrl;
using ZoomLa.BLL.API;
using ZoomLa.Common;
using ZoomLaCMS.Control;
using ZoomLa.BLL.Helper;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
   
    //必须加上区域标识,否则路由不到
    [Area("Admin")]
    public class IndexController : Ctrl_Admin
    {
        public int ValidateCount
        {
            get
            {
                return DataConverter.CLng(HttpContext.Session.GetInt32("ValidateCount"));
            }
            set
            {
                HttpContext.Session.SetInt32("ValidateCount",value);
            }
        }
        M_APIResult retMod = new M_APIResult();
        [Authorize(Policy = "Admin")]
        [Route("/Admin/Default")]
        [Route("/Admin/Index/Default")]
        public IActionResult Default()
        {
            ViewBag.adminMod = adminMod;
            return View();
        }
        [Route("/Admin/I/Main")]
        [Route("/Admin/Main")]
        public IActionResult Main() { return View("Main"); }
        [Authorize(Policy = "Admin")]
        public IActionResult WorkTable()
        {
            return View();
        }
        public IActionResult ASCXLoad(string name)
        {
            if (string.IsNullOrEmpty(name)) { return Content(""); }
            C_Model model = new C_Model();
            model.mode = StrHelper.GetValFromUrl(name, "mode");
            model.value = StrHelper.GetValFromUrl(name, "value");
            string cshtmlName = name.Split('?')[0].Replace(".ascx", "");
            //检测当前页面是否有缓存,无则重新加载
            return PartialView("~/Areas/Admin/Views/Shared/" + cshtmlName + ".cshtml", model);
        }
        #region 登录退出
        //"~/Areas/Admin/Views/Index/Login.cshtml"
        [Route("/Admin/Login")]
        [Route("/Admin/Index/Login")]
        public IActionResult Login()
        {
            //if (B_Admin.GetLogin(HttpContext) != null) { return RedirectToAction("Default"); }
            ViewBag.ValidateCount = ValidateCount;
            M_AdminInfo adminMod = B_Admin.GetLogin(HttpContext);
            if (adminMod != null && adminMod.AdminId > 0) { return Redirect("Default"); }
            return View();
        }
        [HttpPost]
        public string Login_AJAX()
        {
            string result = "";
            try
            {
                //RequestEx["vcode"], RequestEx["zncode"], RequestEx["admincode"]
                result = AjaxVaild(RequestEx["user"],RequestEx["pwd"]);
            }
            catch (Exception ex) { result = ex.Message; }
            return result;

        }
        private string AjaxVaild(string user, string pwd)
        {
            user = user.Trim(); pwd = pwd.Trim();
            if (ValidateCount >= 3)
            {
                if (!ZoomlaSecurityCenter.VCodeCheck(RequestEx["VCode_hid"], RequestEx["vcode"]))
                {
                    return "验证码不正确";
                }
            }
            M_AdminInfo info = B_Admin.AuthenticateAdmin(user, pwd);
            ValidateCount++;
            if (info == null || info.IsNull)
            {
                if (ValidateCount == 3) { return "True"; } else { return "用户名或密码错误！"; }
            }
            else if (info.IsLock)
            {
                return "你的帐户被锁定，请与超级管理员联系";
            }
            else
            {
                ZLLog.L(ZLEnum.Log.alogin, "管理员[" + info.UserName + "]登录");
                ValidateCount = 0;
                B_Admin.SetLoginState(HttpContext, info);
            }
            return "True";
        }
        public void Logout()
        {
            B_Admin.ClearLogin(HttpContext);
            Response.Redirect("/Admin/Login");
        }
        #endregion
    }
}