using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ZoomLa.Model.Other
{
   public class M_Safe_Mobile:M_Base
    {
        public int ID { get; set; }
        /// <summary>
        /// 接收验证码的手机
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 如果该码是通过Email发送,则记录Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 请求验证码的IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 来源,对应页面处理
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 校验码
        /// </summary>
        public string VCode { get; set; }
        /// <summary>
        /// 0未使用,99已使用
        /// </summary>
        public int ZStatus { get; set; }
        /// <summary>
        /// 使用该次短信服务的用户
        /// </summary>
        public int UserID { get; set; }
        public string UserName { get; set; }
        public DateTime CDate { get; set; }
        public string SysRemind { get; set; }
        public M_Safe_Mobile() { ZStatus = 0; VCode = ""; UserID = 0; }
        public override string TbName
        {
            get { return "ZL_Safe_Mobile"; }
        }
        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4" },
                                {"Phone","NVarChar","100" },
                                {"IP","NVarChar","500" },
                                {"CDate","DateTime","8" },
                                {"Source","NVarChar","100" },
                                {"VCode","NVarChar","50" },
                                {"ZStatus","Int","4" },
                                {"UserID","Int","4" },
                                {"UserName","NVarChar","50" },
                                {"Email","NVarChar","100" },
                                {"SysRemind","NVarChar","2000" }
            };
            return Tablelist;
        }
        public override SqlParameter[] GetParameters()
        {
            M_Safe_Mobile model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Phone;
            sp[2].Value = model.IP;
            sp[3].Value = model.CDate;
            sp[4].Value = model.Source;
            sp[5].Value = model.VCode;
            sp[6].Value = model.ZStatus;
            sp[7].Value = model.UserID;
            sp[8].Value = model.UserName;
            sp[9].Value = model.Email;
            sp[10].Value = model.SysRemind;
            return sp;
        }
        public M_Safe_Mobile GetModelFromReader(DbDataReader rdr)
        {
            M_Safe_Mobile model = new M_Safe_Mobile();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Phone = ConverToStr(rdr["Phone"]);
            model.IP = ConverToStr(rdr["IP"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Source = ConverToStr(rdr["Source"]);
            model.VCode = ConverToStr(rdr["VCode"]);
            model.ZStatus = ConvertToInt(rdr["ZStatus"]);
            model.UserID = ConvertToInt(rdr["UserID"]);
            model.UserName = ConverToStr(rdr["UserName"]);
            model.Email = ConverToStr(rdr["Email"]);
            model.SysRemind = ConverToStr(rdr["SysRemind"]);
            rdr.Close();
            return model;
        }
    }
}
