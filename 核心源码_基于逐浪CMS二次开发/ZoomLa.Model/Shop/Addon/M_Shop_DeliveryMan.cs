using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_DeliveryMan : M_Base
    {

        public int ID { get; set; }
        public int UserID { get; set; }
        /// <summary>
        /// 权限
        /// </summary>
        public string Auth { get; set; }
        /// <summary>
        /// 所属店铺
        /// </summary>
        public int StoreID { get; set; }
        /// <summary>
        /// 0:正常,-2:已删除
        /// </summary>
        public int ZStatus { get; set; }
        public string Remind { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 每送一单的提成比(暂为百分比)
        /// </summary>
        public double Bonus { get; set; }
        public M_Shop_DeliveryMan() { StoreID = 0; Auth = ""; }
        public override string TbName { get { return "ZL_Shop_DeliveryMan"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"UserID","Int","4"},
        		        		{"Auth","NVarChar","4000"},
        		        		{"StoreID","Int","4"},
        		        		{"ZStatus","Int","4"},
        		        		{"Remind","NVarChar","500"},
        		        		{"CDate","DateTime","8"},
                                {"Bonus","Money","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_DeliveryMan model = this;
            if (model.CDate <= DateTime.MinValue) {model.CDate=DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.UserID;
            sp[2].Value = model.Auth;
            sp[3].Value = model.StoreID;
            sp[4].Value = model.ZStatus;
            sp[5].Value = model.Remind;
            sp[6].Value = model.CDate;
            sp[7].Value = model.Bonus;
            return sp;
        }
        public M_Shop_DeliveryMan GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_DeliveryMan model = new M_Shop_DeliveryMan();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.Auth = ConverToStr(rdr["Auth"]);
            model.StoreID = ConvertToInt(rdr["StoreID"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Bonus = ConvertToInt(rdr["Bonus"]);
            rdr.Close();
            return model;
        }
    }
}
