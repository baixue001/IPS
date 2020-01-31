using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Order_Settle : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderID { get; set; }
        /// <summary>
        /// 支付单号
        /// </summary>
        public string PaymentNo { get; set; }
        /// <summary>
        /// 旧状态json
        /// </summary>
        public string Old { get; set; }
        /// <summary>
        /// 修改后的状态(disuse)不用存
        /// </summary>
        public string After { get; set; }
        /// <summary>
        /// 预付金额
        /// </summary>
        public double AmountPre { get; set; }
        /// <summary>
        /// 结清金额,与预付相加则为合计金额
        /// </summary>
        public double AmountSettle { get; set; }
        public int UserID { get; set; }
        public int AdminID { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 状态[预留]
        /// </summary>
        public int ZStatus { get; set; }
        public DateTime CDate { get; set; }
        public override string TbName { get { return "ZL_Order_Settle"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"OrderID","Int","4"},
        		        		{"PaymentNo","NVarChar","500"},
        		        		{"Old","NText","20000"},
        		        		{"After","NText","20000"},
        		        		{"AmountPre","Money","8"},
        		        		{"AmountSettle","Money","8"},
        		        		{"UserID","Int","4"},
        		        		{"AdminID","Int","4"},
        		        		{"Remind","NVarChar","500"},
        		        		{"ZStatus","Int","4"},
        		        		{"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Order_Settle model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.OrderID;
            sp[2].Value = model.PaymentNo;
            sp[3].Value = model.Old;
            sp[4].Value = model.After;
            sp[5].Value = model.AmountPre;
            sp[6].Value = model.AmountSettle;
            sp[7].Value = model.UserID;
            sp[8].Value = model.AdminID;
            sp[9].Value = model.Remind;
            sp[10].Value = model.ZStatus;
            sp[11].Value = model.CDate;
            return sp;
        }
        public M_Order_Settle GetModelFromReader(DbDataReader rdr)
        {
            M_Order_Settle model = new M_Order_Settle();
            model.ID = ConvertToInt(rdr["ID"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.PaymentNo = ConverToStr(rdr["PaymentNo"]);
            model.Old = ConverToStr(rdr["Old"]);
            model.After = ConverToStr(rdr["After"]);
            model.AmountPre = ConverToDouble(rdr["AmountPre"]);
            model.AmountSettle = ConverToDouble(rdr["AmountSettle"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
