using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.AdSystem
{
    public class M_AD_Banner : M_Base
    {
        public M_AD_Banner() {
            ZStatus = 99;
            ZType = 0;
            CDate = DateTime.Now;
        }
        public int ID { get; set; }
        public string Alias { get; set; }
        public string images { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public DateTime CDate { get; set; }

        public override string TbName { get { return "ZL_AD_Banner"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","100"},
                                {"images","NVarChar","10000"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_AD_Banner model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.images;
            sp[3].Value = model.ZStatus;
            sp[4].Value = model.ZType;
            sp[5].Value = model.CDate;
            return sp;
        }
        public M_AD_Banner GetModelFromReader(DbDataReader rdr)
        {
            M_AD_Banner model = new M_AD_Banner();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.images = ConverToStr(rdr["images"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
