using System;
using System.Data;
using System.Configuration;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.Common;
using System.Web;
using System.Globalization;
using System.Data.SqlClient;
using System.Collections.Generic;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    /// <summary>
    /// 管理员角色
    /// </summary>
    public class B_Role
    {
        public B_Role()
        {
            PK = initmod.PK;
            TbName = initmod.TbName;
        }
        private string PK, TbName;
        private M_RoleInfo initmod = new M_RoleInfo();
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName,"","RoleID DESC");
        }
        public M_RoleInfo SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
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
        public PageSetting SelPage(int cpage, int psize,Com_Filter filter)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_RoleInfo model)
        {
            return DBCenter.UpdateByID(model,model.RoleID);
        }
        public bool Del(int ID)
        {
            return DBCenter.DelByIDS(TbName, PK, ID.ToString());
        }
        public int Insert(M_RoleInfo model)
        {
            return DBCenter.Insert(model);
        }
        #region 兼容区
        public static DataTable SelectNodeRoleNode(int nodeid)
        {
            string sqlStr = "select RoleID,RoleName,ZL_NodeRole.* from ZL_Role left outer join ZL_NodeRole on ZL_Role.RoleID=ZL_NodeRole.RID and ZL_NodeRole.nid=@nodeid";
            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@nodeid", SqlDbType.Int, 4);
            parameter[0].Value = nodeid;
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, parameter);
        }
        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <returns></returns>
        public static DataTable GetRoleName()
        {
            string sqlStr = "select RoleID,RoleName from ZL_Role";
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, null);
        }
        /// <summary>
        /// 根据管理员角色ID,获取其所拥有的节点权限
        /// </summary>
        public DataTable SelectNodeRoleName(int rid)
        {
            string sql = "select ZL_node.nodeid,ZL_node.NodeName,ZL_NodeRole.* from ZL_node inner join ZL_NodeRole on ZL_node.nodeid=ZL_NodeRole.nid where ZL_NodeRole.rid=" + rid;
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        public DataTable SelectNodeRoleName()
        {
            string sqlStr = "select ZL_node.nodeid,ZL_NodeRole.* from ZL_node inner join ZL_NodeRole on ZL_node.nodeid=ZL_NodeRole.nid ";
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, null);
        }
        //根据角色名判断角色是否存在
        public static bool IsExit(string roleName)
        {
            string strSql = "SELECT COUNT(*) FROM ZL_Role WHERE RoleName=@RoleName";
            SqlParameter[] cmdParams = new SqlParameter[] { new SqlParameter("@RoleName", SqlDbType.NVarChar, 20) };
            cmdParams[0].Value = roleName;
            return DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, cmdParams)) > 0;
        }
        #endregion
    }
}