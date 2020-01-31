using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.Common;
using ZoomLa.BLL.Shop;
using ZoomLa.Model.Shop;

namespace ZoomLa.BLL
{


    public class B_CartPro
    {
        private string TbName, PK;
        private M_CartPro initMod = new M_CartPro();
        private B_Cart_Present cptBll = new B_Cart_Present();
        public B_CartPro()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_CartPro SelReturnModel(int ID)
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
        public M_CartPro GetSelect(int proid, int cartid)
        {
            string sqlStr = "SELECT * FROM ZL_CartPro WHERE ProID=" + proid + " AND CartID=" + cartid;
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlStr, null))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                    return new M_CartPro();
            }
        }
        /// <summary>
        /// 主用于IDC订单，IDC一张订单只有一个商品，其他订单勿用
        /// </summary>
        public M_CartPro SelModByOrderID(int orderID)
        {
            string sqlstr = "Select  Top 1 * from ZL_CartPro where Orderlistid=@Orderlistid";
            SqlParameter[] para = new SqlParameter[]{
                new SqlParameter("@Orderlistid",orderID)
            };
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sqlstr, para))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                    return new M_CartPro();
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public DataTable GetCartProOrderID(int Orderid)
        {
            string sql = "SELECT A.*,B.* FROM " + TbName + " A LEFT JOIN ZL_Commodities B ON A.Proid=B.ID WHERE A.Orderlistid=" + Orderid;
            return SqlHelper.ExecuteTable(CommandType.Text, sql, null);
        }
        public DataTable SelByOrderID(int orderListID)
        {
            return DBCenter.Sel(TbName, "OrderListID=" + orderListID);
        }
        public bool UpdateByID(M_CartPro model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public int GetInsert(M_CartPro model)
        {
            return DBCenter.Insert(model);
        }
        public bool Add(M_CartPro model)
        {
            DBCenter.Insert(model);
            return true;
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        //--------------------------Tools
        /// <summary>
        /// True表示库存大于购买量,False则有库存Stock小于购买量的商品
        /// </summary>
        public bool CheckStock(int orderListID)
        {
            string sql = "Select * From " + TbName + " as a Left Join ZL_Commodities as b on a.ProID=b.ID Where a.ProNum>b.Stock And a.OrderListID=" + orderListID;
            return SqlHelper.ExecuteTable(CommandType.Text, sql).Rows.Count < 1;
        }
        /// <summary>
        /// 拷贝一份至ZL_CartPro长久保存
        /// </summary>
        public void CopyToCartPro(M_UserInfo mu, DataTable dt, int oid)
        {
            B_Product proBll = new B_Product();
            string[] fields = "Additional,StoreID,AllMoney_Json,code".Split(',');
            foreach (string field in fields)
            {
                if (!dt.Columns.Contains(field)) { dt.Columns.Add(new DataColumn(field, typeof(string))); }
            }
            foreach (DataRow dr in dt.Rows)
            {
                M_Product proMod = proBll.GetproductByid(Convert.ToInt32(dr["Proid"]));
                M_CartPro model = new M_CartPro();
                model.Orderlistid = oid;
                model.ProID = proMod.ID;
                model.Pronum = DataConverter.CLng(dr["Pronum"]);
                model.Proname = proMod.Proname;
                model.Shijia = Convert.ToDouble(dr["FarePrice"]);
                model.Danwei = proMod.ProUnit;
                model.Addtime = DateTime.Now;
                model.StoreID = DataConvert.CLng(dr["StoreID"]);
                model.code = DataConvert.CStr(dr["code"]);
                model.Attribute = DataConvert.CStr(dr["ProAttr"]);
                if (!dt.Columns.Contains("AllMoney")) { model.AllMoney = proMod.LinPrice * model.Pronum; }
                else { model.AllMoney = Convert.ToDouble(dr["AllMoney"]); }
                //后期加上记录优惠信息
                //model.AllMoney_Init = DataConvert.CDouble(dr["AllMoney_Init"]);
                //model.AllMoney_Arrive= DataConvert.CDouble(dr["AllMoney_Arrive"]);
                //model.ArriveRemark = DataConvert.CStr(dr["ArriveRemark"]);

                //如果非促销组合,则不保存商品简介和详情
                if (proMod.Class != 2) { proMod.Procontent = ""; proMod.Proinfo = ""; }
                #region 保存购买时用户的信息
                model.Username = mu.UserName;
                model.Additional = DataConvert.CStr(dr["Additional"]);
                model.Remark = DataConvert.CStr(dr["Remark"]);
                //model.Additional = JsonHelper.GetJson(new string[] { "UserID", "GroupID", "PUserID" }, new object[] { mu.UserID, mu.GroupID, mu.ParentUserID });
                #endregion
                #region 将整个商品信息备份(主要是价格和配置部分)
                M_Product backup = new M_Product();
                backup.ID = proMod.ID;
                backup.ParentID = proMod.ParentID;
                backup.Class = proMod.Class;
                backup.Nodeid = proMod.Nodeid;
                backup.ModelID = proMod.ModelID;
                backup.Proname = proMod.Proname;
                backup.ProClass = proMod.ProClass;
                backup.Proinfo = proMod.Proinfo;
                backup.Procontent = proMod.Procontent;
                backup.ShiPrice = proMod.ShiPrice;
                backup.LinPrice = proMod.LinPrice;
                backup.MemberPrice = proMod.MemberPrice;
                backup.ActPrice = proMod.ActPrice;
                backup.Wholesales = proMod.Wholesales;
                backup.Wholesaleone = proMod.Wholesaleone;
                backup.Wholesalesinfo = proMod.Wholesalesinfo;
                backup.Thumbnails = proMod.Thumbnails;
                backup.Recommend = proMod.Recommend;
                backup.Largess = proMod.Largess;
                backup.GuessXML = proMod.GuessXML;
                backup.PointVal = proMod.PointVal;
                backup.UserShopID = proMod.UserShopID;
                backup.BookPrice = proMod.BookPrice;
                backup.UserType = proMod.UserType;
                backup.UserPrice = proMod.UserPrice;
                backup.Recycler = proMod.Recycler;
                backup.FarePrice = proMod.FarePrice;
                backup.BindIDS = proMod.BindIDS;
                model.PClass = proMod.Class.ToString();
                model.ProInfo = JsonConvert.SerializeObject(backup);
                #endregion
                model.ID= GetInsert(model);
                #region 是否包含赠品,将赠品信息保存
                {
                    B_Shop_Present ptBll = new B_Shop_Present();
                    DataTable ptDT = ptBll.WhereLogical(new W_Filter(dr));
                    cptBll.BatInsert(model.ID, ptDT);
                }
                #endregion
            }
        }
        //---------------------------User
        /// <summary>
        /// 专用于展示,带图片等信息
        /// </summary>
        ///<filter>需要过滤参数</filter>
        public DataTable SelForRPT(int oid, string filter)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("filter", filter) };
            string fields = " A.*,B.LinPrice,B.Proname,B.Thumbnails,B.ProClass,B.ProUnit,B.ParentID";
            string where = " A.OrderListID=" + oid;
            if (!string.IsNullOrEmpty(filter))
            {
                where += " AND (CHARINDEX(@filter,A.AddStatus)=0 OR A.AddStatus IS NULL)";
            }
            //if (!string.IsNullOrEmpty(ids))
            //{
            //    SafeSC.CheckIDSEx(ids);
            //    where += "A.ID IN(" + ids + ") ";
            //}
            return SqlHelper.JoinQuery(fields, TbName, new M_Product().TbName, "A.ProID=B.ID", where, "", sp);
        }
        /// <summary>
        /// 用于用户中心OrderList
        /// </summary>
        /// <param name="ids">订单的IDS</param>
        public DataTable U_SelForOrderList(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return new DataTable(); }
            SafeSC.CheckIDSEx(ids);
            return DBCenter.Sel("ZL_CartProView","ID IN ("+ids+")");
        }
        public PageSetting SelForOrderList(Filter_Order filter)
        {
            string where = "OrderType!=" + (int)M_OrderList.OrderEnum.Hide;
            string whereCount = " WHERE OrderType!=" + (int)M_OrderList.OrderEnum.Hide;
            List<SqlParameter> sp = new List<SqlParameter>();
            //是否包含回收站订单
            if (filter.aside != -100) { where += " AND Aside=" + filter.aside; }
            #region 用户中心快速筛选
            switch (filter.fast)
            {
                case "all"://全部(不含回收站)
                    where += " AND Aside=0";
                    break;
                case "unpaid"://待付款==状态为未付款的
                    where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.NoPay;
                    break;
                case "prepay"://已预付款(尚未支付尾款的订单)
                    where += " AND Aside=0 AND Delivery=1 AND IsCount=0";
                    break;
                case "paid"://已支付(只支付了预付款的不在此列)
                    where += " AND Aside=0 AND ((PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed + " AND Delivery=0) OR (Delivery=1 AND IsCount=1 AND Settle=1)) ";
                    break;
                case "needpay"://需付款
                    where += " AND Aside=0 AND PaymentStatus=0";
                    break;
                case "receive"://需确认收货
                    where += " AND Aside=0 AND StateLogistics=1";
                    break;
                case "comment"://已付款未评价
                    //where += " AND (OrderStatus=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived + ")";
                    //where += " AND (SELECT COUNT(*) FROM ZL_CartPro WHERE Orderlistid=ID AND (AddStatus IS NULL OR AddStatus=''))>0";//AddStatus中会有退货记录,所以筛选为必须为空
                    //break;
                case "finish"://客户已付款收货 ||客户已完成退货
                    {
                        where += string.Format(" AND ({0} OR {1})",
                            "(OrderStatus=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived + ")",
                            "(PaymentStatus=" + (int)M_OrderList.PayEnum.Refunded + ")");
                    }
                    break;
                case "issure":
                    where += " AND IsSure=0 ";
                    break;
                case "recycle"://订单回收站
                    where = "OrderType!=" + (int)M_OrderList.OrderEnum.Hide+" AND Aside=1";
                    break;
            }
            #endregion
            #region 后台快速筛选
            switch (filter.addon)
            {
                case "unpaid"://待付款==状态为未付款的
                    where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.NoPay;
                    break;
                case "prepay"://已预付款(尚未支付尾款的订单)
                    where += " AND Delivery=1 AND IsCount=0";
                    break;
                case "paid":
                    where += " AND ((PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed + " AND Delivery=0) OR (Delivery=1 AND IsCount=1 AND Settle=1)) ";
                    //where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed;
                    break;
                case "unexp"://待发货==已付款+未发货
                    where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.NoSend;
                    break;
                case "exped"://已发货==大于未发货状态的订单
                    where += " AND StateLogistics>" + (int)M_OrderList.ExpEnum.NoSend;
                    break;
                case "finished":
                    //where += " AND OrderStatus=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived;
                    //后期移除,暂时支持预付
                    where += " AND (OrderStatus=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived+")";
                    break;
                case "unrefund":
                    where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.RequestRefund;
                    break;
                case "refunded":
                    where += " AND PaymentStatus=" + (int)M_OrderList.PayEnum.Refunded;
                    break;
                case "recycle"://订单回收站==已关闭
                    where = " Aside=1 ";
                    break;
                case "all"://全部(不含回收站)
                default:
                    break;
            }
            #endregion
            //店铺类型
            switch (filter.storeType)
            {
                case "all":
                    break;
                case "store":
                    where += " AND StoreID>0";
                    whereCount += " AND StoreID>0";
                    break;
                case "shop":
                    where += " AND StoreID=0 ";
                    whereCount += " AND StoreID=0";
                    break;
                default:
                    if (DataConvert.CLng(filter.storeType) > 0)
                    {
                        where += " AND StoreID=" + DataConvert.CLng(filter.storeType) + " ";
                        whereCount += " AND StoreID=" + DataConvert.CLng(filter.storeType) + " ";
                    }
                    break;
            }
            if (filter.isSure != -100) { where += " AND IsSure=" + filter.isSure; }
            //订单类型，未指定则抽出常规订单
            if (string.IsNullOrEmpty(filter.orderType))
            {
                where += " AND OrderType IN (0,1,4)";
                whereCount += " AND OrderType IN (0,1,4)";
            }
            else if (filter.orderType.Equals("-100") || filter.orderType.Equals("-1"))
            {

            }
            else
            {
                SafeSC.CheckIDSEx(filter.orderType);
                where += " AND OrderType IN (" + filter.orderType + ")";
                whereCount += " AND OrderType IN (" + filter.orderType + ")";
            }
            //商品名,订单号,用户名,手机号,用户ids
            if (!string.IsNullOrEmpty(filter.proname)) { where += " AND ProName LIKE @proname"; sp.Add(new SqlParameter("proname", "%" + filter.proname + "%")); }
            if (!string.IsNullOrEmpty(filter.orderno)) { where += " AND OrderNo LIKE @orderno"; sp.Add(new SqlParameter("orderno", "%" + filter.orderno + "%")); }
            if (!string.IsNullOrEmpty(filter.reuser)) { where += " AND (Rename LIKE @reuser OR Receiver LIKE @reuser)"; sp.Add(new SqlParameter("reuser", "%" + filter.reuser + "%")); }
            if (!string.IsNullOrEmpty(filter.mobile)) { where += " AND MobileNum LIKE @mobile"; sp.Add(new SqlParameter("mobile", "%" + filter.mobile + "%")); }
            if (!string.IsNullOrEmpty(filter.uids) && SafeSC.CheckIDS(filter.uids)) { where += " AND UserID IN (" + filter.uids + ")"; }
            //下单日期
            if (!string.IsNullOrEmpty(filter.stime))
            {
                DateTime result = DateTime.Now;
                if (DateTime.TryParse(filter.stime, out result)) { where += " AND AddTime>=@stime"; sp.Add(new SqlParameter("stime", result.ToString("yyyy/MM/dd 00:00:00"))); }
            }
            if (!string.IsNullOrEmpty(filter.etime))
            {
                DateTime result = DateTime.Now;
                if (DateTime.TryParse(filter.etime, out result)) { where += " AND AddTime<=@etime"; sp.Add(new SqlParameter("etime", result.ToString("yyyy/MM/dd 23:59:59"))); }
            }
            //发货时间
            if (!string.IsNullOrEmpty(filter.expstime) || !string.IsNullOrEmpty(filter.expetime)) { where += " AND ExpSTime IS NOT NULL "; }
            if (!string.IsNullOrEmpty(filter.expstime))//按发货日期筛选
            {
                DateTime result = DateTime.Now;
                if (DateTime.TryParse(filter.expstime, out result)) { where += " AND ExpSTime>=@expstime"; sp.Add(new SqlParameter("expstime", result.ToString("yyyy/MM/dd 00:00:00"))); }
            }
            if (!string.IsNullOrEmpty(filter.expetime))
            {
                DateTime result = DateTime.Now;
                if (DateTime.TryParse(filter.expetime, out result)) { where += " AND ExpSTime<=@expetime"; sp.Add(new SqlParameter("expetime", result.ToString("yyyy/MM/dd 23:59:59"))); }
            }
            //搜索,支持指定条件
            if (!string.IsNullOrEmpty(filter.skey))
            {
                sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
                switch (filter.stype)
                {
                    case "exp":
                        where += " AND ExpressDelivery LIKE @skey";
                        break;
                    case "oid":
                        where += " AND ID= " + DataConvert.CLng(filter.skey);
                        break;
                }
            }
            if (!String.IsNullOrEmpty(filter.oids))
            {
                SafeSC.CheckIDSEx(filter.oids);
                where += " AND ID IN (" + filter.oids + ")";
            }
            if (filter.payType != -100)
            {
                where += " AND PayType=" + filter.payType;
            }
            ////商品信息
            //string mtbname = "(SELECT *,LinPrice,Thumbnails,GuessXML,ProCode FROM ZL_CartPro A LEFT JOIN ZL_Commodities B ON ProID=ID)";
            ////订单与用户信息
            //string stbname = "(SELECT *,ParentUserID,HoneyName,ZL_Order_Exp.CDate AS ExpSTime FROM ZL_OrderInfo A LEFT JOIN ZL_User B ON UserID=UserID LEFT JOIN ZL_Order_Exp ON ExpressNum=ZL_Order_Exp.ID)";
            //string fields = "ID AS CartID,PClass,ProID,ProName,Shijia,Pronum,AddStatus,AllMoney,StoreID,Thumbnails,GuessXML"//ZL_CartPro
            //             + ",Freight,ID,OrderNo,AddUser,AddTime,OrderStatus,PaymentStatus,OrdersAmount,StateLogistics,Aside,Delivery,Service_charge"//ZL_OrderInfo
            //             + ",OrderType,OrderMessage,Receivablesamount,ExpressNum,PaymentNo" //ZL_OrderInfo
            //             + ",Shengfen,Jiedao,ZipCode,MobileNum,Phone,Email,Receiver,Rename,ExpressDelivery"//地址信息 ZL_OrderInfo
            //             + ",UserID,ParentUserID,HoneyName"//User
            //             + ",ExpSTime";//ZL_Order_Exp
            //PageSetting setting=PageSetting.Double(filter.cpage,filter.psize,mtbname,stbname,"ID","OrderListID=ID",where,"ID DESC",sp,fields);
            //return DBCenter.JoinQuery(fields, mtbname, stbname, "OrderListID=ID", where, "ID DESC", sp.ToArray());

            string view = "ZL_CartProView";
            //只取订单的ID
            PageSetting setting = PageSetting.Single(filter.cpage, filter.psize, view, "ID", where, " GROUP BY ID ORDER BY ID DESC", sp, "ID");
            DBCenter.SelPage(setting);
            string ids = "";
            foreach (DataRow dr in setting.dt.Rows)
            {
                ids += dr["id"] + ",";
            }
            ids = ids.TrimEnd(',');
            setting.itemCount = DataConvert.CLng(DBCenter.Count("(SELECT ID FROM ZL_CartProView WHERE " + where + " GROUP BY ID) A", "", sp));
            setting.pageCount = SqlBase.GetPageCount(setting.itemCount, setting.psize);
            //根据订单ID取出购物车中的数据,需要进行名称等筛选
            if (!string.IsNullOrEmpty(ids))
            {
                sp.Clear();
                string cartWhere = "ID IN (" + ids + ") ";
                if (!string.IsNullOrEmpty(filter.proname)) { cartWhere += " AND ProName LIKE @proname"; sp.Add(new SqlParameter("proname", "%" + filter.proname + "%")); }
                setting.dt = DBCenter.Sel(view, cartWhere, "ID DESC", sp);
            }
            if (filter.needCount)
            {
                string tbname = view;
                filter.countMod = new OrderCount();
                string sql = "SELECT COUNT(ID) AS [all]";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.NoSend + ") unexp";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND PaymentStatus=" + (int)M_OrderList.PayEnum.NoPay + ") unpaid";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND Delivery=1 AND IsCount=0) prepay";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND ((PaymentStatus=1 AND Delivery=0) OR (Delivery=1 AND IsCount=1 AND Settle=1))) paid";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND StateLogistics>" + (int)M_OrderList.ExpEnum.NoSend + ") exped";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND OrderStatus=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND StateLogistics=" + (int)M_OrderList.ExpEnum.HasReceived + ") finished";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND PaymentStatus=" + (int)M_OrderList.PayEnum.RequestRefund + ") unrefund";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=0 AND PaymentStatus=" + (int)M_OrderList.PayEnum.Refunded + ") refunded";
                sql += ",(SELECT COUNT(ID) FROM " + tbname + whereCount + " AND Aside=1) recycle";
                sql += " FROM " + tbname + whereCount + " AND Aside=0";
                DataTable dt = DBCenter.ExecuteTable(sql);
                filter.countMod.all = DataConvert.CLng(dt.Rows[0]["all"]);
                filter.countMod.unexp = DataConvert.CLng(dt.Rows[0]["unexp"]);
                filter.countMod.unpaid = DataConvert.CLng(dt.Rows[0]["unpaid"]);
                filter.countMod.prepay = DataConvert.CLng(dt.Rows[0]["prepay"]);
                filter.countMod.paid = DataConvert.CLng(dt.Rows[0]["paid"]);
                filter.countMod.exped = DataConvert.CLng(dt.Rows[0]["exped"]);
                filter.countMod.finished = DataConvert.CLng(dt.Rows[0]["finished"]);
                filter.countMod.unrefund = DataConvert.CLng(dt.Rows[0]["unrefund"]);
                filter.countMod.refunded = DataConvert.CLng(dt.Rows[0]["refunded"]);
                filter.countMod.recycle = DataConvert.CLng(dt.Rows[0]["recycle"]);

            }
            return setting;
        }
     
    }
    public class Filter_Order
    {
        public int cpage = 1;
        public int psize = 10;
        /// <summary>
        /// 店铺类型 all|store|shop|店铺ID
        /// </summary>
        public string storeType = "";
        /// <summary>
        /// 订单类型IDS
        /// </summary>
        public string orderType = "";
        /// <summary>
        /// 快速筛选
        /// </summary>
        public string addon = "";
        public string proname = "";
        public string orderno = "";
        /// <summary>
        /// 收货人
        /// </summary>
        public string reuser = "";
        public string mobile = "";
        /// <summary>
        /// 购物车记录中订单的IDS
        /// </summary>
        public string oids = "";
        /// <summary>
        /// 用户IDS筛选
        /// </summary>
        public string uids = "";
        /// <summary>
        /// 筛选条件,根据快递单号与订单号筛选
        /// </summary>
        public string stype = "";
        /// <summary>
        /// 筛选关键词,与stype合用
        /// </summary>
        public string skey = "";
        /// <summary>
        /// 起始下单日期
        /// </summary>
        public string stime = "";
        /// <summary>
        /// 结束下单日期
        /// </summary>
        public string etime = "";
        /// <summary>
        /// 起始发货时间
        /// </summary>
        public string expstime = "";
        /// <summary>
        /// 结束发货时间
        /// </summary>
        public string expetime = "";
        /// <summary>
        /// 快速筛选功能
        /// </summary>
        public string fast = "";
        public int payType = -100;
        /// <summary>
        /// -100|0|1(已作废)
        /// </summary>
        public int aside = 0;
        /// <summary>
        /// 订单是否确认 1:已确认
        /// </summary>
        public int isSure = -100;
        /// <summary>
        /// 排序条件  字段名_asc|desc
        /// </summary>
        public string order = "";
        public string nodeids = "";
        //是否需要统计订单数
        public bool needCount = false;
        public OrderCount countMod = null;
    }
    [Serializable]
    public class OrderCount
    {
        public int all = 0;
        public int unpaid = 0;
        public int prepay = 0;
        public int paid = 0;
        //未发货
        public int unexp = 0;
        //已发货
        public int exped = 0;
        public int finished = 0;
        public int unrefund = 0;
        // 已退货订单数
        public int refunded = 0;
        public int recycle = 0;
    }
}
