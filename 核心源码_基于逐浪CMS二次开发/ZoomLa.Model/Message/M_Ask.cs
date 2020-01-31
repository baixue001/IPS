using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace ZoomLa.Model
{
    public class M_Ask: M_Base
    {
        public M_Ask()
        {
            Elite = 0;
            Status = 1;
            IsNi = 0;
            IsSole = 0;
        }
        public int ID { get; set; }
        /// <summary>
        /// 问题
        /// </summary>
        public string Qcontent { get; set; }
        /// <summary>
        /// 问题补充
        /// </summary>
        public string Supplyment { get; set; }
        /// <summary>
        /// 问题分类
        /// </summary>
        public string QueType { get; set; }
        public DateTime AddTime { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 推荐
        /// </summary>
        public int Elite { get; set; }
        /// <summary>
        /// 赏分
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 状态 0未审核 1已审核 2已解决
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// 匿名  1匿名
        /// </summary>
        public int IsNi { get; set; }
        /// <summary>
        /// 是否为独占模式
        /// </summary>
        public int IsSole { get; set; }
        public string QTitle { get; set; }
        public override string TbName { get { return "ZL_Ask"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                  {"ID","Int","4"}, 
                                  {"Qcontent","NText","20000"}, 
                                  {"Supplyment","NText","20000"}, 
                                  {"QueType","NVarChar","100"}, 
                                  {"AddTime","DateTime","8"}, 
                                  {"UserId","Int","4"}, 
                                  {"UserName","NVarChar","255"}, 
                                  {"Elite","Int","4"}, 
                                  {"Score","Int","4"}, 
                                  {"Status","Int","4"}, 
                                  {"IsNi","Int","4"},
                                  {"QTitle","NVarChar","50"},
                                  {"IsSole","Int","4"}
                                 };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Ask model=this;
            if (model.AddTime <= DateTime.MinValue) { model.AddTime = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Qcontent;
            sp[2].Value = model.Supplyment;
            sp[3].Value = model.QueType;
            sp[4].Value = model.AddTime;
            sp[5].Value = model.UserId;
            sp[6].Value = model.UserName;
            sp[7].Value = model.Elite;
            sp[8].Value = model.Score;
            sp[9].Value = model.Status;
            sp[10].Value = model.IsNi;
            sp[11].Value = model.QTitle;
            sp[12].Value = model.IsSole;
            return sp;
        }
        public M_Ask GetModelFromReader(DbDataReader rdr)
        {
            M_Ask model = new M_Ask();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Qcontent = ConverToStr(rdr["Qcontent"]);
            model.Supplyment = ConverToStr(rdr["Supplyment"]);
            model.QueType = ConverToStr(rdr["QueType"]);
            model.AddTime = ConvertToDate(rdr["AddTime"]);
            model.UserId = ConvertToInt(rdr["UserId"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.Elite = ConvertToInt(rdr["Elite"]);
            model.Score = ConvertToInt(rdr["Score"]);
            model.Status = ConvertToInt(rdr["Status"]);
            model.IsNi = ConvertToInt(rdr["IsNi"]);
            model.QTitle = ConverToStr(rdr["QTitle"]);
            model.IsSole = ConvertToInt(rdr["IsSole"]);
            rdr.Dispose();
            return model;
        }
    }
}
