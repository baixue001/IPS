using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Data;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_Ask
    {
        public B_Ask()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        private string PK, strTableName;
        private M_Ask initMod = new M_Ask();
        public M_Ask SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, ID))
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
        public int getnum()
        {
            return DBCenter.Count(strTableName,"");
        }
        //[s]
        public int IsExistInt(string strWhere)
        {
            return DBCenter.Count(strTableName, strWhere);
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        public DataTable Sel(string strWhere, string strOrderby, SqlParameter[] sp)
        {
            List<SqlParameter> splist = new List<SqlParameter>();
            if (sp != null) { splist.AddRange(sp); }
            return DBCenter.Sel(strTableName, strWhere, strOrderby, splist);
        }
        public bool UpdateByID(M_Ask model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public void UpdateByField(string field, string value, int mid)
        {
            UpdateByField(field, value, mid.ToString());
        }
        public void UpdateByField(string field, string value, string ids)
        {
            SafeSC.CheckDataEx(field);
            SafeSC.CheckIDSEx(ids);
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("value", value) };
            DBCenter.UpdateSQL(strTableName, field + " = @value", PK + " IN (" + ids + ")", sp);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(strTableName,PK, ID);
        }
        public int insert(M_Ask model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable SelfieldOrd(string strField, int num)
        {
            string sql = "select top " + num + " " + strField + " from " + strTableName + " Group By " + strField + " Order By " + strField;
            return DBCenter.DB.ExecuteTable(new SqlModel(sql, null));
        }
        public DataTable SelAll()
        {
            string sql = "SELECT * FROM " + strTableName + " WHERE Status=1 ORDER BY AddTime DESC";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
        }
        public DataTable SelWaitQuest()
        {
            string sql = "SELECT * FROM " + strTableName + " A WHERE (SELECT COUNT(*) FROM ZL_GuestAnswer WHERE QueId=A.ID)=0 AND Status=1 ORDER BY AddTime DESC";
            return DBCenter.DB.ExecuteTable(new SqlModel(sql, null));
        }
        public PageSetting SelWaitQuest_SPage(int cpage, int psize, string quetype = "", int stype = 1, string skey = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            switch (stype)
            {
                case 1: where += " AND Status <> 2";break;
                case 2:where += " AND Status = 2";break;
            }
            if (!string.IsNullOrEmpty(quetype))
            {
                where += " AND QueType = @quetype";
                sp.Add(new SqlParameter("quetype", quetype));
            }
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND (Qcontent like @key or Supplyment like @key or QueType like @key)";
                sp.Add(new SqlParameter("key", "%" + skey + "%"));
            }
            string fields = "*,(SELECT COUNT(*) FROM ZL_Favorite WHERE InfoID=A.ID) Favorite,(SELECT COUNT(*) FROM ZL_GuestAnswer WHERE QueID=A.ID) Answer";
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "AddTime DESC", sp, fields);
            DBCenter.SelPage(setting);
            return setting;
        }

        public PageSetting SelMyAsk_SPage(int cpage, int psize, int userID, string quetype = "")
        {
            string where = "UserID=" + userID;
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(quetype))
            {
                where += " AND QueType LIKE @quetype";
                sp.Add(new SqlParameter("quetype", quetype));
            }
            string fields = "*,(SELECT COUNT(*) FROM ZL_Favorite WHERE InfoID=A.ID) Favorite,(SELECT COUNT(*) FROM ZL_GuestAnswer WHERE QueID=A.ID) Answer";
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "AddTime DESC", sp, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
        //----------------------------------------------------------------------------------------------------------
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = " 1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.type != "-100") { where += " AND A.QueType=" + DataConvert.CLng(filter.type); }
            if (!string.IsNullOrEmpty(filter.status)) { SafeSC.CheckIDSEx(filter.status); where += " AND A.Status IN (" + filter.status + ")"; }
            if (!string.IsNullOrEmpty(filter.uids)) { SafeSC.CheckIDSEx(filter.uids); where += " AND A.UserID IN (" + filter.uids + ")"; }
            if (!string.IsNullOrEmpty(filter.skey)) { where += " AND A.Qcontent LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + filter.skey + "%")); }
            if (!string.IsNullOrEmpty(filter.ids)) { SafeSC.CheckIDSEx(filter.ids); where += " AND A.ID IN (" + filter.ids+")"; }
            PageSetting setting = PageSetting.Double(cpage, psize, strTableName,"ZL_Grade","A.ID","A.QueType=B.GradeID",
                where, "ID DESC", sp,"A.*,B.GradeName");
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 根据用户id获得问题数量
        /// </summary>
        public int GetAskCount(int uid)
        {
            return DBCenter.Count(strTableName, "UserID=" + uid);
        }
        /// <summary>
        /// 获得前5名问答积分最多的用户
        /// </summary>
        public DataTable GetTopUser()
        {
            return DBCenter.SelTop(5, "UserID", "*", "ZL_User", "", "GuestScore DESC");
        }
        /// <summary>
        ///根据问题类型统计数量
        /// </summary>
        public int GetCountByQueType(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return 0; }
            SafeSC.CheckIDSEx(ids);
            string sql = "SELECT COUNT(*) FROM " + strTableName + " WHERE QueType IN (" + ids + ")";
            return DataConvert.CLng(DBCenter.DB.ExecuteTable(new SqlModel(sql,null)).Rows[0][0]);
        }
        /// <summary>
        /// 根据问题状态统计数量
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public int GetCountByStatus(int status)
        {
            string sql = "SELECT * FROM " + strTableName + " WHERE Status=" + status;
            DataTable dt = DBCenter.DB.ExecuteTable(new SqlModel(sql,null));
            if (dt.Rows.Count <= 0) { return 0; }
            return DataConvert.CLng(dt.Rows[0][0]);
        }
    }
}

