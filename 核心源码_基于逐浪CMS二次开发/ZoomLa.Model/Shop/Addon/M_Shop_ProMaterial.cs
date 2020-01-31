using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_ProMaterial : M_Base
    {
        public int ID { get; set; }
        public int ProID { get; set; }
        public int MatID { get; set; }
        public double MatNum { get; set; }
        public string Remind { get; set; }
        public override string TbName { get { return "ZL_Shop_ProMaterial"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"ProID","Int","4"},
                                {"MatID","Int","4"},
                                {"MatNum","decimal","9"},
                                {"Remind","NVarChar","200"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Shop_ProMaterial model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.ProID;
            sp[2].Value = model.MatID;
            sp[3].Value = model.MatNum;
            sp[4].Value = model.Remind;
            return sp;
        }
        public M_Shop_ProMaterial GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_ProMaterial model = new M_Shop_ProMaterial();
            model.ID = ConvertToInt(rdr["ID"]);
            model.ProID = ConvertToInt(rdr["ProID"]);
            model.MatID = ConvertToInt(rdr["MatID"]);
            model.MatNum = ConverToDouble(rdr["MatNum"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
}
