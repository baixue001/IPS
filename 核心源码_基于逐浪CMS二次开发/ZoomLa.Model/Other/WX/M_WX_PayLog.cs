using System;using System.Data.Common;
using System.Collections.Generic;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Other
{
    //微信资金出账日志
    public class M_WX_PayLog : M_Base
    {

        public int ID { get; set; }
        /// <summary>
        /// 微信公众号主键
        /// </summary>
        public int AppID { get; set; }
        /// <summary>
        /// 微信用户OpenID
        /// </summary>
        public string OpenID { get; set; }
        public double AMount { get; set; }
        /// <summary>
        /// 收款人
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 支付类型 1:企业付款
        /// </summary>
        public int PayType { get; set; }
        /// <summary>
        /// 提交的表单XML
        /// </summary>
        public string FormXML { get; set; }
        /// <summary>
        /// 微信返回的结果字符串
        /// </summary>
        public string WXResult { get; set; }
        /// <summary>
        /// 1:成功,-1:失败
        /// </summary>
        public int ZStatus { get; set; }
        /// <summary>
        /// 操作管理员ID
        /// </summary>
        public int AdminID { get; set; }
        public DateTime CDate { get; set; }
        /// <summary>
        /// 此次付款备注
        /// </summary>
        public string Remind { get; set; }
        /// <summary>
        /// 系统备注
        /// </summary>
        public string SysRemind { get; set; }
        public override string TbName { get { return "ZL_WX_PayLog"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"OpenID","VarChar","500"},
                                {"AMount","Money","8"},
                                {"TrueName","NVarChar","50"},
                                {"PayType","Int","4"},
                                {"FormXML","NVarChar","4000"},
                                {"WXResult","NVarChar","4000"},
                                {"ZStatus","Int","4"},
                                {"AdminID","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Remind","NVarChar","500"},
                                {"SysRemind","NVarChar","500"},
                                {"AppID","Int","4"}
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_WX_PayLog model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.OpenID;
            sp[2].Value = model.AMount;
            sp[3].Value = model.TrueName;
            sp[4].Value = model.PayType;
            sp[5].Value = model.FormXML;
            sp[6].Value = model.WXResult;
            sp[7].Value = model.ZStatus;
            sp[8].Value = model.AdminID;
            sp[9].Value = model.CDate;
            sp[10].Value = model.Remind;
            sp[11].Value = model.SysRemind;
            sp[12].Value = model.AppID;
            return sp;
        }
        public M_WX_PayLog GetModelFromReader(DbDataReader rdr)
        {
            M_WX_PayLog model = new M_WX_PayLog();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.OpenID = ConverToStr(rdr["OpenID"]);
            model.AMount = ConverToDouble(rdr["AMount"]);
            model.TrueName = ConverToStr(rdr["TrueName"]);
            model.PayType = ConvertToInt(rdr["PayType"]);
            model.FormXML = ConverToStr(rdr["FormXML"]);
            model.WXResult = ConverToStr(rdr["WXResult"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.AdminID = ConvertToInt(rdr["AdminID"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Remind = ConverToStr(rdr["Remind"]);
            model.SysRemind = ConverToStr(rdr["SysRemind"]);
            model.AppID = ConvertToInt(rdr["AppID"]);
            rdr.Close();
            return model;
        }
    }
}
