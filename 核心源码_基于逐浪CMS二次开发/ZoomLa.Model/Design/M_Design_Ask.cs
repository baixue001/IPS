using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Design
{
    public class M_Design_Ask : M_Base
    {
        public int ID { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int CUser { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 问卷类型 1:投票,2:问卷,3:报名
        /// </summary>
        public int ZType { get; set; }
        /// <summary>
        /// 问卷状态
        /// </summary>
        public int ZStatus { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 终止日期
        /// </summary>
        public DateTime EndDate { get; set; }
        /// <summary>
        /// 预览图用于微信分享
        /// </summary>
        public string PreViewImg { get; set; }
        public int AdminID { get; set; }
        public DateTime StartDate { get; set; }
        /// <summary>
        /// 提交问卷后,用户可否见到结果
        /// </summary>
        public int IsShowResult { get; set; }
        /// <summary>
        /// 同IP间隔多久才可再次投票(秒),0不限制
        /// </summary>
        public int IPInterval { get; set; }
        /// <summary>
        /// 0不限制IP,>0可提交数
        /// </summary>
        public int IsIPLimit { get; set; }
        /// <summary>
        /// 0:不需要登录,1:需要登录
        /// </summary>
        public int IsNeedLogin { get; set; }
        /// <summary>
        /// 0:不启用,1:启用
        /// </summary>
        public int IsEnableVCode { get; set; }
        public M_Design_Ask()
        {
            PreViewImg = "";
            ZStatus = 99;
            IsNeedLogin = 0;
            IsIPLimit = 1;
            IsEnableVCode = 0;
            IsShowResult = 1;
            ZType = 2;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddYears(1);
        }
        public override string TbName { get { return "ZL_Design_Ask"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
        		        		{"ID","Int","4"},
        		        		{"Title","NVarChar","200"},
        		        		{"CUser","Int","4"},
        		        		{"Remind","NText","20000"},
        		        		{"ZType","Int","4"},
        		        		{"ZStatus","Int","4"},
        		        		{"CDate","DateTime","8"},
                                {"PreViewImg","NVarChar","1000"},
                                {"EndDate","DateTime","8"},
                                {"AdminID","Int","4"},
                                {"StartDate","DateTime","8"},
                                {"IsShowResult","Int","4"},
                                {"IsIPLimit","Int","4"},
                                {"IsNeedLogin","Int","4"},
                                {"IsEnableVCode","Int","4"},
                                {"IPInterval","Int","4"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Design_Ask model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Title;
            sp[2].Value = model.CUser;
            sp[3].Value = model.Remind;
            sp[4].Value = model.ZType;
            sp[5].Value = model.ZStatus;
            sp[6].Value = model.CDate;
            sp[7].Value = model.PreViewImg;
            sp[8].Value = model.EndDate;
            sp[9].Value = model.AdminID;
            sp[10].Value = model.StartDate;
            sp[11].Value = model.IsShowResult;
            sp[12].Value = model.IsIPLimit;
            sp[13].Value = model.IsNeedLogin;
            sp[14].Value = model.IsEnableVCode;
            sp[15].Value = model.IPInterval;
            return sp;
        }
        public M_Design_Ask GetModelFromReader(DbDataReader rdr)
        {
            M_Design_Ask model = new M_Design_Ask();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Title = ConverToStr(rdr["Title"]);
            model.CUser = ConvertToInt(rdr["CUser"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.PreViewImg = ConverToStr(rdr["PreViewImg"]);
            model.EndDate = ConvertToDate(rdr["EndDate"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.StartDate = ConvertToDate(rdr["StartDate"]);
            model.IsShowResult = ConvertToInt(rdr["IsShowResult"]);
            model.IsIPLimit = ConvertToInt(rdr["IsIPLimit"]);
            model.IsNeedLogin = ConvertToInt(rdr["IsNeedLogin"]);
            model.IsEnableVCode = ConvertToInt(rdr["IsEnableVCode"]);
            model.IPInterval = ConvertToInt(rdr["IPInterval"]);
            rdr.Close();
            return model;
        }
    }
}
