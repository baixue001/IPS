using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Linq;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.Helper;

namespace ZoomLa.BLL
{

    public class B_NodeRole
    {
        public B_NodeRole()
        {
            strTableName = initmod.TbName;
            PK = initmod.PK;
        }
        private string PK, strTableName;
        private M_NodeRole initmod = new M_NodeRole();
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_NodeRole SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
            {
                if (reader.Read())
                {
                    return initmod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public M_NodeRole SelModelByRidAndNid(int nid, int rid)
        {
            return SelReturnModel("WHERE RID=" + rid + " AND NID=" + nid);
        }
        private M_NodeRole SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, strWhere))
            {
                if (reader.Read())
                {
                    return initmod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize, int nodeid = 0, string roleids = "")
        {
            string where = " 1=1";
            if (nodeid > 0) { where += " AND NID=" + nodeid; }
            if (!string.IsNullOrEmpty(roleids)) { SafeSC.CheckIDSEx(roleids); where += " AND RID IN (" + roleids + ")"; }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where);
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_NodeRole model)
        {
            return DBCenter.UpdateByID(model, model.RN_ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, "RN_ID=" + ID);
        }
        public int insert(M_NodeRole model)
        {
            return DBCenter.Insert(model);
        }
        public bool InsertUpdate(M_NodeRole model)
        {
            if (model.RN_ID > 0)
                UpdateByID(model);
            else
                insert(model);
            return true;
        }
        /// <summary>
        /// 返回指定节点和指定角色的权限信息
        /// </summary>
        public DataTable GetSelectNodeANDRid(int Nodeid, string roleIDS)
        {
            SafeSC.CheckIDSEx(roleIDS);
            string sqlstr = "select * from ZL_NodeRole where NID=" + Nodeid + " and RID in (" + roleIDS + ")";
            return SqlHelper.ExecuteTable(CommandType.Text, sqlstr);
        }
        /// <summary>
        /// 查询指定管理员是否拥有节点的某类操作权限
        /// look|modify|addto|comments|columns|state
        /// </summary>
        /// <returns>true:有权限</returns>
        public static bool CheckNodeAuth(M_AdminInfo adminMod, int nid, string auth)
        {
            if (adminMod.IsSuperAdmin()) { return true; }
            if (nid < 1 || string.IsNullOrEmpty(auth)) { return false; }
            string roles = StrHelper.PureIDSForDB(adminMod.RoleList);
            if (string.IsNullOrEmpty(roles)) { return false; }
            //检测权限是否在许可范围之内
            string[] allowAuth = "look|modify|addto|comments|columns|state".Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            auth = allowAuth.FirstOrDefault(p => p.Equals(auth));
            if (string.IsNullOrEmpty(auth)) { return false; }
            //------------
            string where = "Nid=" + nid + " AND RID IN (" + roles + ") ";
            where += " AND " + auth + "=1";
            return DBCenter.SelTop(1, "RN_ID", "*", "ZL_NodeRole", where, "").Rows.Count > 0;
        }
    }
}