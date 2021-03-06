﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.CreateJS
{
    /*
     * 对模型表的操作,如果拥有较复杂的逻辑,建议三层ORM模型
     */ 
    public class B_CodeModel
    {
        public string TbName = "";
        public B_CodeModel(string tbname)
        {
            this.TbName = tbname;
        }
        public DataRow NewModel()
        {
            return SelStruct().NewRow();
        }
        //返回表结构
        public DataTable SelStruct()
        {
            return SqlHelper.ExecuteTable(CommandType.Text, "SELECT TOP 1 * FROM " + TbName + " WHERE 1=2");
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public DataRow SelByID(int id, string field = "ID")
        {
            string sql = "SELECT Top 1 * FROM " + TbName + " WHERE [" + field + "]=" + id;
            DataTable dt = SqlHelper.ExecuteTable(sql);
            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows[0];
            }
            else { return null; }
        }
        public DataTable SelByField(string field, string value)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("value", value) };
            string sql = "SELECT * FROM " + TbName + " WHERE " + field + "=@value";
            return SqlHelper.ExecuteTable(sql, sp);
        }
        public DataTable SelByUid(int uid)
        {
            return SelByField("UserID",uid.ToString());
        }
        /// <summary>
        /// 条件查询,不需要加where与order,仅用于项目独有页面
        /// </summary>
        public DataTable SelByWhere(string where, string order, SqlParameter[] sp = null, string fields = "*")
        {
            string sql = "SELECT " + fields + " FROM " + TbName + " WHERE " + where + " ORDER BY " + order;
            return SqlHelper.ExecuteTable(sql, sp);
        }
        public int Insert(DataRow dr, string pk = "id")
        {
            return Sql.insertID(TbName, BLLCommon.GetParameters(dr, pk), BLLCommon.GetParas(dr, pk), BLLCommon.GetFields(dr, pk));
        }
        public void UpdateByID(DataRow dr, string pk = "id")
        {
            Sql.UpdateByIDs(TbName, pk,dr[pk].ToString(), BLLCommon.GetFieldAndPara(dr, pk), BLLCommon.GetParameters(dr, pk));
        }
        public void Del(int id, string pk = "id")
        {
            DBCenter.Del(TbName, pk, id);
        }
        public void DelByIDS(string ids, string pk = "id")
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, pk, ids);
        }
        public void DelByWhere(string where, List<SqlParameter> splist)
        {
            DBCenter.DelByWhere(TbName, where, splist);
        }
    }
}
