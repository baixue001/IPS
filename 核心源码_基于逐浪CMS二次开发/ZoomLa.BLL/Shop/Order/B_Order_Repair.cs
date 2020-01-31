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
    public class B_Order_Repair : ZL_Bll_InterFace<M_Order_Repair>
    {
        public string TbName, PK;
        public M_Order_Repair initMod = new M_Order_Repair();
        public B_Order_Repair()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }

        public int Insert(M_Order_Repair model)
        {
            return DBCenter.Insert(model);
        }

        public DataTable Sel()
        {
            return Sql.Sel(TbName, "", "CDATE DESC");
        }

        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "", "CDate DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable SelAll(int status = -100, int storeid = -100)
        {
            string where = "1=1 ";
            if (status != -100) { where += " AND A.Status=" + status; }
            if (storeid != -100) { where += " AND StoreID=" + storeid; }
            string mtable = "(SELECT A.*,B.StoreID FROM ZL_Order_Repair A LEFT JOIN ZL_OrderInfo B ON A.OrderNo=B.OrderNo)";
            return DBCenter.JoinQuery("A.*,B.Proname,B.Thumbnails", mtable, "ZL_Commodities", "A.ProID=B.ID", where, "A.ID DESC");
        }
        public PageSetting U_SelAll(int cpage, int psize, int uid)
        {
            string where = "A.UserID=" + uid;
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_Commodities", "A." + PK, "A.ProID=B.ID", where, "A.CDate DESC");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateStatuByIDS(string ids, int status)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "UPDATE " + TbName + " SET [Status]=" + status + " WHERE ID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(sql);
        }
        public bool DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(sql);
        }
        public M_Order_Repair SelReturnModel(int ID)
        {
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
        public M_Order_Repair SelByCartID(int cid)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, "WHERE CartID=" + cid))
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
        public bool UpdateByID(M_Order_Repair model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }


        public string ShowReType(object retype)
        {
            switch (DataConvert.CLng(retype))
            {
                case 1:
                    return "上门自取";
                case 2:
                    return "送货到自提点";
                default:
                    return "" + retype.ToString();
            }
        }
        public string ShowStatus(object status)
        {
            switch (DataConvert.CLng(status))
            {
                case 0:
                    return "未审核";
                case 99:
                    return "已审核";
                default:
                    return status.ToString();
            }
        }
        public string ShowServerType(object stype)
        {
            switch (DataConvert.CStr(stype))
            {
                case "drawback":
                    return "退货";
                case "exchange":
                    return "换货";
                case "repair":
                    return "维修";
                default:
                    return stype.ToString();
            }
        }
    }
}
