using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Client
{
    public class M_CRMS_Attr : M_Base
    {

        public int ID { get; set; }
        public string Value { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
        public string Remark { get; set; }
        public int ZStatus { get; set; }
        /// <summary>
        /// 数据类型 ctype,
        /// </summary>
        public string ZType { get; set; }
        public DateTime CDate { get; set; }

        public override string TbName { get { return "ZL_CRMS_Attr"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Value","NVarChar","500"},
                                {"Value2","NVarChar","500"},
                                {"Value3","NVarChar","500"},
                                {"Remark","NVarChar","500"},
                                {"ZStatus","Int","4"},
                                {"ZType","VarChar","50"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_CRMS_Attr model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Value;
            sp[2].Value = model.Value2;
            sp[3].Value = model.Value3;
            sp[4].Value = model.Remark;
            sp[5].Value = model.ZStatus;
            sp[6].Value = model.ZType;
            sp[7].Value = model.CDate;
            return sp;
        }
        public M_CRMS_Attr GetModelFromReader(DbDataReader rdr)
        {
            M_CRMS_Attr model = new M_CRMS_Attr();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Value = ConverToStr(rdr["Value"]);
            model.Value2 = ConverToStr(rdr["Value2"]);
            model.Value3 = ConverToStr(rdr["Value3"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConverToStr(rdr["ZType"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
