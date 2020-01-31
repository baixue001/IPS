//using qcloudsms_csharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.Components;
using ZoomLa.SQLDAL;

namespace ZoomLa.AppCode.SMS
{
    public class SMS_QCloud : SMS_Base
    {
        //Note 群发一次请求最多支持200个号码
        //Note 无论单发/群发短信还是指定模板ID单发/群发短信都需要从控制台中申请模板并且模板已经审核通过，才可能下发成功，否则返回失败。
        public override CommonReturn QueryBalance()
        {
            return CommonReturn.Failed("无该接口");
        }
        public override CommonReturn Send(string[] mobiles, SMS_Packet packet)
        {
            if (mobiles.Length < 1) { return CommonReturn.Failed("未指定手机号"); }
            if (packet.tlpId < 1) { return CommonReturn.Failed("未指定模板ID"); }
            string smsSign = SMSConfig.Instance.QCloud_Sign; // NOTE: 这里的签名只是示例，请使用真实的已申请的签名, 签名参数使用的是`签名内容`，而不是`签名ID`
            int appid =DataConvert.CLng(SMSConfig.Instance.QCloud_APPID);
            if (appid < 1) { return CommonReturn.Failed("未配置APPID"); }
            string appkey = SMSConfig.Instance.QCloud_APPKey;
            List<string> param = new List<string>();
            //按顺序取值即可
            foreach (var item in packet.param)
            {
                param.Add(item.Value);
            }
            //if (mobiles.Length > 1)
            //{
            //    SmsMultiSender msender = new SmsMultiSender(appid, appkey);
            //    var result = msender.sendWithParam("86", mobiles, packet.tlpId, param.ToArray(), smsSign, "", "");
            //    if (result.result != 0) { return CommonReturn.Failed(result.errMsg); }
            //}
            //else
            //{
            //    SmsSingleSender ssender = new SmsSingleSender(appid, appkey);
            //    // 签名参数未提供或者为空时，会使用默认签名发送短信
            //    var result = ssender.sendWithParam("86", mobiles[0], packet.tlpId, param.ToArray(), smsSign, "", "");
            //    if (result.result != 0) { return CommonReturn.Failed(result.errMsg); }
            //}
            return CommonReturn.Success();
        }
    }
}
