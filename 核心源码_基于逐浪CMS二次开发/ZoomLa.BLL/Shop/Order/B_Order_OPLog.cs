using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.Shop
{
    public class B_Order_OPLog
    {
        private string PK, TbName = "";
        private M_Order_OPLog initMod = new M_Order_OPLog();
        public B_Order_OPLog()
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        public int Insert(M_Order_OPLog model)
        {
            //try
            //{
            //    if (model.AdminID < 1)
            //    {
            //        model.AdminID = B_Admin.GetLogin().AdminId;
            //    }
            //}
            //catch { }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_Order_OPLog model)
        {
            return DBCenter.UpdateByID(model, model.ID);
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
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " ASC");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public M_Order_OPLog SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
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


        //------------------------
        public M_Order_OPLog NewLog(int orderID, string opname, string after = "")
        {
            B_OrderList orderBll = new B_OrderList();
            M_OrderList orderMod = orderBll.SelReturnModel(orderID);
            M_Order_OPLog logMod = new M_Order_OPLog();
            logMod.OPName = opname;
            logMod.OrderNo = orderMod.OrderNo;
            logMod.Before = GetOrderInfo(orderMod);
            logMod.After = after;
            return logMod;
        }
        //public void AddLog(int orderID, string opname, string after, string remind = "")
        //{
        //    B_OrderList orderBll = new B_OrderList();
        //    M_Order_OPLog logMod = new M_Order_OPLog();
        //    M_OrderList orderMod = orderBll.SelReturnModel(orderID);
        //    M_AdminInfo adminMod = B_Admin.GetLogin();
        //    logMod.OrderNo = orderMod.OrderNo;
        //    logMod.AdminID = adminMod.AdminId;
        //    logMod.OPName = opname;
        //    logMod.Remind = remind;
        //    logMod.Before = GetOrderInfo(orderMod);
        //    logMod.After = after;
        //    Insert(logMod);
        //}
        public string GetOrderInfo(M_OrderList orderMod)
        {
            string result = "";
            result = orderMod.OrderStatus + "|" + orderMod.Paymentstatus + "|" + orderMod.PaymentNo + "|" + orderMod.Payment + "|"
                + orderMod.Ordersamount.ToString("F2") + "|" + orderMod.Userid + "|" + orderMod.Freight.ToString("F2");
            return result;
        }
    }
}
