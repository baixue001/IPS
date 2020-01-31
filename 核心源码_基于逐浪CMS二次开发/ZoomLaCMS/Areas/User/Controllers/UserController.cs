using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Helper.Encry;
using ZoomLa.BLL.Plat;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Plat;
using ZoomLaCMS.Ctrl;
using Microsoft.AspNetCore.Session;

namespace ZoomLaCMS.Areas.User.Controllers
{
    //用户方面相关操作,如登录,退出,修改信息等
    [Area("User")]
    public class UserController : Ctrl_User
    {
        public int UserLoginCount
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
        //自有账号登录(跳转页面)
        public string Login_Ajax(string uname, string upwd, string vcode, int regid)
        {
            string err = "";
            M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
            string openVCode=SiteConfig.UserConfig.EnableCheckCodeOfLogin;
            if (( openVCode== "0" && UserLoginCount >= 3) || openVCode == "1")
            {
                if (!VerifyHelper.Check(RequestEx["VCode_hid"]))
                {
                    retMod.retmsg = "验证码不正确";
                    return retMod.ToString();
                }
            }
            upwd = RSAHelper.RsaDecrypt(upwd,SafeSC.ReadFileStr("/config/safe/PrivateKey.config"));
            M_UserInfo mu = LoginByRegID(ref err, uname, upwd, regid);
            if (mu.IsNull) { UserLoginCount++; retMod.retmsg = err; if (openVCode == "0" && UserLoginCount >= 3) { retMod.addon = "showvcode"; } }
            else if (mu.Status != 0) { retMod.retmsg = "你的帐户未通过验证或被锁定，请与网站管理员联系"; }
            else {
                UserLoginCount = 0;
                retMod.retcode = M_APIResult.Success;
                buser.SetLoginState(mu, "Month");
            }
            return retMod.ToString();
        }
        public void Logout()
        {
            buser.ClearCookie();
            string url = "/User/Login";
            string ReturnUrl = RequestEx["ReturnUrl"];
            if (!string.IsNullOrEmpty(ReturnUrl)) {
                //url += "?ReturnUrl=" + ReturnUrl;
                url = ReturnUrl;
            }
            Response.Redirect(url);
            //Response.Write("<script>setTimeout(function(){location='" + url + "';},500);</script>");
        }
        private M_UserInfo LoginByRegID(ref string errmsg, string AdminName, string AdminPass,int RegID)
        {
            AdminName = AdminName.Trim();
            AdminPass = AdminPass.Trim();
            M_UserInfo info = new M_UserInfo(true);
            switch (RegID)
            {
                case 1:
                    errmsg = "邮箱名或密码错误";
                    info = buser.AuthenticateEmail(AdminName, AdminPass);
                    break;
                case 2:
                    errmsg = "用户ID或密码错误";
                    info = buser.AuthenticateID(DataConverter.CLng(AdminName), AdminPass);
                    break;
                case 3:
                    errmsg = "手机号码或密码错误";
                    info = buser.AuthenByMobile(AdminName, AdminPass);
                    break;
                case 4:
                    errmsg = "用户名或密码错误";
                    info = buser.AuthenByUME(AdminName, AdminPass);
                    break;
                default:
                    errmsg = "用户名或密码错误";
                    info = buser.AuthenticateUser(AdminName, AdminPass);
                    break;
            }
            return info;
        }
    }
}
