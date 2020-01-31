using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.AdSystem
{
    public class M_Logo_Icon : M_Base
    {

        public int ID { get; set; }
        public string Alias { get; set; }
        public string ZType { get; set; }
        public int OrderID { get; set; }
        public DateTime CDate { get; set; }
        public int ZStatus { get; set; }
        public int AdminID { get; set; }
        /// <summary>
        /// Icon图片的虚拟路径
        /// </summary>
        public string VPath { get; set; }
        public override string TbName { get { return "ZL_Logo_Icon"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","50"},
                                {"ZType","NVarChar","50"},
                                {"OrderID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"ZStatus","Int","4"},
                                {"AdminID","Int","4"},
                                {"VPath","NVarChar","500"},
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Logo_Icon model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.ZType;
            sp[3].Value = model.OrderID;
            sp[4].Value = model.CDate;
            sp[5].Value = model.ZStatus;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.VPath;
            return sp;
        }
        public M_Logo_Icon GetModelFromReader(DbDataReader rdr)
        {
            M_Logo_Icon model = new M_Logo_Icon();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.ZType = ConverToStr(rdr["SType"]);
            model.OrderID = ConvertToInt(rdr["OrderID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.VPath = ConverToStr(rdr["VPath"]);
            rdr.Close();
            return model;
        }
    }
}
