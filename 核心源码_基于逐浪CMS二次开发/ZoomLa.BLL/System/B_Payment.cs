using Newtonsoft.Json.Linq;
using ZoomLa.SQLDAL.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.BLL.Helper;

namespace ZoomLa.BLL
{

    public class B_Payment
    {
        public string TbName, PK;
        private M_Payment initMod = new M_Payment();
        public B_Payment()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Add(M_Payment model)
        {
            Check(model);
            return DBCenter.Insert(model);
        }
        public bool Update(M_Payment model)
        {
            Check(model);
            return DBCenter.UpdateByID(model, model.PaymentID);
        }
        /// <summary>
        /// 选定支付方式后更新平台信息
        /// </summary>
        public bool UpdatePlat(int id, M_PayPlat.Plat plat, string platInfo = "")
        {
            M_PayPlat platMod = new B_PayPlat().SelModelByClass(plat);
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("platInfo", platInfo) };
            DBCenter.UpdateSQL(TbName, "PayPlatID=" + platMod.PayPlatID + ",PlatFormInfo=@platInfo", PK + "=" + id, sp);
            return true;
        }
        public M_Payment SelReturnModel(int ID)
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
        /// <summary>
        /// 根据订单号获取支付单号,不开放给前端,只支持自营店铺(用于省确优惠卷再计算逻辑)
        /// </summary>
        public M_Payment SelModelByOrderNo(string orderNo)
        {
            if (string.IsNullOrEmpty(orderNo)) { return null; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("orderNo", orderNo) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "PaymentNum=@orderNo", "PaymentID DESC", sp))
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
        public M_Payment SelModelByPayNo(string payno)
        {
            payno = payno.Replace(" ", "");
            if (string.IsNullOrEmpty(payno)) { return null; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("payno", payno) };
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "PayNo=@payno", "", sp))
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
        /// <summary>
        /// 根据订单,取支付单数据
        /// </summary>
        public M_Payment SelModelByOrder(M_OrderList orderinfo)
        {
            M_Payment payMod = null;
            if (!string.IsNullOrEmpty(orderinfo.PaymentNo))//已付过款,则存有支付单 
            { payMod = SelModelByPayNo(orderinfo.PaymentNo); }
            payMod = SelModelByOrderNo(orderinfo.OrderNo);//否则取最近一张订单
            return payMod;
        }
        /// <summary>
        /// 获取支付明细记录
        /// </summary>
        /// <returns></returns>
        public DataTable GetPay(string PaymentNum, string skeetype = "", string skey = "", string order = "id desc", string addon = "", int PayPlatID = -100)
        {
            string where = " 1=1";
            string orderby = "paymentid desc";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(PaymentNum)) { where += " AND PaymentNum LIKE @paynum"; sp.Add(new SqlParameter("paynum", "%" + PaymentNum + "%")); }
            if (PayPlatID != -100)
            {
                where += " AND PayPlatID = " + PayPlatID;
            }
            if (!string.IsNullOrEmpty(skeetype))
            {
                sp.Add(new SqlParameter("skey", "%" + skey + "%"));
                switch (skeetype.ToLower())
                {
                    case "paymentnum":
                        where += " AND PaymentNum LIKE @skey";
                        break;
                    case "username":
                        where += " AND UserID IN (SELECT userid FROM zl_user WHERE username LIKE @skey)";
                        break;
                    case "paytime":
                        where += " AND CONVERT(CHAR, PayTime, 20) LIKE  @skey";
                        break;
                }
            }
            if (!string.IsNullOrEmpty(addon))
            {
                switch (skeetype.ToLower())
                {
                    case "tendays"://10天内记录
                        where += " AND DATEDIFF(DAY,PayTime,'" + DateTime.Now + "')<10";
                        break;
                    case "onemonth"://一个月内记录
                        where += " AND DATEDIFF(DAY,PayTime,'" + DateTime.Now + "')<31";
                        break;
                    case "success":
                        where += " AND Status = 3";
                        break;
                    case "nosuccess":
                        where += " AND Status <> 3";
                        break;
                }
            }
            if (!string.IsNullOrEmpty(order))
            {
                switch (order.ToLower())
                {
                    case "id desc":
                    case "id asc":
                        orderby = order.ToLower().Replace("id", "paymentid");
                        break;
                    case "moneypay desc":
                    case "moneypay asc":
                    case "moneytrue desc":
                    case "moneytrue asc":
                    case "paytime desc":
                    case "paytime asc":
                        orderby = order;
                        break;
                }
            }
            return DBCenter.Sel(TbName, where, orderby, sp);
        }
        public bool ChangeRecycle(string ids, int isdel)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(TbName, "IsDel=" + isdel, "PaymentID IN (" + ids + ")");
            return true;
        }
        public void RealDelByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE PaymentID IN (" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        public void ClearRecycle()
        {
            string sql = "DELETE FROM " + TbName + " WHERE [IsDel]=1";
            SqlHelper.ExecuteSql(sql);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, PK + "=" + ID);
        }
        /// <summary>
        /// 用于充值信息列表页
        /// </summary>
        public PageSetting SelPage(int cpage, int psize,F_Payment filter)
        {
            //int uid = 0, int orderType = -1, double minMoney = 0, double maxMoney = 0, string type = "", string skey = "", int status = 0, string sysremark = ""
            List<SqlParameter> sp = new List<SqlParameter>();
            string mtable = "(SELECT A.*,B.UserName,B.[PERMISSIONS],B.HoneyName FROM (SELECT A.*,B.PayPlatName FROM ZL_Payment A LEFT JOIN ZL_PayPlat B ON A.PayPlatID=B.PayPlatID) A LEFT JOIN ZL_User B ON A.UserID=B.UserID)";
            string where = " 1=1 ";
            if (!string.IsNullOrEmpty(filter.uids)) { SafeSC.CheckIDSEx(filter.uids); where += " AND A.UserID IN ("+filter.uids+")"; }
            if (!string.IsNullOrEmpty(filter.minMoney))
            { where += " AND A.MoneyPay>=" + DataConvert.CDouble(filter.minMoney); }
            if (DataConvert.CLng(filter.maxMoney) != 0)
            { where += " AND A.MoneyPay<=" + DataConvert.CDouble(filter.maxMoney); }
            if (!string.IsNullOrEmpty(filter.uname))
            { where += " AND UserName LIKE @uname";sp.Add(new SqlParameter("uname","%"+filter.uname+"%")); }
            if (!string.IsNullOrEmpty(filter.orderno))
            { where += " AND PaymentNum LIKE @orderno"; sp.Add(new SqlParameter("orderno", "%" + filter.orderno + "%")); }
            if (!string.IsNullOrEmpty(filter.payno))
            { where += " AND PayNO LIKE @payno"; sp.Add(new SqlParameter("payno", "%" + filter.payno + "%")); }
            if (!string.IsNullOrEmpty(filter.orderType))
            { SafeSC.CheckIDSEx(filter.uids); where += " AND OrderType IN (" + filter.orderType + ")"; }

            //回收站
            if (filter.status == (int)M_OrderList.StatusEnum.Recycle) { where += " AND A.[IsDel]=1"; }
            else
            {
                where += " AND (A.[IsDel]!=1 OR A.IsDel IS NULL)";
                if (filter.status != -100) { where += " AND A.[Status] IN (" + filter.status + ")"; }

            }
            if (!string.IsNullOrEmpty(filter.skey)) { where += " AND SysRemark LIKE @skey"; sp.Add(new SqlParameter("skey", "%" + filter.skey + "%")); }
            //处理order语句
            string orderBy = StrHelper.SQL_OrderBy(filter.orderBy, "paymentid,moneypay,moneytrue,paytime");
            //string sql = "(SELECT A.*,B.UserName FROM (SELECT A.*,B.PayPlatName FROM " + TbName + " A LEFT JOIN ZL_PayPlat B ON A.PayPlatID=B.PayPlatID) A ";
            if (string.IsNullOrEmpty(orderBy)) { orderBy = PK + " DESC"; }
            //sql += "LEFT JOIN ZL_User B ON A.UserID=B.UserID)";
            //return SqlHelper.JoinQuery("A.*,B.OrderType", sql, "ZL_OrderInfo", "A.PaymentNum=B.OrderNo", where, "", sp);
            PageSetting setting = PageSetting.Double(cpage, psize, mtable, "ZL_OrderInfo", "A.PaymentID", "A.PaymentNum=B.OrderNo", where, orderBy, sp, "A.*,B.OrderType");
            DBCenter.SelPage(setting);
            return setting;
        }
        #region Tools
        /// <summary>
        /// 用户完成支付,更新支付记录信息
        /// </summary>
        public M_Payment PaySuccess(M_Payment payMod, double money, string payPlat)
        {
            if (payMod == null) { throw new Exception("支付单不存在"); }
            if (payMod.Status != (int)M_Payment.PayStatus.NoPay) { throw new Exception("支付单状态不正确"); }
            if (money < 0) { throw new Exception("金额不正确"); }
            payMod.Status = (int)M_Payment.PayStatus.HasPayed;
            payMod.CStatus = true;
            payMod.SuccessTime = DateTime.Now;
            payMod.PayTime = DateTime.Now;
            payMod.PlatformInfo += payPlat;
            payMod.MoneyTrue = money;
            return payMod;
        }
        public M_Payment CreateByOrder(M_OrderList orderMod)
        {
            var list = new List<M_OrderList>();
            list.Add(orderMod);
            return CreateByOrder(list);
        }
        /// <summary>
        /// 通过订单创建支付单
        /// </summary>
        public M_Payment CreateByOrder(List<M_OrderList> list)
        {
            if (list.Count < 1) { throw new Exception("未指定需要生成的订单"); }
            M_Payment payMod = new M_Payment();
            payMod.PayNo = CreatePayNo();
            foreach (M_OrderList model in list)
            {
                payMod.PaymentNum += model.OrderNo + ",";
                payMod.MoneyPay += model.Ordersamount;
            }
            M_OrderList first = list[0];
            payMod.PaymentNum = payMod.PaymentNum.TrimEnd(',');
            payMod.UserID = first.Userid;
            payMod.Status = 1;
            payMod.MoneyReal = payMod.MoneyPay;
            return payMod;
        }
        public string CreatePayNo()
        {
            return "PD" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + function.GetRandomString(4, 2);
        }
        private void Check(M_Payment model)
        {
            //?在管理员与所属用户不同密码的情况下,可能会获取不到信息导致报错(因为Cookies写入的关系)
            //允许为0,考虑到有优惠卷的情况
            if (model.MoneyPay < 0) { throw new Exception("支付单金额不正确"); }
            if (model.UserID < 1) { throw new Exception("支付单未绑定用户"); }
            if (string.IsNullOrEmpty(model.PaymentNum)) { throw new Exception("支付单未绑定订单"); }
            if (string.IsNullOrEmpty(model.PayNo)) { throw new Exception("未生成支付单号"); }
        }
        public CommonReturn IsCanPay(M_Payment payMod)
        {
            if (payMod == null) { return CommonReturn.Failed("支付单不存在"); }
            else if (payMod.Status != (int)M_Payment.PayStatus.NoPay) { return CommonReturn.Failed("0x14,支付单已付过款,不能重复支付"); }
            else if (payMod.MoneyReal <= 0) { return CommonReturn.Failed("0x56,支付单金额异常"); }
            else { return CommonReturn.Success(); }
        }
        #endregion
    }
    public class F_Payment
    {
        public string uids = "";
        public string orderno = "";
        public string payno = "";
        public string uname = "";
        public int status = -100;
        //订单类型ids
        public string orderType = "";
        //金额搜索范围
        public string minMoney = "";
        public string maxMoney = "";
        //搜索备注
        public string skey = "";
        public string orderBy = "";
    }
}