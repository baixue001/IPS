using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_PageStyle
    {
        public B_PageStyle()
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        private string strTableName ,PK;
        private M_PageStyle initMod = new M_PageStyle();
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, "OrderID ASC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_PageStyle SelReturnModel(int ID)
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
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public bool UpdateByID(M_PageStyle model)
        {
            return DBCenter.UpdateByID(model,model.PageNodeid);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public void DelByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(strTableName, PK, ids);
        }
        public int insert(M_PageStyle model)
        {
           return DBCenter.Insert(model);
        }
        public M_PageStyle GetDefaultStyle()
        {
            string sql = "select top 1 * from ZL_PageStyle where IsDefault=1";
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sql, null))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_PageStyle();
                }
            }
        }
        public bool SetEnableByIds(string ids,int flag)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "Update " + strTableName + " SET IsTrue=" + flag + " WHERE PageNodeid IN ("+ids+")";
            return SqlHelper.ExecuteSql(sql);
        }
    }
}