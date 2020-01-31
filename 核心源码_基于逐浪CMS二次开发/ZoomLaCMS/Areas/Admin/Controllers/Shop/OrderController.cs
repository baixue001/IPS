using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Common;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy="Admin")]
    public class OrderController : Ctrl_Admin
    {
        // GET: /Admin/Order/
        B_OrderList orderBll = new B_OrderList();
        B_OrderList oll = new B_OrderList();
        B_Product proBll = new B_Product();
        B_CartPro cpBll = new B_CartPro();
        B_Payment payBll = new B_Payment();
        B_PayPlat platBll = new B_PayPlat();
        OrderCommon orderCom = new OrderCommon();
        public IActionResult OrderList()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "order")) { return null; }
            Filter_Order filter = new Filter_Order();
            filter.cpage = CPage;
            filter.psize = PSize;
            filter.storeType = GetParam("StoreType_DP");
            filter.orderType = GetParam("OrderType");
            filter.addon = GetParam("filter");
            filter.proname = GetParam("ProName_T");
            filter.orderno = GetParam("OrderNo_T");
            filter.reuser = GetParam("ReUser_T"); 
            filter.mobile = GetParam("Mobile_T");
            filter.uids = GetParam("UserID_T");
            filter.stype = GetParam("SkeyType_DP");
            filter.skey = GetParam("Skey_T");
            filter.stime = GetParam("STime_T");
            filter.etime = GetParam("ETime_T");
            filter.expstime = GetParam("ExpSTime_T");
            filter.expetime = GetParam("ExpETime_T");
            if (!Request.IsAjaxRequest())
            {
                filter.needCount = true;
            }
            PageSetting setting = cpBll.SelForOrderList(filter);
            DataTable StoreDT = orderCom.SelStoreDT(setting.dt);
            DataTable CartDT = setting.dt;
            setting.dt = new DataTableHelper().DistinctByField(setting.dt, "ID");
            //Session["Temp_OrderList"] = CartDT;
            ViewBag.filter = filter;
            ViewBag.CartDT = CartDT;
            if (Request.IsAjaxRequest())
            {
                return PartialView("Order_List", setting);
            }
            else
            {
                ViewBag.countMod = filter.countMod;
                return View(setting);
            }
        }
        //生成Excel表格
        public void Order_Down()
        {
            //Session["Temp_OrderList"] = OrderDT;
            //ZoomLa.BLL.Helper.HtmlHelper htmlHelp = new ZoomLa.BLL.Helper.HtmlHelper();
            //StringWriter sw = new StringWriter();
            //Server.Execute("/Manage/Shop/OrderlistToExcel", sw);
            //string html = sw.ToString();
            //HtmlPage page = htmlHelp.GetPage(html);
            //html = page.Body.ExtractAllNodesThatMatch(new HasAttributeFilter("id", "orderlist"), true).ToHtml();

            //string name = DateTime.Now.ToString("yyyyMMdd") + "订单详单";
            //name = HttpUtility.UrlPathEncode(name);
            //Response.Clear();
            //Response.Buffer = true;
            //Response.ContentType = "application/vnd.ms-excel.numberformat:@";
            //Response.Charset = "UTF-8";
            //Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");//设置输出流为简体中文  
            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + name + ".xls");
            //Response.Write(html);
            //Response.End();
        }
        public IActionResult Order_API()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "order")) { return Content(Failed.ToString()); }
            string action = GetParam("a");
            string ids = GetParam("ids");
            int Mid = DataConvert.CLng(GetParam("ids"));
            switch (action)
            {
                case "info_normal":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "恢复正常");
                        string str = "Aside=0,Suspended=0,Settle=0,BackID=0,OrderStatus=" + (int)M_OrderList.StatusEnum.Normal;
                        orderBll.UpOrderinfo(str, Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_complete"://完结订单
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "完结订单");
                        //前使用必须修改,只更改状态,不执行FinalStep
                        M_OrderList orderMod = oll.SelReturnModel(Mid);
                        if (string.IsNullOrEmpty(orderMod.PaymentNo))//未支付则生成支付单
                        {
                            OrderHelper.FinalStep(orderMod);
                        }
                        else
                        {
                            M_Payment payMod = payBll.SelModelByOrder(orderMod);
                            OrderHelper.FinalStep(payMod, orderMod, new M_Order_PayLog());
                        }
                        logBll.Insert(logMod);
                    }
                    break;
                case "info_invoce"://已开发票
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "已开发票");
                        oll.UpOrderinfo("Developedvotes=1", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_suspend"://冻结,挂起订单
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "冻结处理");
                        oll.UpOrderinfo("Suspended=1", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_suspend_no":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "取消冻结");
                        oll.UpOrderinfo("Suspended=0", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_aside":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "订单作废");
                        oll.UpOrderinfo("Aside=1", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_refund"://退单还款
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "退单还款");
                        M_OrderList orderinfo = oll.GetOrderListByid(Mid);
                        if (orderinfo.Paymentstatus == (int)M_OrderList.PayEnum.NoPay) { return WriteErr("操作失败，订单还未支付"); }
                        if (orderinfo.Paymentstatus == (int)M_OrderList.PayEnum.Refunded) { return WriteErr("操作失败，该订单已退款"); }
                        buser.ChangeVirtualMoney(orderinfo.Userid, new M_UserExpHis()
                        {
                            score = orderinfo.Receivablesamount,
                            ScoreType = 1,
                            detail = "订单[" + orderinfo.id + "]退单返款,返款金额：" + orderinfo.Receivablesamount
                        });
                        oll.UpOrderinfo("Paymentstatus=" + (int)M_OrderList.PayEnum.Refunded, Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_payed":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "已经支付");
                        oll.UpOrderinfo("Paymentstatus=" + (int)M_OrderList.PayEnum.HasPayed, Mid); logBll.Insert(logMod);
                    }
                    break;
                case "info_pay_cancel":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "取消支付");
                        oll.UpOrderinfo("Paymentstatus=" + (int)M_OrderList.PayEnum.NoPay + ",PaymentNo=''", Mid);
                        logBll.Insert(logMod);
                    }
                    break;
                case "info_remind"://更新备注信息
                    {

                        M_OrderList orderinfo = oll.SelReturnModel(Mid);
                        orderinfo.Internalrecords = GetParam("Internalrecords");
                        orderinfo.Ordermessage = GetParam("Ordermessage");
                        oll.UpdateByID(orderinfo);
                    }
                    break;
                case "exp_cancel"://取消发送
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "取消发送");
                        oll.UpOrderinfo("StateLogistics=" + (int)M_OrderList.ExpEnum.NoSend + ",ExpressNum=''", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "exp_sign":
                    {
                        M_Order_OPLog logMod = logBll.NewLog(Mid, "客户已签收");
                        oll.UpOrderinfo("Signed=1,StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived, Mid); logBll.Insert(logMod);
                    }
                    break;
                    //------------------------
                case "sure"://确认订单
                    {
                        orderBll.ChangeSure(ids, 1);
                        //M_Order_OPLog logMod = logBll.NewLog(Mid, "确认订单");
                        //oll.UpOrderinfo("IsSure=1", Mid); logBll.Insert(logMod);
                    }
                    break;
                case "sure_no":
                    {
                        orderBll.ChangeSure(ids,0);
                        //M_Order_OPLog logMod = logBll.NewLog(Mid, "取消确认");
                        //oll.UpOrderinfo("IsSure=0", Mid); logBll.Insert(logMod);
                        //return WriteOK("取消确认成功", "Orderlistinfo?id=" + Mid);
                    }
                    break;
                case "recycle"://回收站
                    {
                        orderBll.ChangeStatus(ids, "recycle");
                    }
                    break;
                case "recover":
                    {
                        orderBll.ChangeStatus(ids, "recover");
                    }
                    break;
                case "del":
                    {
                        orderBll.DelByIDS(ids);
                    }
                    break;
                case "clear":
                    {
                        orderBll.ClearRecycle();
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        B_Order_Exp expBll = new B_Order_Exp();
        B_Order_Back backBll = new B_Order_Back();
        B_Order_PayLog paylogBll = new B_Order_PayLog();
        M_Order_PayLog paylogMod = new M_Order_PayLog();
        B_Order_OPLog logBll = new B_Order_OPLog();
        B_Order_Invoice invBll = new B_Order_Invoice();
        B_Stock Sll = new B_Stock();
        public IActionResult OrderListInfo()
        {
            M_OrderList orderinfo = null;
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "order")) { return null; }
            if (Mid < 1 && string.IsNullOrEmpty(RequestEx["OrderNo"])) { return WriteErr("未指定订单"); }
            if (Mid > 0) { orderinfo = orderBll.GetOrderListByid(Mid); }
            else if (!string.IsNullOrEmpty(RequestEx["OrderNo"])) { orderinfo = orderBll.GetByOrder(RequestEx["OrderNo"], "0"); }
            if (orderinfo == null || orderinfo.id < 1) { return WriteErr("订单不存在");}
            return View(orderinfo);
        }
        public IActionResult OrderRepairAudit()
        {
            return View();
        }
        public IActionResult CartManage()
        {
            return View();
        }
    }
}
