using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model.Sys;
using ZoomLa.SQLDAL;
using static ZoomLa.Components.SendMail;

namespace ZoomLa.BLL.Sys
{
    public class B_EMailHelper
    {
        /// <summary>
        /// 异步发送邮件,并将发送记录备份
        /// </summary>
        /// <param name="model">邮件信息模型</param>
        /// <param name="dataDT">不为空则对每个模板格式化后再发送</param>
        public static void SendAsync(M_EMail_Item model)
        {
            B_EMail_Item itemBll = new B_EMail_Item();
            MailConfig cfg = SiteConfig.MailConfig;
            SendHandler sender = null;
            try
            {
                if (string.IsNullOrEmpty(model.FromEmail))
                {
                    model.FromEmail = cfg.MailServerUserName;
                }
                if (string.IsNullOrEmpty(model.FromName))
                {
                    model.FromName = SiteConfig.SiteInfo.Webmaster;
                }
                if (cfg.Port == 25)
                {
                    sender = SendEmail;
                }
                else if (cfg.Port == 465)
                {
                    sender = SendSSLEmail;
                }
                else { throw new Exception("邮件端口[" + cfg.Port + "]配置不正确"); }
                if (string.IsNullOrEmpty(cfg.MailServerUserName)) { throw new Exception("未配置发送邮箱,取消发送"); }
                if (string.IsNullOrEmpty(cfg.MailServerPassWord)) { throw new Exception("未配置发送邮箱密码,取消发送"); }
                model.Result = 1; model.Error = "";
            }
            catch (Exception ex) { model.Result = -1; model.Error = ex.Message; return; }
            model.ID = itemBll.Insert(model);
            //----------------发送邮件(单个发送)
            string[] emails = model.ToAddress.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string mailBody = model.MailBody;
            for (int i = 0; i < emails.Length; i++)
            {
                M_EMail_Item mailMod = new M_EMail_Item()
                {
                    ID = model.ID,
                    ToAddress = emails[i],
                    Subject = model.Subject,
                    MailBody = mailBody,
                    FromName = model.FromName,
                    FromEmail = model.FromEmail,
                    Attachment = model.Attachment
                };
                //如果不需要每次循环时编译,应该在上层处理好再传入
                if (mailMod.C_NeedTranslate)
                {
                    DataRow dr = null;
                    if (model.C_DataDT != null && model.C_DataDT.Rows.Count > i)
                    {
                        dr = model.C_DataDT.Rows[i];
                    }
                    mailMod.MailBody = TlpDeal(mailBody, dr);
                }
                sender.BeginInvoke(cfg.MailServerUserName, cfg.MailServerPassWord, cfg.MailServer
, mailMod, SendCallBack, null);
            }
        }

        //------------邮件发送
        private delegate void SendHandler(string account, string passwd, string server,M_EMail_Item mailMod);
        private static void SendCallBack(IAsyncResult result)
        {
            //AsyncResult resultObj = (AsyncResult)result;
            //SendHandler sender = (SendHandler)resultObj.AsyncDelegate;//获取原来的委托
            //sender.EndInvoke(resultObj);
        }
        private static void SendEmail(string account, string passwd, string server, M_EMail_Item mailMod)
        {
            M_EMail_SendLog logMod = new M_EMail_SendLog();
            B_EMail_SendLog logBll = new B_EMail_SendLog();
            logMod.ToAddress = mailMod.ToAddress;
            logMod.EmailID = mailMod.ID;
            //--------------------------------------------------
            SmtpClient client = new SmtpClient(server, 25);
            NetworkCredential credential = new NetworkCredential(account, passwd);
            client.UseDefaultCredentials = true;
            client.EnableSsl = false;
            client.Credentials = credential.GetCredential(server, 25, "Basic");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //--------------------------------------------------
            try
            {
                MailMessage mail = new MailMessage();
                mail.SubjectEncoding = Encoding.UTF8;
                mail.BodyEncoding = Encoding.UTF8;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;
                mail.Subject = mailMod.Subject;
                mail.Body = mailMod.MailBody;

                if (!string.IsNullOrEmpty(mailMod.FromName))
                {
                    mail.From = new MailAddress(account, mailMod.FromName);
                }
                else
                {
                    mail.From = new MailAddress(account);
                }
                foreach (string file in mailMod.Attachment.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.Attachments.Add(new Attachment(function.VToP(file)));
                }
                mail.To.Add(new MailAddress(mailMod.ToAddress));
                //foreach (string email in mailMod.ToAddress.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                //{
                //    mail.To.Add(new MailAddress(email));
                //}
                //foreach (string email in mailMod.CCAddress.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                //{
                //    mail.CC.Add(email);
                //}
                //foreach (string email in mailMod.BCCAddress.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                //{
                //    mail.Bcc.Add(email);
                //}
                client.Send(mail);
                logMod.Result = 1;
            }
            catch (Exception ex)
            {
                logMod.ErrorMsg = ex.Message;
                logMod.Result = -1;
            }
            logBll.Insert(logMod);
        }
        private static void SendSSLEmail(string account, string passwd, string server, M_EMail_Item mailMod)
        {
            M_EMail_SendLog logMod = new M_EMail_SendLog();
            B_EMail_SendLog logBll = new B_EMail_SendLog();
            logMod.ToAddress = mailMod.ToAddress;
            logMod.EmailID = mailMod.ID;
            //--------------------------------------------------
            try
            {
              
            }
            catch (Exception ex) { logMod.Result = -1;logMod.ErrorMsg = ex.Message; }
            logBll.Insert(logMod);
        }
        //------------模板处理
        public string TlpDeal(string tlp, DataTable dt)
        {
            if (dt == null || dt.Rows.Count < 1) { return tlp; }
            else { return TlpDeal(tlp, dt.Rows[0]); }
        }
        public static string TlpDeal(string tlp, DataRow dr)
        {
            B_CartPro cartProBll = new B_CartPro();
            string result = tlp;
            if (dr != null)
            {
                #region dt中字段替换
                DataTable dt = dr.Table;
                //遍历表格字段并替换
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string colname = dt.Columns[i].ColumnName;
                    string value = dr[colname].ToString();
                    if (dt.Columns[i].DataType.ToString().Equals("System.Decimal"))
                    {
                        value = DataConvert.CDouble(dr[colname]).ToString("f2");
                    }
                    result = result.Replace("{" + colname + "}", value);
                }
                #endregion
                #region CartPro扩展字段
                //CartPro字段,只取第一条信息循环输出
                if (tlp.Contains("{CartPro."))
                {
                    DataTable cartDT = new DataTable();
                    cartDT = cartProBll.SelByOrderID(Convert.ToInt32(dt.Rows[0]["ID"]));
                    if (cartDT.Rows.Count > 0)
                    {
                        dr = cartDT.Rows[0];
                        for (int i = 0; i < cartDT.Columns.Count; i++)
                        {
                            string colname = cartDT.Columns[i].ColumnName;
                            string value = dr[colname].ToString();
                            if (cartDT.Columns[i].DataType.ToString().Equals("System.Decimal"))
                            {
                                value = Convert.ToDouble(dr[colname]).ToString("f2");
                            }
                            result = result.Replace("{CartPro." + colname + "}", value);
                        }
                    }
                }
                #endregion
                //    #region Extend扩展字段
                //    if (dt.Columns.Contains("Extend") && (!string.IsNullOrEmpty(dr["Extend"].ToString())))//扩展字段,可自由定义
                //    {
                //        JObject model = (JObject)JsonConvert.DeserializeObject(dr["Extend"].ToString());
                //        foreach (var item in model)
                //        {
                //            result = result.Replace("{Extend."+item.Key+"}",item.Value.ToString());
                //        }
                //    }
                //#endregion
            }
            //标签解析
            //B_CreateHtml createBll = new B_CreateHtml(HttpContext.Current.Request);
            //result = createBll.CreateHtml(result);
            return result;
        }
    }
}
