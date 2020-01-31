using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Font
{
    public class M_Font_Write : M_Base
    {
        public M_Font_Write()
        {
            ZStatus = 0;
            CDate = DateTime.Now;
        }
        public int ID { get; set; }
        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName { get; set; }
        /// <summary>
        /// 已完成数
        /// </summary>
        public int CompletedNum { get; set; }
        public int ZType { get; set; }
        /// <summary>
        /// 1:正在生成中(不允许点击)
        /// </summary>
        public int ZStatus { get; set; }
        public string Remind { get; set; }
        public int UserID { get; set; }
        public DateTime CDate { get; set; }
        public string UserName { get; set; }
        public override string TbName { get { return "ZL_Font_Write"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"FontName","NVarChar","100"},
                                {"CompletedNum","Int","4"},
                                {"ZType","Int","4"},
                                {"ZStatus","Int","4"},
                                {"Remind","NVarChar","2000"},
                                {"UserID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"UserName","NVarChar","100"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_Font_Write model = this;
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.FontName;
            sp[2].Value = model.CompletedNum;
            sp[3].Value = model.ZType;
            sp[4].Value = model.ZStatus;
            sp[5].Value = model.Remind;
            sp[6].Value = model.UserID;
            sp[7].Value = model.CDate;
            sp[8].Value = model.UserName;
            return sp;
        }
        public M_Font_Write GetModelFromReader(DbDataReader rdr)
        {
            M_Font_Write model = new M_Font_Write();
            model.ID = ConvertToInt(rdr["ID"]);
            model.FontName = ConverToStr(rdr["FontName"]);
            model.CompletedNum = ConvertToInt(rdr["CompletedNum"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            rdr.Close();
            return model;
        }
    }
}
