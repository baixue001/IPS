using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL;
using ZoomLa.BLL.Plat;
using ZoomLa.Model;

namespace ZoomLaCMS.Ctrl
{
    public class Ctrl_User : Ctrl_Base
    {
        private B_User _buser = null;
        private M_UserInfo _mu = null;
        public B_User buser
        {
            get
            {
                if (_buser == null) { _buser = new B_User(HttpContext); }
                return _buser;
            }
        }
        public M_UserInfo mu
        {
            get
            {
                if (_mu == null) { ViewBag.mu = _mu = buser.GetLogin(); }
                return _mu;
            }
            set { _mu = value; }
        }
    }
    public class Ctrl_Plat : Ctrl_User
    {
        public B_User_Plat upBll = new B_User_Plat();
        private M_User_Plat _upMod = null;
        public M_User_Plat upMod
        {
            get
            {
                if (_upMod == null) { ViewBag.upMod = _upMod = upBll.SelReturnModel(mu.UserID); }
                return _upMod;
            }
            set { _upMod = value; }
        }

        /// <summary>
        /// 是否为 能力中心--用户管理员
        /// </summary>
        /// <returns></returns>
        public bool IsAdmin()//是否为用户管理员
        {
            string rid = "," + B_Plat_UserRole.SelSuperByCid(upMod.CompID) + ",";
            return upMod.Plat_Role.Contains(rid);
        }
        /// <summary>
        /// 检测当前登录用户是否有对应的权限
        /// </summary>
        public bool AuthCheck(string authname)
        {
            if (IsAdmin())
            { return true; }
            else
            {
                B_Plat_UserRole urBll = new B_Plat_UserRole();
                return urBll.AuthCheck(upMod.Plat_Role, authname);
            }
        }
    }
}
