using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    //内部申购单
    public class B_Shop_ProOrder
    {
        private M_OrderList initMod = new M_OrderList();
        public string TbName = "ZL_Shop_ProOrder", PK = "";
        public B_Shop_ProOrder()
        {
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return Sel(new Filter_Order());
        }
        public DataTable Sel(Filter_Order filter)
        {
            string where = "1=1 ";
            if (!string.IsNullOrEmpty(filter.storeType)) { where += " AND StoreID=" + Convert.ToInt32(filter.storeType); }
            return DBCenter.Sel(TbName, where, "ID DESC");
        }
        public M_OrderList SelReturnModel(int ID)
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
        public int Insert(M_OrderList model)
        {
            model.TbName = TbName;
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_OrderList model)
        {
            return DBCenter.UpdateByID(model, model.id);
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
        public M_OrderList NewOrder(M_UserInfo mu,int sid)
        {
            M_OrderList orderMod = new M_OrderList();
            orderMod.Userid = mu.UserID;
            orderMod.AddUser = mu.UserName;
            orderMod.Rename = mu.UserName;
            orderMod.Receiver = mu.UserName;
            orderMod.StoreID = sid;
            orderMod.Ordertype = (int)M_OrderList.OrderEnum.Other;
            orderMod.OrderNo = B_OrderList.CreateOrderNo(M_OrderList.OrderEnum.Other);
            return orderMod;
        }
    }
}
