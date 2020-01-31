using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Sys;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Sys
{
    public class B_EMail_Item
    {
        private M_EMail_Item initMod = new M_EMail_Item();
        public string TbName = "", PK = "";
        public B_EMail_Item()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public DataTable Sel(string title)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "1=1";
            if (!string.IsNullOrEmpty(title))
            {
                sp.Add(new SqlParameter("title", "%" + title + "%"));
                where += " AND Subject LIKE @title";
            }
            return DBCenter.Sel(TbName, where, PK + " DESC");
        }
        public M_EMail_Item SelReturnModel(int ID)
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
        public int Insert(M_EMail_Item model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_EMail_Item model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        } 
    }
}
