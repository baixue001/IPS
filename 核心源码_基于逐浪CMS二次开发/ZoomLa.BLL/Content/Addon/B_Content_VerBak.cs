using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Content
{
    public class B_Content_VerBak
    {
        private M_Content_VerBak initMod = new M_Content_VerBak();
        public string TbName = "", PK = "";
        public B_Content_VerBak()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName);
        }
        public M_Content_VerBak SelReturnModel(int ID)
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
        public int Insert(M_Content_VerBak model)
        {
            if (string.IsNullOrEmpty(model.Flow)) { model.Flow = model.GeneralID + DateTime.Now.ToString("yyyyMMddHHmmss") + function.GetRandomString(4, 2); }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Content_VerBak model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public PageSetting SelPage(int cpage, int psize, int gid,string ztype="content")
        {
            string where = " 1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("ztype", ztype) };
            if (gid > 0) { where += " AND GeneralID=" + gid; }
            if (!string.IsNullOrEmpty(ztype)) { where += " AND ZType=@ztype"; }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "ID DESC",sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
