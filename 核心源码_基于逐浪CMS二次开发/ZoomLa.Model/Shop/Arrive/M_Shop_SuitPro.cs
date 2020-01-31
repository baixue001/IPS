using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Shop
{
    public class M_Shop_SuitPro : M_Base
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string ProIDS { get; set; }
        public int AdminID { get; set; }
        public int ZStatus { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }
        public override string TbName { get { return "ZL_Shop_SuitPro"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Name","NVarChar","200"},
                                {"ProIDS","VarChar","8000"},
                                {"AdminID","Int","4"},
                                {"ZStatus","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Shop_SuitPro model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = model.ProIDS;
            sp[3].Value = model.AdminID;
            sp[4].Value = model.ZStatus;
            sp[5].Value = model.CDate;
            sp[6].Value = model.Remind;
            return sp;
        }
        public M_Shop_SuitPro GetModelFromReader(DbDataReader rdr)
        {
            M_Shop_SuitPro model = new M_Shop_SuitPro();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.ProIDS = ConverToStr(rdr["ProIDS"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
}
