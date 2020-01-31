using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;

using System.Collections.Generic;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.Helper;

namespace ZoomLa.BLL
{


    public class B_Spec
    {
        private string strTableName,PK;
        private M_Spec initMod = new M_Spec();
        public B_Spec() 
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable SelPage(Com_Filter filter)
        {
            return SelPage(1, int.MaxValue, filter).dt;
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string fields = "*";
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.storeId != -100) { where += " AND SpecCate=" + filter.storeId; }
            if (filter.pid != -100)
            {
                where += " AND Pid=" + filter.pid;
            }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                where += " AND SpecName LIKE @name";
                sp.Add(new SqlParameter("name", "%" + filter.skey + "%"));
            }
            if (filter.type != "-100")
            {
                where += " AND SpecCate=" + DataConvert.CLng(filter.type);
            }
            if (filter.mode == "list")
            {
                fields = "A.*,(Select Count(*) From ZL_Special Where Pid=A.SpecID) as ChildCount";
            }
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "OrderID", sp, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdatePidByIDS(string ids,int id)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "UPDATE " + strTableName + " SET Pid=" + id + " WHERE SpecID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(sql);
        }
        private string GetLevelStr(string pre, string preChar, int level)
        {
            string str = "";
            for (int i = 1; i < level; i++)
            {
                str += pre;
            }
            return (str += preChar);
        }
        public M_Spec SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
        private M_Spec SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, strWhere))
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
            return Sql.Sel(strTableName,"","OrderID");
        }
        public bool UpdateByID(M_Spec model)
        {
            return DBCenter.UpdateByID(model, model.SpecID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public bool DelBySpecID(int id)
        {
            string sql = "Delete From "+strTableName+" Where Specid="+id;
            return SqlHelper.ExecuteSql(sql);
        }
        public int insert(M_Spec model)
        {
            return DBCenter.Insert(model);
        }
        public void AddSpec(M_Spec model)
        {
            if (model.SpecID > 0)
                UpdateByID(model);
            else
                insert(model);
        }
        public void UpdateSpec(M_Spec model)
        {
            if (model.SpecID > 0)
                UpdateByID(model);
            else
                insert(model);
        }
        public void DelSpec(int ID)
        {
            Sql.Del(strTableName, PK + "=" + ID);
        }
        public int GetFirstID()
        {
            string strSql = "Select Top 1 SpecID from ZL_Special Order by OrderID ASC";
            return DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, null));
        }
        public M_Spec GetSpec(int SpecID)
        {
            SqlParameter[] cmdParams = new SqlParameter[] { new SqlParameter("@SpecID", SqlDbType.Int, 4) };
            cmdParams[0].Value = SpecID;
            string strSql = "select * from ZL_Special where SpecID=@SpecID";
            using (DbDataReader drd = SqlHelper.ExecuteReader(CommandType.Text, strSql, cmdParams))
            {
                if (drd.Read())
                {
                    return initMod.GetModelFromReader(drd);
                }
                else
                {
                    return new M_Spec(true);
                }
            }
        }
        /// <summary>
        /// 主用于生成发布
        /// </summary>
        public DataTable SelAsNode()
        {
            string sql = "SELECT (SELECT Count(SpecID) From " + strTableName + " WHERE A.SpecID=Pid) ChildCount,SpecID AS NodeID,SpecName AS NodeName,SpecDir AS NodeDir,NodeType=10 FROM " + strTableName + " A ORDER BY OrderID";
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }
        /// <summary>
        /// 查询专题名是否存在
        /// </summary>
        /// <param name="SpecName"></param>
        /// <returns></returns>
        public DataTable GetIsSpecName(string SpecName)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("SpecName", SpecName) };
            string strSql = "select * from ZL_Special where SpecName=@SpecName";
            return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
        }
        public DataTable GetIsSpecDir(string specdir)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("specdir",specdir) };
            string sql = "SELECT * FROM ZL_Special WHERE SpecDir=@specdir";
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public DataTable GetSpecContent(int SpecID, string Order, string Conditions)
        {
            SafeSC.CheckDataEx(Order, Conditions);
            string strSql = string.Format("select a.* from ZL_CommonModel a where a.SpecialID Like @SpecID order by {0} {1}", Order, Conditions);
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("@SpecID","%,"+SpecID+",%")
            };
            return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
        }
        public DataTable SelByIDS(string ids)
        {
            ids = StrHelper.PureIDSForDB(ids);
            if (!string.IsNullOrEmpty(ids))
            {
                SafeSC.CheckIDSEx(ids);
                ids = ids.TrimEnd(new char[] { ',' });
                string sql = "Select * From " + strTableName + " Where SpecID IN (" + ids + ")";
                return SqlHelper.ExecuteTable(CommandType.Text, sql);
            }
            return new DataTable();
        }
        //-------------------------------------------
        public void AddToSpec(string ids, int specid,int cate=0)
        {
            SqlParameter[] sp = new SqlParameter[] {new SqlParameter("specid",","+specid+",") };
            SafeSC.CheckIDSEx(ids);
            string sql = "";
            if (cate == 0)
            {
                sql = "UPDATE ZL_CommonModel SET SpecialID = REPLACE(REPLACE(SpecialID,@specid,','),',,',',')+@specid WHERE GeneralID IN(" + ids + ")";
            }
            else
            {
                DBCenter.UpdateSQL("ZL_Commodities","SpecialID=''","SpecialID IS NULL"); 
                sql = "UPDATE ZL_Commodities SET SpecialID = REPLACE(REPLACE(SpecialID,@specid,','),',,',',')+@specid WHERE ID IN(" + ids + ")";
            }
            SqlHelper.ExecuteSql(sql,sp);
        }
        public int GetNextOrderID(int pid)
        {
            string sql = "SELECT TOP 1 OrderID FROM "+strTableName+" WHERE Pid="+pid+" ORDER BY OrderID DESC";
            DataTable dt = SqlHelper.ExecuteTable(sql);
            if (dt.Rows.Count > 0) { int orderid = DataConvert.CLng(dt.Rows[0]["OrderID"]);return orderid + 1; }
            return 1;
        }
        public void ChangeColumnToNode(DataTable dt)
        {
            dt.Columns["SpecID"].ColumnName = "NodeID";
            dt.Columns["Pid"].ColumnName = "ParentID";
            dt.Columns["SpecName"].ColumnName = "NodeName";
        }
    }
}