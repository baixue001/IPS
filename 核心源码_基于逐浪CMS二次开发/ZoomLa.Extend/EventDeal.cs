using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Model;
using ZoomLa.SQLDAL;

namespace ZoomLa.Extend
{
    public static class EventDeal
    {
        public static void SubscribeEvent()
        {
            OrderHelper.OrderFinish_DE func = OrderFinish;
            OrderHelper.OrderFinish_Event += func;
        }
        public static void OrderFinish(M_OrderList mod, M_Payment pinfo)
        {
            B_User buser=new B_User();
            DataTable prodt = DBCenter.JoinQuery("A.ProID,A.Pronum,B.PointVal", "ZL_CartPro", "ZL_Commodities", "A.ProID=B.ID", "A.OrderListID=" + mod.id);
            foreach (DataRow dr in prodt.Rows)
            {
                double point = DataConvert.CDouble(dr["PointVal"]) * Convert.ToInt32(dr["Pronum"]);
                if (point > 0)
                {
                    buser.AddMoney(mod.Userid, point, M_UserExpHis.SType.Point, "购买商品[" + dr["ProID"] + "],赠送积分");
                }
            }
        }
    }
}
