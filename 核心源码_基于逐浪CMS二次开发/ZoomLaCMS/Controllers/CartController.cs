using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Models.Cart;
using ZoomLaCMS.Ctrl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ZoomLa.BLL.API;

namespace ZoomLaCMS.Controllers
{
    public class CartController : Ctrl_User
    {
        B_Payment payBll = new B_Payment();
        B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        B_CartPro cpBll = new B_CartPro();
        B_Cart cartBll = new B_Cart();
        B_Product proBll = new B_Product();
        B_OrderList orderBll = new B_OrderList();
        B_Order_Invoice invBll = new B_Order_Invoice();
        B_Shop_Present ptBll = new B_Shop_Present();
        B_Arrive avBll = new B_Arrive();
        B_UserRecei receBll = new B_UserRecei();
        OrderCommon orderCom = new OrderCommon();
        #region 购物车
        public int StoreID { get { return DataConvert.CLng(GetParam("StoreID"), -100); } }
        //仅用于标识显示积分商品,或普通商品,不参与其他逻辑
        public int ProClass { get { return DataConvert.CLng(GetParam("Proclass"),1); } }
        //Cookies中的购物车ID new HttpContextWrapper(HttpContext)
        private string _CartCookID = "";
        public string CartCookID
        {
            get
            {
                if (string.IsNullOrEmpty(_CartCookID)) {_CartCookID= B_Cart.GetCartID(HttpContext); }
                return _CartCookID;
            }
        }
        public IActionResult Cart()
        {
            if (!mu.IsNull && mu.Status != 0) { return WriteErr("你的帐户未通过验证或被锁定"); }
            int proid = DataConvert.CLng(GetParam("id"));
            if (proid < 1) { proid = DataConvert.CLng(GetParam("proid")); }
            //int suitid = DataConvert.CLng(GetParam("suitid"));
            int pronum = DataConvert.CLng(GetParam("pronum"), 1);
            int pclass = -1;//非-1则为添加了商品,需要跳转
            if (proid > 0)
            {
                M_Product proMod = proBll.GetproductByid(proid);
                if (proMod == null) { return WriteErr("商品不存在"); }
                AddToCart(mu, proMod, pronum);
                pclass = proMod.ProClass;
            }
            if (Request.IsAjaxRequest()) { return Content(Success.ToString()); }//ajax下不需要数据绑定与跳转
            //通过页面访问
            if (pclass > -1) { Response.Redirect("Cart?ProClass=" + pclass); }
            B_Cart.UpdateUidByCartID(CartCookID, mu);
            VM_Cart model = new VM_Cart(HttpContext,mu);
            model.CartDT = cartBll.SelByCartID(CartCookID, mu.UserID, ProClass);//从数据库中获取
            if (StoreID != -100)//仅显示指定商铺的商品
            {
                model.CartDT.DefaultView.RowFilter = "StoreID=" + StoreID;
                model.CartDT = model.CartDT.DefaultView.ToTable();
            }
            model.StoreDT = orderCom.SelStoreDT(model.CartDT);
            //totalmoney.InnerText = TotalPrice.ToString("f2");
            return View(model);
        }
        [HttpPost]
        public ContentResult OrderCom()
        {
            M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
            switch (action)
            {
                case "cart_del":
                    {
                        cartBll.DelByIDS(CartCookID, buser.GetLogin().UserName, ids);
                        retMod.retcode = M_APIResult.Success;
                    }
                    break;
                case "setnum"://兼容
                case "cart_setnum"://ID,数量,Cookies,可不登录,数量不能小于1
                    {
                        int id = DataConverter.CLng(GetParam("id"));
                        int pronum = DataConverter.CLng(GetParam("pronum"));
                        if (id < 1 || pronum < 1)
                        {
                            retMod.retmsg = "商品ID与数量不能小于1";
                        }
                        else if (string.IsNullOrEmpty(CartCookID))
                        {
                            retMod.retmsg = "CartCookID不存在";
                        }
                        else
                        {
                            cartBll.UpdateProNum(CartCookID, mu.UserID, id, pronum);
                            retMod.retcode = M_APIResult.Success;
                        }
                    }
                    break;
                case "deladdress":
                    {
                        int id = DataConverter.CLng(GetParam("id"));
                        if (mu == null || mu.UserID == 0 || id < 1) { return Content(Failed.ToString()); }
                        else
                        {
                            receBll.U_DelByID(id, mu.UserID); return Content(Success.ToString());
                        }
                    }
                case "arrive":
                    {
                        string flow = GetParam("flow");
                        double money = double.Parse(GetParam("money"));
                        DataTable cartdt = cartBll.SelByCartID(CartCookID, mu.UserID, -100, ids);
                        if (cartdt.Rows.Count < 1) { retMod.retmsg = "购物车为空"; return Content(retMod.ToString()); }
                        M_Arrive avMod = avBll.SelModelByFlow(flow, mu.UserID);
                        M_Arrive_Result arrMod = avBll.U_CheckArrive(avMod, mu.UserID, cartdt, money);
                        if (arrMod.enabled)
                        {
                            retMod.retcode = M_APIResult.Success;
                            //已优惠金额,优惠后金额
                            retMod.result = Newtonsoft.Json.JsonConvert.SerializeObject(arrMod);
                        }
                        else { retMod.retmsg = arrMod.err; }
                    }
                    break;
                default:
                    retMod.retmsg = "[" + action + "]接口不存在";
                    break;
            }
            return Content(retMod.ToString());
        }
        //跳转至结算页
        [HttpPost]
        [Authorize(Policy="User")]
        public IActionResult Cart_Submit()
        {
            //AJAX就先检测一遍,未登录则弹窗
            B_Cart.UpdateUidByCartID(CartCookID, mu,false);
            string ids = GetParam("prochk");
            return Redirect("GetOrderInfo?ids=" + ids + "&ProClass=" + ProClass);//"#none"
        }
        //------------------------------Tools
        // 生成购物车编号("Shopby OrderNo"的value) 
        //返回当前登录用户,如未登录,则返回0
        private M_UserInfo GetLogin()
        {
            if (mu == null||mu.IsNull)
            {
                mu = new M_UserInfo();
                mu.UserID = 0;
                mu.UserName = "未登录";
            }
            return mu;
        }
        //根据传参将商品加入购物车后跳转(支持按商品ID,套装ID购买)
        private void AddToCart(M_UserInfo mu, M_Product proMod, int pronum)
        {
            if (pronum < 1) { pronum = 1; }
            if (proMod == null || proMod.ID < 1) {  }//商品不存在
            if (proMod.ProClass == (int)M_Product.ClassType.IDC) { Response.Redirect("/Cart/FillIDCInfo?proid=" + proMod.ID); }
            OrderCommon.ProductCheck(proMod);
            //-----------------检测完成加入购物车
            M_Cart cartMod = new M_Cart();
            cartMod.Cartid = CartCookID;
            cartMod.StoreID = proMod.UserShopID;
            cartMod.ProID = proMod.ID;
            cartMod.Pronum = pronum;
            cartMod.userid = mu.UserID;
            cartMod.Username = mu.UserName;
            cartMod.FarePrice = proMod.LinPrice.ToString();
            cartMod.ProAttr = GetParam("attr");
            cartMod.Proname = proMod.Proname;
            int id = cartBll.AddModel(cartMod);
        }

        #endregion
        #region 订单
        private string[] verifyFields = "ID,ProID,Pronum,FarePrice,AllMoney".Split(',');
        //private DataTable VerifyDT
        //{
        //    get
        //    {
        //        return HttpContext.Session.Get("VerifyDT") as DataTable;
        //    }
        //    set { Session["VerifyDT"] = value; }
        //}
        [Authorize(Policy = "User")]
        public IActionResult GetOrderInfo()
        {
            M_UserInfo mu = buser.GetLogin(false);
            if (mu.Status != 0) { return WriteErr("你的帐户未通过验证或被锁定"); }
            if (string.IsNullOrEmpty(ids)) { return WriteErr("请先选定需要购买的商品"); }
            //--------------------MyBind
            //StringWriter sw = new StringWriter();
            VM_CartOrder vmMod = new VM_CartOrder();
            DataTable CartDT = vmMod.CartDT = cartBll.SelByCartID(CartCookID, mu.UserID, ProClass, ids);
            if (vmMod.CartDT.Rows.Count < 1)
            {
                return WriteErr("你尚未选择商品,<a href='/User/Order/OrderList'>查看我的订单</a>");
            }
            #region 旅游,酒店等不需要检测地址栏
            switch (DataConvert.CLng(vmMod.CartDT.Rows[0]["ProClass"]))
            {
                case (int)M_Product.ClassType.LY:
                    {
                        M_Cart_Travel model = JsonConvert.DeserializeObject<M_Cart_Travel>(CartDT.Rows[0]["Additional"].ToString());
                        model.Guest.AddRange(model.Contract);
                        vmMod.Guest = model.Guest;
                    }
                    break;
                case (int)M_Product.ClassType.JD:
                    {
                        M_Cart_Hotel model = JsonConvert.DeserializeObject<M_Cart_Hotel>(CartDT.Rows[0]["Additional"].ToString());
                        model.Guest.AddRange(model.Contract);
                        vmMod.Guest = model.Guest;
                    }
                    break;
                default: //------地址
                    break;
            }
            #endregion
            //------核算费用
            vmMod.StoreDT = orderCom.SelStoreDT(CartDT);
            vmMod.ProClass = DataConvert.CLng(vmMod.CartDT.Rows[0]["ProClass"]);
            vmMod.mu = mu;
            vmMod.allmoney = UpdateCartAllMoney(vmMod.CartDT);
            vmMod.usepoint = Point_CanBeUse(vmMod.allmoney);
            vmMod.IsShowAddress(vmMod.ProClass);
            vmMod.StoreDT.Columns.Add(new DataColumn("fareHtml", typeof(string)));
            for (int i = 0; i < vmMod.StoreDT.Rows.Count; i++)
            {
                CartDT.DefaultView.RowFilter = "StoreID=" + vmMod.StoreDT.Rows[i]["ID"];
                DataTable dt = CartDT.DefaultView.ToTable();
                if (dt.Rows.Count < 1) { continue; }
                DataTable fareDT = GetFareDT(dt);
                vmMod.StoreDT.Rows[i]["fareHtml"] = CreateFareHtml(fareDT);
            }
            //VerifyDT = CartDT.DefaultView.ToTable(false, verifyFields);
            return View(vmMod);
        }
        [Authorize(Policy="User")]
        public IActionResult GetOrderInfo_Submit()
        {
            //1,生成订单,2,关联购物车中商品为已绑定订单
            string ids = GetParam("ids");
            M_UserInfo mu = buser.GetLogin(false);
            DataTable cartDT = cartBll.SelByCartID(B_Cart.GetCartID(HttpContext), mu.UserID, ProClass, ids);//需要购买的商品
            if (cartDT.Rows.Count < 1) { return WriteErr("你尚未选择商品,<a href='/User/Order/OrderList'>查看我的订单</a>"); }
            VM_CartOrder vmMod = new VM_CartOrder();
            vmMod.IsShowAddress(DataConvert.CLng(cartDT.Rows[0]["ProClass"]));
            //------------------------------
            #region 检测缓存中的值与数据库中是否匹配
            //string refreshTip = ",请点击<a href='" + Request.RawUrl() + "'>刷新页面</a>";
            //if (VerifyDT == null || VerifyDT.Rows.Count < 1 || VerifyDT.Rows.Count != cartDT.Rows.Count)
            //{ return WriteErr("验证失效" + refreshTip, Request.RawUrl()); }
            //for (int i = 0; i < VerifyDT.Rows.Count; i++)
            //{
            //    //检测每一个商品,是否发生了ID/金额/数量/价格/总金额上面的差异或缺少
            //    DataRow verifyDR = VerifyDT.Rows[i];
            //    DataRow cartDR = GetDRFromDT(cartDT, Convert.ToInt32(verifyDR["ID"]));
            //    if (cartDR == null) { return WriteErr("购物车信息不匹配" + refreshTip, Request.RawUrl()); }
            //    foreach (string field in verifyFields)
            //    {
            //        double verifyVal = DataConvert.CDouble(verifyDR[field]);
            //        double cartVal = DataConvert.CDouble(cartDR[field]);
            //        if (verifyVal != cartVal) { return WriteErr("购物车的[" + field + "]不匹配" + refreshTip, Request.RawUrl()); }
            //    }
            //}
            #endregion
            //------生成订单前检测区
            foreach (DataRow dr in cartDT.Rows)
            {
                if (!HasStock(dr["Allowed"], DataConvert.CLng(dr["stock"]), Convert.ToInt32(dr["Pronum"])))
                    return WriteErr("购买失败," + dr["proname"] + "的库存数量不足");
            }
            //------检测End
            //按店铺生成订单
            DataTable storeDT = cartDT.DefaultView.ToTable(true, "StoreID");
            List<M_OrderList> orderList = new List<M_OrderList>();//用于生成临时订单,统计计算(Disuse)
            foreach (DataRow dr in storeDT.Rows)
            {
                #region 暂不使用字段
                //Odata.province = this.DropDownList1.SelectedValue;
                //Odata.city = this.DropDownList2.SelectedValue;//将地址省份与市ID存入,XML数据源
                //Odata.Guojia = "";//国家
                //Odata.Chengshi = DropDownList2.SelectedItem.Text;//城市
                //Odata.Diqu = DropDownList3.SelectedItem.Text;//地区
                //Odata.Delivery = DataConverter.CLng(Request.Form["Delivery"]);
                //Odata.Deliverytime = DataConverter.CLng(this.Deliverytime.Text);
                //Odata.Mobile = receMod.MobileNum;
                #endregion
                M_OrderList Odata = new M_OrderList();
                Odata.Ordertype = OrderHelper.GetOrderType(ProClass);
                Odata.OrderNo = B_OrderList.CreateOrderNo((M_OrderList.OrderEnum)Odata.Ordertype);
                Odata.StoreID = Convert.ToInt32(dr["StoreID"]);
                cartDT.DefaultView.RowFilter = "StoreID=" + Odata.StoreID;
                DataTable storeCartDT = cartDT.DefaultView.ToTable();
                switch (vmMod.ProClass)//旅游机票等,以联系人信息为地址
                {
                    case 7:
                    case 8:
                        M_Cart_Travel model = JsonConvert.DeserializeObject<M_Cart_Travel>(storeCartDT.Rows[0]["Additional"].ToString());
                        M_Cart_Contract user = model.Contract[0];
                        Odata.Receiver = user.Name;
                        Odata.Phone = user.Mobile;
                        Odata.MobileNum = user.Mobile;
                        Odata.Email = user.Email;
                        break;
                    default:
                        if (vmMod.ShowAddress)
                        {
                            int arsID = DataConvert.CLng(GetParam("address_rad"));
                            string arsChk = GetParam("ars_chk");
                            if (!string.IsNullOrEmpty(arsChk))
                            {
                                #region 自提或微信共享地址
                                switch (arsChk)
                                {
                                    case "self":
                                        {
                                            Odata.Receiver = "[用户自提]" + GetParam("arsInfo_name");//用户上门购买,然后自提取商品
                                            Odata.MobileNum = GetParam("arsInfo_mobile");
                                        }
                                        break;
                                    case "wechat":
                                        {
                                            Odata.Receiver = GetParam("wxad_name");
                                            Odata.MobileNum = GetParam("wxad_mobile");
                                            Odata.Shengfen = GetParam("wxad_city");
                                            Odata.Jiedao = GetParam("address");
                                        }
                                        break;
                                }
                                #endregion
                            }
                            else
                            {
                                M_UserRecei receMod = receBll.GetSelect(arsID, mu.UserID);
                                if (receMod == null) { return WriteErr("用户尚未选择送货地址,或地址不存在"); }
                                Odata.Receiver = receMod.ReceivName;
                                Odata.Phone = receMod.phone;
                                Odata.MobileNum = receMod.MobileNum;
                                Odata.Email = receMod.Email;
                                Odata.Shengfen = receMod.Provinces;
                                Odata.Jiedao = receMod.Street;
                                Odata.ZipCode = receMod.Zipcode;
                            }
                        }
                        break;
                }
                Odata.Invoiceneeds = DataConverter.CLng(GetParam("invoice_rad"));//是否需开发票
                Odata.Rename = mu.UserName;
                Odata.AddUser = mu.UserName; ;
                Odata.Userid = mu.UserID;
                Odata.Ordermessage = GetParam("ORemind_T");//订货留言
                //-----金额计算
                Odata.Balance_price = GetTotalMoney(storeCartDT);
                Odata.Freight = GetFarePrice(storeCartDT, Odata.StoreID);//运费计算
                Odata.Ordersamount = Odata.Balance_price + Odata.Freight;//订单金额
                Odata.AllMoney_Json = orderCom.GetTotalJson(storeCartDT);//附加需要的虚拟币
                Odata.Specifiedprice = Odata.Ordersamount; //订单金额;
                Odata.OrderStatus = (int)M_OrderList.StatusEnum.Normal;//订单状态
                Odata.Paymentstatus = (int)M_OrderList.PayEnum.NoPay;//付款状态
                //Odata.Integral = DataConverter.CLng(Request.QueryString["jifen"]);
                Odata.ExpTime = GetParam("exptime_hid");
                Odata.id = orderBll.insert(Odata);
                //是否需要开发票
                if (Odata.Invoiceneeds == 1)
                {
                    M_Order_Invoice invMod = new M_Order_Invoice();
                    invMod.OrderID = Odata.id;
                    invMod.InvHead = GetParam("InvoTitle_T").Trim(',');
                    invMod.UserCode = GetParam("InvoUserCode_T").Trim(',');
                    invMod.InvClass = GetParam("invUseType_rad");
                    invMod.InvContent = GetParam("Invoice_T").Trim(',');
                    invMod.UserID = Odata.Userid;
                    invBll.Insert(invMod);
                    new B_Order_InvTlp().Sync(invMod);
                }
                cpBll.CopyToCartPro(mu, storeCartDT, Odata.id);
                orderList.Add(Odata);
                orderCom.SendMessage(Odata, null, "ordered");
            }
            cartBll.DelByids(ids);
            //-----------------订单生成后处理
            //进行减库存等操作
            foreach (DataRow dr in cartDT.Rows)
            {
                M_Product model = proBll.GetproductByid(Convert.ToInt32(dr["Proid"]));
                model.Stock = model.Stock - DataConvert.CLng(dr["Pronum"]);
                SqlHelper.ExecuteSql("Update ZL_Commodities Set Stock=" + model.Stock + " Where ID=" + model.ID);
            }
            //生成支付单,处理优惠券,并进入付款步骤
            M_Payment payMod = payBll.CreateByOrder(orderList);
            //优惠券
            if (!string.IsNullOrEmpty(GetParam("Arrive_Hid")))
            {
                M_Arrive avMod = avBll.SelModelByFlow(GetParam("Arrive_Hid"), mu.UserID);
                double money = payMod.MoneyPay;
                string remind = "支付单抵扣[" + payMod.PayNo + "]";
                M_Arrive_Result retMod = avBll.U_UseArrive(avMod, mu.UserID, cartDT, money, remind);
                if (retMod.enabled)
                {
                    payMod.MoneyPay = retMod.money;
                    payMod.ArriveMoney = retMod.amount;
                    payMod.ArriveDetail = avMod.ID.ToString();
                }
                else { payMod.ArriveDetail = "优惠券[" + avMod.ID + "]异常 :" + retMod.err; }
            }
            //积分处理
            int maxPoint = Point_CanBeUse(payMod.MoneyPay + payMod.ArriveMoney);
            if (maxPoint > 0 && DataConvert.CLng(GetParam("Point_T")) > 0)
            {
                int point = DataConvert.CLng(GetParam("Point_T"));
                //此处需咨询,上限额度是否要扣减掉优惠券
                //if (point <= 0) {  return WriteErr("积分数值不正确"); }
                if (point > mu.UserExp) { return WriteErr("您的积分不足!"); }
                if (point > maxPoint) { return WriteErr("积分不能大于可兑换金额[" + maxPoint + "]!"); }
                //生成支付单时扣除用户积分
                buser.ChangeVirtualMoney(mu.UserID, new M_UserExpHis() { ScoreType = (int)M_UserExpHis.SType.Point, score = -point, detail = "积分抵扣,支付单号:" + payMod.PayNo });
                payMod.UsePoint = point;
                payMod.UsePointArrive = Point_ToMoney(point);
                payMod.MoneyPay = payMod.MoneyPay - payMod.UsePointArrive;
            }
            payMod.MoneyReal = payMod.MoneyPay;
            payMod.Remark = cartDT.Rows.Count > 1 ? "[" + cartDT.Rows[0]["ProName"] as string + "]等" : cartDT.Rows[0]["ProName"] as string;
            payMod.PaymentID = payBll.Add(payMod);
            {
                string notify_url = CustomerPageAction.customPath2 + "Shop/Orderlistinfo?ID=" + orderList[0].id;
                ZoomLa.BLL.User.M_User_Notify notify = new ZoomLa.BLL.User.M_User_Notify();
                notify.Title = "有新的订单了";
                notify.Content = "<a href='" + notify_url + "' target='main_right' style='font-size:12px;' title='查看详情'>新订单:" + orderList[0].OrderNo + "</a>";
                notify.NType = 1;
                notify.Gid = orderList[0].id.ToString();
                notify.AppendReceUser("admin");
                ZoomLa.BLL.User.B_User_Notify.Add(notify);
            }
            return Redirect("/PayOnline/Orderpay?PayNo=" + payMod.PayNo);
        }
        public IActionResult Order_Address()
        {
            DataTable dt = receBll.SelByUID(mu.UserID);
            return PartialView("Comp/Order_Address",dt);
        }
        #region 重算商品金额
        //更新购物车中的AllMoney(实际购买总价),便于后期查看详情
        private double UpdateCartAllMoney(DataTable dt)
        {
            double allmoney = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                M_Cart cartMod = new M_Cart().GetModelFromReader(dr);
                M_Product proMod = proBll.GetproductByid(Convert.ToInt32(dr["Proid"]));
                //根据商品价格类型,看使用  零售|批发|会员|会员组价格
                //多区域价格
                //if (string.IsNullOrEmpty(Region))
                //{
                //    Region = buser.GetRegion(mu.UserID);
                //}
                //double price = regionBll.GetRegionPrice(proMod.ID, proMod.LinPrice, Region, mu.GroupID);
                ////如果多区域价格未匹配,则匹配会员价
                //if (price == proMod.LinPrice) { price = proBll.P_GetByUserType(proMod, mu); }
                double price = proBll.P_GetByUserType(proMod, mu);
                //--多价格编号,则使用多价格编号的价钱,ProName(已在购物车页面更新)
                //double price =proBll.GetPriceByCode(dr["code"], proMod.Wholesalesinfo, ref price);
                cartMod.AllMoney_Init = cartMod.AllMoney = price * cartMod.Pronum;
                cartMod.FarePrice = price.ToString("F2");
                cartMod.Shijia = price;
                //----检查有无价格方面的促销活动,如果有,检免多少金额
                {
                    W_Filter filter = new W_Filter();
                    filter.cartMod = cartMod;
                    filter.TypeFilter = "money";
                    ptBll.WhereLogical(filter);
                    cartMod.AllMoney_Arrive += filter.DiscountMoney;
                    cartMod.ArriveRemark += "促销:" + filter.DiscountMoney.ToString("F2");
                }
                //----计算折扣
                cartMod.AllMoney = cartMod.AllMoney_Init - cartMod.AllMoney_Arrive;
                if (cartMod.AllMoney < 0) { cartMod.AllMoney = 0; }
                //------------------------------
                dr["AllMoney"] = cartMod.AllMoney;
                cartBll.UpdateByID(cartMod);
                allmoney += cartMod.AllMoney;
            }
            //缓存数据,提交时验证
            return allmoney;
        }
        //获取总金额
        private double GetTotalMoney(DataTable dt)
        {
            //不需要再重新计算,因为每次进入页面都会重算
            return Convert.ToDouble(dt.Compute("SUM(AllMoney)", ""));
        }
        #endregion
        #region 运费
        string[] expnames = SiteConfig.ShopConfig.ExpNames.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //获取前端所选定的ID,返回运费价
        private double GetFarePrice(DataTable storecartDT, int storeid)
        {
            DataTable faredt = GetFareDT(storecartDT);
            string selectedVal = GetParam("fare_" + storeid);//前台选定的快递方式,后台重新计价
            faredt.DefaultView.RowFilter = "ID=" + selectedVal;
            if (faredt.DefaultView.ToTable().Rows.Count < 1) { throw new Exception("运费出错"); }
            DataRow dr = faredt.DefaultView.ToTable().Rows[0];
            return Convert.ToDouble(dr["Price"]) + DataConvert.CDouble(dr["Plus"]);
        }
        /// <summary>
        /// 根据模板和购物车商品数量/金额,计算出某一店铺的快递费用
        /// </summary>
        /// <param name="cartdt">某一店铺的购物车</param>
        private DataTable GetFareDT(DataTable cartdt)
        {
            //以初始运费高的模板为准(运费,免邮条件等)(避免有漏造成商户损失)
            Dictionary<string, M_Shop_Fare> expMap = new Dictionary<string, M_Shop_Fare>();
            foreach (string name in expnames)
            {
                expMap.Add(name, new M_Shop_Fare() { name = name, price = "0", plus = "0", enabled = false });
            }
            DataTable fareTlpDT = cartdt.DefaultView.ToTable(true, "FarePrice1");//当前选中的商品有多少运费模板
            for (int i = 0; i < fareTlpDT.Rows.Count; i++)
            {
                int id = DataConvert.CLng(fareTlpDT.Rows[i]["FarePrice1"]);
                if (id < 1) continue;
                //------------------------------
                M_Shop_FareTlp fareMod = fareBll.SelReturnModel(id);
                if (fareMod == null) { throw new Exception("错误,快递模板[" + id + "]不存在"); }
                JArray arr = JsonConvert.DeserializeObject<JArray>(fareMod.Express);
                //选出条件寄送方式不同,未禁用,价格最高的快递方式
                foreach (JObject obj in arr)
                {
                    M_Shop_Fare model = JsonConvert.DeserializeObject<M_Shop_Fare>(obj.ToString());
                    //快递方式被禁用或已移除
                    if (!model.enabled || !expMap.ContainsKey(model.name)) { continue; }
                    if (model.Price > expMap[model.name].Price) { expMap[model.name] = model; }
                }
            }
            DataTable faredt = CreateFareDT(Convert.ToInt32(cartdt.Rows[0]["StoreID"]));
            return FareDT(cartdt, faredt, expMap);
        }
        //实际运算填充faredt
        private DataTable FareDT(DataTable cartdt, DataTable faredt, Dictionary<string, M_Shop_Fare> expMap)
        {
            int pronum = Convert.ToInt32(cartdt.Compute("sum(Pronum)", ""));//统计金额和件数,看其是否满足包邮条件
            double allmoney = Convert.ToDouble(cartdt.Compute("sum(AllMoney)", ""));

            foreach (var item in expMap)
            {
                bool isfree = false;//是否免费用(符合免邮条件,或设置为了免运费)
                M_Shop_Fare model = item.Value;
                DataRow dr = faredt.Select("Name='" + model.name + "'")[0];
                if (!model.enabled) { dr["Enabled"] = false; continue; }
                //根据快递费模板,组合出本次的金额,以价高者为准
                switch (model.free_sel)
                {
                    case 0:
                        break;
                    case 1:
                        if (pronum >= model.Free_num) { isfree = true; }
                        break;
                    case 2:
                        if (allmoney >= model.Free_Money) { isfree = true; }
                        break;
                    case 3:
                        if (pronum >= model.Free_num || allmoney >= model.Free_Money) { isfree = true; }
                        break;
                }
                if (isfree) { continue; }
                else
                {
                    if (model.Price > Convert.ToDouble(dr["Price"])) { dr["Price"] = model.Price; }
                    dr["Plus"] = Convert.ToDouble(dr["Plus"]) + model.Plus * (pronum - 1);
                }
            }
            return faredt;
        }
        //创建一个空的运费基础表,最终用于生成html
        private DataTable CreateFareDT(int storeid)
        {
            DataTable faredt = new DataTable();
            faredt.Columns.Add(new DataColumn("StoreID", typeof(int)));//所属店铺
            faredt.Columns.Add(new DataColumn("Enabled", typeof(bool)));//是否启用
            faredt.Columns.Add(new DataColumn("ID", typeof(int)));
            faredt.Columns.Add(new DataColumn("Name", typeof(string)));//Alias
            faredt.Columns.Add(new DataColumn("Price", typeof(double)));//基础运费
            faredt.Columns.Add(new DataColumn("Plus", typeof(double)));//续件运费
            faredt.Columns.Add(new DataColumn("Total", typeof(double)));//小计运费
            faredt.Columns.Add(new DataColumn("Desc", typeof(string)));//备注
            for (int i = 0; i < expnames.Length; i++)
            {
                DataRow dr = faredt.NewRow();
                dr["StoreID"] = storeid;
                dr["ID"] = i;
                dr["Enabled"] = true;
                dr["name"] = expnames[i];
                dr["Price"] = 0;
                dr["Plus"] = 0;
                dr["Desc"] = "";
                faredt.Rows.Add(dr);
            }
            return faredt;
        }
        //根据店铺的运费dt,生成下拉html
        private string CreateFareHtml(DataTable dt)
        {
            string selectTlp = "<select name='{0}' class='fare_select'>{1}</select>";//fare_storeid,optinohtml
            string optionTlp = "<option data-price='{0}' value='{1}'>{2}</option>";//price,id,Desc
            string html = "", result = "";
            if (dt.Select("Enabled='true'").Length < 1) { dt.Rows[0]["Enabled"] = true; }
            foreach (DataRow dr in dt.Rows)
            {
                if (!DataConvert.CBool(dr["Enabled"].ToString()))
                {
                    continue;
                }
                dr["Total"] = Convert.ToDouble(dr["Price"]) + Convert.ToDouble(dr["Plus"]);
                if (Convert.ToDouble(dr["Total"]) == 0)
                {
                    dr["Desc"] = dr["Name"] + " 免邮";
                }
                else
                {
                    dr["Desc"] = dr["Name"] + " " + dr["Total"] + "元";
                }
                html += string.Format(optionTlp, dr["Total"], dr["ID"], dr["Desc"]);
            }
            result = string.Format(selectTlp, "fare_" + dt.Rows[0]["StoreID"], html);
            return result;
        }
        #endregion
        #region 积分抵扣
        //用户最大能使用多少带你分
        public int Point_CanBeUse(double orderMoney)
        {
            int usepoint = 0;
            if (SiteConfig.ShopConfig.PointRatiot <= 0 || SiteConfig.ShopConfig.PointRatiot > 100 || SiteConfig.ShopConfig.PointRate <= 0) { return usepoint; }
            //可使用多少积分
            usepoint = (int)((orderMoney * (SiteConfig.ShopConfig.PointRatiot * 0.01)) / SiteConfig.ShopConfig.PointRate);
            usepoint = usepoint < 1 ? 0 : usepoint;
            return usepoint;
        }
        //积分兑换为资金
        public double Point_ToMoney(int points)
        {
            if (points <= 0) { return 0; }
            return points * SiteConfig.ShopConfig.PointRate;
        }
        #endregion
        private bool HasStock(object allowed, int stock, int pronum)
        {
            bool flag = true;
            if (allowed.ToString().Equals("0") && stock < pronum)
            {
                flag = false;
            }
            return flag;
        }
        private DataRow GetDRFromDT(DataTable dt, int id)
        {
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToInt32(dr["ID"]) == id) { return dr; }
            }
            return null;
        }
        #endregion
    }
}
