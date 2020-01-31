using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ZoomLa.Common;

namespace ZoomLa.Components
{
    public class SMSConfig
    {
        private static string filePath { get { return function.VToP("/Config/SMS.config"); } }
        private static SMSConfigInfo model = null;
        //获取实例
        private static SMSConfigInfo GetInstance()
        {
            if (model == null)
            {
                model = ConfigReadFromFile();
            }
            return model;
        }
        /// <summary>
        /// 使用时实例= ConfigReadFromFile();
        /// </summary>
        /// <returns></returns>
        private static SMSConfigInfo ConfigReadFromFile()
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SMSConfigInfo));
                    return (SMSConfigInfo)serializer.Deserialize(stream);
                }
            }
            catch { return new SMSConfigInfo(); }
        }
        public static void Update()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SMSConfigInfo));
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
                serializer.Serialize(stream, GetInstance(), namespaces);
                stream.Close();
            }
        }
        public static void ReInstance()
        {
            model = ConfigReadFromFile();
        }
        public static SMSConfigInfo Instance{ get { return GetInstance(); } }
    }
    [Serializable, XmlRoot("SMS")]
    public class SMSConfigInfo
    {
        #region old
        public string MssUser
        {
            get;
            set;
        }
        public string MssPsw
        {
            get;
            set;
        }
        public string G_mtype
        {
            get;
            set;
        }
        public string G_content
        {
            get;
            set;
        }
        public string G_blackList
        {
            get;
            set;
        }
        public string G_uid
        {
            get;
            set;
        }
        public string G_eid
        {
            get;
            set;
        }
        public string G_pwd
        {
            get;
            set;
        }
        public string G_gate_id
        {
            get;
            set;
        }
        /// <summary>
        /// 亿美短信Key
        /// </summary>
        public string sms_key { get; set; }
        /// <summary>
        /// 亿美短信Passwd
        /// </summary>
        public string sms_pwd { get; set; }
        #endregion
        /// <summary>
        /// 默认使用哪个短信接口1:北京网通,2:深圳电信,3:亿美软件,qcloud,aliyun
        /// </summary>
        public string DefaultSMS { get; set; }
        //验证码模板(仅用于可直接发短信的接口)
        public string Tlp_Reg = "";
        //通过手机号找回密码
        public string Tlp_GetBack = "";
        //修改手机号码
        public string Tlp_ChangeMobile = "";

        public string QCloud_APPID = "";
        public string QCloud_APPKey = "";
        public string QCloud_Sign = "";

        public int Aliyun_VCodeTlp = 0;
    }
}
