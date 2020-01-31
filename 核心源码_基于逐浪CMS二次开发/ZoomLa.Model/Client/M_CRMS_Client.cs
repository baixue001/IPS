using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Client
{
    public class M_CRMS_Client : M_Base
    {
        public M_CRMS_Client()
        {
            ClientStatus = 1;
            ClientLevel = "B";
            FUserID = 0;
            CUserID = 0;
            Flow = "";
        }
        public int ID { get; set; }
        /// <summary>
        /// 客户编号(自动生成)
        /// </summary>
        public string Flow { get; set; }
        public string ClientName { get; set; }
        public string ClientLevel { get; set; }
        public string ClientClass { get; set; }
        /// <summary>
        /// 客户类型,多选
        /// </summary>
        public string ClientType { get; set; }
        public int ClientStatus { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 客户网站URL
        /// </summary>
        public string WebSiteUrl { get; set; }
        public string MicroBlogUrl { get; set; }
        public string ZipCode { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        /// <summary>
        /// 跟进人(用户ID)
        /// </summary>
        public int FUserID { get; set; }
        /// <summary>
        /// 跟进人(用户名)
        /// </summary>
        public string FUserName { get; set; }
        /// <summary>
        /// 跟进备注
        /// </summary>
        public string FRemind { get; set; }
        /// <summary>
        /// 创建人(管理员ID)
        /// </summary>
        public int CAdminID { get; set; }
        /// <summary>
        /// 创建人(用户ID)
        /// </summary>
        public int CUserID { get; set; }
        public DateTime CDate { get; set; }
        public string Remind { get; set; }
        /// <summary>
        /// 关联的其它客户IDS
        /// </summary>
        public string LinkIds { get; set; }
        //------------
        public int ModelID { get; set; }
        public string ModelTable { get; set; }
        public int ItemID { get; set; }
        public override string TbName { get { return "ZL_CRMS_Client"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Flow","NVarChar","50"},
                                {"ClientName","NVarChar","100"},
                                {"ClientLevel","NVarChar","100"},
                                {"ClientClass","NVarChar","100"},
                                {"ClientType","NVarChar","2000"},
                                {"ClientStatus","Int","4"},
                                {"WebSiteUrl","NVarChar","500"},
                                {"MicroBlogUrl","NVarChar","500"},
                                {"Province","NVarChar","50"},
                                {"City","NVarChar","50"},
                                {"County","NVarChar","50"},
                                {"Address","NVarChar","200"},
                                {"ZipCode","NVarChar","50"},
                                {"Fax","NVarChar","50"},
                                {"Phone","NVarChar","50"},
                                {"FUserID","Int","4"},
                                {"FUserName","NVarChar","200"},
                                {"FRemind","NVarChar","500"},
                                {"CAdminID","Int","4"},
                                {"CUserID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"},
                                {"ModelID","Int","4"},
                                {"ModelTable","NVarChar","50"},
                                {"ItemID","Int","4"},
                                {"LinkIds","VarChar","4000"}
        };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_CRMS_Client model = this;
            SqlParameter[] sp = GetSP();
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            sp[0].Value = model.ID;
            sp[1].Value = model.Flow;
            sp[2].Value = model.ClientName;
            sp[3].Value = model.ClientLevel;
            sp[4].Value = model.ClientClass;
            sp[5].Value = model.ClientType;
            sp[6].Value = model.ClientStatus;
            sp[7].Value = model.WebSiteUrl;
            sp[8].Value = model.MicroBlogUrl;
            sp[9].Value = model.Province;
            sp[10].Value = model.City;
            sp[11].Value = model.County;
            sp[12].Value = model.Address;
            sp[13].Value = model.ZipCode;
            sp[14].Value = model.Fax;
            sp[15].Value = model.Phone;
            sp[16].Value = model.FUserID;
            sp[17].Value = model.FUserName;
            sp[18].Value = model.FRemind;
            sp[19].Value = model.CAdminID;
            sp[20].Value = model.CUserID;
            sp[21].Value = model.CDate;
            sp[22].Value = model.Remind;
            sp[23].Value = model.ModelID;
            sp[24].Value = model.ModelTable;
            sp[25].Value = model.ItemID;
            sp[26].Value = model.LinkIds;
            return sp;
        }
        public M_CRMS_Client GetModelFromReader(DbDataReader rdr)
        {
            M_CRMS_Client model = new M_CRMS_Client();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Flow = ConverToStr(rdr["Flow"]);
            model.ClientName = ConverToStr(rdr["ClientName"]);
            model.ClientLevel = ConverToStr(rdr["ClientLevel"]);
            model.ClientClass = ConverToStr(rdr["ClientClass"]);
            model.ClientType = ConverToStr(rdr["ClientType"]);
            model.ClientStatus = ConvertToInt(rdr["ClientStatus"]);
            model.WebSiteUrl = ConverToStr(rdr["WebSiteUrl"]);
            model.MicroBlogUrl = ConverToStr(rdr["MicroBlogUrl"]);
            model.Province = ConverToStr(rdr["Province"]);
            model.City = ConverToStr(rdr["City"]);
            model.County = ConverToStr(rdr["County"]);
            model.Address = ConverToStr(rdr["Address"]);
            model.ZipCode = ConverToStr(rdr["ZipCode"]);
            model.Fax = ConverToStr(rdr["Fax"]);
            model.Phone = ConverToStr(rdr["Phone"]);
            model.FUserID = ConvertToInt(rdr["FUserID"]);
            model.FUserName = ConverToStr(rdr["FUserName"]);
            model.FRemind = ConverToStr(rdr["FRemind"]);
            model.CAdminID = ConvertToInt(rdr["CAdminID"]);
            model.CUserID = ConvertToInt(rdr["CUserID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.ModelTable = ConverToStr(rdr["ModelTable"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.LinkIds = ConverToStr(rdr["LinkIds"]);
            rdr.Close();
            return model;
        }
        public M_CRMS_Client GetModelFromReader(DataRow rdr)
        {
            M_CRMS_Client model = new M_CRMS_Client();
            model.ID = ConvertToInt(rdr["ID"]);
            model.Flow = ConverToStr(rdr["Flow"]);
            model.ClientName = ConverToStr(rdr["ClientName"]);
            model.ClientLevel = ConverToStr(rdr["ClientLevel"]);
            model.ClientClass = ConverToStr(rdr["ClientClass"]);
            model.ClientType = ConverToStr(rdr["ClientType"]);
            model.ClientStatus = ConvertToInt(rdr["ClientStatus"]);
            model.WebSiteUrl = ConverToStr(rdr["WebSiteUrl"]);
            model.MicroBlogUrl = ConverToStr(rdr["MicroBlogUrl"]);
            model.Province = ConverToStr(rdr["Province"]);
            model.City = ConverToStr(rdr["City"]);
            model.County = ConverToStr(rdr["County"]);
            model.Address = ConverToStr(rdr["Address"]);
            model.ZipCode = ConverToStr(rdr["ZipCode"]);
            model.Fax = ConverToStr(rdr["Fax"]);
            model.Phone = ConverToStr(rdr["Phone"]);
            model.FUserID = ConvertToInt(rdr["FUserID"]);
            model.FUserName = ConverToStr(rdr["FUserName"]);
            model.FRemind = ConverToStr(rdr["FRemind"]);
            model.CAdminID = ConvertToInt(rdr["CAdminID"]);
            model.CUserID = ConvertToInt(rdr["CUserID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.ModelID = ConvertToInt(rdr["ModelID"]);
            model.ModelTable = ConverToStr(rdr["ModelTable"]);
            model.ItemID = ConvertToInt(rdr["ItemID"]);
            model.LinkIds = ConverToStr(rdr["LinkIds"]);
            return model;
        }
    }
}
