using System;using System.Data.Common;
using System.Data.SqlClient;
namespace ZoomLa.Model
{
    public class M_PayPlat : M_Base
    {
        #region 字段定义
        public int PayClass { get; set; }
        public int PayPlatID { get; set; }
        /// <summary>
        /// 平台名
        /// </summary>
        public string PayPlatName { get; set; }
        /// <summary>
        /// 商户号(支付宝:商家PID)
        /// </summary>
        public string AccountID { get; set; }
        public string APPID { get; set; }
        public string Secret { get; set; }
        /// <summary>
        /// 平台安全校验码
        /// </summary>
        public string MD5Key { get; set; }
        /// <summary>
        /// 手续费
        /// </summary>
        public float Rate { get; set; }
        /// <summary>
        /// 排序序号
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// 是否禁用 1:禁用,0:否
        /// </summary>
        public bool IsDisabled { get; set; }
        /// <summary>
        /// 是否默认平台
        /// </summary>
        public bool IsDefault { get; set; }
        /// <summary>
        /// 存储支付信息(支付宝网银--银行号,微信--APPID)
        /// </summary>
        public string payType { get; set; }
        /// <summary>
        /// 支付平台信息(支付宝网银--存储允许使用哪些支付银行,号分割)
        /// </summary>
        public string PayPlatinfo { get; set; }
        /// <summary>
        /// 仅用于PayPal
        /// </summary>
        public int UID { get; set; }
        /// <summary>
        /// 私钥文件路径|证书私钥名称
        /// </summary>
        public string PrivateKey { get; set; }
        /// <summary>
        /// 公钥文件路径
        /// </summary>
        public string PublicKey { get; set; }
        /// <summary>
        /// 支付平台公钥路径(用于验证签名)
        /// </summary>
        public string ServerPublicKey { get; set; }
        /// <summary>
        /// 私钥密码(如像微信直接挂载到系统,则可不需要输入)
        /// </summary>
        public string PrivateKey_Pwd { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remind { get; set; }

        //-----------已移除,为保证兼容,暂留
        public string Other { get; set; }
        private string leadtoGroup { get; set; }
        /// <summary>
        /// 商家PID(仅用于支付宝)
        /// </summary>
        public string SellerEmail { get; set; }
        #endregion
        public override string PK { get { return "PayPlatID"; } }
        public override string TbName { get { return "ZL_PayPlat"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"PayPlatID","Int","4"},
                                  {"UID","Int","4"},
                                  {"PayClass","Int","4"},
                                  {"PayPlatName","NVarChar","255"},
                                  {"AccountID","NVarChar","255"},
                                  {"MD5Key","NVarChar","255"},
                                  {"SellerEmail","NVarChar","255"},
                                  {"IsDisabled","Bit","4"},
                                  {"IsDefault","Bit","4"},
                                  {"Rate","Float","8"},
                                  {"OrderID","Int","4"},
                                  {"payType","NVarChar","50"},
                                  {"leadtoGroup","NVarChar","500"},
                                  {"PayPlatinfo","NVarChar","1000"},
                                  {"PrivateKey","NVarChar","200"},
                                  {"PublicKey","NVarChar","200"},
                                  {"Other","NVarChar","3000"},
                                  {"PrivateKey_Pwd","NVarChar","200"},
                                  {"APPID","NVarChar","500"},
                                  {"Secret","NVarChar","500"},
                                  {"Remind","NVarChar","500"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_PayPlat model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.PayPlatID;
            sp[1].Value = model.UID;
            sp[2].Value = model.PayClass;
            sp[3].Value = model.PayPlatName;
            sp[4].Value = model.AccountID;
            sp[5].Value = model.MD5Key;
            sp[6].Value = model.SellerEmail;
            sp[7].Value = model.IsDisabled;
            sp[8].Value = model.IsDefault;
            sp[9].Value = model.Rate;
            sp[10].Value = model.OrderID;
            sp[11].Value = model.payType;
            sp[12].Value = model.leadtoGroup;
            sp[13].Value = model.PayPlatinfo;
            sp[14].Value = model.PrivateKey;
            sp[15].Value = model.PublicKey;
            sp[16].Value = model.ServerPublicKey;
            sp[17].Value = model.PrivateKey_Pwd;
            sp[18].Value = model.APPID;
            sp[19].Value = model.Secret;
            sp[20].Value = model.Remind;
            return sp;
        }
        public M_PayPlat GetModelFromReader(DbDataReader rdr)
        {
            M_PayPlat model = new M_PayPlat();
            model.PayPlatID = Convert.ToInt32(rdr["PayPlatID"]);
            model.UID = ConvertToInt(rdr["UID"]);
            model.PayClass = Convert.ToInt32(rdr["PayClass"]);
            model.PayPlatName = ConverToStr(rdr["PayPlatName"]);
            model.AccountID = ConverToStr(rdr["AccountID"]);
            model.MD5Key = ConverToStr(rdr["MD5Key"]);
            model.SellerEmail = ConverToStr(rdr["SellerEmail"]);
            model.IsDisabled = Convert.ToBoolean(rdr["IsDisabled"]);
            model.IsDefault = Convert.ToBoolean(rdr["IsDefault"]);
            model.Rate = (float)ConverToDouble(rdr["Rate"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.payType = ConverToStr(rdr["payType"]);
            model.leadtoGroup = ConverToStr(rdr["leadtoGroup"]);
            model.PayPlatinfo = ConverToStr(rdr["PayPlatinfo"]);
            model.PrivateKey = ConverToStr(rdr["PrivateKey"]);
            model.PublicKey = ConverToStr(rdr["PublicKey"]);
            model.ServerPublicKey = ConverToStr(rdr["Other"]);
            model.PrivateKey_Pwd = ConverToStr(rdr["PrivateKey_Pwd"]);
            model.APPID = ConverToStr(rdr["APPID"]);
            model.Secret = ConverToStr(rdr["Secret"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Dispose();
            return model;
        }
        //其他的支付平台除虚拟币外不支持
        public enum Plat { Bill = 2, UnionPay = 3, ChinaUnionPay = 9, Alipay_Instant = 12, Alipay_H5 = 13, Alipay_Bank = 15, PayPal = 17, WXPay = 21, ICBC_NC = 23, Ebatong = 25, CCB = 26, ECPSS = 27, Offline = 99, CashOnDelivery = 100 }
        public static string GetPayPlatName(int payclass)
        {
            switch (payclass)
            {
                case (int)Plat.Bill:
                    return "快钱";
                case (int)Plat.UnionPay:
                    return "银联";
                case (int)Plat.ChinaUnionPay:
                    return "中国银联";
                case (int)Plat.Alipay_Instant:
                    return "支付宝[即时到账]";
                case (int)Plat.Alipay_H5:
                    return "支付宝[手机网页支付]";
                case (int)Plat.Alipay_Bank:
                    return "支付宝[网银]";
                case (int)Plat.PayPal:
                    return "PayPal";
                case (int)Plat.WXPay:
                    return "微信支付";
                case (int)Plat.ICBC_NC:
                    return "南昌工商银行";
                case (int)Plat.Ebatong:
                    return "贝付通";
                case (int)Plat.CCB:
                    return "工行支付";
                case (int)Plat.ECPSS:
                    return "汇潮支付";
                case (int)Plat.Offline:
                    return "线下支付";
                case (int)Plat.CashOnDelivery:
                    return "货到付款";
                default:
                    return "[" + payclass + "]";
            }
        }
    }
}