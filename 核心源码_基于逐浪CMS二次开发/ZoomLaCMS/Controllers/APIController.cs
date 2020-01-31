using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class APIController : Ctrl_User
    {
        public int LoginCount
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
        B_User_Friend friendBll = new B_User_Friend();
        B_Group gpBll = new B_Group();
        M_APIResult retMod = new M_APIResult();
        public ContentResult UserCheck()
        {
            M_AJAXUser ajaxUser = new M_AJAXUser();
            string action = GetParam("action");
            retMod.retcode = M_APIResult.Success;
            switch (action)
            {
                case "HasLogged":
                    mu = buser.GetLogin();
                    if (mu != null && !mu.IsNull)
                    {
                        ajaxUser.Copy(mu);
                        return Content(ajaxUser.ToJson());
                    }
                    else { return Content("-1"); }
                case "GetBarUInfo":
                    {
                        int uid = Convert.ToInt32(GetParam("uid"));
                        mu = buser.GetUserByUserID(uid);
                        M_Uinfo ubMod = buser.GetUserBaseByuserid(uid);
                        string result = "{\"UserFace\":\"" + ubMod.UserFace + "\",\"UserExp\":\"" + mu.UserExp + "\",\"UserSex\":\"" + (ubMod.UserSex ? "男" : "女") + "\",\"GroupName\":\"" + gpBll.GetByID(DataConverter.CLng(mu.GroupID)).GroupName + "\",\"UserBirth\":\"" + ubMod.BirthDay + "\",\"RegTime\":\"" + mu.RegTime + "\",\"UserID\":\"" + mu.UserID + "\",\"UserName\":\"" + mu.UserName + "\"}";
                        return Content(result);
                    }
                case "CheckKey":
                    string chkUname = GetParam("uname");
                    M_UserInfo usermod = buser.GetUserByName(chkUname);
                    if (usermod != null && !string.IsNullOrEmpty(usermod.ZnPassword))
                        return Content("1");
                    else
                        return Content("-1");
                case "UserLogin":
                    {
                        string key = GetParam("key");
                        string uname = GetParam("uname");
                        string upwd = GetParam("upwd");
                        mu = buser.AuthenticateUser(uname, upwd);
                        if (mu.IsNull)
                        {
                            retMod.retcode = M_APIResult.Failed; retMod.retmsg = "登录失败,用户名或密码错误";
                        }
                        else
                        {
                            ajaxUser.Copy(mu);
                            retMod.result = ajaxUser.ToJson();
                        }
                        return Content(retMod.ToString());
                    }
                    break;
                case "GetUser"://用于远程登录等,返回基本用户信息
                    {
                        string uname = RequestEx["uname"];
                        string upwd = RequestEx["upwd"];//未加密的
                        mu = buser.AuthenticateUser(uname, upwd);
                        if (mu.IsNull)
                        {
                            retMod.retcode = M_APIResult.Failed; retMod.retmsg = "用户不存在";
                        }
                        else
                        {
                            ajaxUser.Copy(mu);
                            retMod.retmsg = ajaxUser.ToJson();
                        }
                        return Content(retMod.ToString());
                    }
                    break;
                case "ExistEmail":
                    {
                        string email = RequestEx["email"];
                        if (buser.IsExistMail(email)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "邮箱已存在!"; }
                        return Content(retMod.ToString());
                    }
                    break;
                case "ExistUName":
                    {
                        string uname = RequestEx["uname"];
                        if (buser.IsExistUName(uname)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "用户名已存在"; }
                        return Content(retMod.ToString());
                    }
                    break;
                case "ExistMobile":
                    {
                        string mobile = RequestEx["mobile"];
                        if (buser.IsExist("mobile", mobile)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "手机号已存在"; }
                        return Content(retMod.ToString());
                    }
                    break;
                case "exist_ue"://检测用户名与邮箱(选填)
                    {
                        string email = RequestEx["email"];
                        string uname = RequestEx["uname"];
                        if (buser.IsExistUName(uname)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "用户名已存在"; }
                        if (!string.IsNullOrEmpty(email))
                        {
                            if (buser.IsExistMail(email)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "邮箱已存在!"; }
                        }
                        return Content(retMod.ToString());
                    }
                case "exist_um"://用户名与手机号(选填)
                    {
                        string uname = RequestEx["uname"];
                        string mobile = RequestEx["mobile"];
                        if (buser.IsExistUName(uname)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "用户名已存在"; }
                        if (!string.IsNullOrEmpty(mobile))
                        {
                            if (buser.IsExist("mobile", mobile)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "手机号已存在"; }
                        }
                        return Content(retMod.ToString());
                    }
                case "exist_ume":
                    {
                        string uname = RequestEx["uname"];
                        if (buser.IsExist("ume", uname)) { retMod.retcode = M_APIResult.Failed; retMod.retmsg = "用户名已存在"; }
                        return Content(retMod.ToString());
                    }
                case "spwd":
                    {
                        retMod.retcode = M_APIResult.Failed;
                        mu = buser.GetLogin(false);
                        string spwd = RequestEx["spwd"];
                        if (string.IsNullOrEmpty(mu.PayPassWord)) { retMod.retmsg = "用户未设置支付密码,验证失败"; }
                        else if (!mu.PayPassWord.Equals(StringHelper.MD5(spwd))) { retMod.retmsg = "支付密码错误"; }
                        else if (mu.PayPassWord.Equals(StringHelper.MD5(spwd)))
                        {
                            retMod.retcode = M_APIResult.Success;
                        }
                        else
                        {
                            retMod.retmsg = "支付密码错误";
                        }
                        return Content(retMod.ToString());
                    }
                case "Login":
                default://Login
                    #region -1登录失败,-2验证码失败,-10启用验证码
                    {
                        string value = RequestEx["value"];
                        string uname = value.Split(':')[0], upwd = value.Split(':')[1];
                        //兼容以前旧版未加密请求
                        if (upwd.Length > 10) { upwd = ZoomlaSecurityCenter.SiteDecrypt(upwd); }
                        if (LoginCount >= 3)//验证码
                        {
                            var key = value.Split(':')[2]; var code = value.Split(':')[3];
                            if (key.StartsWith("{"))//新验证码
                            {
                                int start = value.IndexOf("{");
                                int end = value.LastIndexOf("}");
                                string json = value.Substring(start, end - start + 1);
                                if (!VerifyHelper.Check(json)) { return Content("-2");  }
                            }
                            else if (!ZoomlaSecurityCenter.VCodeCheck(key, code))
                            {
                                return Content("-2");
                            }
                        }
                        mu = buser.AuthenticateUser(uname, upwd);
                        if (mu == null || mu.IsNull)
                        {
                            LoginCount++;
                            if (LoginCount >= 3)
                            {
                                return Content("-10");
                            }
                            else
                            {
                                return Content("-1");
                            }
                        }
                        else
                        {
                            LoginCount = 0;
                            buser.SetLoginState(mu, "Day");
                            ajaxUser.Copy(mu);
                            return Content(ajaxUser.ToJson());
                        }
                    }
                    #endregion
                    break;
            }
        }
    }
}