using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;
using System.Collections.Generic;
namespace ZoomLa.BLL
{

    public class B_Promotion
    {
        private M_Promotion initMod = new M_Promotion();
        private string TbName, PK;
        public B_Promotion()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public int Insert(M_Promotion model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel(string key = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(key)) { where += " AND PromoName LIKE @key"; sp.Add(new SqlParameter("key", "%" + key + "%")); }
            return DBCenter.Sel(TbName, where, "", sp);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Promotion SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
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
        public bool UpdateByID(M_Promotion model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }

    }
}
