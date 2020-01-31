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
    public class B_Order_Invoice
    {
        private M_Order_Invoice initMod = new M_Order_Invoice();
        public string TbName = "", PK = "";
        public B_Order_Invoice()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        /// <summary>
        /// 选取最近几次的不重复的发票信息(根据Head)
        /// </summary>
        /// <returns></returns>
        public DataTable SelForTop(int uid)
        {
            string childSql = "SELECT max(a.ID) FROM ZL_Order_Invoice A LEFT JOIN ZL_Orderinfo B ON A.ORDERID = B.ID WHERE B.UserID=" + uid + " GROUP BY A.InvHead, b.Userid";
            string sql = "SELECT TOP 5 * FROM ZL_Order_Invoice WHERE ID IN (" + childSql + ")";
            DataTable dt = DBCenter.ExecuteTable(sql);
            return dt;
        }
        public M_Order_Invoice SelReturnModel(int ID)
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
        public M_Order_Invoice SelModelByOid(int oid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "OrderID", oid))
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
        public int Insert(M_Order_Invoice model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Order_Invoice model)
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
