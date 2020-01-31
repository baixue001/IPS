using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_Material : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 原材料名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 原材料单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 材料价格
        /// </summary>
        public double Price { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }
        public override string TbName { get { return "ZL_Shop_Material"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Name","NVarChar","200"},
                                {"Unit","NVarChar","50"},
                                {"Price","Money","8"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_Material model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = model.Unit;
            sp[3].Value = model.Price;
            sp[4].Value = model.CDate;
            sp[5].Value = model.Remind;
            return sp;
        }
        public M_Shop_Material GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_Material model = new M_Shop_Material();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.Unit = ConverToStr(rdr["Unit"]);
            model.Price = ConverToDouble(rdr["Price"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
}
