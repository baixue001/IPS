using System;using System.Data.Common;
using System.Collections.Generic;
using System.Data;

using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ZoomLa.Model.Sys
{
    /// <summary>
    /// 标题,内容,收件人,发件人姓名
    /// </summary>
    public class M_EMail_Item : M_Base
    {
        public M_EMail_Item()
        {
            Priority = 1;
            SaveSendLog = 1;
            FromName = "";
            Attachment = "";
            Result = 0;
            C_NeedTranslate = true;
        }
        public int ID { get; set; }
        /// <summary>
        /// 优先级别
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 发送人名称,不能为Email
        /// </summary>
        public string FromName { get; set; }
        /// <summary>
        /// 发送者Email(不需要填写,自动读配置)
        /// </summary>
        public string FromEmail { get; set; }
        /// <summary>
        /// 回复的邮件地址,可为空
        /// </summary>
        public string ReplyTo { get; set; }
        /// <summary>
        /// CC抄送地址,号分隔
        /// </summary>
        public string CCAddress { get; set; }
        /// <summary>
        /// 接收邮件地址,号分隔
        /// </summary>
        public string ToAddress { get; set; }
        /// <summary>
        /// 秘抄送地址,号分隔
        /// </summary>
        public string BCCAddress { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 内容支持HTML
        /// </summary>
        public string MailBody { get; set; }
        /// <summary>
        /// 附件地址|号分隔
        /// </summary>
        public string Attachment { get; set; }
        /// <summary>
        /// 是否存储发送日志0==关闭
        /// </summary>
        public int SaveSendLog { get; set; }
        public DateTime CDate { get; set; }

        //--------------返回与模板数据

        /// <summary>
        /// 信息不对(如端口,用户名等),直接被拒,标识该值
        /// </summary>
        public int Result { get; set; }
        /// <summary>
        /// 直接被拒的错误记录
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 模板数据表(不写入数据库)
        /// </summary>
        public DataTable C_DataDT { get; set; }
        /// <summary>
        /// 是否需要经过编译(标签与替换数据)
        /// </summary>
        public bool C_NeedTranslate { get; set; }

        public override string TbName { get { return "ZL_EMail_Item"; } }
        public override string PK { get { return "ID"; } }

        public override string[,] FieldList()
        {
            string[,] Tablelist = {
                                {"ID","Int","4"},
                                {"Priority","Int","4"},
                                {"FromName","NVarChar","500"},
                                {"FromEmail","NVarChar","200"},
                                {"ReplyTo","NVarChar","200"},
                                {"CCAddress","NText","20000"},
                                {"ToAddress","NText","20000"},
                                {"BCCAddress","NText","20000"},
                                {"Subject","NVarChar","1000"},
                                {"MailBody","NText","20000"},
                                {"Attachment","NVarChar","4000"},
                                {"SaveSendLog","Int","4"},
                                {"CDate","DateTime","8"},
                                {"Result","Int","4"},
                                {"Error","NVarChar","500"},
        };
            return Tablelist;
        }

        public override SqlParameter[] GetParameters()
        {
            M_EMail_Item model = this;
            if (model.CDate <= DateTime.MinValue) { model.CDate = DateTime.Now; }
            SqlParameter[] sp = GetSP();
            sp[0].Value = model.ID;
            sp[1].Value = model.Priority;
            sp[2].Value = model.FromName;
            sp[3].Value = model.FromEmail;
            sp[4].Value = model.ReplyTo;
            sp[5].Value = model.CCAddress;
            sp[6].Value = model.ToAddress;
            sp[7].Value = model.BCCAddress;
            sp[8].Value = model.Subject;
            sp[9].Value = model.MailBody;
            sp[10].Value = model.Attachment;
            sp[11].Value = model.SaveSendLog;
            sp[12].Value = model.CDate;
            sp[13].Value = model.Result;
            sp[14].Value = model.Error;
            return sp;
        }
        public M_EMail_Item GetModelFromReader(DbDataReader rdr)
        {
            M_EMail_Item model = new M_EMail_Item();
            model.ID = Convert.ToInt32(rdr["ID"]);
            model.Priority = ConvertToInt(rdr["Priority"]);
            model.FromName = ConverToStr(rdr["FromName"]);
            model.FromEmail = ConverToStr(rdr["FromEmail"]);
            model.ReplyTo = ConverToStr(rdr["ReplyTo"]);
            model.CCAddress = ConverToStr(rdr["CCAddress"]);
            model.ToAddress = ConverToStr(rdr["ToAddress"]);
            model.BCCAddress = ConverToStr(rdr["BCCAddress"]);
            model.Subject = ConverToStr(rdr["Subject"]);
            model.MailBody = ConverToStr(rdr["MailBody"]);
            model.Attachment = ConverToStr(rdr["Attachment"]);
            model.SaveSendLog = ConvertToInt(rdr["SaveSendLog"]);
            model.CDate = ConvertToDate(rdr["CDate"]);
            model.Result = ConvertToInt(rdr["Result"]);
            model.Error = ConverToStr(rdr["Error"]);
            rdr.Close();
            return model;
        }
    }
}
