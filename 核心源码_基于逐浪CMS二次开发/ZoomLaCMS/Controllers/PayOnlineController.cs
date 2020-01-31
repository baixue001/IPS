using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.BLL.Alipay;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.BLL.WxPayAPI;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class PayOnlineController : Ctrl_User
    {
        B_Payment payBll = new B_Payment();
        B_PayPlat platBll = new B_PayPlat();
        B_Order_PayLog paylogBll = new B_Order_PayLog();//支付日志类,用于记录用户付款信息
        B_OrderList orderBll = new B_OrderList();
        B_CartPro cpBll = new B_CartPro();
        OrderCommon orderCom = new OrderCommon();
        M_Order_PayLog paylogMod = new M_Order_PayLog();

        public string PayMethod { get { return RequestEx["method"]; } }
        #region OrderPay
        //支持以,号分隔,在该页生成支付单
        public string OrderNo { get { return RequestEx["OrderCode"]; } }
        //支付单号码
        //根据需要,可在自己页面生成,或传入OrderNo在本页生成
        public string PayNo { get { return RequestEx["PayNo"]; } }
        public double Money { get { return DataConverter.CDouble(RequestEx["Money"]); } }
        double price = 0, fare = 0, arrive = 0, allamount = 0, point = 0, point_arrive = 0;
        [Authorize(Policy="user")]
        public IActionResult OrderPay()
        {
            /*
             * 币种与平台支付均在该页面指定
             * 现金支付的中转页,支付以其金额为准,支持传入订单号(多张订单以,切割)生成支付单或支付单号补完信息
             */
            M_Payment payMod = new M_Payment();
            DataTable orderDT = new DataTable();
            if (Money > 0)//直接传要充多少，用于充值余额等
            {
                payMod = CreateChargePay(mu);
                return RedirectToAction("OrderPay",new {Payno = payMod.PayNo });
            }
            else if (!string.IsNullOrEmpty(PayNo))
            {
                payMod = payBll.SelModelByPayNo(PayNo);
                OrderHelper.OrdersCheck(payMod);
                orderDT = orderBll.GetOrderbyOrderNo(payMod.PaymentNum);
            }
            else if (!string.IsNullOrEmpty(OrderNo))
            {
                M_OrderList orderMod = orderBll.SelModelByOrderNo(OrderNo);
                OrderHelper.CheckIsCanPay(orderMod);
                orderDT = orderBll.GetOrderbyOrderNo(OrderNo);
            }
            else { return WriteErr("未指定订单或支付单号"); }
            bool ShowVMoney = true;
            if (payMod.SysRemark == "用户充值 ") { ShowVMoney = false; }
            if (string.IsNullOrEmpty(SiteConfig.SiteOption.SiteID.Replace(",", ""))) { ShowVMoney = false; }
            if (orderDT != null && orderDT.Rows.Count > 0)
            {
                //如果是跳转回来的,检测其是否包含充值订单
                foreach (DataRow dr in orderDT.Rows)
                {
                    if (DataConverter.CLng(dr["Ordertype"]) == (int)M_OrderList.OrderEnum.Purse)
                    {
                        ShowVMoney = false; break;
                    }
                }
                //总金额,如有支付单,以支付单的为准(运费暂以订单为准)
                GetTotal(orderDT, ref price, ref fare, ref allamount);
                if (!string.IsNullOrEmpty(PayNo))
                {
                    allamount = (double)payMod.MoneyPay;
                    arrive = payMod.ArriveMoney;
                    point = payMod.UsePoint;
                    point_arrive = payMod.UsePointArrive;
                }
                ViewBag.allmount = allamount.ToString("F2");
                ViewBag.price = price.ToString("F2");
                ViewBag.fare = fare.ToString("F2");
                ViewBag.arrive = arrive.ToString("F2");
                ViewBag.point = point.ToString("F0");
                ViewBag.point_arrive = point_arrive.ToString("F0");

                if (string.IsNullOrEmpty(OrderNo) && Money > 0) { ViewBag.orderNo = "用户充值"; }
                else if (payMod.PaymentID > 0) { ViewBag.orderNo = payMod.PaymentNum.Trim(','); }
                else { ViewBag.orderNo = OrderNo; }
            }
            //如果已用优惠券|积分,抵扣了金额,则可直接支付
            if (payMod.PaymentID > 0 && payMod.MoneyReal == 0)
            {
                FinalStep(payMod);
            }
            ViewBag.ShowVMoney = ShowVMoney;
            return View(payMod);
        }
        [Authorize(Policy = "User")]
        public IActionResult OrderPay_Submit()
        {
            int platid = DataConverter.CLng(GetParam("payplat_rad"));
            M_UserInfo mu = buser.GetLogin(false);
            M_Payment pinfo = new M_Payment();
            M_OrderList orderMod = new M_OrderList();
            if (!string.IsNullOrEmpty(PayNo))//支付单付款
            {
                pinfo = payBll.SelModelByPayNo(PayNo);
                if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { return WriteErr("该支付单已付款,请勿重复支付"); }
            }
            else
            {
                DataTable orderDT = orderBll.GetOrderbyOrderNo(OrderNo);
                GetTotal(orderDT, ref price, ref fare, ref allamount);
                if (allamount < 0.01) { return WriteErr("每次划款金额不能低于0.01元"); }
                if (orderDT != null && orderDT.Rows.Count > 0)
                {
                    orderMod = orderBll.GetOrderListByid(DataConverter.CLng(orderDT.Rows[0]["id"]));
                    orderBll.Update(orderMod);
                }
                pinfo.PaymentNum = OrderNo;
                pinfo.MoneyPay = allamount;
                DataTable cartDT = new DataTable();
                if (orderMod.id > 0)
                {
                    cartDT = cpBll.GetCartProOrderID(orderMod.id);
                    pinfo.Remark = cartDT.Rows.Count > 1 ? "[" + cartDT.Rows[0]["ProName"] as string + "]等" : cartDT.Rows[0]["ProName"] as string;
                }
                else { pinfo.Remark = "没有对应订单"; }
            }
            pinfo.UserID = mu.UserID;
            pinfo.PayPlatID = platid;
            pinfo.MoneyID = 0;
            pinfo.MoneyReal = pinfo.MoneyPay;
            //用于支付宝网银
            pinfo.PlatformInfo = GetParam("payplat_rad");
            if (!string.IsNullOrEmpty(PayNo)) { payBll.Update(pinfo); }
            else
            {
                pinfo.Status = (int)M_Payment.PayStatus.NoPay;
                pinfo.PayNo = payBll.CreatePayNo();
                payBll.Add(pinfo);
            }
            string method = "";
            if (pinfo.PayPlatID == 0)
            {
                method = GetParam("payplat_rad");
            }
            return RedirectToAction("Payonline", "Payonline", new { Payno = pinfo.PayNo, method = method });
        }
        private void GetTotal(DataTable dt, ref double price, ref double fare, ref double allamount)
        {
            foreach (DataRow dr in dt.Rows)
            {
                price += Convert.ToDouble(dr["Balance_price"]);
                fare += Convert.ToDouble(dr["Freight"]);
                allamount += Convert.ToDouble(dr["Ordersamount"]);
            }
        }
        //如果是充值,则生成充值支付单
        private M_Payment CreateChargePay(M_UserInfo mu)
        {
            if (Money <= 0) { throw new Exception("未指定充值金额"); }
            M_OrderList orderMod = orderBll.NewOrder(mu, M_OrderList.OrderEnum.Purse);
            orderMod.Ordersamount = Money;
            orderMod.Specifiedprice = Money;
            orderMod.Balance_price = Money;
            orderMod.id = orderBll.insert(orderMod);
            M_Payment payMod = payBll.CreateByOrder(orderMod);
            payMod.MoneyReal = payMod.MoneyPay;
            payMod.SysRemark = "用户充值";
            payMod.Remark = "用户充值";
            payMod.PaymentID = payBll.Add(payMod);
            return payMod;
        }
        private void FinalStep(M_Payment pinfo)
        {
            if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { throw new Exception("该支付单已处理过,不可重复执行"); }
            pinfo = payBll.PaySuccess(pinfo, 0, "");
            pinfo.Remark = "";
            foreach (string orderNo in pinfo.PaymentNum.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                M_OrderList orderMod = orderBll.SelModelByOrderNo(orderNo);
                OrderHelper.FinalStep(pinfo, orderMod, null);
            }
            payBll.Update(pinfo);
            Response.Redirect("/User/Order/OrderList");
        }
        #endregion
        #region Payonline
        [Authorize(Policy = "User")]
        public IActionResult PayOnline()
        {
            M_UserInfo mu = buser.GetLogin(false);
            if (string.IsNullOrEmpty(SiteConfig.SiteInfo.SiteUrl)) { return WriteErr("错误,管理员未定义网站地址,SiteUrl"); }
            if (string.IsNullOrEmpty(PayNo)) { return WriteErr("请传入支付单号"); }
            M_Payment pinfo = payBll.SelModelByPayNo(PayNo);
            if (pinfo == null || pinfo.PaymentID < 1) { return WriteErr("支付单不存在"); }
            M_PayPlat payPlat = platBll.SelReturnModel(pinfo.PayPlatID);
            if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { return WriteErr("支付单不能重复付款"); }

            string siteurl = (SiteConfig.SiteInfo.SiteUrl.TrimEnd('/') + "/PayOnline/");
            string urlReq1 = SiteConfig.SiteInfo.SiteUrl + "/PayOnline";
            ViewBag.formHtml = "";
            if (string.IsNullOrEmpty(PayMethod))
            {
                #region 现金支付
                DataTable orderDB1 = orderBll.GetOrderbyOrderNo(pinfo.PaymentNum);//订单表,ZL_OrderInfo
                int orderType = 0;
                if (orderDB1.Rows.Count > 0)
                {
                    orderType = DataConvert.CLng(orderDB1.Rows[0]["OrderType"]);
                }
                if (orderType == 8)//有需要检测库存的订单类型，放此
                {
                    if (!cpBll.CheckStock(Convert.ToInt32(orderDB1.Rows[0]["OrderType"])))
                    {
                        return WriteErr("商品库存数量不足，请重新购买");
                    }
                }
                DataTable ordertable = orderBll.GetOrderbyOrderNo(pinfo.PaymentNum);
                //if (pinfo.PayPlatID == 0 && !string.IsNullOrEmpty(pinfo.PlatformInfo))//支付宝网银支付 
                //{
                //    payPlat = platBll.SelModelByClass(M_PayPlat.Plat.Alipay_Bank);
                //    alipayBank(pinfo.PlatformInfo);
                //}
                if (payPlat.PayClass == 99)//线下支付
                {
                    return WriteErr("信息已记录,请等待商家联系完成线下付款","/User/Order/OrderList");
                }
                if (payPlat == null || payPlat.PayPlatID < 1)
                {
                   return WriteErr("没有找到对应的支付平台信息!");
                }
                switch ((M_PayPlat.Plat)payPlat.PayClass)//现仅开通 12:支付宝即时到账和支付宝网银服务,15支付宝网银服务(上方代码中处理),银币与余额服务
                {
                    #region 各种支付方式
                    case M_PayPlat.Plat.UnionPay:
                        break;
                    case M_PayPlat.Plat.ChinaUnionPay:
                        break;
                    case M_PayPlat.Plat.Alipay_Instant:
                        #region 支付宝[即时到帐]
                        string input_charset1 = "utf-8";
                        string notify_url1 = urlReq1 + "/Return/AlipayNotify";//付完款后服务器AJAX通知的页面 要用 http://格式的完整路径，不允许加?id=123这类自定义参数
                        string return_url1 = urlReq1 + "/Return/AlipayReturn";//付完款后跳转的页面 要用 http://格式的完整路径，不允许加?id=123这类自定义参数
                        string show_url1 = "";
                        string sign_type1 = "MD5";
                        ///////////////////////以下参数是需要通过下单时的订单数据传入进来获得////////////////////////////////
                        //必填参数
                        string price1 = pinfo.MoneyReal.ToString("f2");//订单总金额，显示在支付宝收银台里的“商品单价”里
                        string logistics_fee1 = "0.00";//物流费用，即运费。
                        string logistics_type1 = "POST";//物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
                        string logistics_payment1 = "SELLER_PAY";//物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）
                        string out_trade_no1 = pinfo.PayNo;            //请与贵网站订单系统中的唯一订单号匹配
                        string subject1 = pinfo.Remark;                //订单名称，显示在支付宝收银台里的“商品名称”里，显示在支付宝的交易管理的“商品名称”的列表里。
                        string body1 = pinfo.Remark;                   //订单描述、订单详细、订单备注，显示在支付宝收银台里的“商品描述”里         		
                        string quantity1 = "1";                        //商品数量，建议默认为1，不改变值，把一次交易看成是一次下订单而非购买一件商品。
                        string receive_name1 = "";                     //收货人姓名，如：张三
                        string receive_address1 = "";                  //收货人地址，如：XX省XXX市XXX区XXX路XXX小区XXX栋XXX单元XXX号
                        string receive_zip1 = "";                      //收货人邮编，如：123456
                        string receive_phone1 = "";                    //收货人电话号码，如：0571-81234567
                        string receive_mobile1 = "";                   //收货人手机号码，如：13312341234
                                                                       //---------------------
                        string receive_name = orderDB1.Rows[0]["Reuser"] + "";                 //收货人姓名，如：张三
                        string receive_address = orderDB1.Rows[0]["Jiedao"] + "";                       //收货人地址，如：XX省XXX市XXX区XXX路XXX小区XXX栋XXX单元XXX号
                        string receive_zip = orderDB1.Rows[0]["ZipCode"] + "";                              //收货人邮编，如：123456
                        string receive_phone = orderDB1.Rows[0]["Phone"] + "";          //收货人电话号码，如：0571-81234567
                        string receive_mobile = orderDB1.Rows[0]["MobileNum"] + "";                     //收货人手机号码，如：13312341234
                                                                                                        //扩展参数——第二组物流方式
                                                                                                        //物流方式是三个为一组成组出现。若要使用，三个参数都需要填上数据；若不使用，三个参数都需要为空
                                                                                                        //有了第一组物流方式，才能有第二组物流方式，且不能与第一个物流方式中的物流类型相同，
                                                                                                        //即logistics_type="EXPRESS"，那么logistics_type_1就必须在剩下的两个值（POST、EMS）中选择
                        string logistics_fee_3 = "";                                    //物流费用，即运费。
                        string logistics_type_3 = "";                                   //物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
                        string logistics_payment_3 = "";                                //物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）

                        //扩展参数——第三组物流方式
                        //物流方式是三个为一组成组出现。若要使用，三个参数都需要填上数据；若不使用，三个参数都需要为空
                        //有了第一组物流方式和第二组物流方式，才能有第三组物流方式，且不能与第一组物流方式和第二组物流方式中的物流类型相同，
                        //即logistics_type="EXPRESS"、logistics_type_1="EMS"，那么logistics_type_2就只能选择"POST"
                        string logistics_fee_4 = "";                                    //物流费用，即运费。
                        string logistics_type_4 = "";                                   //物流类型，三个值可选：EXPRESS（快递）、POST（平邮）、EMS（EMS）
                        string logistics_payment_4 = "";                                //物流支付方式，两个值可选：SELLER_PAY（卖家承担运费）、BUYER_PAY（买家承担运费）
                                                                                        //扩展功能参数——其他
                        string buyer_email1 = "";                                       //默认买家支付宝账号
                        string discount1 = "";                                          //折扣，是具体的金额，而不是百分比。若要使用打折，请使用负数，并保证小数点最多两位数
                                                                                        /////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                                        //构造请求函数，无需修改
                        B_AliPay_trades_Services aliService1 = new B_AliPay_trades_Services(
                        payPlat.AccountID,
                        payPlat.SellerEmail,
                        return_url1,
                        notify_url1,
                        show_url1,
                        out_trade_no1,
                        subject1,
                        body1,
                        price1,
                        logistics_fee1,
                        logistics_type1,
                        logistics_payment1,
                        quantity1,
                        receive_name1,
                        receive_address1,
                        receive_zip1,
                        receive_phone1,
                        receive_mobile1,
                        logistics_fee_3,
                        logistics_type_3,
                        logistics_payment_3,
                        logistics_fee_4,
                        logistics_type_4,
                        logistics_payment_4,
                        buyer_email1,
                        discount1,
                        payPlat.MD5Key,
                        input_charset1,
                        sign_type1);
                        ViewBag.formHtml = aliService1.Build_Form();
                        //LblHiddenValue.InnerHtml = aliService1.Build_Form();
                        #endregion
                        break;
                    case M_PayPlat.Plat.Alipay_H5:
                        {
                            Response.Redirect("/API/Pay/Alipay_H5?Payno=" + PayNo);
                        }
                        break;
                    case M_PayPlat.Plat.Alipay_Bank://支付宝网银,已上方处理
                        break;
                    case M_PayPlat.Plat.Bill:
                        break;
                    case M_PayPlat.Plat.WXPay:
                        #region 微信扫码支付
                        {
                            //if (DeviceHelper.GetBrower() == DeviceHelper.Brower.Micro)
                            //{
                            //    Response.Redirect("/PayOnline/wxpay_mp?payno=" + pinfo.PayNo);
                            //    return;
                            //}
                            pinfo.PlatformInfo = "0";
                            payBll.Update(pinfo);
                            WxPayData wxdata = new WxPayData();
                            wxdata.SetValue("out_trade_no", pinfo.PayNo);
                            wxdata.SetValue("body", string.IsNullOrEmpty(pinfo.Remark) ? "微信支付" : pinfo.Remark);
                            wxdata.SetValue("total_fee", Convert.ToInt32(pinfo.MoneyReal * 100));
                            wxdata.SetValue("trade_type", "NATIVE");
                            wxdata.SetValue("notify_url", urlReq1 + "/Return/WxPayReturn");
                            wxdata.SetValue("product_id", "1");
                            WxPayData result = WxPayApi.UnifiedOrder(wxdata, WxPayApi.Pay_GetByID());
                            if (result.GetValue("return_code").ToString().Equals("FAIL"))
                                return WriteErr("商户" + result.GetValue("return_msg"));
                            Response.Redirect("/PayOnline/WxCodePay?PayNo=" + pinfo.PayNo + "&wxcode=" + result.GetValue("code_url"));
                        }
                        #endregion
                        break;
                    case M_PayPlat.Plat.Ebatong:
                        break;
                    case M_PayPlat.Plat.CCB:
                        break;
                    case M_PayPlat.Plat.ECPSS:
                        break;
                    case M_PayPlat.Plat.ICBC_NC:
                        break;
                    case M_PayPlat.Plat.PayPal:
                        {
                            Response.Redirect("PP/Pay?Payno=" + pinfo.PayNo);
                        }
                        break;
                    default:
                        throw new Exception("错误:支付方式不存在：" + payPlat.PayClass);
                        #endregion
                }
                ViewBag.vname = payPlat.PayPlatName;
                #endregion
            }
            else//非现金支付,给用户显示确认支付页,实际支付行为在Confirm_Click
            {
                switch (PayMethod)
                {
                    case "Purse":
                        ViewBag.vname = "帐户余额";
                        ViewBag.vmoney = mu.Purse.ToString("F2");
                        break;
                    case "SilverCoin":
                        ViewBag.vname = "帐户银币";
                        ViewBag.vmoney = mu.SilverCoin.ToString("F2");
                        break;
                    case "Score":
                        ViewBag.vname = "帐户积分";
                        ViewBag.vmoney = mu.UserExp.ToString("F2");
                        //Fee.Text = "帐户积分：";
                        //LblRate.Text = mu.UserExp + " 分";
                        //VMoneyPayed_L.Text = "账户积分";
                        break;
                    default:
                        throw new Exception("支付类型错误");
                }
            }
            ViewBag.mu= mu;
            ViewBag.platMod = payPlat;
            return View(pinfo);
        }
        [Authorize(Policy = "User")]
        public IActionResult PayOnline_Submit()
        {
            M_UserInfo mu = buser.GetLogin(false);
            if (string.IsNullOrEmpty(SiteConfig.SiteInfo.SiteUrl)) { return WriteErr("错误,管理员未定义网站地址,SiteUrl"); }
            if (string.IsNullOrEmpty(PayNo)) { return WriteErr("请传入支付单号"); }
            M_Payment pinfo = payBll.SelModelByPayNo(PayNo);
            if (pinfo == null || pinfo.PaymentID < 1) {  return WriteErr("支付单不存在"); }
            M_PayPlat payPlat = platBll.SelReturnModel(pinfo.PayPlatID);
            if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { return WriteErr("支付单不能重复付款"); }


            if (!string.IsNullOrEmpty(mu.PayPassWord))
            {
                if (!mu.PayPassWord.Equals(StringHelper.MD5(Request.Form["pwd_t"]))) { return WriteErr("支付密码不正确"); }
            }
            DataTable orderDT = new DataTable();
            orderDT = orderBll.GetOrderbyOrderNo(pinfo.PaymentNum);
            if (string.IsNullOrEmpty(PayMethod) && !string.IsNullOrEmpty(PayNo))//现金支付,跳转
            {

            }
            else if (!string.IsNullOrEmpty(PayMethod))//虚拟币支付
            {
                //paymentMod = payBll.SelModelByPayNo(PayNo);
                PayByVirtualMoney(PayMethod, pinfo);
            }
            else { return WriteErr("支付出错,支付单不存在"); }
            return View("Payment_Finish",pinfo);
        }
        //支付单虚拟币付款
        [Authorize(Policy = "User")]
        private CommonReturn PayByVirtualMoney(string payMethod, M_Payment payMod)
        {
            M_UserInfo mui = buser.GetLogin(false);
            List<M_OrderList> orderList = OrderHelper.OrdersCheck(payMod);
            ViewBag.vname = "";
            ViewBag.vmoney = 0;
            //ActualAmount.Visible = false;
            //paylogMod.UserID = mui.UserID;
            switch (payMethod)//完成支付
            {
                case "Purse":
                    if (!SiteConfig.SiteOption.SiteID.Contains("purse")) { return CommonReturn.Failed("管理员已关闭余额支付功能!"); }
                    if (mui.Purse < (double)payMod.MoneyReal) { return CommonReturn.Failed("对不起,余额不足! 请<a href='/PayOnline/OrderPay?Money=" + payMod.MoneyReal + "' target='_blank' style='margin-left:5px;color:#f00;'>充值!</a>"); }
                    buser.ChangeVirtualMoney(mui.UserID, new M_UserExpHis()
                    {
                        score = -(double)payMod.MoneyReal,
                        ScoreType = (int)M_UserExpHis.SType.Purse,
                        detail = "支付成功,支付单号:" + payMod.PayNo
                    });
                    mui = buser.GetLogin(false);
                    ViewBag.vname = "帐户余额";
                    ViewBag.vmoney = mui.Purse.ToString("F2");
                    break;
                case "SilverCoin":
                    if (!SiteConfig.SiteOption.SiteID.Contains("sicon")) { return CommonReturn.Failed("管理员已关闭银币支付功能!"); }
                    if (mui.SilverCoin < (double)payMod.MoneyReal) { return CommonReturn.Failed("对不起,银币不足!"); }
                    buser.ChangeVirtualMoney(mui.UserID, new M_UserExpHis()
                    {
                        score = -(double)payMod.MoneyReal,
                        ScoreType = (int)M_UserExpHis.SType.SIcon,
                        detail = "支付成功,支付单号:" + payMod.PayNo
                    });
                    mui = buser.GetLogin(false);
                    mui = buser.GetLogin(false);
                    ViewBag.vname = "帐户银币";
                    ViewBag.vmoney = mui.SilverCoin.ToString("F2");
                    break;
                case "Score":
                    if (!SiteConfig.SiteOption.SiteID.Contains("point")) { return CommonReturn.Failed("管理员已关闭积分支付功能!"); }
                    if (mui.UserExp < (double)payMod.MoneyReal) { return CommonReturn.Failed("对不起,积分不足!"); }
                    buser.ChangeVirtualMoney(mui.UserID, new M_UserExpHis()
                    {
                        score = -(double)payMod.MoneyReal,
                        ScoreType = (int)M_UserExpHis.SType.Point,
                        detail = "支付成功,支付单号:" + payMod.PayNo
                    });
                    mui = buser.GetLogin(false);
                    ViewBag.vname = "帐户积分";
                    ViewBag.vmoney = mui.UserExp.ToString("F2");
                    break;
                default:
                    return CommonReturn.Failed("指定的支付方式不存在,请检查大小写是否正确!");
            }
            for (int i = 0; i < orderList.Count; i++)//更改订单状态
            {
                M_OrderList orderMod = orderList[i];
                OrderHelper.SaveSnapShot(orderMod);
                #region 写入日志,更新订单状态
                switch (payMethod)
                {
                    case "Purse":
                        orderMod.Paymentstatus = (int)M_OrderList.PayEnum.HasPayed;
                        orderMod.Receivablesamount = orderMod.Ordersamount;
                        if (orderBll.Update(orderMod))
                        {
                            orderCom.SendMessage(orderMod, paylogMod, "payed");
                            paylogMod.PayMethod = (int)M_Order_PayLog.PayMethodEnum.Purse;
                            paylogMod.Remind += "商城订单" + orderMod.OrderNo + "余额付款成功";
                        }
                        break;
                    case "SilverCoin":
                        orderMod.Paymentstatus = (int)M_OrderList.PayEnum.HasPayed;
                        orderMod.Receivablesamount = orderMod.Ordersamount;
                        if (orderBll.Update(orderMod))
                        {
                            orderCom.SendMessage(orderMod, paylogMod, "payed");
                            paylogMod.PayMethod = (int)M_Order_PayLog.PayMethodEnum.Silver;
                            paylogMod.Remind += "商城订单" + orderMod.OrderNo + "银币付款成功";
                        }
                        break;
                    case "Score":
                        orderMod.Paymentstatus = (int)M_OrderList.PayEnum.HasPayed;
                        orderMod.Receivablesamount = orderMod.Ordersamount;
                        if (orderBll.Update(orderMod))
                        {
                            orderCom.SendMessage(orderMod, paylogMod, "payed");
                            paylogMod.PayMethod = (int)M_Order_PayLog.PayMethodEnum.Score;
                            paylogMod.Remind = "商城订单" + orderMod.OrderNo + "积分付款成功";
                        }
                        break;
                    default:
                        return CommonReturn.Failed("指定的支付方式不存在,请检查大小写是否正确!");
                        break;
                }
                //-----------------------付款后处理区域
                //orderCom.SaveSnapShot(orderMod);
                payMod.MoneyTrue = payMod.MoneyReal;
                OrderHelper.FinalStep(payMod, orderMod, paylogMod);//支付成功后处理步步骤,允许操作paylogMod
                #endregion
            }
            //-----------------For End
            //PayNum_L2.Text = payMod.MoneyReal.ToString("f2");
            //sjhbje.Text = payMod.MoneyReal.ToString("f2");
            payMod.Status = (int)M_Payment.PayStatus.HasPayed;
            payMod.CStatus = true;
            payBll.Update(payMod);
            return CommonReturn.Success();
        }
        #endregion
        //--------------------------------
        [Authorize(Policy = "User")]
        public IActionResult wxpay_mp()
        {
            return View();
        }
        public ContentResult WxPayReturn()//Notify
        {
            string result = new WXPayProcess(HttpContext).Process();
            return Content(result);
        }
        public ContentResult AlipayNotify()
        {
            string result = new AlipayProcess(HttpContext).Process();
            return Content(result);
        }
        public IActionResult AlipayReturn()
        {
            return View();
        }
        private class AlipayProcess
        {
            private string PayPlat = "支付宝在线支付:";
            B_Order_PayLog paylogBll = new B_Order_PayLog();//支付日志类,用于记录用户付款信息
            B_OrderList orderBll = new B_OrderList();
            B_CartPro cartBll = new B_CartPro();//数据库中购物车业务类
            B_User buser = new B_User();
            B_PayPlat payPlatBll = new B_PayPlat();
            B_Payment payBll = new B_Payment();
            OrderCommon orderCOM = new OrderCommon();
            private HttpRequest Request = null;
            private HttpResponse Response = null;
            public AlipayProcess(HttpContext ctx)
            {
                this.Request = ctx.Request;
                this.Response = ctx.Response;
            }
            public string Process()
            {
                string result = "fail";
                //DataTable pay = payPlatBll.GetPayPlatByClassid(12);
                M_PayPlat platMod = payPlatBll.SelModelByClass(M_PayPlat.Plat.Alipay_Instant);
                SortedDictionary<string, string> sArrary = GetRequestPost();
                ///////////////////////以下参数是需要设置的相关配置参数，设置后不会更改的//////////////////////
                ZoomLa.Model.M_Alipay_config con = new ZoomLa.Model.M_Alipay_config();
                string partner = platMod.AccountID;
                string key = platMod.MD5Key;
                string input_charset = con.Input_charset;
                string sign_type = con.Sign_type;
                string transport = con.Transport;
                //////////////////////////////////////////////////////////////////////////////////////////////
                if (sArrary.Count > 0)//判断是否有带返回参数
                {
                    B_Alipay_notify aliNotify = new B_Alipay_notify(sArrary, Request.Form["notify_id"], partner, key, input_charset, sign_type, transport);
                    string responseTxt = aliNotify.ResponseTxt; //获取远程服务器ATN结果，验证是否是支付宝服务器发来的请求
                    string sign = Request.Form["sign"];
                    string mysign = aliNotify.Mysign;           //获取通知返回后计算后（验证）的签名结果
                                                                //判断responsetTxt是否为ture，生成的签名结果mysign与获得的签名结果sign是否一致
                                                                //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
                                                                //mysign与sign不等，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
                    string order_no = Request.Form["out_trade_no"];     //获取订单号
                    ZLLog.L(ZLEnum.Log.pay, PayPlat + aliNotify.ResponseTxt + ":" + order_no + ":" + Request.Form["buyer_email"] + ":" + Request.Form["trade_status"] + ":" + Request.Form["price"] + ":" + Request.Form["subject"]);
                    if (responseTxt == "true" && sign == mysign)//验证成功
                    {
                        //——请根据您的业务逻辑来编写程序（以下代码仅作参考）——
                        //获取通知返回参数，可参考技术文档中服务器异步通知参数列表
                        string trade_no = Request.Form["trade_no"];         //交易号
                        string total_fee = Request.Form["price"];           //获取总金额
                        string subject = Request.Form["subject"];           //商品名称、
                        string body = Request.Form["body"];                 //商品描述、订单备注、描述
                        string buyer_email = Request.Form["buyer_email"];   //买家账号
                        string trade_status = Request.Form["trade_status"]; //交易状态
                        if (Request.Form["trade_status"] == "WAIT_BUYER_PAY")//没有付款
                        {

                        }
                        else if (trade_status.Equals("WAIT_SELLER_SEND_GOODS"))//付款成功，但是卖家没有发货
                        {

                        }
                        else if (trade_status.Equals("TRADE_SUCCESS"))//付款成功
                        {
                            try
                            {
                                M_Payment pinfo = payBll.SelModelByPayNo(order_no);
                                if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { return "fail"; }
                                pinfo.Status = (int)M_Payment.PayStatus.HasPayed;
                                pinfo.PlatformInfo = PayPlat;    //平台反馈信息
                                pinfo.SuccessTime = DateTime.Now;//交易成功时间
                                pinfo.CStatus = true; //处理状态
                                pinfo.AlipayNO = trade_no;//保存支付宝交易号
                                pinfo.MoneyTrue = Convert.ToDouble(total_fee);
                                payBll.Update(pinfo);
                                DataTable orderDT = orderBll.GetOrderbyOrderNo(pinfo.PaymentNum);
                                foreach (DataRow dr in orderDT.Rows)
                                {
                                    M_Order_PayLog paylogMod = new M_Order_PayLog();
                                    M_OrderList orderMod = orderBll.SelModelByOrderNo(dr["OrderNo"].ToString());
                                    OrderHelper.FinalStep(pinfo, orderMod, paylogMod);
                                    orderCOM.SendMessage(orderMod, paylogMod, "payed");
                                }
                                ZLLog.L(ZLEnum.Log.pay, PayPlat + "成功!支付单:" + order_no);
                                return "success";
                            }
                            catch (Exception ex)
                            {
                                ZLLog.L(ZLEnum.Log.pay, new M_Log()
                                {
                                    Action = "支付回调报错",
                                    Message = PayPlat + ",支付单:" + order_no + ",原因:" + ex.Message
                                });
                            }
                        }
                        else if (Request.Form["trade_status"] == "WAIT_BUYER_CONFIRM_GOODS")//卖家已经发货，等待买家确认
                        {

                        }
                        else if (Request.Form["trade_status"] == "TRADE_FINISHED")
                        {
                        }
                        else//其他状态判断。普通即时到帐中，其他状态不用判断，直接打印success。
                        {
                            ZLLog.L(PayPlat + "付款未成功截获,单号:[" + trade_status + "]");
                        }
                    }
                    else//验证失败
                    {
                        ZLLog.L(ZLEnum.Log.pay, new M_Log()
                        {
                            Action = "支付验证失败",
                            Message = PayPlat + ",支付单:" + order_no
                        });
                        return "fail";
                    }
                }
                else
                {
                    result = "success";
                }
                return result;
            }
            /// <summary>
            /// 获取POST过来通知消息，并以“参数名=参数值”的形式组成数组
            /// </summary>
            /// <returns>request回来的信息组成的数组</returns>
            private SortedDictionary<string, string> GetRequestPost()
            {
                //int i = 0;
                SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
                ////Load Form variables into NameValueCollection variable.
                //var coll = Request.Form;

                //// Get names of all forms into a string array.
                //String[] requestItem = coll.Keys;

                //for (i = 0; i < requestItem.Length; i++)
                //{
                //    sArray.Add(requestItem[i], Request.Form[requestItem[i]]);
                //}
                foreach (var item in Request.Form)
                {
                    sArray.Add(item.Key, Request.GetParam(item.Key));
                }
                return sArray;
            }
        }
        private class WXPayProcess
        {
            private string PayPlat = "微信PC扫码|公众号支付";
            ZoomLa.BLL.WxPayAPI.Notify wxnotify = null;
            B_Payment payBll = new B_Payment();
            B_Order_PayLog paylogBll = new B_Order_PayLog();//支付日志类,用于记录用户付款信息
            B_OrderList orderBll = new B_OrderList();
            OrderCommon orderCom = new OrderCommon();
            B_User buser = new B_User();

            private HttpRequest Request = null;
            private HttpResponse Response = null;
            public WXPayProcess(HttpContext ctx)
            {
                this.Request = ctx.Request;
                this.Response = ctx.Response;
            }
            public string Process()
            {
                wxnotify = new ZoomLa.BLL.WxPayAPI.Notify();
                return ProcessNotify();
            }
            private string ProcessNotify()
            {
                //如果有多个公众号支付,此处要取返回中的公众号ID,再取Key验证
                WxPayData notifyData = wxnotify.GetNotifyData(Request.Body);
                WxPayData res = GetWxDataMod();
                //检查支付结果中transaction_id是否存在
                if (!notifyData.IsSet("out_trade_no"))
                {
                    //若transaction_id不存在，则立即返回结果给微信支付后台
                    res.SetValue("return_code", "FAIL");
                    res.SetValue("return_msg", "支付结果中订单号不存在");
                    ZLLog.L(ZLEnum.Log.pay, new M_Log()
                    {
                        Action = "支付平台异常",
                        Message = PayPlat + ",原因:支付结果中订单号不存在!XML:" + notifyData.ToXml()
                    });
                    return res.ToXml();
                }
                string transaction_id = notifyData.GetValue("out_trade_no").ToString();
                //查询订单，判断订单真实性
                //if (!QueryOrder(transaction_id))
                //{
                //    //若订单查询失败，则立即返回结果给微信支付后台
                //    WxPayData res = GetWxDataMod();
                //    res.SetValue("return_code", "FAIL");
                //    res.SetValue("return_msg", "订单查询失败");
                //    ZLLog.L(ZLEnum.Log.pay, new M_Log()
                //    {
                //        Action = "支付平台异常",
                //        Message = PayPlat + ",支付单:" + transaction_id + ",原因:订单查询失败!XML:" + notifyData.ToXml()
                //    });
                //    return res.ToXml();
                //}
                //查询订单成功
                //else
                //{
                //}
                //未指定,则默认加载PC扫码配置
                M_Payment pinfo = payBll.SelModelByPayNo(notifyData.GetValue("out_trade_no").ToString());
                M_WX_APPID appMod = WxPayApi.Pay_GetByID(DataConvert.CLng(pinfo.PlatformInfo));
                notifyData.PayKey = appMod.Pay_Key;
                try
                {
                    notifyData.CheckSign();
                    PayOrder_Success(pinfo, notifyData);
                }
                catch (Exception ex)
                {
                    ZLLog.L(ZLEnum.Log.pay, new M_Log() { Action = PayPlat + "报错,支付单:" + transaction_id, Message = ex.Message + "||XML:" + notifyData.ToXml() });
                }
                res.SetValue("return_code", "SUCCESS");
                res.SetValue("return_msg", "OK");
                return res.ToXml();
            }
            //支付成功时执行的操作
            private void PayOrder_Success(M_Payment pinfo, WxPayData result)
            {
                ZLLog.L(ZLEnum.Log.pay, PayPlat + " 支付单:" + result.GetValue("out_trade_no") + " 金额:" + result.GetValue("total_fee"));
                try
                {
                    M_Order_PayLog paylogMod = new M_Order_PayLog();
                    if (pinfo == null) { throw new Exception("支付单不存在"); }//支付单检测合为一个方法
                    if (pinfo.Status != (int)M_Payment.PayStatus.NoPay) { throw new Exception("支付单状态不为未支付"); }
                    pinfo.Status = (int)M_Payment.PayStatus.HasPayed;
                    pinfo.PlatformInfo += PayPlat;
                    pinfo.SuccessTime = DateTime.Now;
                    pinfo.PayTime = DateTime.Now;
                    pinfo.CStatus = true;
                    //1=100,
                    double tradeAmt = Convert.ToDouble(result.GetValue("total_fee")) / 100;
                    pinfo.MoneyTrue = tradeAmt;
                    payBll.Update(pinfo);
                    DataTable orderDT = orderBll.GetOrderbyOrderNo(pinfo.PaymentNum);
                    foreach (DataRow dr in orderDT.Rows)
                    {
                        M_OrderList orderMod = orderBll.SelModelByOrderNo(dr["OrderNo"].ToString());
                        OrderHelper.FinalStep(pinfo, orderMod, paylogMod);
                        //if (orderMod.Ordertype == (int)M_OrderList.OrderEnum.Purse)
                        //{

                        //    M_UserInfo mu = buser.SelReturnModel(orderMod.Userid);
                        //    new B_Shop_MoneyRegular().AddMoneyByMin(mu, orderMod.Ordersamount, ",订单号[" + orderMod.OrderNo + "]");
                        //}
                        orderCom.SendMessage(orderMod, paylogMod, "payed");
                        //orderCom.SaveSnapShot(orderMod);
                    }
                    ZLLog.L(ZLEnum.Log.pay, PayPlat + "成功!支付单:" + result.GetValue("out_trade_no").ToString());
                }
                catch (Exception ex)
                {
                    ZLLog.L(ZLEnum.Log.pay, new M_Log()
                    {
                        Action = "支付回调报错",
                        Message = PayPlat + ",支付单:" + result.GetValue("out_trade_no").ToString() + ",原因:" + ex.Message
                    });
                }
            }
            //查询订单
            private bool QueryOrder(string transaction_id)
            {
                WxPayData req = GetWxDataMod();
                req.SetValue("out_trade_no", transaction_id);
                WxPayData res = WxPayApi.OrderQuery(req, WxPayApi.Pay_GetByID());
                if (res.GetValue("return_code").ToString() == "SUCCESS" &&
                    res.GetValue("result_code").ToString() == "SUCCESS")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            private WxPayData GetWxDataMod() { return new WxPayData(); }
        }
    }
}
