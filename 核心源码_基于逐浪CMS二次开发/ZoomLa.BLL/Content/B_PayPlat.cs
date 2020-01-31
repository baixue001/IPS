using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{


    public class B_PayPlat
    {
        private string strTableName,PK;
        private M_PayPlat initMod = new M_PayPlat();
        public B_PayPlat() 
        {
            strTableName = initMod.TbName;
            PK = initMod.PK;
        }
        public M_PayPlat SelReturnModel(int ID)
        {
            if (ID < 1) { return null; }
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
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
        public M_PayPlat GetPayPlatByid(int ID)
        {
            return SelReturnModel(ID);
        }
        public M_PayPlat SelModelByClass(M_PayPlat.Plat type)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, " WHERE PayClass=" + (int)type))
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
        /// 获取微信支付配置(payclass=21)
        /// </summary>
        public static M_PayPlat GetModelForWx()
        {
            return new B_PayPlat().SelModelByClass(M_PayPlat.Plat.WXPay);
        }
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_PayPlat model)
        {
            return DBCenter.UpdateByID(model, model.PayPlatID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(strTableName, ID);
        }
        public int insert(M_PayPlat model)
        {
            return DBCenter.Insert(model);
        }
        /// <summary>
        /// 读取所有启用的平台
        /// </summary>
        public DataTable GetPayPlatAll()
        {
            return Sel();
        }
        /// <summary>
        /// 获取系统配置的支付平台
        /// </summary>
        public DataTable GetSysPayPlat()
        {
            string cmdText = "SELECT * FROM [ZL_PayPlat] WHERE [IsDisabled]=0 AND [UID]=0 ORDER BY [IsDefault] DESC,[OrderID] ASC";
            return SqlHelper.ExecuteTable(CommandType.Text, cmdText, null);
        }
        /// <summary>
        /// 读取所有平台列表
        /// </summary>
        public DataTable GetPayPlatListAll(int flag = -1)//All,Enable,Disabled
        {
            string where = "";
            if (flag > -1) { where = "IsDisabled=" + flag; }
            return DBCenter.Sel(strTableName, where, "OrderID ASC");
        }
        /// <summary>
        /// 最大排序序号
        /// </summary>
        public int GetMaxOrder()
        {
            string strSql = "select Max(OrderID) from ZL_PayPlat";
            return DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, null));
        }
        /// <summary>
        /// 最小排序序号
        /// </summary>
        public int GetMinOrder()
        {
            string strSql = "select Min(OrderID) from ZL_PayPlat";
            return DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, null));
        }
        /// <summary>
        /// 当前平台排序前一个平台实例
        /// </summary>
        public M_PayPlat GetPrePayPlat(int OrderID)
        {
            string strSql = "select Top 1 PayPlatID from ZL_PayPlat where OrderID<@CurrentID order by OrderID desc";
            SqlParameter[] cmdParam = new SqlParameter[] {
                new SqlParameter("@CurrentID",SqlDbType.Int,4)
            };
            cmdParam[0].Value = OrderID;
            int Pid = DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, cmdParam));
            return SelReturnModel(Pid);
        }
        /// <summary>
        /// 当前平台排序后一个平台实例
        /// </summary>
        public M_PayPlat GetNextPayPlat(int OrderID)
        {
            string strSql = "select top 1 PayPlatID from ZL_PayPlat where OrderID>@CurrentID order by OrderID asc";
            //string strSql = "select Top 1 PayPlatID from ZL_PayPlat where OrderID<@CurrentID order by OrderID desc";
            SqlParameter[] cmdParam = new SqlParameter[] {
                new SqlParameter("@CurrentID",SqlDbType.Int,4)
            };
            cmdParam[0].Value = OrderID;
            int nextid = DataConverter.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, cmdParam));
            return SelReturnModel(nextid);
        }
        public bool SetDefault(int ID)
        {
            DBCenter.UpdateSQL(strTableName, "IsDefault=0", "");
            return DBCenter.UpdateSQL(strTableName, "IsDefault=1,IsDisabled=0", "PayPlatID=" + ID);
        }
        //-----------------------------Tools
        public string GetPayPlatName(M_Payment payMod)
        {
            if (payMod == null) { return "无支付信息"; }
            string plat = "";
            if (payMod.PayPlatID == (int)M_PayPlat.Plat.Offline) { plat = "现金支付"; }
            else if (payMod.PayPlatID == (int)M_PayPlat.Plat.CashOnDelivery) { plat = "货到付款"; }
            else if (payMod.PayPlatID < 1)
            {
                plat = "虚拟币";
                switch (payMod.PlatformInfo.ToLower())
                {
                    case "purse":
                        plat += "[余额]";
                        break;
                    case "point":
                        plat += "[积分]";
                        break;
                    case "sicon":
                    case "silvercoin":
                        plat += "[银币]";
                        break;
                    default:
                        plat += "[" + payMod.PlatformInfo + "]";
                        break;
                }
            }
            else
            {
                B_PayPlat platBll = new B_PayPlat();
                if (payMod.PayPlatID < 1)
                {
                    plat = "平台不存在";
                }
                else
                {
                    M_PayPlat platMod = null;
                    //兼容之前的错误(微信存了PayClass)
                    if (payMod.PayPlatID == (int)M_PayPlat.Plat.WXPay) { platMod = platBll.SelModelByClass((M_PayPlat.Plat)payMod.PayPlatID); }
                    else { platMod = platBll.SelReturnModel(payMod.PayPlatID); }
                    plat = (platMod == null ? "平台[" + payMod.PayPlatID + "]不存在" : platMod.PayPlatName);
                }
            }
            return plat;
        }
        public string GetPayPlatName(string payno)
        {
            M_Payment payMod = new B_Payment().SelModelByPayNo(payno);
            return GetPayPlatName(payMod);
        }
    }
}