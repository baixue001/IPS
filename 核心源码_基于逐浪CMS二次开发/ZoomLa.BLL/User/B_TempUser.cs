using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ZoomLa.BLL.System.Security;
using ZoomLa.Common;
using ZoomLa.Model;

namespace ZoomLa.BLL.User
{
    /*
     * 游客支持类库
     */
    public class B_TempUser
    {
        private string prefix = "TempUser_";
        private B_User buser = null;
        private HttpContext ctx = null;
        public B_TempUser(HttpContext ctx) { this.ctx = ctx;buser = new B_User(ctx); }
        /// <summary>
        /// 如果用户已登录返回用户,否则返回游客记录(无则新建)
        /// </summary>
        public M_UserInfo GetLogin(string uname="游客")
        {
            M_UserInfo mu = buser.GetLogin();
            if (mu.IsNull)
            {
                mu = new M_UserInfo();
                if (string.IsNullOrEmpty(CookieHelper.Get(ctx, prefix + "UserID")))
                {
                    mu.UserName = uname;
                    mu.UserID = C_UserID();
                    mu.WorkNum = function.GetRandomString(10);
                    mu.RegTime = DateTime.Now;
                    C_NewUser(mu.UserID, mu.UserName, mu.WorkNum);
                }
                else
                {
                    mu.UserID = DataConverter.CLng(CookieHelper.Get(ctx,prefix+"UserID"));
                    mu.UserName = HttpUtility.UrlDecode(CookieHelper.Get(ctx, prefix + "LoginName"));
                    mu.WorkNum = CookieHelper.Get(ctx, prefix + "WorkNum");
                    mu.RegTime = DataConverter.CDate(CookieHelper.Get(ctx, prefix + "RegTime"));
                }
                mu.IsTemp = true;
                return mu;//游客
            }
            return mu;//正常已登录用户
        }
        /// <summary>
        /// 创建一个新的游客用户,写入Cookies
        /// </summary>
        public void C_NewUser(int uid,string uname,string worknum) 
        {
            CookieHelper.Set(ctx, prefix + "UserID", uid.ToString());
            CookieHelper.Set(ctx, prefix + "LoginName", uname);
            CookieHelper.Set(ctx, prefix + "WorkNum", worknum);//游客唯一身份认证
            CookieHelper.Set(ctx, prefix + "RegTime", DateTime.Now.ToString());
        }
        private int C_UserID() 
        {
            return new Random().Next(Int32.MinValue, -1);
        }
    }
    
}
