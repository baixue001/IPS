using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.BLL.Helper;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_GroupPro
    {
        private M_Shop_GroupPro initMod = new M_Shop_GroupPro();
        private string TbName, PK;
        public B_Shop_GroupPro()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
        public int Insert(M_Shop_GroupPro model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel(string key = "")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(key)) { where += " AND Name LIKE @key"; sp.Add(new SqlParameter("key", "%" + key + "%")); }
            return DBCenter.Sel(TbName, where, "", sp);
        }
        public PageSetting SelPage(int cpage, int psize,F_Shop_GroupPro filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.key)) { where += " AND Name LIKE @key";
                sp.Add(new SqlParameter("key", "%" + filter.key + "%")); }
            if (filter.StoreID != -100)
            {
                where += " AND StoreID="+filter.StoreID;
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where,"ID DESC",sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Shop_GroupPro SelReturnModel(int ID)
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
        public bool UpdateByID(M_Shop_GroupPro model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void UpdateParent(string ids, int pid)
        {
            DBCenter.UpdateSQL("ZL_Commodities", "ParentID=0", "ID IN (" + ids + ")");
        }
        public void UpdateProParent(M_Shop_GroupPro model)
        {
            DBCenter.UpdateSQL("ZL_Commodities", "ParentID=0", "ParentID=" + model.ID);
            if (!string.IsNullOrEmpty(model.ProIDS))
            {
                DBCenter.UpdateSQL("ZL_Commodities", "ParentID=" + model.ID, "ID IN (" + StrHelper.PureIDSForDB(model.ProIDS) + ")");
            }
        }
    }
    public class F_Shop_GroupPro
    {
        public string key = "";
        public int StoreID = -100;
        public int UserID = -100;

    }
}
