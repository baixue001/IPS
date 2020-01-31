using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Message
{
    public class M_Message_SMS : M_Base
    {
        public M_Message_SMS()
        {
            ZStatus = 0;
            ZType = 0;
        }
        public int ID { get; set; }
        public string Balance { get; set; }
        /// <summary>
        /// 短信内容
        /// </summary>
        public string MsgContent { get; set; }
        /// <summary>
        /// CRM客户类别
        /// </summary>
        public string Rece_CRM { get; set; }
        /// <summary>
        /// 手输手机号
        /// </summary>
        public string Rece_Mobile { get; set; }
        /// <summary>
        /// 用户组
        /// </summary>
        public string Rece_Group { get; set; }
        /// <summary>
        /// 用户IDS
        /// </summary>
        public string Rece_User { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public int CAdmin { get; set; }
        public int CUser { get; set; }
        public DateTime CDate { get; set; }
        public string Remark { get; set; }
        public override string TbName { get { return "ZL_Message_SMS"; } }
        public override string PK { get { return "ID"; } }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"MsgContent","NVarChar","1000"},
                                {"Rece_CRM","NVarChar","4000"},
                                {"Rece_Mobile","NVarChar","4000"},
                                {"Rece_Group","NVarChar","4000"},
                                {"Rece_User","NVarChar","4000"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"CAdmin","Int","4"},
                                {"CUser","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remark","NVarChar","500"},
                                {"Balance","NVarChar","50"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Message_SMS model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.MsgContent;
            sp[2].Value = model.Rece_CRM;
            sp[3].Value = model.Rece_Mobile;
            sp[4].Value = model.Rece_Group;
            sp[5].Value = model.Rece_User;
            sp[6].Value = model.ZStatus;
            sp[7].Value = model.ZType;
            sp[8].Value = model.CAdmin;
            sp[9].Value = model.CUser;
            sp[10].Value = model.CDate;
            sp[11].Value = model.Remark;
            sp[12].Value = model.Balance;
            return sp;
        }
        public M_Message_SMS GetModelFromReader(DbDataReader rdr)
        {
            M_Message_SMS model = new M_Message_SMS();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.MsgContent = ConverToStr(rdr["MsgContent"]);
            model.Rece_CRM = ConverToStr(rdr["Rece_CRM"]);
            model.Rece_Mobile = ConverToStr(rdr["Rece_Mobile"]);
            model.Rece_Group = ConverToStr(rdr["Rece_Group"]);
            model.Rece_User = ConverToStr(rdr["Rece_User"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CAdmin = ConvertToInt(rdr["CAdmin"]);
            model.CUser = ConvertToInt(rdr["CUser"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remark = ConverToStr(rdr["Remark"]);
            model.Balance = ConverToStr(rdr["Balance"]);
            rdr.Close();
            return model;
        }
    }
}
