using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_ProOrderList : M_Base
    {
        public int ID { get; set; }
        public int OrderListID { get; set; }
        public int ProID { get; set; }
        public string ProName { get; set; }
        public double ProPrice { get; set; }
        public double AllMoney { get; set; }
        public int Pronum { get; set; }
        public int Pronum_Rece { get; set; }
        /// <summary>
        /// 商品优惠金额
        /// </summary>
        public double ProArrive { get; set; }
        public string Remind { get; set; }
        public string ZStatus { get; set; }
        public DateTime CDate { get; set; }
        public int UserID { get; set; }
        public M_Shop_ProOrderList()
        {
            ProArrive = 0;
            Pronum_Rece = 0;
            ZStatus = "";
        }

        public override string TbName { get { return "ZL_Shop_ProOrderList"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"OrderListID","Int","4"},
                                {"ProID","Int","4"},
                                {"ProName","NVarChar","100"},
                                {"ProPrice","Money","8"},
                                {"Pronum","Int","4"},
                                {"Pronum_Rece","Int","4"},
                                {"ProArrive","Money","8"},
                                {"Remind","NVarChar","500"},
                                {"ZStatus","NVarChar","200"},
                                {"CDate","DateTime","8"},
                                {"UserID","Int","4"},
                                {"AllMoney","Money","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_ProOrderList model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.OrderListID;
            sp[2].Value = model.ProID;
            sp[3].Value = model.ProName;
            sp[4].Value = model.ProPrice;
            sp[5].Value = model.Pronum;
            sp[6].Value = model.Pronum_Rece;
            sp[7].Value = model.ProArrive;
            sp[8].Value = model.Remind;
            sp[9].Value = model.ZStatus;
            sp[10].Value = model.CDate;
            sp[11].Value = model.UserID;
            sp[12].Value = model.AllMoney;
            return sp;
        }
        public M_Shop_ProOrderList GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_ProOrderList model = new M_Shop_ProOrderList();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.OrderListID = ConvertToInt(rdr["OrderListID"]);
            model.ProID = ConvertToInt(rdr["ProID"]);
            model.ProName = ConverToStr(rdr["ProName"]);
            model.ProPrice = ConverToDouble(rdr["ProPrice"]);
            model.Pronum = ConvertToInt(rdr["Pronum"]);
            model.Pronum_Rece = ConvertToInt(rdr["Pronum_Rece"]);
            model.ProArrive = ConverToDouble(rdr["ProArrive"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.ZStatus = ConverToStr(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.AllMoney = ConverToDouble(rdr["AllMoney"]);
            rdr.Close();
            return model;
        }
    }
}
