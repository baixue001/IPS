namespace ZoomLa.Components
{
    using System;
    /// <summary>
    /// 商城配置
    /// </summary>
    [Serializable]
    public class ShopConfig
    {
        private string m_ItemRegular = "";
        private string m_expnames = "";
        //---------新增
        /// <summary>
        /// 商品是否确认后支付 1:是
        /// </summary>
        public int IsCheckPay { get; set; }
        /// <summary>
        /// Item生成规则(21yyyyMMddHH)+5位数字随机
        /// </summary>
        public string ItemRegular
        {
            get { if (string.IsNullOrEmpty(m_ItemRegular)) { m_ItemRegular = "21yyyyMMdd"; } return m_ItemRegular; }
            set { m_ItemRegular = value; }
        }
        /// <summary>
        /// 订单过期时间 单位:小时
        /// </summary>
        public int OrderExpired { get; set; }
        
        /// <summary>
        /// 订单可用积分支付比率,默认:10==10%,使用时需要*0.01
        /// </summary>
        public double PointRatiot { get; set; }
        /// <summary>
        /// 积分与现金兑换比率,默认:1积分==0.01
        /// </summary>
        public double PointRate { get; set; }
        /// <summary>
        /// 快递方式,格式为 (快递名|快递名2|快递名3) 
        /// </summary>
        public string ExpNames { get { return string.IsNullOrEmpty(m_expnames) ? "快递|EMS|平邮" : m_expnames; } set { m_expnames = value; } }
        #region 邮件提醒配置
        public string EmailOfDeliver
        {
            get;
            set;
        }
        public string EmailOfInvoice
        {
            get;
            set;
        }
        public string EmailOfOrderConfirm
        {
            get;
            set;
        }
        public string EmailOfReceiptMoney
        {
            get;
            set;
        }
        public string EmailOfRefund
        {
            get;
            set;
        }
        public string EmailOfSendCard
        {
            get;
            set;
        }
        #endregion
    }
}