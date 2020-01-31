using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.AdSystem;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
namespace ZoomLa.BLL.AdSystem
{
    public class B_Logo_Design
    {
        private M_Logo_Design initMod = new M_Logo_Design();
        public string TbName = "", PK = "";
        public B_Logo_Design()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public DataTable Sel(string skey)
        {
            string where = "";
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("skey", "%" + skey + "%") };
            if (!string.IsNullOrEmpty(skey))
            {
                where += " CompName LIKE @skey";
            }
            return DBCenter.Sel(TbName, where, PK + " DESC", sp);
        }
        public M_Logo_Design SelReturnModel(int ID)
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
        public int Insert(M_Logo_Design model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Logo_Design model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize, F_Logo_Design filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.ztype != -100) { where += " AND ZType=" + filter.ztype; }
            if (!string.IsNullOrEmpty(filter.skey)) { where += " AND Alias LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + filter.skey + "%")); }
            if (filter.uid != -100) { where += " AND UserID=" + filter.uid; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
    public class F_Logo_Design
    {
        public int ztype = -100;
        public string skey = "";
        public int uid = -100;
    }
}
