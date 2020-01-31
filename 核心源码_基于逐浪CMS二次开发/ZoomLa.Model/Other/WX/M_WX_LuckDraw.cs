using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Other
{
    public class M_WX_LuckDraw : M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 抽奖名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 提供了哪些奖品
        /// </summary>
        public string Prize { get; set; }
        public DateTime SDate { get; set; }
        public DateTime EDate { get; set; }
        /// <summary>
        /// 限制抽奖与报名人数
        /// </summary>
        public int LimitNum { get; set; }
        /// <summary>
        /// 创建人(管理员ID)
        /// </summary>
        public int AdminID { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }

        public override string TbName { get { return "ZL_WX_LuckDraw"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Name","NVarChar","200"},
                                {"Prize","NText","20000"},
                                {"SDate","DateTime","8"},
                                {"EDate","DateTime","8"},
                                {"LimitNum","Int","4"},
                                {"AdminID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","2000"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_WX_LuckDraw model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Name;
            sp[2].Value = model.Prize;
            sp[3].Value = model.SDate;
            sp[4].Value = model.EDate;
            sp[5].Value = model.LimitNum;
            sp[6].Value = model.AdminID;
            sp[7].Value = model.CDate;
            sp[8].Value = model.Remind;
            return sp;
        }
        public M_WX_LuckDraw GetModelFromReader(DbDataReader rdr)
        {
            M_WX_LuckDraw model = new M_WX_LuckDraw();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.Prize = ConverToStr(rdr["Prize"]);
            model.SDate = ConvertToDate(rdr["SDate"]);
            model.EDate = ConvertToDate(rdr["EDate"]);
            model.LimitNum = ConvertToInt(rdr["LimitNum"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            rdr.Close();
            return model;
        }
    }
    public class M_WX_LuckPrize
    {
        public string Name = "";//奖品名称
        public double Percent = 0;//中奖比率是多少
        public int Count_Total = -100;//总计多少数量,到达了该数量则不会再抽中此奖，-100则不限制数量
        public double Index_Start = 0;
        public double Index_End = 0;
    }
}
