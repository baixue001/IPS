namespace ZoomLa.Model
{
    using Newtonsoft.Json;
    using System;using System.Data.Common;
    using System.Data;
    
    using System.Data.SqlClient;
    public class M_Payment : M_Base
    {
        /// <summary>
        /// 1:未支付,3:已支付,5:已充值入账
        /// </summary>
        public enum PayStatus { NoPay = 1, HasPayed = 3, HasCharged = 5 }
        public int PaymentID
        {
            get;
            set;
        }
        //支付宝|PayPal交易ID
        public String AlipayNO
        {
            get;
            set;
        }
        public int UserID
        {
            get;
            set;
        }
        /// <summary>
        /// 订单号,支持以,切割
        /// </summary>
        public string PaymentNum
        {
            get;
            set;
        }
        /// <summary>
        /// 支付平台ID
        /// </summary>
        public int PayPlatID
        {
            get;
            set;
        }
        /// <summary>
        /// 支付币种,0则为默认人民币,否则为币种ID
        /// </summary>
        public int MoneyID { get; set; }
        /// <summary>
        /// RMB支付金额,以此计算费率
        /// </summary>
        public double MoneyPay
        {
            get;
            set;
        }
        /// <summary>
        /// 计算货币汇率之后的金额,用于支付(OrderPay完成计算,PayOnline页面使用此值)
        /// </summary>
        public double MoneyReal { get; set; }
        /// <summary>
        /// 实际收到的金额
        /// </summary>
        public double MoneyTrue
        {
            get;
            set;
        }
        /// <summary>
        /// 交易时间
        /// </summary>
        public DateTime PayTime
        {
            get;
            set;
        }
        /// <summary>
        /// 交易成功时间
        /// </summary>
        public DateTime SuccessTime
        {
            get;
            set;
        }
        /// <summary>
        /// 交易状态1未付款,3已付款,5已充值游戏币
        /// </summary>
        public int Status
        {
            get;
            set;
        }
        /// <summary>
        /// 1:已删除,0:正常
        /// </summary>
        public int IsDel { get; set; }
        /// <summary>
        /// 处理状态(disuse,不参与逻辑) true:已处理
        /// </summary>
        public bool CStatus
        {
            get;
            set;
        }
        /// <summary>
        /// 支付平台中文信息(支付宝网银ID,微信Mid,支付后平台信息)
        /// Purse|Score|SilverCoin
        /// </summary>
        public string PlatformInfo
        {
            get;
            set;
        }
        /// <summary>
        /// 支付单号
        /// </summary>
        public string PayNo { get; set; }
        /// <summary>
        /// 已使用的优惠券面值(如多张的话则为总和)(仅记录优惠卷的金额)
        /// </summary>
        public double ArriveMoney { get; set; }
        /// <summary>
        /// 优惠券的ID,支持IDS
        /// </summary>
        public string ArriveDetail { get; set; }
        /// <summary>
        /// 该支付单使用了多少积分
        /// </summary>
        public double UsePoint { get; set; }
        /// <summary>
        /// 使用积分抵扣了多少金额(积分的面值不固定)
        /// </summary>
        public double UsePointArrive { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark
        {
            get;
            set;
        }
        /// <summary>
        /// 系统备注,标注此支付单的信息,如捐赠:donate
        /// </summary>
        public string SysRemark { get; set; }
        /// <summary>
        /// 预付信息,仅在预付时生效,json格式
        /// </summary>
        public string PrePayInfo { get; set; }
        /// <summary>
        /// 见M_OrderList.PayType
        /// </summary>
        public int PayType { get; set; }
        #region 备用字段
        public string code { get; set; }
        #endregion
        public M_Payment()
        {
            Status = (int)M_Payment.PayStatus.NoPay;
            PayType = (int)M_OrderList.PayTypeEnum.Normal;
            MoneyID = 0;
            PlatformInfo = "";
        }
        public override string PK { get { return "PaymentID"; } }
        public override string TbName { get { return "ZL_Payment"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"PaymentID","Int","4"},
                                  {"UserID","Int","4"},
                                  {"PaymentNum","NVarChar","200"},
                                  {"PayPlatID","Int","4"},
                                  {"MoneyPay","Money","8"},
                                  {"MoneyTrue","Money","8"},
                                  {"PayTime","DateTime","8"},
                                  {"SuccessTime","DateTime","8"},
                                  {"Status","Int","4"},
                                  {"PlatformInfo","NVarChar","200"},
                                  {"Remark","NVarChar","255"},
                                  {"CStatus","Bit","4"},
                                  {"AlipayNO","NVarChar","100"},
                                  {"PayNo","NVarChar","200"},
                                  {"ArriveMoney","Money","8"},
                                  {"ArriveDetail","NVarChar","500"},
                                  {"MoneyID","Int","4"},
                                  {"MoneyReal","Money","8"},
                                  {"IsDel","Int","4"},
                                  {"UsePoint","Money","8" },
                                  {"SysRemark","NVarChar","255" },
                                  {"UsePointArrive","Money","8"},
                                  {"PayType","Int","8"},
                                  {"PrePayInfo","NVarChar","4000"},
                                  {"code","NVarChar","4000"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Payment model = this;
            if (model.PayTime <= DateTime.MinValue) model.PayTime = DateTime.Now;
            if (model.SuccessTime <= DateTime.MinValue) model.SuccessTime = DateTime.Now;
            if (model.MoneyReal == 0) { model.MoneyReal = model.MoneyPay; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.PaymentID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.PaymentNum;
            sp[3].Value = model.PayPlatID;
            sp[4].Value = model.MoneyPay;
            sp[5].Value = model.MoneyTrue;
            sp[6].Value = model.PayTime;
            sp[7].Value = model.SuccessTime;
            sp[8].Value = model.Status;
            sp[9].Value = model.PlatformInfo;
            sp[10].Value = model.Remark;
            sp[11].Value = model.CStatus;
            sp[12].Value = model.AlipayNO;
            sp[13].Value = model.PayNo;
            sp[14].Value = model.ArriveMoney;
            sp[15].Value = model.ArriveDetail;
            sp[16].Value = model.MoneyID;
            sp[17].Value = model.MoneyReal;
            sp[18].Value = model.IsDel;
            sp[19].Value = model.UsePoint;
            sp[20].Value = model.SysRemark;
            sp[21].Value = model.UsePointArrive;
            sp[22].Value = model.PayType;
            sp[23].Value = model.PrePayInfo;
            sp[24].Value = model.code;
            return sp;
        }
        public M_Payment GetModelFromReader(DbDataReader rdr)
        {
            M_Payment model = new M_Payment();
            model.PaymentID = Convert.ToInt32(rdr["PaymentID"]);
            model.UserID = Convert.ToInt32(rdr["UserID"]);
            model.PaymentNum = rdr["PaymentNum"].ToString();
            model.PayPlatID = Convert.ToInt32(rdr["PayPlatID"]);
            model.MoneyPay = Convert.ToDouble(rdr["MoneyPay"]);
            model.MoneyTrue = Convert.ToDouble(rdr["MoneyTrue"]);
            model.PayTime = ConvertToDate(rdr["PayTime"]);
            model.SuccessTime = ConvertToDate(rdr["SuccessTime"]);
            model.Status = Convert.ToInt32(rdr["Status"]);
            model.PlatformInfo = ConverToStr(rdr["PlatformInfo"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.CStatus = Convert.ToBoolean(rdr["CStatus"]);
            model.AlipayNO = ConverToStr(rdr["AlipayNO"]);
            model.PayNo = ConverToStr(rdr["PayNo"]);
            model.ArriveMoney = ConverToDouble(rdr["ArriveMoney"]);
            model.ArriveDetail = ConverToStr(rdr["ArriveDetail"]);
            model.MoneyID = ConvertToInt(rdr["MoneyID"]);
            model.MoneyReal = Convert.ToDouble(rdr["MoneyReal"]);
            model.IsDel = ConvertToInt(rdr["IsDel"]);
            model.UsePoint = ConverToDouble(rdr["UsePoint"]);
            model.SysRemark = ConverToStr(rdr["SysRemark"]);
            model.UsePointArrive = ConverToDouble(rdr["UsePointArrive"]);
            model.PrePayInfo = ConverToStr(rdr["PrePayInfo"]);
            model.PayType = ConvertToInt(rdr["PayType"]);
            model.code = ConverToStr(rdr["code"]);
            //兼容旧系统
            if (MoneyReal < 0) { MoneyReal = MoneyPay; }
            rdr.Close();
            return model;
        }
    }
    public class M_PrePayinfo
    {
        //double money_pre = DataConvert.CDouble(preInfo["money_pre"]);
        //double money_after = DataConvert.CDouble(preInfo["money_after"]);
        //double money_after_payed = DataConvert.CDouble(preInfo["money_after_payed"]);
        //double money_total = DataConvert.CDouble(preInfo["money_total"]);
        public  M_PrePayinfo(string json)
        {
            M_PrePayinfo  model = JsonConvert.DeserializeObject<M_PrePayinfo>(json);
            this.money_pre = model.money_pre;
            this.money_pre_payed = model.money_pre_payed;
            this.pre_payMethod = model.pre_payMethod;
            this.pre_payno = model.pre_payno;
            this.money_after = model.money_after;
            this.money_after_payed = model.money_after_payed;
            this.after_payMethod = model.after_payMethod;
            this.after_payno = model.after_payno;
            this.money_total = model.money_total;
            this.status = model.status;
        }
        public M_PrePayinfo() { }
        /// <summary>
        /// 预付款
        /// </summary>
        public double money_pre = 0;
        /// <summary>
        /// 预付实收款项
        /// </summary>
        public double money_pre_payed = 0;
        /// <summary>
        /// 预付支付方式 示例:虚拟币[余额]
        /// </summary>
        public string pre_payMethod = "";
        public string pre_payno = "";
        /// <summary>
        /// 待付尾款
        /// </summary>
        public double money_after = 0;
        /// <summary>
        /// 已付尾款
        /// </summary>
        public double money_after_payed = 0;
        /// <summary>
        /// 尾款支付方式
        /// </summary>
        public string after_payMethod = "";
        public string after_payno = "";
        /// <summary>
        /// 总计需付款项
        /// </summary>
        public double money_total = 0;
        /// <summary>
        /// 99表示完成
        /// </summary>
        public int status = 0;
    }
}