using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Sys
{
    public class M_EMail_SendLog : M_Base
    {
        public M_EMail_SendLog()
        {
            Result = 0;
        }
        public int ID { get; set; }
        /// <summary>
        /// 对应的EmailID
        /// </summary>
        public int EmailID { get; set; }
        /// <summary>
        /// 接收人Email
        /// </summary>
        public string ToAddress { get; set; }
        /// <summary>
        /// 1:成功,-1:失败
        /// </summary>
        public int Result { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime CDate { get; set; }

        public override string TbName { get { return "ZL_EMail_SendLog"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"EmailID","Int","4"},
                                {"ToAddress","NVarChar","200"},
                                {"Result","Int","4"},
                                {"ErrorMsg","NVarChar","500"},
                                {"CDate","DateTime","8"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_EMail_SendLog model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.EmailID;
            sp[2].Value = model.ToAddress;
            sp[3].Value = model.Result;
            sp[4].Value = model.ErrorMsg;
            sp[5].Value = model.CDate;
            return sp;
        }
        public M_EMail_SendLog GetModelFromReader(DbDataReader rdr)
        {
            M_EMail_SendLog model = new M_EMail_SendLog();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.EmailID = ConvertToInt(rdr["EmailID"]);
            model.ToAddress = ConverToStr(rdr["ToAddress"]);
            model.Result = ConvertToInt(rdr["Result"]);
            model.ErrorMsg = ConverToStr(rdr["ErrorMsg"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            rdr.Close();
            return model;
        }
    }
}
