using Newtonsoft.Json;
using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_Present : M_Base
    {
        public int ID { get; set; }
        public int ProID { get; set; }
        /// <summary>
        /// 促销方式,暂只支持赠送
        /// </summary>
        public int W_Type { get; set; }
        /// <summary>
        /// 赠送条件,默认为1
        /// </summary>
        public int W_StartNum { get; set; }
        /// <summary>
        /// 赠品ID
        /// </summary>
        public int P_ID { get; set; }
        /// <summary>
        /// 每满足一次条件,赠送多少
        /// </summary>
        public int P_Num { get; set; }
        public string P_Name { get; set; }
        /// <summary>
        /// 赠品金额,非0则需要计售价
        /// </summary>
        public double P_Price { get; set; }
        /// <summary>
        /// 促销名称
        /// </summary>
        public string Name { get; set; }
        [JsonIgnore]
        public override string TbName { get { return "ZL_Shop_Present"; } }
        [JsonIgnore]
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"ProID","Int","4"},
                                {"W_StartNum","Int","4"},
                                {"P_ID","Int","4"},
                                {"P_Num","Int","4"},
                                {"P_Name","NVarChar","100"},
                                {"P_Price","Money","8"},
                                {"W_Type","Int","4"},
                                {"Name","NVarChar","100"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_Present model = this;
            SqlParameter[] sp = GetSP();
            if (model.W_StartNum < 1) { model.W_StartNum = 1; }
            sp[0].Value = model.ID;
            sp[1].Value = model.ProID;
            sp[2].Value = model.W_StartNum;
            sp[3].Value = model.P_ID;
            sp[4].Value = model.P_Num;
            sp[5].Value = model.P_Name;
            sp[6].Value = model.P_Price;
            sp[7].Value = model.W_Type;
            sp[8].Value = model.Name;
            return sp;
        }
        public M_Shop_Present GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_Present model = new M_Shop_Present();
            model.ID = ConvertToInt(rdr["ID"]);
            model.ProID = ConvertToInt(rdr["ProID"]);
            model.W_StartNum = ConvertToInt(rdr["W_StartNum"]);
            model.P_ID = ConvertToInt(rdr["P_ID"]);
            model.P_Num = ConvertToInt(rdr["P_Num"]);
            model.P_Name = ConverToStr(rdr["P_Name"]);
            model.P_Price = ConverToDouble(rdr["P_Price"]);
            model.W_Type = ConvertToInt(rdr["W_Type"]);
            model.Name = ConverToStr(rdr["Name"]);
            rdr.Close();
            return model;
        }
        public M_Shop_Present GetModelFromReader(DataRow rdr)
        {
            M_Shop_Present model = new M_Shop_Present();
            model.ID = ConvertToInt(rdr["ID"]);
            model.ProID = ConvertToInt(rdr["ProID"]);
            model.W_StartNum = ConvertToInt(rdr["W_StartNum"]);
            model.P_ID = ConvertToInt(rdr["P_ID"]);
            model.P_Num = ConvertToInt(rdr["P_Num"]);
            model.P_Name = ConverToStr(rdr["P_Name"]);
            model.P_Price = ConverToDouble(rdr["P_Price"]);
            model.W_Type = ConvertToInt(rdr["W_Type"]);
            model.Name = ConverToStr(rdr["Name"]);
            return model;
        }
    }
}
