using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using System.Web;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_OrderList
    {
        private string TbName, PK;
        private M_OrderList initMod = new M_OrderList();
        public B_OrderList()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        //--------------------------------------------SELECT
        public M_OrderList SelReturnModel(int ID)
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
        public M_OrderList GetOrderListByid(int ID)
        {
            return SelReturnModel(ID);
        }
        public M_OrderList GetOrderListByid(int OrderListId, int orderType)
        {
            return SelReturnModel(OrderListId);
        }
        /// <summary>
        /// 取上一张或下一张订单
        /// </summary>
        public M_OrderList SelNext(int id, string direction = "next")
        {
            string sql = "SELECT * FROM " + TbName + " WHERE {0} ORDER BY {1}";
            string where = "", order = "";
            switch (direction)
            {
                case "pre":
                    where = "ID<" + id; order = "ID DESC";
                    break;
                default:
                    where = "ID>" + id; order = "ID ASC";
                    break;
            }
            sql = string.Format(sql, where, order);
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, sql))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                    return null;
            }
        }
        public M_OrderList SelModelByOrderNo(string orderno)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("OrderNo", orderno) };
            using (DbDataReader reader = Sql.SelReturnReader(TbName, "Where OrderNo=@OrderNo", sp))
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
        public M_OrderList GetByOrder(string OrderNo, string orderType)
        {
            SqlParameter[] sp = new SqlParameter[]
            {
                new SqlParameter("OrderNo", OrderNo),
                new SqlParameter("OrderType",orderType)
            };
            string strSql = "select * from " + TbName + " WHERE OrderNo = @OrderNo AND OrderType=@OrderType";
            using (DbDataReader reader = SqlHelper.ExecuteReader(CommandType.Text, strSql, sp))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                    return null;
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        /// <summary>
        /// 用于旅游订单等,后期可移除
        /// </summary>
        public PageSetting U_SelPage(Filter_Order filter)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("Skey", "%" + filter.skey + "%"), new SqlParameter("Code", filter.skey) };
            string where = "OrderType!=" + (int)M_OrderList.OrderEnum.Hide;
            if (!string.IsNullOrEmpty(filter.uids)) { SafeSC.CheckIDSEx(filter.uids); where += " AND A.UserID IN (" + filter.uids + ")"; }
            //switch (filter.fast)
            //{
            //    case "all_normal"://后期移除,全部非预付订单
            //        where += " AND Aside=0 AND Delivery!=1";
            //        break;
            //    case "all_repay"://全部预付订单
            //        where += " AND Aside=0 AND Delivery=1";
            //        break;
            //}
            switch (filter.fast)
            {
                case "all"://全部(不含回收站)
                    where += " AND A.Aside=0";
                    break;
                case "unpaid"://待付款==状态为未付款的
                    where += " AND A.PaymentStatus=" + (int)M_OrderList.PayEnum.NoPay;
                    break;
                case "prepay"://已预付款(尚未支付尾款的订单)
                    where += " AND A.Aside=0 AND A.Delivery=1 AND IsCount=0";
                    break;
                case "paid"://已支付(只支付了预付款的不在此列)
                    where += " AND A.Aside=0 AND ((A.PaymentStatus=" + (int)M_OrderList.PayEnum.HasPayed + " AND A.Delivery=0) OR (A.Delivery=1 AND A.IsCount=1 AND A.Settle=1)) ";
                    break;
                case "needpay"://需付款
                    where += " AND A.Aside=0 AND A.PaymentStatus=0";
                    break;
                case "receive"://需确认收货
                    where += " AND A.Aside=0 AND A.StateLogistics=1";
                    break;
                case "comment"://已完结可评价(上层筛出IDS,再根据IDS得出订单,或单独一个页面展示可评价的商品)
                    //where += " AND A.Aside=0 AND A.OrderStatus>=" + (int)M_OrderList.StatusEnum.OrderFinish + " AND (CHARINDEX('comment',B.AddStatus)=0 OR B.AddStatus IS NULL) ";
                    where += " AND A.Aside=0 AND A.OrderStatus>=" + (int)M_OrderList.StatusEnum.OrderFinish;
                    where += " AND (SELECT COUNT(*) FROM ZL_CartPro WHERE Orderlistid=A.ID AND (AddStatus IS NULL OR AddStatus=''))>0";//AddStatus中会有退货记录,所以筛选为必须为空
                    break;
                case "recycle"://订单回收站
                    where += " AND A.Aside=1";
                    break;
            }
            if (filter.isSure != -100) { where += " AND A.IsSure=" + filter.isSure; }
            if (filter.payType != -100)
            {
                if (filter.payType == 0) { where += " AND (B.PayType IS NULL OR B.PayType=0) "; }
                else { where += " AND B.PayType=" + filter.payType; }
            }
            if (DataConvert.CLng(filter.storeType) > 0) { where += " AND A.StoreID=" + Convert.ToInt32(filter.storeType); }
            if (!string.IsNullOrEmpty(filter.orderType) && filter.orderType != "-1") { SafeSC.CheckIDSEx(filter.orderType); where += " AND OrderType IN(" + filter.orderType + ")"; }
            //PageSetting setting = PageSetting.Single(filter.cpage, filter.psize, TbName, PK, where, PK + " DESC", sp);
            PageSetting setting = PageSetting.Double(filter.cpage, filter.psize, TbName, "ZL_Payment", "A.ID", "A.PaymentNo=B.PayNo", where, "A.ID desc", sp, "A.*,B.PayType");
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable GetOrderListByUid(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        //支持,号切割
        public DataTable GetOrderbyOrderNo(string orderno)
        {
            string insql = "";
            string[] orderNoArr = orderno.Split(',');
            SqlParameter[] sp = new SqlParameter[orderNoArr.Length];
            for (int i = 0; i < orderNoArr.Length; i++)
            {
                sp[i] = new SqlParameter("@OrderNo" + i, orderNoArr[i]);
                insql += sp[i].ParameterName + ",";
            }
            insql = insql.Trim(',');
            string sql = "SELECT * FROM " + TbName + " WHERE OrderNo IN (" + insql + ") ORDER BY [ID] DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public DataTable GetOrderbyOrderlist(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sqlStr = "select * from ZL_Orderinfo where id in (" + ids + ") order by(id) desc";
            //string sqlStr = "select PaymentNum from zl_payment where paymentid=" + idlist;
            return SqlHelper.ExecuteTable(CommandType.Text, sqlStr, null);
        }
        /// <summary>
        /// 用于财务流水 搜索类型 1:按userid检索,2:按UserName检索
        /// </summary>
        public DataTable SelByHistory(int searchtype = 1, string search = "")
        {
            string strwhere = " WHERE 1=1";
            if (!string.IsNullOrEmpty(search))
            {
                if (searchtype == 1)
                {
                    strwhere += " AND UserID=@search";
                    search = DataConverter.CLng(search).ToString();
                }
                else
                {
                    strwhere += " AND AddUser LIKE @search";
                    search = "%" + search + "%";
                }
            }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@search", search) };
            string sql = "SELECT OrderNo AS ExpHisID,UserID,AddTime AS HisTime,AddUser AS UserName,Balance_price AS Score,Balance_remark AS Detail FROM " + TbName + strwhere + " ORDER BY AddTime DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        /// <summary>
        /// 依据UserID与支付方式,查找目标,包含商品信息,用于IDC
        /// </summary>
        public DataTable SelByUserID(int userID, int payment = 1)
        {
            string field = "A.OrderNo,A.Internalrecords,A.OrderMessage,A.Paymentstatus,B.*";
            //string sql = "Select " + field + " From ZL_OrderInfo as a Left Join ZL_CartPro as b on a.id=b.orderlistid Where a.UserID=" + userID + " And a.OrderType=7 And a.PayMentStatus=" + payment + " Order by b.AddTime Desc";
            //return SqlHelper.ExecuteTable(CommandType.Text, sql);
            string where = "A.UserID=" + userID + " AND A.OrderType=7";
            if (payment != 2)//2显示全部
            {
                where += " AND A.PayMentStatus= " + payment;
            }
            return SqlHelper.JoinQuery(field, "ZL_OrderInfo", "ZL_CartPro", "A.ID=B.OrderListID", where, " B.AddTime DESC");
        }
        /// <summary>
        /// 依据订单ID查找目标,包含商品信息,用于IDC,续费
        /// </summary>
        public DataTable SelByOrderNo(string orderNo)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("OrderNo", orderNo) };
            string field = "a.OrderNo,a.Internalrecords,b.*";
            string sql = "Select " + field + " From ZL_OrderInfo as a Left Join ZL_CartPro as b on a.id=b.orderlistid Where a.OrderNo=@OrderNo And a.OrderType=7 Order by b.AddTime Desc";
            return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
        }
        public DataTable SelByTime(string stime, string etime, int type)
        {
            string sql = "Select * From " + TbName + " Where 1=1";
            string cds = "";
            string cdsd = "";
            if (stime != "")
            {
                cds = " AND AddTime>@stime ";
            }
            if (etime != "")
            {
                if (cds != "")
                {
                    cds = cds + " and ";
                }

                cds = cds + "AddTime<@etime";
            }
            if (cds != "")
            {
                cdsd = cds;
                cds = " and " + cds;

            }
            switch (type)
            {
                case 1://客户平均订单
                    sql += "and Aside=0 " + cds;
                    break;
                case 2://每次访问订单
                    sql += cdsd;
                    break;
                case 3://匿名购买率
                    sql += "and Rename='' and Aside=0 and Paymentstatus=1 " + cds;
                    break;
                case 4://会员购买率
                    sql += "and Rename<>'' and Aside=0 and Paymentstatus=1 " + cds;
                    break;
            }
            try
            {
                SqlParameter[] sp = new SqlParameter[] { new SqlParameter("stime", stime), new SqlParameter("etime", etime) };
                return SqlHelper.ExecuteTable(CommandType.Text, sql, sp);
            }
            catch { throw new Exception(sql); }
        }
        //--------------------------------------------INSERT
        public int insert(M_OrderList model)
        {
            return DBCenter.Insert(model);
        }
        //--------------------------------------------UPDATE
        public bool UpdateByID(M_OrderList model)
        {
            return DBCenter.UpdateByID(model,model.id);
        }
        public bool UpdateByField(string fieldName, string value, int id)
        {
            SafeSC.CheckDataEx(fieldName);
            string sql = "Update " + TbName + " Set " + fieldName + " =@value Where [id] =" + id;
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("value", value) };
            SqlHelper.ExecuteNonQuery(CommandType.Text, sql, sp);
            return true;
        }
        public bool Update(M_OrderList model)
        {
            return UpdateByID(model);
        }
        //页面无注入
        public bool UpOrderinfo(string info, int id)
        {
            string sqlStr = "update ZL_Orderinfo set " + info + " where id=" + id + "";
            return SqlHelper.ExecuteSql(sqlStr, null);
        }
        /// <summary>
        /// 插入快递单号
        /// </summary>
        public int UpdateExpressNum(string Num, int id)
        {
            string sql = "update ZL_Orderinfo set ExpressNum=@ExpressNum where id=" + id;
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@ExpressNum", Num), };
            return Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, sql, sp));
        }
        /// <summary>
        /// 订单状态批量修改_后台调用
        /// </summary>
        public void ChangeStatus(string ids, M_OrderList.StatusEnum status)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            switch (status)
            {
                case M_OrderList.StatusEnum.Sured://确认订单
                    DBCenter.UpdateSQL(TbName, "IsSure=1", "ID IN (" + ids + ")");
                    break;
                default:
                    throw new Exception("[" + status + "]的操作不存在");
            }
        }
        public void ChangeSure(string ids, int sure)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(TbName,"IsSure="+sure,"ID IN ("+ids+")");
        }
        public void ChangeStatus(string ids, string status)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            switch (status.ToLower())
            {
                case "recover":
                    DBCenter.UpdateSQL(TbName, "Aside=0", "ID IN (" + ids + ")");
                    break;
                case "recycle":
                    DBCenter.UpdateSQL(TbName, "Aside=1", "ID IN (" + ids + ")");
                    break;
                default:
                    throw new Exception("[" + status + "]操作不存在");
            }
        }
        //----------------------------DELETE
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public void DelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN(" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        public void ClearRecycle()
        {
            DBCenter.DelByWhere(TbName, "Aside=1");
        }
        //----------------------------Tools
        public static string CreateOrderNo(M_OrderList.OrderEnum orderType)
        {
            string result = "";
            string name = DateTime.Now.ToString("yyyyMMddHHmmssfff") + function.GetRandomString(4, 2);
            switch (orderType)
            {
                case M_OrderList.OrderEnum.Normal:
                    result = "DD" + name;
                    break;
                case M_OrderList.OrderEnum.IDC:
                    result = "IDC" + name;
                    break;
                case M_OrderList.OrderEnum.IDCRen:
                    result = "IDCR" + name;
                    break;
                case M_OrderList.OrderEnum.Score:
                    result = "DP" + name;
                    break;
                case M_OrderList.OrderEnum.Cloud:
                    result = "YG" + name;
                    break;
                case M_OrderList.OrderEnum.Purse://充值
                    result = "RC" + name;
                    break;
                case M_OrderList.OrderEnum.Trval:
                    result = "TR" + name;
                    break;
                case M_OrderList.OrderEnum.Hotel:
                    result = "HT" + name;
                    break;
                case M_OrderList.OrderEnum.Fast:
                    result = "FT" + name;
                    break;
                case M_OrderList.OrderEnum.Other:
                    result = "OT" + name;
                    break;
                case M_OrderList.OrderEnum.Donate:
                    result = "DO" + name;
                    break;
                default:
                    result = "OT" + name;
                    break;
            }
            return result;
        }
        /// <summary>
        /// 创建一张新订单
        /// </summary>
        public M_OrderList NewOrder(M_UserInfo mu, M_OrderList.OrderEnum orderType)
        {
            M_OrderList orderMod = new M_OrderList();
            orderMod.Ordertype = (int)orderType;
            orderMod.OrderNo = CreateOrderNo(orderType);
            orderMod.Userid = mu.UserID;
            orderMod.AddUser = mu.UserName;
            orderMod.Receiver = mu.UserName;
            return orderMod;
        }
        public string ShowReceAddress(M_OrderList orderMod, string format = "")
        {
            return orderMod.Shengfen + " " + orderMod.Jiedao + "(" + orderMod.Receiver + ")";
        }
        //----------------------------Logical
        /// <summary>
        /// 支付完成结后,更改订单状态(支付单传入时无ID号)
        /// </summary>
        public bool FinishOrder(int mid, M_Payment payMod)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("PaymentNo", payMod.PayNo) };
            DBCenter.UpdateSQL(TbName, "OrderStatus=99,PaymentNo=@PaymentNo", "ID=" + mid, sp);
            return true;
        }
        /// <summary>
        /// 0:正常状态
        /// 1:用户回收站
        /// 2:用户彻底删除|管理员删除订单时,返还优惠卷,并删除支付单
        ///(仅可操作未完成支付的支付单)
        /// </summary>
        public bool CancelOrder(M_OrderList orderMod)
        {
            B_Payment payBll = new B_Payment();
            M_Payment payMod = payBll.SelModelByOrder(orderMod);
            //支付单不存在,或状态不为未支付,则忽略
            if (payMod == null) { return false; }
            //返还优惠卷
            if (!string.IsNullOrEmpty(payMod.ArriveDetail))
            {
                B_Arrive avBll = new B_Arrive();
                M_Arrive avMod = avBll.SelReturnModel(DataConvert.CLng(payMod.ArriveDetail));
                if (avMod != null && avMod.State == 10)
                {
                    avMod.State = 1;
                    avMod.UseRemind = "订单[" + orderMod.OrderNo + "]取消,返还优惠卷";
                    avBll.GetUpdate(avMod);
                }
            }
            //返回积分,需计算积分的值
            if (payMod.UsePoint > 0) { }
            DBCenter.UpdateSQL(payMod.TbName, "Status=" + (int)M_Payment.PayStatus.NoPay + ",MoneyTrue=0,IsDel=1", "PaymentID=" + payMod.PaymentID);
            return true;
        }
        //----------------------------用户使用方法,所有必须传入UserID验证
        public void DelByIDS_U(string ids, int uid, int aside = 1)
        {
            if (string.IsNullOrEmpty(ids)) return;
            SafeSC.CheckIDSEx(ids);
            string sql = "";
            sql = "Update " + TbName + " SET Aside=" + aside + " Where ID in(" + ids + ")";
            if (uid != -100)//也允许后台调用此方法删
            {
                sql += " AND UserID=" + uid;
            }
            SqlHelper.ExecuteSql(sql);
        }
        public DataTable SelInvoByUser(int uid)
        {
            string sql = "Select Top 5 * From (Select Distinct(Invoice) From " + TbName + " Where Userid=" + uid + " And Invoice !='')as A";
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, sql);
            dt.Columns.Add(new DataColumn("Head", typeof(string)));
            dt.Columns.Add(new DataColumn("Detail", typeof(string)));
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["Head"] = Regex.Split(dt.Rows[i]["Invoice"].ToString(), Regex.Escape("||"))[0];
                dt.Rows[i]["Detail"] = Regex.Split(dt.Rows[i]["Invoice"].ToString(), Regex.Escape("||"))[1];
            }
            return dt;
        }
        /// <summary>
        /// 主用用户中心订单获取方法(只显示正常未作废订单)
        /// </summary>
        public DataTable U_SelByUserID(int uid)
        {
            string sql = "SELECT * FROM " + TbName + " WHERE Aside=0 AND UserID=" + uid + " ORDER BY AddTime DESC";
            return SqlHelper.ExecuteTable(sql);

        }
        //--------------------------Report
        /// <summary>
        /// 按日期获取数据,精确到日
        /// </summary>
        public DataTable Report_SelByDate(DateTime stime, DateTime etime,int sid=-100)
        {
            string field = "A.ID,A.OrderNo,A.Ordersamount,A.Receivablesamount,B.PayTime,B.PayPlatID";
            string where = "(A.PaymentNO IS NOT NULL AND A.PaymentNO!='') AND A.OrderStatus>=0 ";//已支付且在正常流程中
            where += " AND PayTime>='" + stime.ToString("yyyy/MM/dd 00:00:00") + "'";
            where += " AND PayTime<='" + etime.ToString("yyyy/MM/dd 23:59:59") + "'";
            if (sid != -100)
            {
                where += " AND A.StoreID="+sid;
            }
            return DBCenter.JoinQuery(field, "ZL_OrderInfo", "ZL_Payment", "A.PaymentNo=B.PayNo", where);
        }
    }
}