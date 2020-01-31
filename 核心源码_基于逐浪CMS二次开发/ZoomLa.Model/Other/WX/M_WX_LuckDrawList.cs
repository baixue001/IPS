using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Other
{
    public class M_WX_LuckDrawList : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 抽奖活动ID
        /// </summary>
        public int LuckID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 所获得的奖品
        /// </summary>
        public string Prize { get; set; }
        public string SysRemind { get; set; }

        public override string TbName { get { return "ZL_WX_LuckDrawList"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"LuckID","Int","4"},
                                {"UserID","Int","4"},
                                {"UserName","NVarChar","100"},
                                {"CDate","DateTime","8"},
                                {"Prize","NVarChar","100"},
                                {"SysRemind","NVarChar","1000"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_WX_LuckDrawList model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.LuckID;
            sp[2].Value = model.UserID;
            sp[3].Value = model.UserName;
            sp[4].Value = model.CDate;
            sp[5].Value = model.Prize;
            sp[6].Value = model.SysRemind;
            return sp;
        }
        public M_WX_LuckDrawList GetModelFromReader(DbDataReader rdr)
        {
            M_WX_LuckDrawList model = new M_WX_LuckDrawList();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.LuckID = ConvertToInt(rdr["LuckID"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Prize = ConverToStr(rdr["Prize"]);
            model.SysRemind = ConverToStr(rdr["SysRemind"]);
            rdr.Close();
            return model;
        }
    }
}
