using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.AdSystem
{
    public class M_Logo_Design : M_Base
    {
        public int ID { get; set; }
        public string Alias { get; set; }
        public string PreviewImg { get; set; }
        public string LogoContent { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime CDate { get; set; }
        public override string TbName { get { return "ZL_Logo_Design"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Alias","NVarChar","50"},
                                {"PreviewImg","Text","500000"},
                                {"LogoContent","NText","100000"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"AdminID","Int","4"},
                                {"UserID","Int","4"},
                                {"UserName","NVarChar","50"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Logo_Design model = this;
            SqlParameter[] sp = GetSP();
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.Alias;
            sp[2].Value = model.PreviewImg;
            sp[3].Value = model.LogoContent;
            sp[4].Value = model.ZStatus;
            sp[5].Value = model.ZType;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.UserID;
            sp[8].Value = model.UserName;
            sp[9].Value = model.CDate;
            return sp;
        }
        public M_Logo_Design GetModelFromReader(DbDataReader rdr)
        {
            M_Logo_Design model = new M_Logo_Design();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Alias = ConverToStr(rdr["Alias"]);
            model.PreviewImg = ConverToStr(rdr["PreviewImg"]);
            model.LogoContent = ConverToStr(rdr["LogoContent"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
