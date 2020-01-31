using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Order_OPLog : M_Base
    {

        public int ID { get; set; }
        public string OrderNo { get; set; }
        /// <summary>
        /// 操作名称
        /// </summary>
        public string OPName { get; set; }
        /// <summary>
        /// 操作前订单状态(主要信息)
        /// </summary>
        public string Before { get; set; }
        /// <summary>
        /// 操作后订单状态(只记录变更后的值),如果opname足够描述操作,则此项为空
        /// </summary>
        public string After { get; set; }
        public DateTime CDate { get; set; }
        public int AdminID { get; set; }
        public string Remind { get; set; }
        public override string TbName { get { return "ZL_Order_OPLog"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"OrderNo","VarChar","100"},
        		        		{"OPName","NVarChar","100"},
        		        		{"Before","NText","20000"},
        		        		{"After","NText","20000"},
        		        		{"CDate","DateTime","8"},
        		        		{"AdminID","Int","4"},
        		        		{"Remind","NVarChar","1000"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Order_OPLog model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.OrderNo;
            sp[2].Value = model.OPName;
            sp[3].Value = model.Before;
            sp[4].Value = model.After;
            sp[5].Value = model.CDate;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.Remind;
            return sp;
        }
        public M_Order_OPLog GetModelFromReader(DbDataReader rdr)
        {
            M_Order_OPLog model = new M_Order_OPLog();
            model.ID = ConvertToInt(rdr["ID"]);
            model.OrderNo = ConverToStr(rdr["OrderNo"]);
            model.OPName = ConverToStr(rdr["OPName"]);
            model.Before = ConverToStr(rdr["Before"]);
            model.After = ConverToStr(rdr["After"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
}
