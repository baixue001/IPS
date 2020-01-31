using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_SuitPro
    {
        private M_Shop_SuitPro initMod = new M_Shop_SuitPro();
        private string TbName, PK;
        public B_Shop_SuitPro()
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
        public int Insert(M_Shop_SuitPro model)
        {
            return DBCenter.Insert(model);
        }
        public DataTable Sel(string skey ="")
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(skey)) { where += " AND Name LIKE @key"; sp.Add(new SqlParameter("key", "%" + skey + "%")); }
            return DBCenter.JoinQuery("A.*,B.AdminName",TbName, "ZL_Manager","A.AdminID=B.AdminID",where,"",sp.ToArray());
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Shop_SuitPro SelReturnModel(int ID)
        {
            if (ID < 1) {return null; }
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
        public bool UpdateByID(M_Shop_SuitPro model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
    }
}
