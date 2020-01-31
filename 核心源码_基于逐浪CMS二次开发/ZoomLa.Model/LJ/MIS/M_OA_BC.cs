using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ZoomLa.Model
{
    public class M_OA_BC:M_Base
    {
        public int ID { get; set; }
        public string BCName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Remind { get; set; }
        public DateTime AddTime { get; set; }
        public string BackColor { get; set; }
        public M_OA_BC()
        {

        }

        public override string TbName { get { return "ZL_OA_BC"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        	            {"ID","Int","4"},            
                        {"BCName","NVarChar","50"},            
                        {"StartTime","DateTime","8"},            
                        {"EndTime","DateTime","8"},            
                        {"Remind","NVarChar","200"},            
                        {"AddTime","DateTime","8"},
                        {"BackColor","NVarChar","50"}
              
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            var model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.BCName;
            sp[2].Value = model.StartTime;
            sp[3].Value = model.EndTime;
            sp[4].Value = model.Remind;
            sp[5].Value = model.AddTime;
            sp[6].Value = model.BackColor;
            return sp;
        }
        public M_OA_BC GetModelFromReader(DbDataReader rdr)
        {
            M_OA_BC model = new M_OA_BC();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.BCName = rdr["BCName"].ToString();
            model.StartTime = Convert.ToDateTime(rdr["StartTime"]);
            model.EndTime = Convert.ToDateTime(rdr["EndTime"]);
            model.Remind = rdr["Remind"].ToString();
            model.AddTime = Convert.ToDateTime(rdr["AddTime"]);
            model.BackColor = rdr["BackColor"].ToString();
            rdr.Close();
            rdr.Dispose();
            return model;
        }
    }
}
