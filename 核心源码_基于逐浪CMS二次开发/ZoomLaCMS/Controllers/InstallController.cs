using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class InstallController : Ctrl_Base
    {
        public IActionResult Index()
        {
            if (SiteConfig.SiteInfo.IsInstalled) { return RedirectToAction("Index", "Front"); }
            return View();
        }
        public IActionResult DBScript()
        {
            //type source name uname passwd
            M_APIResult retMod = new M_APIResult(Failed);
            try
            {
                string connStr = GetConnstr();
                DBCenter.DB = SqlBase.CreateHelper("mssql");
                DBCenter.DB.ConnectionString = SiteConfig.SiteInfo.ConnectionString = connStr;
                if (!DataConvert.CBool(GetParam("ignoreSql")))
                {
                    string sqlPath = function.VToP("/App_Data/Data.sql");
                    DBHelper.ExecuteSqlScript(GetConnstr(), sqlPath);
                }
                SiteConfig.Update();
                retMod.retcode = M_APIResult.Success;

            }
            catch (Exception ex)
            {
                retMod.retmsg = ex.Message;
                ZLLog.L("安装时出错,原因:" + ex.Message);
            }
            return Content(retMod.ToString());
        }
        public IActionResult Final()
        {
            M_APIResult retMod = new M_APIResult(Failed);
            B_User buser = new B_User();
            try
            {
                string adminPwd = GetParam("adminPwd").Replace(" ", "");
                string adminPwd2 = GetParam("adminPwd2").Replace(" ", "");
                if (string.IsNullOrEmpty(adminPwd)) { throw new Exception("管理员密码不能为空"); }
                if (!adminPwd.Equals(adminPwd2)) { throw new Exception("管理员确认密码不匹配"); }
                if (adminPwd.Length < 6) { throw new Exception("管理员密码过短"); }

                SiteConfig.SiteInfo.SiteTitle = GetParam("siteTitle");
                SiteConfig.SiteInfo.SiteName = GetParam("siteName");
                SiteConfig.SiteInfo.SiteUrl = GetParam("siteUrl");
                SiteConfig.SiteInfo.WebmasterEmail = GetParam("email");
                SiteConfig.SiteOption.SiteManageCode = "888888";
                SiteConfig.SiteInfo.IsInstalled = true;
                SiteConfig.Update();
                //----------------------------------------------------
                M_AdminInfo adminMod = new M_AdminInfo()
                {
                    AdminName = "admin",
                    AdminPassword = adminPwd
                };
                //---添加用户
                M_UserInfo muser = buser.GetUserByName("admin");
                if (muser.IsNull)
                {
                    muser = new M_UserInfo() { UserName = "admin" };
                    muser.UserPwd = StringHelper.MD5(adminMod.AdminPassword);
                    muser.RegTime = DateTime.Now;
                    muser.RegTime = DateTime.Now;
                    muser.LastLockTime = DateTime.Now;
                    muser.LastLoginTimes = DateTime.Now;
                    muser.LastPwdChangeTime = DateTime.Now;
                    muser.Email = GetParam("email");
                    muser.Question = "admin";
                    muser.Answer = function.GetRandomString(8);
                    muser.GroupID = 1;
                    muser.UserRole = ",1,";
                    muser.SiteID = 1;
                    muser.VIP = 0;
                    muser.LastLoginIP = IPScaner.GetUserIP(HttpContext);
                    muser.CheckNum = function.GetRandomString(6);
                    muser.UserID = buser.Add(muser);
                }
                adminMod.AddUserID = muser.UserID;
                //存在则更新
                ZoomLa.BLL.Install.Add(adminMod);
                retMod.retcode = M_APIResult.Success;
            }
            catch (Exception ex) { retMod.retmsg = ex.Message; }
            return Content(retMod.ToString());
        }
        private string GetConnstr()
        {
            string type = GetParam("type");
            string source = GetParam("source");
            string name = GetParam("name");
            string uname = GetParam("uname");
            string passwd = GetParam("passwd");
            switch (type.ToLower())
            {
                case "local":
                    return @"Data Source=(localdb)\v11.0;Integrated Security=true;AttachDbFileName =" + function.VToP("/App_Data/ZoomlaDB.mdf") + ";";
                default:
                    return string.Format(@"Data Source={0};Initial Catalog={1};User ID={2};Password={3};", source, name, uname, passwd);
            }
        }
    }
}