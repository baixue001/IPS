using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using ZoomLa.BLL.Content;

namespace ZoomLa.BLL
{
 
    public class B_Permission
    {
        private string TbName, PK;
        private M_Permission initMod = new M_Permission();
        public B_Permission()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_Permission SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public DataTable SelByRole(string rname)
        {
            string sql = "Select * From " + TbName + " Where RoleName Like @rname";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("rname", "%" + rname + "%") };
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public DataTable Select_All()
        {
            return Sql.Sel(TbName);
        }
        public DataTable SelByUserGrop(string usergrop)
        {
            string sql = "SELECT Perlist,Precedence FROM " + TbName + " WHERE UserGroup LIKE @usergroup AND IsTrue=1";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@usergroup", "%" + usergrop + "%") };
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public PageSetting SelPage(int cpage, int psize,Com_Filter filter)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool GetUpdate(M_Permission model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public void DelByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName,PK,ids);
        }
        public int GetInsert(M_Permission model)
        {
            return DBCenter.Insert(model);
        }
        public bool InsertUpdate(M_Permission model)
        {
            if (model.ID > 0)
                GetUpdate(model);
            else
                GetInsert(model);
            return true;
        }
        /// <summary>
        /// 传入1,2,3，返回角色名
        /// </summary>
        public string GetRoleNameByIDs(string ids)
        {
            ids = ids ?? "";
            ids = ids.Trim(',');
            string result = "";
            if (string.IsNullOrEmpty(ids))return result;
            SafeSC.CheckIDSEx(ids);
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "Select RoleName From " + TbName + " Where ID in(" + ids + ")");
            foreach (DataRow dr in dt.Rows)
            {
                result += dr["RoleName"] as string + ",";
            }
            result = result.TrimEnd(',');
            return result;
        }
        //----------------------OA
        /// <summary>
        /// 根据用户权限datatable,判断权限
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="authdt">用权限限DT</param>
        /// <returns>True:有权限</returns>
        public bool CheckAuth(DataTable authdt,string auth)
        {
            if (authdt == null || authdt.Rows.Count < 1 || string.IsNullOrEmpty(auth)) return false;
            foreach (DataRow row in authdt.Rows)
            {
                string[] rolestr = row["Auth_OA"].ToString().Split(',');
                foreach (var str in rolestr)
                {
                    if (str.ToLower().Equals(auth.ToLower()))
                        return true;
                }
                
            }
            return false;
        }
        public bool CheckAuth(string rids, string auth)
        {
            DataTable authdt = SelAuthByRoles(rids);
            return CheckAuth(authdt, auth);
        }
        /// <summary>
        /// 是否包含允许角色
        /// </summary>
        /// <param name="authrids">已授权的用户角色IDS</param>
        /// <param name="myrids">用户所拥有的角色IDS</param>
        /// <returns>True通过</returns>
        public bool ContainRole(string authrids, string myrids)
        {
            if (string.IsNullOrEmpty(authrids)) return true;
            if (string.IsNullOrEmpty(myrids)) return false;
            string[] ridArr = myrids.Trim(',').Split(',');
            foreach (string rid in ridArr)
            {
                if (authrids.Contains("," + rid + ",")) { return true; }
            }
            return false;
        }
        /// <summary>
        /// 根据用户角色IDS,返回用户的权限表
        /// </summary>
        /// <param name="rids"></param>
        /// <returns></returns>
        public DataTable SelAuthByRoles(string rids) 
        {
            rids = rids.Trim(',').Replace(",,", ",");
            if (string.IsNullOrEmpty(rids)) return null;
            SafeSC.CheckDataEx(rids);
            string sql = "SELECT * FROM "+TbName+" WHERE ID IN ("+rids+")";
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        //-------------------用户权限验证
        /// <summary>
        /// 对当前登录用户验证权限
        /// </summary>
        /// <param name="auth">需要验证的权限</param>
        /// <returns>True:通过</returns>
        public static void CheckAuthEx(string auth)
        {
            //M_UserInfo mu = new B_User().GetLogin();
            //if (!new B_Permission().CheckAuth(mu.UserRole, auth)) 
            //{
            //    throw new Exception("你当前没有访问该页面的权限");
            //}
        }
        /// <summary>
        /// 检测当前登录用户是否有指定权限
        /// </summary>
        /// <returns>True:拥有</returns>
        public static bool CheckAuth(string auth)
        {
            //M_UserInfo mu = new B_User().GetLogin();
            //return new B_Permission().CheckAuth(mu.UserRole, auth);
            return false;
        }
    }
}