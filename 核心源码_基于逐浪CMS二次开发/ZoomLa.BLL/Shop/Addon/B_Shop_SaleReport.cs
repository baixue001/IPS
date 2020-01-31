using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Shop
{
    public class B_Shop_SaleReport
    {
        #region old 
        /// <summary>
        /// 0为全部
        /// </summary>
        public string Html_Date(string type, string value, bool hasall = false)
        {
            string yearTlp = "<button type=\"button\"  class='btn btn-default {0} filter_year' data-num='{1}'>{1}年</a>";
            string monthTlp = "<button type=\"button\" class='btn btn-default {0} filter_month' data-num='{1}'>{1}月</a>";
            string html = "";
            switch (type)
            {
                case "year":
                    for (int i = 0; i <= 11; i++)
                    {
                        bool ischk = false;
                        int val = (DateTime.Now.Year - i);
                        if (value == val.ToString()) { ischk = true; }
                        html += string.Format(yearTlp, ischk ? "active" : "", val);
                    }
                    if (hasall) { html = "<button type=\"button\"  class='btn btn-default " + (value == "0" ? "active" : "") + " filter_year' data-num='0'>全部</a>" + html; }
                    break;
                case "month":
                    for (int i = 1; i <= 12; i++)
                    {
                        bool ischk = false;
                        if (value == i.ToString()) { ischk = true; }
                        html += string.Format(monthTlp, ischk ? "active" : "", i);
                    }
                    if (hasall) { html = "<button type=\"button\" class='btn btn-default " + (value == "0" ? "active" : "") + " filter_month' data-num='0'>全部</a>" + html; }
                    break;
            }
            return html;
        }
        /// <summary>
        /// 返回店铺销售汇总
        /// </summary>
        /// <param name="sid"></param>
        /// <returns></returns>
        public DataRow GetSummary(F_Shop_SaleReport filter)
        {
            string fields = "Count(ID) OrderNum,SUM(Receivablesamount) OrderTotal";
            string where = "1=1 ";
            if (filter.StoreID != -100) { where += " AND StoreID=" + filter.StoreID; }
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.SDate))
            {
                where += " AND PayTime>=@stime";
                sp.Add(new SqlParameter("stime", Convert.ToDateTime(filter.SDate).ToString("yyyy/MM/dd 00:00:00")));
            }
            if (!string.IsNullOrEmpty(filter.EDate))
            {
                where += " AND PayTime<=@etime";
                sp.Add(new SqlParameter("etime", Convert.ToDateTime(filter.EDate).ToString("yyyy/MM/dd 23:59:59")));
            }
            DataTable dt = DBCenter.SelWithField("ZL_Order_PayedView", fields, where, "", sp);
            dt.Columns.Add("KDJ", typeof(string));
            DataRow dr = dt.Rows[0];
            dr["OrderTotal"] = DataConvert.CDouble(dr["OrderTotal"]).ToString("F2");
            dr["KDJ"] = (DataConvert.CDouble(dr["OrderTotal"]) / DataConvert.CLng(dr["OrderNum"])).ToString("F2");
            return dr;
        }
        public DataTable GetSalesByProduct(F_Shop_SaleReport filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "(PaymentNO IS NOT NULL AND PaymentNO!='') AND OrderStatus=99 ";
            if (!string.IsNullOrEmpty(filter.SDate))
            {
                where += " AND PayTime>=@stime";
                sp.Add(new SqlParameter("stime", Convert.ToDateTime(filter.SDate).ToString("yyyy/MM/dd 00:00:00")));
            }
            if (!string.IsNullOrEmpty(filter.EDate))
            {
                where += " AND PayTime<=@etime";
                sp.Add(new SqlParameter("etime", Convert.ToDateTime(filter.EDate).ToString("yyyy/MM/dd 23:59:59")));
            }
            if (!string.IsNullOrEmpty(filter.NodeIDS))
            {
                SafeSC.CheckIDSEx(filter.NodeIDS);
                where += " AND NodeID IN (" + filter.NodeIDS + ")";
            }
            if (!string.IsNullOrEmpty(filter.ProName))
            {
                where += " AND ProName LIKE @proname";
                sp.Add(new SqlParameter("proname", "%" + filter.ProName + "%"));
            }
            string mtable = "(SELECT SUM(AllMoney)AS AllMoney,SUM(Pronum)AS Pronum,ProID,ProName,Nodeid FROM ZL_Order_ProSaleView  WHERE " + where + " GROUP BY ProID,ProName,Nodeid)";
            return DBCenter.JoinQuery("A.*,B.NodeName", mtable, "ZL_Node", "A.NodeID=B.NodeID", "", "AllMoney DESC", sp.ToArray());
        }
        public DataTable GetSalesByClass(F_Shop_SaleReport filter)
        {
            string where = "(PaymentNO IS NOT NULL AND PaymentNO!='') AND OrderStatus=99 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.SDate))
            {
                where += " AND PayTime>=@stime";
                sp.Add(new SqlParameter("stime", Convert.ToDateTime(filter.SDate).ToString("yyyy/MM/dd 00:00:00")));
            }
            if (!string.IsNullOrEmpty(filter.EDate))
            {
                where += " AND PayTime<=@etime";
                sp.Add(new SqlParameter("etime", Convert.ToDateTime(filter.EDate).ToString("yyyy/MM/dd 23:59:59")));
            }
            //return DBCenter.JoinQuery(field,mtable,"ZL_Order_PayedView","A.Orderlistid=B.ID",where," ALLMoney DESC");
            return DBCenter.SelWithField("ZL_Order_ProSaleView", "SUM(AllMoney)AS AllMoney,NodeID,NodeName", where + " GROUP BY NodeID,NodeName", "ALLMoney DESC", sp);
        }
        public DataTable GetSaleByDay(F_Shop_SaleReport filter)
        {
            B_OrderList orderBll = new B_OrderList();
            if (string.IsNullOrEmpty(filter.SDate)) { filter.SDate = DateTime.Now.AddMonths(-2).ToString("yyyy/MM/dd"); }
            if (string.IsNullOrEmpty(filter.EDate)) { filter.EDate = DateTime.Now.ToString("yyyy/MM/dd"); }
            DateTime STime = DataConvert.CDate(filter.SDate), ETime = DataConvert.CDate(filter.EDate);
            DataTable saleDT = orderBll.Report_SelByDate(STime, ETime, filter.StoreID);
            DataTable dayDT = new DataTable();
            dayDT.Columns.Add("date", typeof(string));
            dayDT.Columns.Add("total", typeof(double));
            dayDT.Columns.Add("pay_online", typeof(double));
            dayDT.Columns.Add("pay_purse", typeof(double));
            for (DateTime s = STime; s <= ETime; s = s.AddDays(1))
            {
                DataRow day = dayDT.NewRow();
                //DateTime sdate = Convert.ToDateTime("{0}/{1}/{2} 00:00:00");
                string sdate = s.ToString("#yyyy/MM/dd 00:00:00#"), edate = s.ToString("#yyyy/MM/dd 23:59:59#");
                saleDT.DefaultView.RowFilter = "PayTime>= " + sdate + " AND PayTime<= " + edate;
                day["date"] = s.ToString("yyyy-MM-dd");
                day["total"] = 0;
                day["pay_online"] = 0;
                day["pay_purse"] = 0;
                foreach (DataRow dr in saleDT.DefaultView.ToTable().Rows)
                {
                    day["Total"] = DataConvert.CDouble(day["Total"]) + DataConvert.CDouble(dr["OrdersAmount"]);
                    if (Convert.ToInt32(dr["PayPlatID"]) == 0) { day["pay_purse"] = DataConvert.CDouble(day["pay_purse"]) + DataConvert.CDouble(dr["OrdersAmount"]); }
                    else { day["pay_online"] = DataConvert.CDouble(day["pay_online"]) + DataConvert.CDouble(dr["OrdersAmount"]); }
                }
                dayDT.Rows.Add(day);
            }
            return dayDT;
        }
        #endregion
        /*
         * 1,ZL_Order_ProSaleView:已付款商品信息表
         * 2,ZL_Order_PayedView  :已付款订单表
         */
        /// <summary>
        /// 根据条件,返回符合条件的已支付订单
        /// </summary>
        public static DataTable SelPayment(F_Order_Sale filter)
        {
            string stime = filter.stime, etime = filter.etime;
            List<SqlParameter> sp = new List<SqlParameter>();
            string where = "";
            switch (filter.fast)
            {
                case "payed":
                    where = "(A.PaymentNO IS NOT NULL AND A.PaymentNO!='') AND A.PaymentStatus=1 ";
                    break;
                default:
                    where = "1=1 ";
                    break;
            }
            DateTime time = DateTime.Now;
            if (!string.IsNullOrEmpty(stime) && DateTime.TryParse(stime, out time))
            {
                where += " AND PayTime>=@stime";
                sp.Add(new SqlParameter("stime", time));
            }
            if (!string.IsNullOrEmpty(etime) && DateTime.TryParse(etime, out time))
            {
                where += " AND PayTime<@etime";
                sp.Add(new SqlParameter("etime", time));
            }
            if (!string.IsNullOrEmpty(filter.storeIds) && !filter.storeIds.Equals("-100"))
            {
                SafeSC.CheckIDSEx(filter.storeIds);
                where += " AND A.StoreID IN (" + filter.storeIds + ")";
            }
            if (!string.IsNullOrEmpty(filter.orderType))
            {
                SafeSC.CheckIDSEx(filter.orderType);
                where += " AND A.OrderType IN (" + filter.orderType + ")";
            }
            //不含重复的支付单
            //string childSql = "SELECT PaymentID FROM ZL_OrderInfo A LEFT JOIN ZL_Payment B ON A.PaymentNo=B.PayNO WHERE " + where + "";
            //DataTable payDT = DBCenter.Sel("ZL_Payment", "PaymentID IN (" + childSql + ")", "", sp);
            //return payDT;
            DataTable payDT = DBCenter.Sel("ZL_Order_PayedView", where, "", sp);
            return payDT;
        }
        /// <summary>
        /// 查询详细销售情况,并按分类统计
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static DataTable SelByClass(F_Order_Sale filter)
        {
            string where = "(PaymentNO IS NOT NULL AND PaymentNO!='') AND OrderStatus>=0 ";
            DateTime time = DateTime.Now;
            if (DateTime.TryParse(filter.stime, out time))
            {
                where += " AND PayTime>='" + time.ToString("yyyy/MM/dd 00:00:00") + "'";
            }
            if (DateTime.TryParse(filter.etime, out time))
            {
                where += " AND PayTime<='" + time.ToString("yyyy/MM/dd 23:59:59") + "'";
            }
            return DBCenter.SelWithField("ZL_Order_ProSaleView", "SUM(AllMoney)AS AllMoney,NodeID,NodeName", where + " GROUP BY NodeID,NodeName", "ALLMoney DESC");
        }
        public static DataTable SelByProduct(F_Order_Sale filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>();
            //已付款的订单,不包含退货
            string where = "(PaymentNO IS NOT NULL AND PaymentNO!='') AND PaymentStatus=1";
            if (!string.IsNullOrEmpty(filter.stime))
            {
                where += " AND PayTime>=@stime";
                sp.Add(new SqlParameter("stime", Convert.ToDateTime(filter.stime).ToString("yyyy/MM/dd 00:00:00")));
            }
            if (!string.IsNullOrEmpty(filter.etime))
            {
                where += " AND PayTime<=@etime";
                sp.Add(new SqlParameter("etime", Convert.ToDateTime(filter.etime).ToString("yyyy/MM/dd 23:59:59")));
            }
            if (!string.IsNullOrEmpty(filter.nodeIds)) { SafeSC.CheckIDSEx(filter.storeIds); where += " AND NodeID IN (" + filter.nodeIds + ")"; }
            if (!string.IsNullOrEmpty(filter.storeIds)) { SafeSC.CheckIDSEx(filter.storeIds); where += " AND UserShopID IN (" + filter.storeIds + ")"; }
            string mtable = "(SELECT SUM(AllMoney)AS AllMoney,SUM(Pronum)AS Pronum,ProID,ProName,Nodeid FROM ZL_Order_ProSaleView  WHERE " + where + " GROUP BY ProID,ProName,Nodeid)";
            DataTable saleDT = DBCenter.JoinQuery("A.*,B.NodeName", mtable, "ZL_Node", "A.NodeID=B.NodeID", "", "", sp.ToArray());
            return saleDT;
        }
        public static DataTable SumMoneyByPlat(F_Order_Sale filter)
        {
            DataTable payDT = SelPayment(filter);
            return SumMoneyByPlat(payDT);
        }
        /// <summary>
        /// 根据支付方式统计
        /// </summary>
        public static DataTable SumMoneyByPlat(DataTable payDT)
        {
            DataTable rdt = new DataTable();
            //订单金额,订单实收,支付单实收,收款方式
            //rdt.Columns.Add(new DataColumn("money_order", typeof(string)));
            //rdt.Columns.Add(new DataColumn("money_order_rece",typeof(string)));
            rdt.Columns.Add(new DataColumn("money_all", typeof(string)));
            rdt.Columns.Add(new DataColumn("money_purse", typeof(string)));
            rdt.Columns.Add(new DataColumn("money_online", typeof(string)));
            //客单数|客单价
            rdt.Columns.Add(new DataColumn("kd_num", typeof(string)));
            rdt.Columns.Add(new DataColumn("kd_price", typeof(string)));
            DataRow rdr = rdt.NewRow();
            double money_all = 0, money_online = 0, money_purse = 0;
            foreach (DataRow dr in payDT.Rows)
            {
                int platID = Convert.ToInt32(dr["PayPlatID"]);
                double money = Convert.ToDouble(dr["MoneyTrue"]);
                money_all += money;
                if (platID == 0) { money_purse += money; }
                else { money_online += money; }
            }
            //rdr["money_order"] = 0;
            //rdr["money_order_rece"] = 0;
            rdr["money_all"] = money_all.ToString("F2");
            rdr["money_purse"] = money_purse.ToString("F2");
            rdr["money_online"] = money_online.ToString("F2");
            rdr["kd_num"] = payDT.Rows.Count;
            rdr["kd_price"] = (money_all / payDT.Rows.Count).ToString("F2");
            rdt.Rows.Add(rdr);
            return rdt;
        }
        /// <summary>
        /// 按天统计销售数据
        /// </summary>
        public static DataTable SumByDay(DataTable saleDT,string stime,string etime)
        {
            DataTable dayDT = new DataTable();
            dayDT.Columns.Add("date", typeof(string));
            dayDT.Columns.Add("total", typeof(double));
            dayDT.Columns.Add("pay_online", typeof(double));
            dayDT.Columns.Add("pay_purse", typeof(double));
            DateTime STime = Convert.ToDateTime(stime);
            DateTime ETime = Convert.ToDateTime(etime);
            for (DateTime s = STime; s <= ETime; s = s.AddDays(1))
            {
                DataRow day = dayDT.NewRow();
                string sdate = s.ToString("#yyyy/MM/dd 00:00:00#"), edate = s.ToString("#yyyy/MM/dd 23:59:59#");
                saleDT.DefaultView.RowFilter = "PayTime>= " + sdate + " AND PayTime<= " + edate;
                day["date"] = s.ToString("yyyy-MM-dd");
                day["total"] = 0;
                day["pay_online"] = 0;
                day["pay_purse"] = 0;
                foreach (DataRow dr in saleDT.DefaultView.ToTable().Rows)
                {
                    day["Total"] = DataConvert.CDouble(day["Total"]) + DataConvert.CDouble(dr["MoneyTrue"]);
                    if (Convert.ToInt32(dr["PayPlatID"]) == 0) { day["pay_purse"] = DataConvert.CDouble(day["pay_purse"]) + DataConvert.CDouble(dr["MoneyTrue"]); }
                    else { day["pay_online"] = DataConvert.CDouble(day["pay_online"]) + DataConvert.CDouble(dr["MoneyTrue"]); }
                }
                dayDT.Rows.Add(day);
            }
            return dayDT;
        }
    }
    public class F_Order_Sale
    {
        /// <summary>
        /// 默认仅筛选正常类型的订单
        /// </summary>
        public string orderType = "0";
        public string stime = "";
        public string etime = "";
        public string storeIds = "";
        /// <summary>
        /// 商品名筛选
        /// </summary>
        public string proname = "";
        /// <summary>
        /// 只筛选指定节点下的商品
        /// </summary>
        public string nodeIds = "";
        /// <summary>
        /// payed=仅正常完成支付的订单(不含退货)
        /// </summary>
        public string fast = "payed";
        public string action = "";
    }
    public class F_Shop_SaleReport
    {
        public int StoreID = -100;
        public string SDate = "";
        public string EDate = "";
        public string NodeIDS = "";
        public int ProID = -100;
        public string ProName = "";
    }
}
