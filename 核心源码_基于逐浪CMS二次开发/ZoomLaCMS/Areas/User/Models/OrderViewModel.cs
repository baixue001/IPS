using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZoomLaCMS.Common;

namespace ZoomLaCMS.Areas.User.Models
{
    public class OrderViewModel
    {
        B_User buser = new B_User();
        B_CartPro cartBll = new B_CartPro();
        B_Payment payBll = new B_Payment();
        B_PayPlat platBll = new B_PayPlat();
        B_OrderList orderBll = new B_OrderList();
        B_Order_IDC idcBll = new B_Order_IDC();
        OrderCommon orderCom = new OrderCommon();
        public PageSetting setting = new PageSetting();
        public M_UserInfo mu = null;
        public DataTable OrderProDT = null;//包含商品数据的原始表
        public DataTable StoreDT = null;   //只包含店铺信息
        public DataTable OrderDT = null;   //只包含不重复的订单数据
        public int payType = 0;
        public int OrderType = -1;
        public string Filter = "";
        public string usefor = "user";//用于标识是用于商品列表还是店铺管理
        public Filter_Order filter = new Filter_Order();
        public OrderViewModel(Filter_Order filter)
        {
            this.filter = filter;
        }
        //all,receive,needpay,comment
        //用户商城订单页
        public OrderViewModel(int cpage, int psize, M_UserInfo mu, HttpRequest Request)
        {
            this.mu = mu;
            this.OrderType = DataConvert.CLng(Request.GetParam("ordertype"), -1);
            this.Filter = string.IsNullOrEmpty(Request.GetParam("filter")) ? "all" : Request.GetParam("Filter");
            payType = DataConvert.CLng(Request.GetParam("payType"), -100);
            //string ids = "";
            filter = new Filter_Order()
            {
                cpage = cpage,
                psize = psize,
                uids = mu.UserID.ToString(),
                fast = Filter,
                stype = Request.GetParam("SType"),
                skey = Request.GetParam("SKey_T"),
                proname = Request.GetParam("ProName_T"),
                orderno=Request.GetParam("OrderNo_T"),
                mobile=Request.GetParam("Mobile_T"),
                reuser=Request.GetParam("ReUser_T"),
                stime=Request.GetParam("STime_T"),
                etime=Request.GetParam("ETime_T"),
                expstime=Request.GetParam("ExpSTime_T"),
                expetime=Request.GetParam("ExpETime_T"),
               
                orderType = OrderType.ToString(),
                payType = payType,
                isSure = DataConvert.CLng(Request.GetParam("isSure"), -100)
            };
            setting = cartBll.SelForOrderList(filter);
            OrderProDT = setting.dt;
            StoreDT = orderCom.SelStoreDT(OrderProDT);
            OrderDT = new DataTableHelper().DistinctByField(OrderProDT, "ID");
        }


        //用户店铺页面使用
        public OrderViewModel(Filter_Order filter, HttpRequest Request)
        {
            this.filter = filter;
            this.OrderType = DataConvert.CLng(Request.GetParam("ordertype"), -1);
            this.Filter = filter.fast = string.IsNullOrEmpty(Request.GetParam("filter")) ? "all" : Request.GetParam("Filter");
            this.payType = filter.payType = DataConvert.CLng(Request.GetParam("payType"), -100);

            filter.stype = Request.GetParam("Stype");
            filter.skey = Request.GetParam("SKey_T");
            filter.proname = Request.GetParam("ProName_T");
            filter.orderno = Request.GetParam("OrderNo_T");
            filter.mobile = Request.GetParam("Mobile_T");
            filter.reuser = Request.GetParam("ReUser_T");
            filter.stime = Request.GetParam("STime_T");
            filter.etime = Request.GetParam("ETime_T");
            filter.expstime = Request.GetParam("ExpSTime_T");
            filter.expetime = Request.GetParam("ExpETime_T");
            filter.orderType = this.OrderType.ToString();
            filter.isSure = DataConvert.CLng(Request.GetParam("isSure"), -100);
            setting = cartBll.SelForOrderList(filter);
            OrderProDT = setting.dt;
            StoreDT = orderCom.SelStoreDT(OrderProDT);
            OrderDT = new DataTableHelper().DistinctByField(OrderProDT, "ID");
        }
        //获取订单操作按钮,分为已下单(未付款),已下单(已付款),已完结(显示)
        public string Getoperator(DataRow dr)
        {
            switch (usefor)
            {
                case "store":
                    return Getoperator_Store(dr);
                default:
                    return Getoperator_User(dr);
            }
        }
        public string Getoperator_User(DataRow dr)
        {
            M_OrderList orderMod = orderBll.SelReturnModel(DataConvert.CLng(dr["ID"]));
            string result = "";
            int orderStatus = DataConverter.CLng(dr["OrderStatus"]);
            int payStatus = DataConverter.CLng(dr["Paymentstatus"]);
            int oid = Convert.ToInt32(dr["ID"]);
            int aside = Convert.ToInt32(dr["Aside"]);
            string orderNo = dr["OrderNo"].ToString();
            //----------如果已经删除,则不执行其余的判断
            if (aside != 0)//如果已删除,则不进行其余的判断
            {
                result += "<div><a href='javascript:;' data-oid='" + oid + "' onclick='ConfirmAction(this,\"reconver\");'>还原订单</a></div>";
                result += "<div><a href='javascript:;' data-oid='" + oid + "' onclick='ConfirmAction(this,\"realdel\");'>彻底删除</a></div>";
                return result;
            }
            if (payStatus == (int)M_OrderList.PayEnum.NoPay)//未付款,显示倒计时和付款链接
            {
                bool isnormal = true;
                //订单过期判断
                if (SiteConfig.ShopConfig.OrderExpired > 0)
                {
                    int seconds = GetOrderUnix(dr);
                    if (seconds <= 0)
                    { result += "<div class='gray9'>订单关闭</div>"; isnormal = false; }
                    else
                    { result += "<div class='ordertime' data-time='" + seconds + "'></div>"; }
                }
                //订单未完成,且为正常状态,显示付款
                if (isnormal && OrderHelper.IsCanPay(dr))
                {
                    M_Payment payMod = GetPaymentByOrderNo(orderNo);
                    if (payMod == null) { result += "<div><a href='/Payonline/OrderPay?OrderCode=" + orderNo + "' target='_blank' class='btn btn-info'>前往付款</a></div>"; }
                    else
                    {
                        result += "<div><a href='/Payonline/OrderPay?PayNo=" + payMod.PayNo + "' target='_blank' class='btn btn-info'>前往付款</a></div>";
                    }
                }
                result += "<div><a href='javascript:;' data-oid='" + oid + "' onclick='ConfirmAction(this,\"del\");'>取消订单</a></div>";
            }
            else
            {
                //---物流
                switch (Convert.ToInt32(dr["StateLogistics"]))
                {
                    case 1:
                        if (Convert.ToInt32(dr["Ordertype"]) != 7 && Convert.ToInt32(dr["Ordertype"]) != 9)
                        {
                            result += "<div><a href='javascript:;' class='btn btn-info' data-oid='" + oid + "' onclick='ConfirmAction(this,\"receive\");'>确认收货</a></div>";
                        }
                        break;
                }
                //已付款,但处理申请退款等状态
                if (orderStatus < (int)M_OrderList.StatusEnum.Normal)
                {
                    //result += "<a href='AddShare?CartID=" + dr["CartID"] + "'>取消退款</a><br />";
                }
                //已付款未收货,可申请退款
                if (orderStatus >= (int)M_OrderList.StatusEnum.Normal && orderMod.StateLogistics <= (int)M_OrderList.ExpEnum.HasReceived)
                {
                    result += "<a href='javascript:;' onclick='ShowDrawback(" + oid + ");'>取消订单</a><br />";
                }
                //已完结,可评价晒单
                if (OrderHelper.IsFinished(dr, "normal") && !DataConvert.CStr(dr["AddStatus"]).Contains("comment"))
                {
                    result += "<a href='AddShare?OrderID=" + dr["ID"] + "' title='评价'><i class='zi zi_comments'></i></a><br />";
                    //result += "<a href='/Shop/" + dr["ProID"] + "' target='_blank' class='btn btn-info'>立即购买</a>";
                }
                //已预付款,且已结算,显示支付尾款按钮
                if (orderMod.PayType == (int)M_OrderList.PayTypeEnum.PrePay && orderMod.Settle == 1 && orderMod.IsCount == false)
                {
                    result += "<div><a href='/BU/Order/SettlePay?ID=" + orderMod.id + "' target='_blank'>支付尾款</a></div>";
                }
            }
            return result;
        }
        //结算,订单退款
        public string Getoperator_Store(DataRow dr)
        {
            string result = "";
            int orderStatus = DataConverter.CLng(dr["OrderStatus"]);
            int payStatus = DataConverter.CLng(dr["Paymentstatus"]);
            int oid = Convert.ToInt32(dr["ID"]);
            int aside = Convert.ToInt32(dr["Aside"]);
            string orderNo = dr["OrderNo"].ToString();
            //----------如果已经删除,则不执行其余的判断
            if (payStatus == (int)M_OrderList.PayEnum.NoPay)//未付款,显示倒计时和付款链接
            {
                //订单过期判断
                if (SiteConfig.ShopConfig.OrderExpired > 0)
                {
                    int seconds = GetOrderUnix(dr);
                    if (seconds <= 0)
                    { result += "<div class='gray9'>订单关闭</div>"; }
                    else
                    { result += "<div class='ordertime' data-time='" + seconds + "'></div>"; }
                }
            }
            else
            {
                //已付款,且未结算的订单,显示结算
                if (DataConvert.CLng(dr["Delivery"]) == 1 && DataConvert.CLng(dr["Settle"]) == 0)
                {
                    result += "<div><a href='/BU/Money/OrderSettle?ID=" + oid + "' class='btn btn-xs btn-info'>订单结算</a></div>";
                }
            }
            return result;
        }
        //还差多少分钟到期
        public int GetOrderUnix(DataRow dr)
        {
            DateTime addTime = Convert.ToDateTime(dr["AddTime"]);
            int seconds = (SiteConfig.ShopConfig.OrderExpired * 60 * 60) - (int)(DateTime.Now - addTime).TotalSeconds;
            return seconds;
        }
        public string GetRepair(DataRow dr)
        {
            string result = "";
            if (!OrderHelper.IsFinished(dr, "normal")) { return result; }
            //--------------------------------------------
            string guess = DataConvert.CStr(dr["GuessXML"]);
            if (dr["AddStatus"].ToString().Contains("exchange"))
            {
                result += "<a href='javascript:;' class='gray9'>已申请换货</a>";
            }
            else if (dr["AddStatus"].ToString().Contains("repair"))
            {
                result += "<a href='javascript:;' class='gray9'>已申请返修</a>";
            }
            else if (dr["AddStatus"].ToString().Contains("drawback"))
            {
                result += "<a href='javascript:;' class='gray9'>已申请退货</a>";
            }
            else if (!string.IsNullOrEmpty(guess))
            {
                result += "<a href='ReqRepair?cid=" + dr["CartID"] + "' style='color:#015fb6;'>返修/退换货</a>";
            }
            return result;
        }
        public string GetImgUrl(DataRow dr)
        {
            return function.GetImgUrl(dr["Thumbnails"]);
        }
        public string GetShopUrl(DataRow dr)
        {
            return OrderHelper.GetShopUrl(DataConvert.CLng(dr["StoreID"]), Convert.ToInt32(dr["ProID"]));
        }
        public string GetMessage(DataRow dr)
        {
            if (Convert.ToInt32(dr["OrderType"]) == 7)
            {
                return "<tr class='idm_tr'><td colspan='6'><p class='idcmessage'>详记：" + dr["Ordermessage"] + "</p></td></tr>";
            }
            else return "";
        }
        public string GetSnap(DataRow dr)
        {
            string result = "";
            int paystatus = Convert.ToInt32(dr["PaymentStatus"]);
            if (paystatus == (int)M_OrderList.PayEnum.HasPayed)
            {
                string dir = ZLHelper.GetUploadDir_User(mu, "snapdir") + dr["OrderNo"] + "/" + dr["ProID"] + ".mht";
                if (File.Exists(function.VToP(dir))) { result += "<a href='" + dir + "' target='_blank' title='查看快照'> [交易快照]</a>"; }
                if (Convert.ToInt32(dr["OrderType"]) == 7 && Convert.ToInt32(dr["OrderStatus"]) == 99)
                {
                    string orderNo = DataConvert.CStr(dr["OrderNo"]);
                    M_Order_IDC idcMod = idcBll.SelModelByOrderNo(orderNo);
                    if (idcMod != null)
                    {
                        result += "<span style='color:green;font-size:12px;'>(到期时间:" + idcMod.ETime.ToString("yyyy/MM/dd") + ") </span>";
                    }
                }
            }
            return result;
        }
        public string GetStoreName(int storeid)
        {
            DataRow[] dr = StoreDT.Select("ID=" + storeid);
            if (dr.Length > 0) { return dr[0]["StoreName"].ToString(); }
            else { return ""; }
        }
        public DataTable GetProByOrder(string orderNo)
        {
            OrderProDT.DefaultView.RowFilter = "OrderNo='" + orderNo + "'";
            return OrderProDT.DefaultView.ToTable();
        }
        //绑定订单操作列
        public IHtmlContent BindOrderOP(DataRow dr) 
        {
            M_Payment payMod = GetPaymentByOrderNo(DataConvert.CStr(dr["OrderNo"]));
            if (payMod == null)
            {
                payMod = new M_Payment();
                payMod.MoneyReal=Convert.ToDouble(dr["OrdersAmount"]);
            }
            int count = GetProByOrder(DataConvert.CStr(dr["OrderNo"])).Rows.Count;
            //收货人 <td class='td_md gray9' rowspan='" + count + "'><i class='zi zi_user'> " + dr["AddUser"] + "</i></td>
            string html = "";
            //金额栏
            html += "<td class='rowtd' rowspan='" + count + "' style='font-size:12px;width:150px;'>";
            html += "<div>总计:" + Convert.ToDouble(dr["OrdersAmount"]).ToString("f2") + "</div>";

            html += "<div>商品:" + (DataConvert.CDouble(dr["OrdersAmount"]) - DataConvert.CDouble(dr["Freight"])).ToString("F2") + "</div>";
            html += "<div>运费:" + DataConvert.CDouble(dr["Freight"]).ToString("F2") + "</div>";
            html += "<div title='优惠券'>优惠:" + payMod.ArriveMoney.ToString("F2") + "</div>";
            html += "<div>积分:" + payMod.UsePointArrive.ToString("f2") + "(" + payMod.UsePoint.ToString("F0") + ")</div>";
            switch (payMod.PayType)
            {
                case (int)M_OrderList.PayTypeEnum.PrePay:
                    try
                    {
                        M_PrePayinfo preInfo = new M_PrePayinfo(payMod.PrePayInfo);
                        string payedTlp = "<div>(<span style='color:green;'>已付款</span>:{0},{1})</div>";
                        string nopayTlp = "<div>(<span style='color:red;'>未付款</span>)</div>";
                        html += "<div style='color:#d9534f;'>定金:" + preInfo.money_pre.ToString("F2") + "</div>";
                        html += preInfo.money_pre_payed > 0 ? string.Format(payedTlp, preInfo.money_pre_payed.ToString("F2"), preInfo.pre_payMethod) : nopayTlp;
                        html += "<div style='color:#d9534f'>尾款:" + preInfo.money_after.ToString("F2") + "</div>";
                        html += preInfo.money_after_payed > 0 ? string.Format(payedTlp, preInfo.money_after_payed.ToString("F2"), preInfo.after_payMethod) : nopayTlp;
                        html += "<div style='color:#d9534f'>总计:" + preInfo.money_total.ToString("F2") + "</div>";

                    }
                    catch {
                        html += "预付信息错误";
                    }
                    break;
                default:
                    html += "<div style='color:#d9534f;'>需付:" + payMod.MoneyReal.ToString("F2") + "</div>";
                    break;
            }
            if (!string.IsNullOrEmpty(DataConvert.CStr(dr["PaymentNo"])))
            {
                string plat = platBll.GetPayPlatName(DataConvert.CStr(dr["PaymentNo"]));
                html += "<div><span style='color:#337ab7;'>" + plat + "</span>"
                    + "(" + OrderHelper.GetStatus(dr, OrderHelper.TypeEnum.Pay) + ")</div>";
            }
            else
            {
                html += "<div>(" + OrderHelper.GetStatus(dr, OrderHelper.TypeEnum.Pay) + ")</div>";
            }
            html += "</td>";

            //string _paytypeHtml = OrderHelper.GetStatus(dr, OrderHelper.TypeEnum.PayType);
            //if (!string.IsNullOrEmpty(_paytypeHtml)) { _paytypeHtml = _paytypeHtml + "<br />"; }
            //html += _paytypeHtml;
            //html += "<div>(" + OrderHelper.GetStatus(dr, OrderHelper.TypeEnum.Pay) + ")</div></td>";
            //订单状态
            html += "<td class='td_md rowtd' rowspan='" + count + "'><span class='gray9'>" + OrderHelper.GetStatus(dr, OrderHelper.TypeEnum.Order) + "</span> <br />";
            int ordertype = DataConvert.CLng(dr["OrderType"]);
            if (ordertype != 7 && ordertype != 9) { html += OrderHelper.GetExpStatus(Convert.ToInt32(dr["StateLogistics"])) + " <br/>"; }
            html += "</td>";
            //操作栏
            string orderInfoUrl = (usefor.Equals("store") ? "/BU/Order/OrderListInfo?ID=" + dr["ID"]: "OrderProList?OrderNo=" + dr["OrderNo"]);
            html += "<td class='td_md rowtd' rowspan='" + count + "'><a href='"+orderInfoUrl + "' class='order_bspan'>订单详情</a><br/>" + Getoperator(dr) + "</td>";
            return MvcHtmlString.Create(html);
        }
        private List<M_Payment> payList = new List<M_Payment>();
        private M_Payment GetPaymentByOrderNo(string orderNo)
        {
            M_Payment payMod = payList.FirstOrDefault(p => p.PaymentNum.Equals(orderNo));
            payMod = payBll.SelModelByOrderNo(orderNo);
            return payMod;
        }
    }
}