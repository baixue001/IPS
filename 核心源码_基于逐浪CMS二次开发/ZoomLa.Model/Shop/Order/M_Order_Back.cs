using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Order_Back : M_Base
    {

        public int ID { get; set; }
        /// <summary>
        /// 关闭的订单ID
        /// </summary>
        public int OrderID { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 用户退款原因
        /// </summary>
        public string UserRemind { get; set; }
        /// <summary>
        /// 管理员备注
        /// </summary>
        public string AdminRemind { get; set; }
        /// <summary>
        /// 订单备份(JSON数据)
        /// </summary>
        public string OrderBak { get; set; }
        /// <summary>
        /// 申请人ID
        /// </summary>
        public int UserID { get; set; }
        /// <summary>
        /// 处理该申请的管理员ID
        /// </summary>
        public int AdminID { get; set; }
        /// <summary>
        /// 处理状态 99处理完成
        /// </summary>
        public int ZStatus { get; set; }

        public override string TbName { get { return "ZL_Order_Back"; } }
        public override string PK { get { return "ID"; } }
        public M_Order_Back()
        {
            ZStatus = 0;
            CDate = DateTime.Now;
        }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"OrderID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"UserRemind","NVarChar","500"},
                                {"AdminRemind","NVarChar","500"},
                                {"OrderBak","NText","20000"},
                                {"UserID","Int","4"},
                                {"AdminID","Int","4"},
                                {"ZStatus","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Order_Back model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.OrderID;
            sp[2].Value = model.CDate;
            sp[3].Value = model.UserRemind;
            sp[4].Value = model.AdminRemind;
            sp[5].Value = model.OrderBak;
            sp[6].Value = model.UserID;
            sp[7].Value = model.AdminID;
            sp[8].Value = model.ZStatus;
            return sp;
        }
        public M_Order_Back GetModelFromReader(DbDataReader rdr)
        {
            M_Order_Back model = new M_Order_Back();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.OrderID = Convert.ToInt32(rdr["OrderID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.UserRemind = ConverToStr(rdr["UserRemind"]);
            model.AdminRemind = ConverToStr(rdr["AdminRemind"]);
            model.OrderBak = ConverToStr(rdr["OrderBak"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            rdr.Close();
            return model;
        }
    }
}
