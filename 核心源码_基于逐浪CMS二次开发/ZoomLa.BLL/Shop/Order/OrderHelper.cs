using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;


namespace ZoomLa.BLL.Shop
{
    public class OrderHelper
    {
        private static B_OrderList orderBll = new B_OrderList();
        private static B_CartPro cartProBll = new B_CartPro();
        //----------------生成状态
        //初始(无色),进程中(淡蓝色),退货等(红色),比红色重要(橙色),完成(绿色),删除(灰色)
        /// <summary>
        /// 付款,支付方式,订单,物流,是否删除
        /// </summary>
        public enum TypeEnum { Pay, PayType, Order, Express, Aside };
        /// <summary>
        /// DataRowView.Row
        /// </summary>
        public static string GetStatus(DataRow dr, TypeEnum type)
        {
            M_OrderList orderMod = new M_OrderList();
            orderMod.Paymentstatus = Convert.ToInt32(dr["Paymentstatus"]);
            orderMod.OrderStatus = Convert.ToInt32(dr["OrderStatus"]);
            orderMod.StateLogistics = Convert.ToInt32(dr["StateLogistics"]);
            orderMod.Aside = Convert.ToInt32(dr["Aside"]);
            orderMod.PayType = Convert.ToInt32(dr["Delivery"]);
            return GetStatus(orderMod, type);
        }
        /// <summary>
        /// 返回状态信息(后期扩展为与支付单一起做判断)
        /// </summary>
        public static string GetStatus(M_OrderList orderMod, TypeEnum type)
        {
            switch (type)
            {
                case TypeEnum.Pay:
                    return GetPayStatus(orderMod.Paymentstatus);
                case TypeEnum.Order:
                    {
                        if (orderMod.Aside > 0) { return GetOrderStatus(orderMod.OrderStatus, orderMod.Aside, orderMod.StateLogistics); }
                        //订单已处理,但用户尚未签收
                        else if (orderMod.OrderStatus == 99 &&
                            (orderMod.StateLogistics == (int)M_OrderList.ExpEnum.NoSend || orderMod.StateLogistics == (int)M_OrderList.ExpEnum.HasSend))
                        {
                            return GetOrderStatus(2);
                        }
                        return GetOrderStatus(orderMod.OrderStatus);
                    }
                case TypeEnum.Express:
                    return GetExpStatus(orderMod.StateLogistics);
                case TypeEnum.Aside:
                    return GetOrderStatus(orderMod.OrderStatus,orderMod.Aside,orderMod.StateLogistics);
                case TypeEnum.PayType:
                    return OrderConfig.GetPayType(orderMod.PayType);
                default:
                    throw new Exception(type.ToString() + "状态码错误");
            }
        }
        public static string GetOrderStatus(int orderStatus, int aside, int exp)
        {
            if (aside == 1) { return "<span style='color:gray;'>已作废</span>"; }
            else { return GetOrderStatus(orderStatus); }
        }
        //订单状态
        public static string GetOrderStatus(int orderstaus)
        {
            try
            {
                return OrderConfig.GetOrderStatus(orderstaus);
            }
            catch
            {
                return "<span style='color:orange'>异常状态</span>";
            }
        }
        // 是否应该分离为支付类型与是否已支付?
        public static string GetPayStatus(int payemntStatus)
        {
            try{ return OrderConfig.GetPayStatus(payemntStatus); }
            catch (Exception){ return "<span style='color:orange;'>异常状态</span>"; }
        }
        //物流状态
        public static string GetExpStatus(int status)
        {
            try{return OrderConfig.GetExpStatus(status);}
            catch (Exception){ return "<span color=red>异常状态</span>"; }
        }
        //----------------状态判断返回True,False
        /// <summary>
        /// 该订单是否允许支付
        /// </summary>
        public static bool IsCanPay(DataRow dr)
        {
            OrderInfo model = new OrderInfo(dr);
            if (model.Aside != 0) { return false; }
            if (model.PayStatus != (int)M_OrderList.PayEnum.NoPay) { return false; }
            if (model.OrderStatus < (int)M_OrderList.StatusEnum.Normal) { return false; }
            if (model.OrderStatus >= (int)M_OrderList.StatusEnum.OrderFinish) { return false; }
            return true;
        }
        /// <summary>
        /// 该订单是否已付款
        /// </summary>
        public static bool IsHasPayed(DataRow dr)
        {
            OrderInfo model = new OrderInfo(dr);
            return (model.PayStatus >= (int)M_OrderList.PayEnum.HasPayed);
        }
        //如果列中无此值,则默认为有效状态
        private class OrderInfo 
        {
            public OrderInfo(DataRow dr) 
            {
                PayStatus = Convert.ToInt32(dr["PaymentStatus"]);
                OrderStatus = Convert.ToInt32(dr["OrderStatus"]);
                if (dr.Table.Columns.Contains("Aside"))
                { Aside = Convert.ToInt32(dr["Aside"]); }
            }
            public int PayStatus = 0;
            public int OrderStatus = 0;
            public int Aside = 0;
        }
        //----------------付款后回调
        public delegate void OrderFinish_DE(M_OrderList orderMod, M_Payment pinfo);
        public static event OrderFinish_DE OrderFinish_Event;
        /// <summary>
        /// 用于后台确认支付
        /// </summary>
        public static void FinalStep(M_OrderList mod)
        {
            if (mod.id < 1) { throw new Exception("未指定订单ID"); }
            if (mod.Ordertype < 0) { throw new Exception("未指定订单类型"); }
            if (string.IsNullOrEmpty(mod.OrderNo)) { throw new Exception("未指定订单号"); }
            //M_AdminInfo adminMod = B_Admin.GetLogin();
            B_Payment payBll = new B_Payment();
            M_Payment pinfo = new M_Payment();
            pinfo.PaymentNum = mod.OrderNo;
            pinfo.UserID = mod.Userid;
            pinfo.PayNo = payBll.CreatePayNo();
            pinfo.MoneyPay = mod.Ordersamount;
            pinfo.MoneyTrue = pinfo.MoneyPay;//看是否需要支持手输
            pinfo.Status = (int)M_Payment.PayStatus.HasPayed;
            pinfo.CStatus = true;
            //if (adminMod != null)
            //{
            //    pinfo.Remark = "管理员确认支付,ID:" + adminMod.AdminId + ",登录名:" + adminMod.AdminName + ",真实姓名:" + adminMod.AdminTrueName;
            //}
            pinfo.PaymentID = payBll.Add(pinfo);
            pinfo.SuccessTime = DateTime.Now;
            pinfo.PayPlatID = (int)M_PayPlat.Plat.CashOnDelivery;//默认为线下支付
            M_Order_PayLog paylogMod = new M_Order_PayLog();
            FinalStep(pinfo, mod, paylogMod);
        }
        /// <summary>
        /// 异步回调后-->验证支付单状态-->如果正常,更新订单状态
        /// 多张订单在外层循环,这里只处理单订单
        /// </summary>
        /// <param name="mod">订单模型</param>
        /// <param name="paylogMod">订单支付日志模型</param>
        public static void FinalStep(M_Payment pinfo, M_OrderList mod, M_Order_PayLog paylogMod)
        {
            //B_Order_PayLog paylogBll = new B_Order_PayLog();
            B_PayPlat platBll = new B_PayPlat();
            B_Payment payBll = new B_Payment();
            B_User buser = new B_User();
            //[*]特殊处理,预付与尾款逻辑
            {
                #region
                if (pinfo.PayType == (int)M_OrderList.PayTypeEnum.PrePay)
                {
                    M_PrePayinfo preInfo = new M_PrePayinfo(pinfo.PrePayInfo);
                    preInfo.money_pre_payed = pinfo.MoneyTrue;
                    preInfo.pre_payno = pinfo.PayNo;
                    preInfo.pre_payMethod = platBll.GetPayPlatName(pinfo);
                    pinfo.PrePayInfo = JsonConvert.SerializeObject(preInfo);
                    DBCenter.UpdateSQL(pinfo.TbName, "PrePayInfo=@preInfo", "PayMentID=" + pinfo.PaymentID,
                          new List<SqlParameter>() { new SqlParameter("preInfo", pinfo.PrePayInfo) });
                }
                else if (pinfo.PayType == (int)M_OrderList.PayTypeEnum.AfterPay)
                {
                    try
                    {
                        M_OrderList realOrder = orderBll.SelModelByOrderNo(mod.Ordermessage);
                        M_Payment preMod = payBll.SelModelByPayNo(realOrder.PaymentNo);
                        //修改订单与支付单信息
                        M_PrePayinfo preInfo = new M_PrePayinfo(preMod.PrePayInfo);
                        preInfo.money_after_payed = pinfo.MoneyTrue;
                        preInfo.status = (int)ZLEnum.ConStatus.Audited;
                        preInfo.after_payno = pinfo.PayNo;
                        preInfo.after_payMethod = platBll.GetPayPlatName(pinfo);
                        //修改二张订单与二张支付单的状态
                        realOrder.IsCount = true;
                        realOrder.Ordersamount += pinfo.MoneyTrue;
                        realOrder.Receivablesamount += pinfo.MoneyTrue;
                        orderBll.UpdateByID(realOrder);
                        mod.OrderStatus = 99;
                        mod.Paymentstatus = 1;
                        mod.PaymentNo = pinfo.PayNo;
                        orderBll.UpdateByID(mod);
                        DBCenter.UpdateSQL(preMod.TbName, "PrePayInfo=@preInfo", "PayMentID=" + preMod.PaymentID,
                            new List<SqlParameter>() { new SqlParameter("preInfo", JsonConvert.SerializeObject(preInfo)) });
                        return;
                    }
                    catch (Exception ex)
                    {
                        ZLLog.L(Model.ZLEnum.Log.pay, new M_Log()
                        {
                            Action = "支付尾款异常",
                            Message = "订单号:" + mod.OrderNo + ",支付单:" + pinfo.PayNo + ",原因:" + ex.Message
                        });
                        return;
                    }
                }
                #endregion
            }
            //订单已处理,避免重复（如已处理过,则继续处理下一张订单）
            if (mod.OrderStatus >= 99)
            {
                ZLLog.L(Model.ZLEnum.Log.pay, new M_Log()
                {
                    Action = "支付回调异常,订单状态已为99",
                    Message = "订单号:" + mod.OrderNo + ",支付单:" + pinfo.PayNo
                });
                return;
            }
            //已经收到钱了,所以先执行(如多订单,则为订单金额)
            if (mod.Receivablesamount <= 0)
            {
                //DD201701112113293112618,DD201701112113293754790
                int orderNum = pinfo.PaymentNum.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
                if (orderNum == 1) { mod.Receivablesamount = pinfo.MoneyTrue; }
                else
                {
                    //订单需要改进,支持优惠后的金额等,不能只以支付单中的为准,每张订单需要计算出自己的优惠额与积分支付额
                    mod.Receivablesamount = mod.Ordersamount;
                }
            }
            orderBll.UpOrderinfo("Paymentstatus=1,Receivablesamount=" + mod.Receivablesamount, mod.id);
            //if (mod.Ordertype == (int)M_OrderList.OrderEnum.Domain)//域名订单
            //{
            //    orderBll.UpOrderinfo("OrderStatus=1,PaymentNo='" + pinfo.PayNo + "'", mod.id);
            //    //Response.Redirect("~/Plugins/Domain/DomReg2?OrderNo=" + mod.OrderNo);
            //}
            //else if (mod.Ordertype == (int)M_OrderList.OrderEnum.IDC)//IDC服务
            //{
            //    B_Order_IDC idcBll = new B_Order_IDC();
            //    orderBll.FinishOrder(mod.id, pinfo);
            //    idcBll.UpdateEndTimeByNo(mod.OrderNo);
            //}
            //else if ((mod.Ordertype == (int)M_OrderList.OrderEnum.IDCRen))//IDC服务续费
            //{
            //    B_Order_IDC idcBll = new B_Order_IDC();
            //    orderBll.FinishOrder(mod.id, pinfo);
            //    idcBll.RennewTime(mod);
            //}
            if (mod.Ordertype == (int)M_OrderList.OrderEnum.Purse)//余额充值,不支持银币
            {
                buser.AddMoney(mod.Userid, mod.Ordersamount,
                          M_UserExpHis.SType.Purse, "余额充值,订单号:" + mod.OrderNo);
                //检测是否采用了充值赠送规则
                if (DataConvert.CLng(mod.Money_code) > 0)
                {
                    B_Shop_MoneyRegular regBll = new B_Shop_MoneyRegular();
                    M_Shop_MoneyRegular regMod = regBll.SelReturnModel(DataConvert.CLng(mod.Money_code));
                    if (regMod.Purse > 0)
                    {
                        buser.AddMoney(mod.Userid, regMod.Purse,
                          M_UserExpHis.SType.Purse, "余额充值--余额赠送,订单号:" + mod.OrderNo);
                    }
                    if (regMod.Sicon > 0)
                    {
                        buser.AddMoney(mod.Userid, regMod.Sicon,
                          M_UserExpHis.SType.SIcon, "余额充值--银币赠送,订单号:" + mod.OrderNo);
                    }
                    if (regMod.Point > 0)
                    {
                        buser.AddMoney(mod.Userid, regMod.Point,
                          M_UserExpHis.SType.Point, "余额充值--积分赠送,订单号:" + mod.OrderNo);
                    }
                }
                orderBll.FinishOrder(mod.id, pinfo);//成功的订单
            }
            else if (mod.Ordertype == (int)M_OrderList.OrderEnum.Cloud)//虚拟商品订单
            {
                orderBll.FinishOrder(mod.id, pinfo);
            }
            else//其他旅游订单等,只更新状态
            {
                orderBll.FinishOrder(mod.id, pinfo);//成功的订单
            }
            //-------支付成功处理,快照并写入日志
            SaveSnapShot(mod);
            //paylogMod.Remind += "订单" + mod.OrderNo + "购买生效";
            //paylogMod.UserID = mod.Userid;
            //paylogMod.OrderID = mod.id;
            //paylogMod.PayMoney = mod.Ordersamount;
            //paylogMod.PayMethod = (int)M_Order_PayLog.PayMethodEnum.Other;//外部指定
            //paylogMod.PayPlatID = pinfo.PayPlatID;
            //paylogBll.insert(paylogMod);
            //------商品是否赠送积分
            //{//好酒多积分不直接入账
            //    DataTable prodt = DBCenter.JoinQuery("A.ProID,A.Pronum,B.PointVal", "ZL_CartPro", "ZL_Commodities", "A.ProID=B.ID", "A.OrderListID=" + mod.id);
            //    foreach (DataRow dr in prodt.Rows)
            //    {
            //        double point = DataConvert.CDouble(dr["PointVal"])*Convert.ToInt32(dr["Pronum"]);
            //        if (point > 0)
            //        {
            //            buser.AddMoney(mod.Userid, point, M_UserExpHis.SType.Point, "购买商品[" + dr["ProID"] + "],赠送积分");
            //        }
            //    }
            //}
            if (OrderFinish_Event != null)
            {
                OrderFinish_Event(mod, pinfo);
            }
        }
        /// <summary>
        /// 商品快照,存为mht,
        /// mht缺点 :在于非IE下只能下载,但360等双核浏览器可以自动切换
        /// html缺点:只是保存了页面不存图片和css,这样如果页面删除,则快照失效
        /// </summary>
        public static void SaveSnapShot(M_OrderList orderMod)
        {
            try
            {
                string snapDir = "/UploadFiles/SnapDir/" + orderMod.Rename + orderMod.Userid + "/" + orderMod.OrderNo + "/";
                DataTable dt = cartProBll.SelByOrderID(orderMod.id);
                foreach (DataRow dr in dt.Rows)
                {
                    int storeid = DataConverter.CLng(dr["StoreID"]);
                    int proid = DataConverter.CLng(dr["Proid"]);
                    string url = SiteConfig.SiteInfo.SiteUrl + GetShopUrl(storeid, proid);
                    new HtmlHelper().DownToMHT(url, snapDir + proid + ".mht");
                }
            }
            catch (Exception ex) { ZLLog.L(Model.ZLEnum.Log.exception, "订单:" + orderMod.OrderNo + "快照保存失败,原因:" + ex.Message); }
        }
        #region 附加币价格判断(disuse)为保兼容暂不删
        public static string GetPriceStr(double price, object price_obj)
        {
            if (!HasPrice(price_obj)) //无附加商品价
            {
                return price.ToString("f2");
            }
            string price_json = price_obj.ToString();
            string html = "现金:<span>" + price.ToString("f2") + "</span>";
            if (HasPrice(price_json))
            {
                string json = DataConvert.CStr(price_json);
                M_LinPrice priceMod = JsonConvert.DeserializeObject<M_LinPrice>(json);
                if (priceMod.Purse > 0)
                {
                    html += "|余额:<span>" + priceMod.Purse.ToString("f2") + "</span>";
                }
                if (priceMod.Sicon > 0)
                {
                    html += "|银币:<span>" + priceMod.Sicon.ToString("f0") + "</span>";
                }
                if (priceMod.Point > 0)
                {
                    html += "|积分:<span>" + priceMod.Point.ToString("f0") + "</span>";
                }
            }
            return html;
        }
        public static string GetPriceStr(M_OrderList model)
        {
            return GetPriceStr(model.Ordersamount, model.AllMoney_Json);
        }
        public static string GetPriceStr(M_Cart model)
        {
            return GetPriceStr(model.AllMoney, model.AllMoney_Json);
        }
        /// <summary>
        /// 是否有附加商品价True:有
        /// </summary>
        public static bool HasPrice(object obj)
        {
            if (obj == null || obj == DBNull.Value || string.IsNullOrEmpty(obj.ToString())) return false;
            string json = DataConvert.CStr(obj);
            M_LinPrice priceMod = JsonConvert.DeserializeObject<M_LinPrice>(json);
            return (priceMod.Purse > 0 || priceMod.Sicon > 0 || priceMod.Point > 0);
        }
        //-------------------支付前订单和支付单检测
        #endregion
        /// <summary>
        /// 订单是否可支付检测,支持支付单与订单,检测状态,过期时间,金额
        /// </summary>
        public static List<M_OrderList> OrdersCheck(M_Payment payMod)
        {
            if (payMod.Status != (int)M_Payment.PayStatus.NoPay) { function.WriteErrMsg("支付单付款状态不正确"); return null; }
            if (payMod.IsDel == 1) { function.WriteErrMsg("支付单已被删除"); return null; }
            string[] orders = payMod.PaymentNum.Split(',');
            List<M_OrderList> orderList = new List<M_OrderList>();
            for (int i = 0; i < orders.Length; i++)//全部检测,支付单中订单有任一不符合条件则不允许购买
            {
                M_OrderList orderMod = orderBll.SelModelByOrderNo(orders[i]);
                CheckIsCanPay(orderMod);
                orderList.Add(orderMod);
            }
            return orderList;
        }
        public static void CheckIsCanPay(M_OrderList orderMod)
        {
            if (orderMod == null) { function.WriteErrMsg("订单不存在"); }
            if (string.IsNullOrEmpty(orderMod.OrderNo)) { function.WriteErrMsg("未指定订单号"); }
            if (orderMod.OrderStatus < (int)M_OrderList.OrderEnum.Normal) { function.WriteErrMsg(orderMod.OrderNo + "订单状态异常,无法完成支付"); }
            if (orderMod.Ordersamount < 0) { function.WriteErrMsg(orderMod.OrderNo + "订单应付金额异常"); }
            if (orderMod.Paymentstatus != (int)M_OrderList.PayEnum.NoPay) { function.WriteErrMsg(orderMod.OrderNo + "订单已支付过,不能重复付款!"); }
            //配置文件,开放性检测
            if (SiteConfig.ShopConfig.IsCheckPay == 1 && orderMod.OrderStatus == (int)M_OrderList.OrderEnum.Normal) { function.WriteErrMsg(orderMod.OrderNo + "订单未确认,请等待确认后再支付!"); }
            if (SiteConfig.ShopConfig.OrderExpired > 0 && (DateTime.Now - orderMod.AddTime).TotalHours > SiteConfig.ShopConfig.OrderExpired) { function.WriteErrMsg(orderMod.OrderNo + "订单已过期,关闭支付功能!"); }
            //if (orderMod.Ordertype == 8)//需要检测库存的商品,如有任意一项不足，则订单不允许进行,主用于云购
            //{
            //    if (!cartProBll.CheckStock(orderMod.id)) function.WriteErrMsg(orderMod.OrderNo + "中的商品库存数量不足,取消购买");
            //}
        }
        //-------------------其它公用方法
        /// <summary>
        /// 获取店铺链接
        /// </summary>
        public static string GetShopUrl(object storeid, object proid)
        {
            return GetShopUrl(DataConvert.CLng(storeid), Convert.ToInt32(proid));
        }
        public static string GetShopUrl(int storeid, int proid)
        {
            return "/Shop/" + proid + "";
        }
        public static string GetSnapUrl(object uid, object orderNo, object proid)
        {
            if (StrHelper.StrNullCheck(uid.ToString(), orderNo.ToString(), proid.ToString())) { return ""; }
            string url = "/UploadFiles/SnapDir/" + uid + "/" + orderNo + "/" + proid + ".html";
            if (File.Exists(function.VToP(url)))
            {
                return "<a href='" + url + "' target='_blank'>[交易快照]</a>";
            }
            return "";
        }
        /// <summary>
        /// 商品类型与订单类型的转换,后期优化
        /// </summary>
        public static int GetOrderType(int proclass)
        {
            int type = 0;
            switch (proclass)
            {
                case 3://积分
                    type = (int)M_OrderList.OrderEnum.Score;
                    break;
                case 5://虚拟商品
                    type = (int)M_OrderList.OrderEnum.Cloud;
                    break;
                case 6:
                    type = (int)M_OrderList.OrderEnum.IDC;
                    break;
                case 8:
                    type = (int)M_OrderList.OrderEnum.Hotel;
                    break;
                case 7:
                    type = (int)M_OrderList.OrderEnum.Trval;
                    break;
                default:
                    type = (int)M_OrderList.OrderEnum.Normal;
                    break;
            }
            return type;
        }

        //--------------------------------------------------------------
        /// <summary>
        /// 仅用于列表页,获取订单对应的支付单信息,以便重复付款和显示优惠信息
        /// </summary>
        /// <returns></returns>
        public static M_Payment GetPaymentByOrderNo(DataRow dr)
        {
            return GetPaymentByOrderNo(DataConvert.CStr(dr["PaymentNo"]), DataConvert.CStr(dr["OrderNo"]), Convert.ToDouble(dr["OrdersAmount"]));
        }
        public static M_Payment GetPaymentByOrderNo(string payno, string orderno, double allmoney)
        {
            B_Payment payBll = new B_Payment();
            M_Payment payMod = null;
            if (!string.IsNullOrEmpty(payno))
            {
                payMod = payBll.SelModelByPayNo(payno);
            }
            else if (!string.IsNullOrEmpty(orderno))
            {
                payMod = payBll.SelModelByOrderNo(orderno);
            }
            if (payMod == null)
            {
                payMod = new M_Payment();
                payMod.MoneyReal = allmoney;
            }  
            return payMod;
        }
        //--------------------------------------------------------------
        /// <summary>
        /// 订单是否完结(订单状态|支付状态|物流状态联合判断)
        /// </summary>
        /// <param name="mode">校验模式 normal|back</param>
        public static bool IsFinished(DataRow dr, string mode = "")
        {
            int OrderStatus = DataConvert.CLng(dr["OrderStatus"]);
            int Paymentstatus = DataConvert.CLng(dr["Paymentstatus"]);
            int PayType = DataConvert.CLng(dr["Delivery"]);
            int StateLogistics = DataConvert.CLng(dr["StateLogistics"]);
            bool IsCount = Convert.ToBoolean(dr["IsCount"]);
            bool flag = false;
            //正常完结
            if (mode == "normal" || mode == "")
            {
                if (OrderStatus >= (int)M_OrderList.StatusEnum.OrderFinish
                    && Paymentstatus >= (int)M_OrderList.PayEnum.HasPayed
                    && StateLogistics == (int)M_OrderList.ExpEnum.HasReceived){
                    //如果是预付方式,则需要判断是否付清尾款
                    if (PayType == (int)M_OrderList.PayTypeEnum.PrePay) { flag = IsCount; }
                    else { flag = true; }
                }
            }
            if (mode == "back" || mode == "")
            {
                //退款完结
                if (OrderStatus == (int)M_OrderList.StatusEnum.CheckDrawBack
                    && Paymentstatus == (int)M_OrderList.PayEnum.Refunded
                    && StateLogistics == (int)M_OrderList.ExpEnum.HasBack) { flag = true; }
            }
            return flag;
        }
    }
}
