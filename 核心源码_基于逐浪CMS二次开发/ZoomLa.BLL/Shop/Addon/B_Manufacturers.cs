using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Collections.Generic;
using System.Data.Common;
namespace ZoomLa.BLL
{


    public class B_Manufacturers
    {
        private string TbName, PK;
        private M_Manufacturers initMod = new M_Manufacturers();
        public B_Manufacturers() 
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_Manufacturers SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
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
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public DataTable Sel(string skey)
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(skey))
            {
                where += " AND Producername LIKE @skey ";
                sp.Add(new SqlParameter("skey", "%" + skey + "%"));
            }
            return DBCenter.Sel(TbName, where, PK + " DESC", sp);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_Manufacturers model)
        {
            return DBCenter.UpdateByID(model,model.id);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public int insert(M_Manufacturers model)
        {
            return DBCenter.Insert(model);
        }
        public bool DeleteByID(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DeleteBylist(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sqlStr = "delete from ZL_Manufacturers where (id in (" + ids + "))";
            return SqlHelper.ExecuteSql(sqlStr, null);
        }
    }
}
