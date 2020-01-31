using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ZoomLa.AppCode.SMS;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Plat;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Other;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class ChangeController : Ctrl_User
    {
        RegexHelper regHelp = new RegexHelper();
        M_Uinfo basemu = new M_Uinfo();
        M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
        B_Safe_Mobile mobileBll = new B_Safe_Mobile();
        #region 修改Email|手机
        private string CheckNum { get { return HttpContext.Session.GetString("Mail_CheckNum"); } set { HttpContext.Session.SetString("Mail_CheckNum",value); } }
        private string NewCheckNum { get { return HttpContext.Session.GetString("Mail_NewCheckNum"); } set { HttpContext.Session.SetString("Mail_NewCheckNum", value); } }
        private int Step { get { return DataConverter.CLng(HttpContext.Session.GetInt32("Mail_Step")); } set { HttpContext.Session.SetInt32("Mail_Step",value); ViewBag.step  = value; } }
        //private string NewMobile { get { return ViewBag["Mail_NewMobile"] as string; } set { ViewBag["Mail_NewMobile"] = value; } }
        public IActionResult Mobile()
        {
            basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.mu = mu;
            ViewBag.basemu = basemu;
            if (SiteConfig.UserConfig.UserMobilAuth.Equals("1"))//可直接修改手机号
            {
                return View("MobileFree");
            }
            if (string.IsNullOrEmpty(basemu.Mobile))//刷新step
            {
                Step = 2;
            }
            else { Step = 1; }
            return View();
        }
        public IActionResult Mobile_1(string CheckNum_T)
        {
            
            basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.mu = mu;
            ViewBag.basemu = basemu;
            if (string.IsNullOrEmpty(CheckNum)) { ShowAlert("校验码不存在,请重新发送校验码"); }
            else if (!CheckNum_T.Equals(CheckNum)) { ShowAlert("校验码不匹配"); }
            else
            {
                ShowInfo("<span style='color:green;'>原手机号校验成功,请输入您的新手机号</span>");
                Step = 2;
            }
            return View("Mobile");
        }
        public IActionResult Mobile_2()
        {
            
            string mobile = RequestEx["NewMobile_T"];
            string key = RequestEx["NewVCode_hid"];
            string vcode = RequestEx["NewVCode"];
            string checknum = RequestEx["NewCheckNum_T"];
            basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.newmoblie = mobile;
            Step = 2;
            if (string.IsNullOrEmpty(NewCheckNum)) { ShowAlert("校验码不存在,请重新发送校验码"); return View("Mobile"); }
            else if (!checknum.Equals(NewCheckNum)) { ShowAlert("校验码不匹配"); return View("Mobile"); }
            else if (buser.IsExist("ume", mobile)) { ShowAlert("该手机号已存在"); return View("Mobile"); }
            else if (!RegexHelper.IsMobilPhone(mobile)) { ShowAlert("手机格式不正确"); return View("Mobile"); }
            else
            {
                basemu.Mobile = mobile;
                buser.UpdateBase(basemu);
            }
            return WriteOK("修改手机号成功", "/User/Info/UserInfo"); 
        }
        //直接修改手机号,需要后端配置
        public IActionResult Mobile_Free()
        {
            if (!SiteConfig.UserConfig.UserMobilAuth.Equals("1")) { return WriteErr("未允许自由修改手机号"); }
            string page = "MobileFree";
            string mobile = RequestEx["NewMobile_T"];
            basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.mu = mu;
            ViewBag.basemu = basemu;
            if (buser.IsExist("ume", mobile)) { ShowAlert("该手机号已存在"); return View(page); }
            else if (!RegexHelper.IsMobilPhone(mobile)) { ShowAlert("手机格式不正确"); return View(page); }
            else
            {
                basemu = buser.GetUserBaseByuserid(mu.UserID);
                basemu.Mobile = mobile;
                buser.UpdateBase(basemu);
            }
            return WriteOK("修改手机号成功", "/User/Info/UserInfo"); 
        }
        public IActionResult Answer() {  return View(); }
        public IActionResult Answer_Submit(string quest, string answer, string newanswer)
        {
            
            if (string.IsNullOrEmpty(newanswer) || string.IsNullOrEmpty(answer)) { return WriteErr("安全问题与答案不能为空");  }
            else if (!mu.Question.Equals(quest)) { return WriteErr("安全问题不正确");  }
            else if (!mu.Answer.Equals(answer)) { return WriteErr("问题答案不正确");  }
            else
            {
                mu.Answer = newanswer;
                buser.UpdateByID(mu);
                return WriteOK("修改安全问题成功", "/User/Info/"); 
            }
        }
        #region 修改邮箱
        public IActionResult Email()
        {
            
            ViewBag.email = mu.Email;
            if (mu.Email.Contains("@random")) //随机生成的则可直接改
            {
                Step = 2;
            }
            else { Step = 1; }
            return View();
        }
        public IActionResult Email_1()
        {
            string checknum = RequestEx["checknum"];
            if (string.IsNullOrEmpty(CheckNum)) { ShowMsg("验证码不存在,请重新发送验证码", "danger"); }
            else if (!checknum.Equals(CheckNum)) { ShowMsg("验证码不匹配", "danger"); }
            else
            {
                Step = 2;
                ShowMsg("请填入您的新邮箱,并完成验证", "info");
            }
            ViewBag.email = mu.Email;
            return View("Email");
        }
        public IActionResult Email_2()
        {
            string newchknum = RequestEx["newchecknum"];
            string newEmail = RequestEx["newemail"];
            ViewBag.newemail = newEmail;
            Step = 2;
            ViewBag.email = mu.Email;
            if (string.IsNullOrEmpty(NewCheckNum)) { ShowMsg("验证码不存在,请重新发送验证码", "danger"); return View("Email"); }
            if (!newchknum.Equals(NewCheckNum)) { ShowMsg("验证码不匹配", "danger"); return View("Email"); }
            if (string.IsNullOrEmpty(newEmail)) { ShowMsg("邮箱不能为空","danger"); return View("Email"); }
            if (buser.IsExistMail(newEmail)) { ShowMsg("该邮箱已存在", "danger"); return View("Email"); }
            if (!RegexHelper.IsEmail(newEmail)) { ShowMsg("邮箱格式不正确", "danger"); return View("Email"); }
            mu.Email = newEmail;
            buser.UpdateByID(mu);
            return WriteOK("修改邮箱成功", "/User/Info/UserInfo"); 
        }
        public IActionResult Email_SendEmail()
        {
            CheckNum = function.GetRandomString(8).ToLower();
            string mailcontent = "您好，您正在<a href='" + SiteConfig.SiteInfo.SiteUrl + "'>" + SiteConfig.SiteInfo.SiteName + "</a>网站修改邮箱，您本次的验证码为：" + CheckNum;
            MailInfo mailInfo = SendMail.GetMailInfo(mu.Email, SiteConfig.SiteInfo.SiteName, "修改邮箱[" + SiteConfig.SiteInfo.SiteName + "]", mailcontent);
            SendMail.Send(mailInfo);
            ShowMsg("注册验证码已成功发送到你的注册邮箱,<a href='" + B_Plat_Common.GetMailSite(mu.Email) + "' target='_blank'>请前往邮箱查收并验证</a>!", "info");
            ViewBag.email = mu.Email;
            return View("Email");
        }
        public IActionResult Email_SendNewEmail()
        {
            string newEmail = RequestEx["newemail"];
            ViewBag.newemail = newEmail;
            Step = 2;
            if (string.IsNullOrEmpty(newEmail)) { ShowMsg("邮箱不能为空", "danger"); return View("Email"); }
            if (buser.IsExistMail(newEmail)) { ShowMsg("该邮箱已存在", "danger"); return View("Email"); }
            NewCheckNum = function.GetRandomString(8).ToLower();
            string mailcontent = "您好，您正在<a href='" + SiteConfig.SiteInfo.SiteUrl + "'>" + SiteConfig.SiteInfo.SiteName + "</a>网站修改邮箱，您本次的验证码为：" + NewCheckNum;
            MailInfo mailInfo = SendMail.GetMailInfo(newEmail, SiteConfig.SiteInfo.SiteName, "修改邮箱[" + SiteConfig.SiteInfo.SiteName + "]", mailcontent);
            SendMail.Send(mailInfo);
            ShowMsg("注册验证码已成功发送到你的注册邮箱,<a href='" + B_Plat_Common.GetMailSite(newEmail) + "' target='_blank'>请前往邮箱查收并验证</a>!", "info");
            return View("Email");
        }
        public void ShowMsg(string msg, string type)
        {
            ViewBag.msg = msg;
            ViewBag.msgtype = type;
        }
        #endregion

        private void ShowAlert(string msg)
        {
            ViewBag.info = msg;
        }
        private void ShowInfo(string msg)
        {
            ViewBag.info = msg;
        }
        //发送手机验证码(步骤1或步骤2的)
        public string SendValidCode(string key, string vcode, string mobile)
        {
            
            B_Safe_Mobile mbBll = new B_Safe_Mobile();
            basemu = buser.GetUserBaseByuserid(mu.UserID);
            CheckNum = ""; NewCheckNum = "";
            switch (Step)
            {
                case 2:
                    NewCheckNum = function.GetRandomString(6, 2).ToLower();
                    basemu.Mobile = mobile;
                    break;
                default:
                    CheckNum = function.GetRandomString(6, 2).ToLower();
                    break;
            }
            if (!VerifyHelper.Check(RequestEx["VCode_hid"]))
            {
                retMod.retmsg = "验证码不正确";
            }
            else
            {
                if (mbBll.CheckMobile(HttpContext, basemu.Mobile))
                {
                    string content = "你正在使用修改手机号服务,校验码:" + CheckNum + NewCheckNum;

                    M_Message messInfo = new M_Message();
                    messInfo.Sender = basemu.UserId;
                    messInfo.Title = "验证码:修改手机号[" + basemu.Mobile + "]";
                    messInfo.Content = content;
                    messInfo.MsgType = 2;
                    messInfo.status = 1;
                    messInfo.Incept = basemu.UserId.ToString();
                    new B_Message().GetInsert(messInfo);
                    retMod.retcode = M_APIResult.Success;
                    retMod.retmsg = "校验码已成功发送到你的手机";
                }
                else { retMod.retmsg = "禁止向该号码发送短信,请联系管理员"; }
            }
            return retMod.ToString();
        }
        #endregion
        #region 问答|邮件|短信 找回密码
        public string UserMobile { get {return HttpContext.Session.GetString("GetPwd_UserMobile"); } set { HttpContext.Session.SetString("GetPwd_UserMobile", value); } }
        public string GetPwdStep { get { return HttpContext.Session.GetString("GetPwdStep"); } set { ViewBag.step = value;HttpContext.Session.SetString("GetPwdStep",value); } }
        public string GetPwdUName { get { return HttpContext.Session.GetString("GetPwdUName"); } set { HttpContext.Session.SetString("GetPwdUName",value); } }
        //1,能过问答,邮件|手机短信,验证用户身份
        //2,验证通过显示Final_Div,让其填入新密码
        //3,新密码生效,自动进入用户中心
        public IActionResult GetPassword()
        {
            //密码找回方式
            string Method = (RequestEx["method"] ?? "").ToLower();
            int Uid = DataConverter.CLng(RequestEx["uid"]);
            string Key = (RequestEx["key"] ?? "");
            switch (Method)
            {
                case "answer":
                    GetPwdStep = "answer";
                    break;
                default:
                    GetPwdStep = "email";
                    break;
            }
            if (!string.IsNullOrEmpty(Key) && Uid > 0)//通过邮件校验找回
            {
                M_UserInfo mu = buser.SelReturnModel(Uid);
                if (mu.IsNull) { return WriteErr("用户不存在");  }
                if (string.IsNullOrEmpty(mu.seturl)) { return WriteErr("用户未发起找回密码");  }
                if (!mu.seturl.Equals(Key)) { return WriteErr("key值不正确");  }
                GetPwdStep = "final";
            }
            return View();
        }
        public IActionResult GetPassword_Answer()
        {
            string answer = RequestEx["Answer_T"];
            M_UserInfo mu = GetUserByName(RequestEx["TxtUserName"]);
            if (string.IsNullOrEmpty(mu.Answer) || string.IsNullOrEmpty(mu.Question)) { return WriteErr("用户未设置问答内容,无法通过问答找回");  }
            if (mu.Answer.Equals(answer))
            {
                GetPwdStep = "final";
            }
            else
            {
                return WriteErr("密码提示答案不正确"); 
            }
            return View("GetPassWord");
        }
        public IActionResult GetPassWord_Mobile()
        {
            if (!VerifyHelper.Check(RequestEx["VCode_hid"]))
            {
                return WriteErr("验证码不正确", "/User/Change/GetPassword"); 
            }
            M_UserInfo mu = GetUserByName(RequestEx["TxtUserName"]);
            M_Uinfo basemu = buser.GetUserBaseByuserid(mu.UserID);
            if (string.IsNullOrEmpty(basemu.Mobile)) { return WriteErr("用户未设置手机号,无法通过手机号找回");  }
            string code = function.GetRandomString(6, 2);
            string mobile = basemu.Mobile;
            if (mobileBll.CheckMobile(HttpContext,mobile))
            {
                CommonReturn ret = SMS_Helper.SendVCode(mobile, code, SMSConfig.Instance.Tlp_GetBack);
                //添加一条发送手机短信记录
                mobileBll.Insert(new M_Safe_Mobile() { Phone = mobile, VCode = code, Source = "GetPassWord_Mobile", UserID = mu.UserID, UserName = mu.UserName, SysRemind = ret.err });
            }
            else
            {
                return WriteErr("短信发送次数超过上限!"); 
            }
            UserMobile = mobile;
            GetPwdUName = mu.UserName;
            GetPwdStep = "mobile_code";
            return View("GetPassWord");
        }
        //验证手机校验码,返回密码修改页
        public IActionResult GetPassword_Mobile_Code()
        {
            string code = RequestEx["CheckCode_T"];
            CommonReturn retMod = B_Safe_Mobile.CheckVaildCode(UserMobile, code, "");
            if (retMod.isok)
            {
                GetPwdStep = "final";
                return View("GetPassWord");
            }
            else
            {
                return WriteErr(retMod.err); 
            }
        }
        public IActionResult GetPassWord_Email()
        {
            B_MailManage mailBll = new B_MailManage();
            if (!VerifyHelper.Check(RequestEx["VCode_Hid"]))
            {
                return WriteErr("验证码不正确", "/User/GetPassword"); 
            }
            M_UserInfo mu = GetUserByName(RequestEx["TxtUserName"]);
            if (string.IsNullOrEmpty(mu.Email) || mu.Email.Contains("@random")) { return WriteErr("用户未设置邮箱,无法通过邮箱找回");  }
            //生成Email验证链接
            string seturl = function.GetRandomString(12) + "," + DateTime.Now.ToString();
            mu.seturl = EncryptHelper.AESEncrypt(seturl);
            buser.UpDateUser(mu);
            //Email发送
            string url = SiteConfig.SiteInfo.SiteUrl + "/User/Change/GetPassWord?key=" + mu.seturl + "&uid=" + mu.UserID;
            string returnurl = "<a href=\"" + url + "\" target=\"_blank\">" + url + "</a>";
            string content = mailBll.SelByType(B_MailManage.MailType.RetrievePWD);
            content = new OrderCommon().TlpDeal(content, GetPwdEmailDt(mu.UserName, SiteConfig.SiteInfo.SiteName, returnurl));
            MailInfo mailInfo = SendMail.GetMailInfo(mu.Email, SiteConfig.SiteInfo.SiteName, SiteConfig.SiteInfo.SiteName + "_找回密码", content);
            SendMail.Send(mailInfo);
            //不需要更新步骤,其从邮箱进入地址栏后再更新
            string emailUrl = B_Plat_Common.GetMailSite(mu.Email);
            return WriteOK("密码重设请求提交成功,<a href='" + emailUrl + "' target='_blank'>请前往邮箱查收</a>!!"); 
        }
        public IActionResult GetPassWord_Final()
        {
            if (!GetPwdStep.Equals("final")) { return WriteErr("你无权访问该页面");  }
            string newpwd = RequestEx["TxtPassword"];
            string cnewpwd = RequestEx["TxtConfirmPassword"];
            int Uid = DataConverter.CLng(RequestEx["uid"]);
            M_UserInfo mu = buser.GetUserByName(GetPwdUName);
            if (Uid > 0) { mu = buser.SelReturnModel(Uid); }
            if (mu.IsNull) { return WriteErr("[" + GetPwdUName + "]用户不存在");  }
            if (!newpwd.Equals(cnewpwd)) { return WriteErr("两次输入密码不一致");  }
            mu.UserPwd = StringHelper.MD5(cnewpwd);
            mu.seturl = "";
            buser.UpDateUser(mu);
            return WriteOK("密码修改成功!", "/User/");
        }
        private M_UserInfo GetUserByName(string uname)
        {
            GetPwdUName = (uname ?? "").Trim();
            if (string.IsNullOrEmpty(GetPwdUName)) { throw new Exception("用户名不能为空"); }
            M_UserInfo mu = buser.GetUserByName(GetPwdUName);
            if (mu.IsNull) { throw new Exception("[" + GetPwdUName + "]用户不存在"); }
            return mu;
        }
        private DataTable GetPwdEmailDt(string username, string sitename, string returnurl)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("UserName");
            dt.Columns.Add("SiteName");
            dt.Columns.Add("ReturnUrl");
            dt.Rows.Add(dt.NewRow());
            dt.Rows[0]["UserName"] = username;
            dt.Rows[0]["SiteName"] = sitename;
            dt.Rows[0]["ReturnUrl"] = returnurl;
            return dt;
        }
        #endregion
        //-----------修改密码
        public IActionResult Pwd() {  return View(); }
        public IActionResult Pwd_Edit()
        {
            string oldPwd = StringHelper.MD5(RequestEx["TxtOldPassword"]);
            string newPwd = RequestEx["TxtPassword"];
            string cnewPwd = RequestEx["TxtPassword2"];
            if (!mu.UserPwd.Equals(oldPwd)) { return WriteErr("原密码错误,请重新输入");  }
            if (StrHelper.StrNullCheck(newPwd, cnewPwd)) { return WriteErr("新密码与确认密码不能为空");  }
            if (!newPwd.Equals(cnewPwd)) { return WriteErr("新密码与确认密码不匹配");  }
            if (newPwd.Length < 6) { return WriteErr("密码最少需要6位");  }
            mu.UserPwd = StringHelper.MD5(newPwd);
            buser.UpdateByID(mu);
            buser.ClearCookie();
            return WriteOK("修改密码成功,请重新登录", "/User/"); 
        }
    }
}
