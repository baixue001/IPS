using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Client
{
    public class M_CRMS_Contact : M_Base
    {
        public M_CRMS_Contact()
        {
            ZType = 0;
            ZStatus = 1;
        }
        public int ID { get; set; }
        /// <summary>
        /// 所属客户ID
        /// </summary>
        public int ClientID { get; set; }
        /// <summary>
        /// 联系人姓名
        /// </summary>
        public string Name { get; set; }
        public string Sex { get; set; }
        /// <summary>
        /// 联系人职位
        /// </summary>
        public string Post { get; set; }
        public string Mobile_Office { get; set; }
        public string Mobile_Home { get; set; }
        public string Mobile1 { get; set; }
        public string Mobile2 { get; set; }
        /// <summary>
        /// 联系人相关信息
        /// </summary>
        public string QQ { get; set; }
        public string SinaBlog { get; set; }
        public string Wechat { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 联系人部门
        /// </summary>
        public string DepName { get; set; }
        public DateTime CDate { get; set; }
        public int ZStatus { get; set; }
        public int ZType { get; set; }
        public int CUserID { get; set; }
        public int CAdminID { get; set; }

        public override string TbName { get { return "ZL_CRMS_Contact"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"ClientID","Int","4"},
                                {"Name","NVarChar","50"},
                                {"Post","NVarChar","100"},
                                {"Mobile1","NVarChar","50"},
                                {"Email","NVarChar","100"},
                                {"Address","NVarChar","200"},
                                {"Remind","NVarChar","200"},
                                {"DepName","NVarChar","200"},
                                {"CDate","DateTime","8"},
                                {"ZStatus","Int","4"},
                                {"ZType","Int","4"},
                                {"CUserID","Int","4"},
                                {"CAdminID","Int","4"},
                                {"Mobile_Office","NVarChar","50"},
                                {"Mobile_Home","NVarChar","50"},
                                {"Mobile2","NVarChar","50"},
                                {"QQ","NVarChar","50"},
                                {"Wechat","NVarChar","200"},
                                {"SinaBlog","NVarChar","200"},
                                {"Sex","NVarChar","50"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_CRMS_Contact model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.ClientID;
            sp[2].Value = model.Name;
            sp[3].Value = model.Post;
            sp[4].Value = model.Mobile1;
            sp[5].Value = model.Email;
            sp[6].Value = model.Address;
            sp[7].Value = model.Remind;
            sp[8].Value = model.DepName;
            sp[9].Value = model.CDate;
            sp[10].Value = model.ZStatus;
            sp[11].Value = model.ZType;
            sp[12].Value = model.CUserID;
            sp[13].Value = model.CAdminID;
            sp[14].Value = model.Mobile_Office;
            sp[15].Value = model.Mobile_Home;
            sp[16].Value = model.Mobile2;
            sp[17].Value = model.QQ;
            sp[18].Value = model.Wechat;
            sp[19].Value = model.SinaBlog;
            sp[20].Value = model.Sex;
            return sp;
        }
        public M_CRMS_Contact GetModelFromReader(DbDataReader rdr)
        {
            M_CRMS_Contact model = new M_CRMS_Contact();
            model.ID = ConvertToInt(rdr["ID"]);
            model.ClientID = ConvertToInt(rdr["ClientID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.Post = ConverToStr(rdr["Post"]);
            model.Mobile1 = ConverToStr(rdr["Mobile1"]);
            model.Email = ConverToStr(rdr["Email"]);
            model.Address = ConverToStr(rdr["Address"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.DepName = ConverToStr(rdr["DepName"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CUserID = ConvertToInt(rdr["CUserID"]);
            model.CAdminID = ConvertToInt(rdr["CAdminID"]);
            model.Mobile_Office = ConverToStr(rdr["Mobile_Office"]);
            model.Mobile_Home = ConverToStr(rdr["Mobile_Home"]);
            model.Mobile2 = ConverToStr(rdr["Mobile2"]);
            model.QQ = ConverToStr(rdr["QQ"]);
            model.Wechat = ConverToStr(rdr["Wechat"]);
            model.SinaBlog = ConverToStr(rdr["SinaBlog"]);
            model.Sex = ConverToStr(rdr["Sex"]);
            rdr.Close();
            return model;
        }
        public M_CRMS_Contact GetModelFromReader(DataRow rdr)
        {
            M_CRMS_Contact model = new M_CRMS_Contact();
            model.ID = ConvertToInt(rdr["ID"]);
            model.ClientID = ConvertToInt(rdr["ClientID"]);
            model.Name = ConverToStr(rdr["Name"]);
            model.Post = ConverToStr(rdr["Post"]);
            model.Mobile1 = ConverToStr(rdr["Mobile1"]);
            model.Email = ConverToStr(rdr["Email"]);
            model.Address = ConverToStr(rdr["Address"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.DepName = ConverToStr(rdr["DepName"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.ZType = ConvertToInt(rdr["ZType"]);
            model.CUserID = ConvertToInt(rdr["CUserID"]);
            model.CAdminID = ConvertToInt(rdr["CAdminID"]);
            model.Mobile_Office = ConverToStr(rdr["Mobile_Office"]);
            model.Mobile_Home = ConverToStr(rdr["Mobile_Home"]);
            model.Mobile2 = ConverToStr(rdr["Mobile2"]);
            model.QQ = ConverToStr(rdr["QQ"]);
            model.Wechat = ConverToStr(rdr["Wechat"]);
            model.SinaBlog = ConverToStr(rdr["SinaBlog"]);
            model.Sex = ConverToStr(rdr["Sex"]);
            return model;
        }
    }
}
