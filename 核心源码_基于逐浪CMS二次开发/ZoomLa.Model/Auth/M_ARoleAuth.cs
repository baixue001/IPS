using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Auth
{
    public class M_ARoleAuth : M_Base
    {

        public int ID { get; set; }
        public int Rid { get; set; }
        public string model { get; set; }
        public string content { get; set; }
        public string shop { get; set; }
        public string page { get; set; }
        public string exam { get; set; }
        public string user { get; set; }
        public string system { get; set; }
        public string office { get; set; }
        public string portable { get; set; }
        public string sites { get; set; }
        public string other { get; set; }
        public string extend { get; set; }
        public int AdminID { get; set; }

        public override string TbName { get { return "ZL_ARoleAuth"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Rid","Int","4"},
                                {"model","VarChar","8000"},
                                {"content","VarChar","8000"},
                                {"shop","VarChar","8000"},
                                {"page","VarChar","8000"},
                                {"exam","VarChar","8000"},
                                {"user","VarChar","8000"},
                                {"system","VarChar","8000"},
                                {"office","VarChar","8000"},
                                {"portable","VarChar","8000"},
                                {"sites","VarChar","8000"},
                                {"other","VarChar","8000"},
                                {"extend","VarChar","8000"},
                                {"AdminID","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_ARoleAuth model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Rid;
            sp[2].Value = model.model;
            sp[3].Value = model.content;
            sp[4].Value = model.shop;
            sp[5].Value = model.page;
            sp[6].Value = model.exam;
            sp[7].Value = model.user;
            sp[8].Value = model.system;
            sp[9].Value = model.office;
            sp[10].Value = model.portable;
            sp[11].Value = model.sites;
            sp[12].Value = model.other;
            sp[13].Value = model.extend;
            sp[14].Value = model.AdminID;
            return sp;
        }
        public M_ARoleAuth GetModelFromReader(DbDataReader rdr)
        {
            M_ARoleAuth model = new M_ARoleAuth();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Rid = ConvertToInt(rdr["Rid"]);
            model.model = ConverToStr(rdr["model"]);
            model.content = ConverToStr(rdr["content"]);
            model.shop = ConverToStr(rdr["shop"]);
            model.page = ConverToStr(rdr["page"]);
            model.exam = ConverToStr(rdr["exam"]);
            model.user = ConverToStr(rdr["user"]);
            model.system = ConverToStr(rdr["system"]);
            model.office = ConverToStr(rdr["office"]);
            model.portable = ConverToStr(rdr["portable"]);
            model.sites = ConverToStr(rdr["sites"]);
            model.other = ConverToStr(rdr["other"]);
            model.extend = ConverToStr(rdr["extend"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            rdr.Close();
            return model;
        }
    }
}
