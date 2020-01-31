using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ZoomLa.BLL.Helper;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{
    public class B_Structure
    {
        private B_User buser = new B_User();
        private string PK, TbName;
        private M_Structure initMod = new M_Structure();
        private StrHelper strHelp = new StrHelper();
        public B_Structure()
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        public int Insert(M_Structure model)
        {
            return DBCenter.Insert(model);
        }
        public M_Structure SelReturnModel(int id)
        {
            if (id == 0)
            {
                M_Structure model = new M_Structure();
                model.Name = "根结构";
                model.ParentID = 0;
                return model;
            }
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, id))
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
        public M_Structure SelModelByUid(int uid)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, "WHERE UserIDS LIKE '%," + uid + ",%'"))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_Structure();
                }
            }
        }
        public bool Update(M_Structure model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = " 1=1";
            //if (group != -100) { where += " AND [Group]=" + group; }
            //if (status != -100) { where += " AND Status=" + status; }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                sp.Add(new SqlParameter("name", "%" + filter.skey + "%"));
                where += " AND Name LIKE @name";
            }
            if (filter.pid > 0)
            {
                where += " And ParentID=" + filter.pid;
            }
            else { where += " And ParentID=0"; }
            //不倒序
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 组织名是否存在
        /// </summary>
        public bool IsExist(string strName)
        {
            strName = strName.Trim();
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("Name", strName) };
            return DBCenter.IsExist(TbName,"Name=@name",sp);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DelByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return Sql.Del(TbName, "ID in (" + ids + ")");
        }
        public void RemoveByIDS(string ids, int strid)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Update ZL_User Set StructureID=REPLACE(StructureID,'," + strid + ",','') Where UserID in(" + ids + ")";
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql);
        }
        /// <summary>
        /// 为部门添加用户
        /// </summary>
        public string AddUsers(string uids, int structID, string op = "add")
        {
            SafeSC.CheckIDSEx(uids);
            if (structID < 1 || string.IsNullOrEmpty(uids)) return "";
            M_Structure strMod = SelReturnModel(structID);
            if (op.Equals("add"))
            {
                strMod.UserIDS = StrHelper.AddToIDS(strMod.UserIDS, uids.Split(','));
            }
            else
            {
                strMod.UserIDS = StrHelper.RemoveRepeat(strMod.UserIDS.Split(','), uids.Split(','));
            }
            string sql = "UPDATE " + TbName + " SET UserIDS=@uids WHERE ID=" + structID;
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("uids", strMod.UserIDS) };
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql, sp);
            return strMod.UserIDS;
        }
        public string AddManager(string uids, int structID, string op = "add")
        {
            SafeSC.CheckIDSEx(uids);
            if (structID < 1 || string.IsNullOrEmpty(uids)) return "";
            M_Structure strMod = SelReturnModel(structID);
            if (op.Equals("add"))
            {
                strMod.ManagerIDS = StrHelper.AddToIDS(strMod.ManagerIDS, uids.Split(','));
            }
            else
            {
                strMod.ManagerIDS = StrHelper.RemoveRepeat(strMod.ManagerIDS.Split(','), uids.Split(','));
            }
            string sql = "UPDATE " + TbName + " SET ManagerIDS=@uids WHERE ID=" + structID;
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("uids", strMod.ManagerIDS) };
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql, sp);
            return strMod.ManagerIDS;
        }
        #region SELECT
        //获取所有组织结构相关的用户
        public string SelUserIds()
        {
            string sql = "SELECT UserIDS FROM " + TbName;
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            string ids = "";
            foreach (DataRow item in dt.Rows)
            {
                ids += item["UserIDS"] + ",";
            }
            return ids.Trim(',');

        }
        //根据uid查询相关部门
        public DataTable SelByUid(int uid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE ','+UserIDS+',' LIKE '%," + uid + ",%'";
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        /// <summary>
        /// 根据部门IDS返回结构名称字符串
        /// </summary>
        public string SelStrNameByIDS(string ids, string tlp = "[{0}],")
        {
            ids = ids.Trim(',');
            if (string.IsNullOrEmpty(ids)) return "";
            SafeSC.CheckIDSEx(ids);
            string result = "";
            string sql = "SELECT Name FROM " + TbName + " WHERE ID IN(" + ids + ")";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            foreach (DataRow dr in dt.Rows)
            {
                result += string.Format(tlp, dr["Name"]);
            }
            result = result.TrimEnd(',');
            return result;
        }
        /// <summary>
        /// 根据UserID返回所属部门名,默认返回第一个
        /// </summary>
        public string SelNameByUid(int uid, int num = 1)
        {
            string sql = "SELECT TOP " + num + " Name FROM " + TbName + " WHERE UserIDS Like '%," + uid + ",%'";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            string result = "";
            foreach (DataRow dr in dt.Rows)
            {
                result += dr["Name"] + ",";
            }
            return (result.TrimEnd(','));
        }
        #endregion
    }
}
