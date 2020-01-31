using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Content
{
    public class M_Hits : M_Base
    {

        public int ID
        {
            get;
            set;
        }

        public int UID
        {
            get;
            set;
        }
        public int GID
        {
            get;
            set;
        }
        public DateTime UpdateTime
        {
            get;
            set;
        }
        public string IP
        {
            get;
            set;
        }
        public int Status
        {
            get;
            set;
        }
        public override string TbName { get { return "ZL_Hits"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"id","Int","4"},
                                  {"GID","Int","4"},
                                  {"Uid","Int","4"},
                                  {"UpdateTime","DateTime","8"},
                                  {"Ip","NVarChar","50"},
                                  {"status","Int","4"}
                                 };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Hits model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.GID;
            sp[2].Value = model.UID;
            sp[3].Value = model.UpdateTime;
            sp[4].Value = model.IP;
            sp[5].Value = model.Status;
            return sp;
        }

        public M_Hits GetModelFromReader(DbDataReader rdr)
        {
            M_Hits model = new M_Hits();
            model.ID = Convert.ToInt32(rdr["id"]);
            model.GID = ConvertToInt(rdr["GID"]);
            model.UID = ConvertToInt(rdr["Uid"]);
            model.UpdateTime =ConvertToDate(rdr["UpdateTime"]);
            model.IP = ConverToStr(rdr["Ip"]);
            model.Status = ConvertToInt(rdr["status"]);
            rdr.Close();
            return model;
        }
    }
}
