using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Sys
{
    public class M_Sys_CSSManage : M_Base
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime CDate { get; set; }
        public int AdminID { get; set; }
        public string Remind { get; set; }
        public string CSS { get; set; }
        public string CSSList { get; set; }
        /// <summary>
        /// 0:启用,-1:不启用
        /// </summary>
        public int ZStatus { get; set; }
        public override string TbName { get { return "ZL_Sys_CSSManage"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Name","NVarChar","50"},
                                {"CDate","DateTime","8"},
                                {"AdminID","Int","4"},
                                {"Remind","NVarChar","500"},
                                {"CSS","NText","100000"},
                                {"CSSList","NText","100000"},
                                {"ZStatus","Int","500"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Sys_CSSManage model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = model.CDate;
            sp[3].Value = model.AdminID;
            sp[4].Value = model.Remind;
            sp[5].Value = model.CSS;
            sp[6].Value = model.CSSList;
            sp[7].Value = model.ZStatus;
            return sp;
        }
        public M_Sys_CSSManage GetModelFromReader(DbDataReader rdr)
        {
            M_Sys_CSSManage model = new M_Sys_CSSManage();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.CSS = ConverToStr(rdr["CSS"]);
            model.CSSList = ConverToStr(rdr["CSSList"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            rdr.Close();
            return model;
        }
    }
}
