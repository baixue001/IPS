using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.Safe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using ZoomLa.BLL.System.Security;
using ZoomLa.BLL.Helper;

namespace ZoomLa.BLL
{
    public class B_Admin 
    {
        private M_AdminInfo initMod = new M_AdminInfo();
        private const string tbName = "ZL_Manager";
        private const string PK = "AdminID";
        private const string prefix = "Admin_";
        public bool CheckSPwd(M_AdminInfo admin, string pwd)
        {
            if (string.IsNullOrEmpty(pwd)) return false;
            return admin.RandNumber.Equals(pwd);
        }
        public static void SetLoginState(HttpContext ctx,M_AdminInfo model)
        {
            CookieHelper.Set(ctx, prefix + "name", model.AdminName);
            CookieHelper.Set(ctx, prefix + "pwd", model.AdminPassword);
            ctx.Session.SetString(prefix + "name", model.AdminName);
        }
        public static void ClearLogin(HttpContext ctx)
        {
            ctx.Session.Remove(prefix + "name");
            foreach (var cookie in ctx.Request.Cookies)
            {
                ctx.Response.Cookies.Delete(cookie.Key);
            }
        }
        /// <summary>
        /// admin must safe exit ervery time or cookie live just in brower
        /// </summary>
        public static M_AdminInfo GetLogin(HttpContext ctx)
        {
            string sname = "";
            try
            {
                sname = ctx.Session.GetString(prefix + "name");
            }
            catch { }
            if (string.IsNullOrEmpty(sname))
            {
                //从Cookies中重读
                string name = CookieHelper.Get(ctx, prefix + "name");
                string pwd = CookieHelper.Get(ctx, prefix + "pwd");
                if (StrHelper.StrNullCheck(name, pwd)) { return null; }
                return B_Admin.AuthenticateAdmin(name, pwd);
            }
            else
            {
                return GetAdminByAdminName(sname);
            }
        }

        public static int Add(M_AdminInfo model)
        {
            model.AdminName = model.AdminName.Replace(" ", "");
            if (!SafeSC.CheckUName(model.AdminName)) { throw new Exception("用户名含有非法字符!!"); }
            return DBCenter.Insert(model);
        }
        public static bool Update(M_AdminInfo model)
        {
            DBCenter.UpdateByID(model, model.AdminId);
            return true;
        }
        /// <summary>
        /// 不可删除超管
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public static bool DelAdminById(int adminId)
        {
            if (adminId <= 1) { return false; }
            DBCenter.Del(tbName, PK, adminId); return true;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(initMod.TbName);
        }
        public PageSetting SelPage(int cpage, int psize, F_Admin filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.adminName))
            {
                where += " AND AdminName LIKE @name";
                sp.Add(new SqlParameter("name", "%" + filter.adminName + "%"));
            }
            if (filter.rid != -100)
            {
                where += " AND AdminRole LIKE '%," + filter.rid + ",%'";
            }
            if (filter.isLock != -100)
            {
                where += " AND IsLock=" + filter.isLock;
            }
            PageSetting setting = PageSetting.Single(cpage, psize, tbName, PK, where, filter.Order, sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable SelByIds(string ids, int islock = -1)
        {
            SafeSC.CheckDataEx(ids);
            string where = "AdminID IN (" + ids + ")";
            where += islock > -1 ? " AND IsLock=" + islock : "";
            return DBCenter.Sel(tbName, where, "CDATE DESC");
        }
        public void UpdatePwdByIDS(string ids, string pwd)
        {
            pwd = StringHelper.MD5(pwd);
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("pwd", pwd) };
            DBCenter.UpdateSQL(tbName, "AdminPassword=@pwd", "AdminID IN(" + ids + ") AND AdminID!=1", sp);
        }
        public static bool IsExist(string adminName)
        {
            adminName = adminName.Replace(" ","");
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("AdminName", adminName) };
            return DBCenter.Sel(tbName, "AdminName=@AdminName", "", sp).Rows.Count > 0;
        }
        public static bool IsExist(int adminID)
        {
            return DBCenter.Sel(tbName, "AdminID=" + adminID).Rows.Count > 0;
        }
        /// <summary>
        /// 根据管理员名和密码
        /// </summary>
        /// <param name="loginName"></param>
        /// <param name="password">MD5后的密码</param>
        /// <returns></returns>
        private M_AdminInfo GetLoginAdmin(string loginName, string password)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("AdminName", loginName), new SqlParameter("AdminPass", password) };
            using (DbDataReader reader = DBCenter.SelReturnReader(tbName, "AdminName=@AdminName AND AdminPassword=@AdminPass", sp))
            {
                if (reader.Read())
                    return new M_AdminInfo().GetModelFromReader(reader);
                else
                    return null;
            }
        }
        public static M_AdminInfo GetAdminByID(int adminId)
        {
            return GetAdminByAdminId(adminId);
        }
        //-------
        public static M_AdminInfo GetAdminByAdminId(int adminId)
        {
            if (adminId <= 0) { return null; }
            using (DbDataReader reader = DBCenter.SelReturnReader(tbName, PK, adminId))
            {
                if (reader.Read())
                {
                    return new M_AdminInfo().GetModelFromReader(reader);
                }
                else
                { return null; }
            }
        }
        public static M_AdminInfo GetAdminByAdminName(string adminName)
        {
            if (string.IsNullOrEmpty(adminName)) { return null; }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("adminName", adminName) };
            using (DbDataReader reader = DBCenter.SelReturnReader(tbName, "adminName=@adminName", sp))
            {
                if (reader.Read())
                {
                    return new M_AdminInfo().GetModelFromReader(reader);
                }
                else
                { return null; }
            }
        }
        /// <summary>
        /// 如非超管,则跳转
        /// </summary>
        public static void IsSuperManage(M_AdminInfo model)
        {
            if (model == null || model.AdminId < 1 || !model.IsSuperAdmin())
            {
                throw new Exception("非超级管理员，无权访问该页面");
            }
        }
        public static M_AdminInfo AuthenticateAdmin(string AdminName, string Password)
        {
            if (string.IsNullOrEmpty(AdminName) || string.IsNullOrEmpty(Password)) { return null; }
            M_AdminInfo adminMod = null;
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("AdminName", AdminName), new SqlParameter("AdminPassword", StringHelper.MD5(Password)) };
            using (DbDataReader reader = DBCenter.SelReturnReader(tbName, "AdminName=@AdminName AND AdminPassword=@AdminPassword", sp))
            {
                if (reader.Read())
                {
                    adminMod = new M_AdminInfo().GetModelFromReader(reader);
                }
                else
                { return null; }
            }
            //adminMod.LastLoginIP = IPScaner.GetUserIP();
            adminMod.LastLoginTime = DateTime.Now;
            adminMod.LoginTimes++;
            Update(adminMod);
            return adminMod;
        }
        /// <summary>
        /// 仅系统调用
        /// </summary>
        public static void UpdateField(string field, string value, int adminID)
        {
            DBCenter.UpdateSQL(tbName, field + "=@value", "AdminID=" + adminID, null);
        }
        /// <summary>
        /// 更改管理员锁定状态(不包含超管)
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="islock">true:1锁定</param>
        /// <returns></returns>
        public bool LockAdmin(string ids, bool islock)
        {
            SafeSC.CheckIDSEx(ids);
            int isLocked = islock ? 1 : 0;
            DBCenter.UpdateSQL(tbName, "IsLock=" + isLocked, "AdminID IN(" + ids + ") AND AdminID!=1", null);
            return true;
        }
        /// <summary>
        /// 获取当前登录用户在节点中所拥有的权限列表，(ZL_NodeRole)RID:角色,NID:节点,look:查看
        /// </summary>
        /// <returns></returns>
        public DataTable GetNodeAuthList(M_AdminInfo m, string auth = "")
        {
            SafeSC.CheckDataEx(auth);
            if (m == null || m.AdminId < 1 || string.IsNullOrEmpty(m.RoleList.Replace(",", ""))) return null;
            string where = "Rid in (" + m.RoleList.Trim(',') + ")";
            if (!string.IsNullOrEmpty(auth))
            {
                where += " And " + auth + " =1";
            }
            return DBCenter.Sel("ZL_NodeRole", where);
        }
    }
    public class F_Admin
    {
        //角色IDS
        public int rid = -100;
        //排序条件
        private string[] orderArr = "AdminID,CDate,LoginTimes,LastLogoutTime".ToLower().Split(',');
        public string orderField = "adminid";
        public string orderMethod = "asc";
        public string Order
        {
            get
            {
                if (string.IsNullOrEmpty(orderField)) { return ""; }
                if (orderArr.FirstOrDefault(p => p.Equals(orderField)) == null) { return ""; }
                switch (orderMethod.ToLower().Replace(" ", ""))
                {
                    case "desc":
                        orderMethod = "desc";
                        break;
                    default:
                    case "asc":
                        orderMethod = "asc";
                        break;
                }
                return orderField + " " + orderMethod;
            }
        }
        //管理员名称模糊查询
        public string adminName = "";
        public int isLock = -100;
    }
}