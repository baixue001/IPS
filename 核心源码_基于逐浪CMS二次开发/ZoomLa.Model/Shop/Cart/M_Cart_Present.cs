using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Cart_Present : M_Base
    {

        public int ID { get; set; }
        public int CartID { get; set; }
        public string Name { get; set; }
        public int W_StartNum { get; set; }
        public int P_ID { get; set; }
        public int P_Num { get; set; }
        public string P_Name { get; set; }
        public double P_Price { get; set; }
        public int W_Type { get; set; }
        /// <summary>
        /// 共优惠了多少金额
        /// </summary>
        public double R_Price { get; set; }
        /// <summary>
        /// 共赠送了多少数量
        /// </summary>
        public int R_Num { get; set; }
        public string Remind { get; set; }

        public override string TbName { get { return "ZL_Cart_Present"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"CartID","Int","4"},
                                {"Name","NVarChar","500"},
                                {"W_StartNum","Int","4"},
                                {"P_ID","Int","4"},
                                {"P_Num","Int","4"},
                                {"P_Name","NVarChar","500"},
                                {"P_Price","Money","8"},
                                {"W_Type","Int","4"},
                                {"R_Price","Money","8"},
                                {"R_Num","Int","4"},
                                {"Remind","NVarChar","500"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Cart_Present model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.CartID;
            sp[2].Value = model.Name;
            sp[3].Value = model.W_StartNum;
            sp[4].Value = model.P_ID;
            sp[5].Value = model.P_Num;
            sp[6].Value = model.P_Name;
            sp[7].Value = model.P_Price;
            sp[8].Value = model.W_Type;
            sp[9].Value = model.R_Price;
            sp[10].Value = model.R_Num;
            sp[11].Value = model.Remind;
            return sp;
        }
        public M_Cart_Present GetModelFromReader(DbDataReader rdr)
        {
            M_Cart_Present model = new M_Cart_Present();
            model.ID = ConvertToInt(rdr["ID"]);
            model.CartID = ConvertToInt(rdr["CartID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.W_StartNum = ConvertToInt(rdr["W_StartNum"]);
            model.P_ID = ConvertToInt(rdr["P_ID"]);
            model.P_Num = ConvertToInt(rdr["P_Num"]);
            model.P_Name = ConverToStr(rdr["P_Name"]);
            model.P_Price = ConverToDouble(rdr["P_Price"]);
            model.W_Type = ConvertToInt(rdr["W_Type"]);
            model.R_Price = ConverToDouble(rdr["R_Price"]);
            model.R_Num = ConvertToInt(rdr["R_Num"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
}
