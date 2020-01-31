using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.Components;
using ZoomLa.SQLDAL;

namespace ZoomLa.AppCode.SMS
{
    public class SMS_Helper
    {
        public static CommonReturn Send(string[] mobiles, SMS_Packet packet)
        {
            SMS_Base sms = null;
            switch (SMSConfig.Instance.DefaultSMS)
            {
                case "1":
                case "2":
                case "3":
                    sms = new SMS_Old();
                    break;
                case "qcloud":
                    sms = new SMS_QCloud();
                    break;
                case "aliyun":
                    break;
                case "0":
                default:
                    break;
            }
            if (sms == null) { return CommonReturn.Failed("未开启短信接口"); }
            //---------------------
            return sms.Send(mobiles, packet);
        }
        /// <summary>
        /// 发送验证码短信
        /// </summary>
        public static CommonReturn SendVCode(string mobile, string vcode, string tlp)
        {
            SMS_Packet packet = new SMS_Packet();
            packet.message = tlp;
            switch (SMSConfig.Instance.DefaultSMS)
            {
                case "qcloud":
                    packet.param.Add("0", vcode);
                    packet.tlpId = DataConvert.CLng(tlp);
                    break;
                default:
                    if (string.IsNullOrEmpty(packet.message))
                    {
                        return CommonReturn.Failed("未指定短信模板内容");
                    }
                    break;
            }
            packet.message = packet.message.Replace("{vcode}", vcode);
            return Send(new string[] { mobile }, packet);
        }
    }
}
