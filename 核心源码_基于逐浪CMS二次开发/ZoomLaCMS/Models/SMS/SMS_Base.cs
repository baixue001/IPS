using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;

namespace ZoomLa.AppCode.SMS
{
    public abstract class SMS_Base
    {
        //依据模板发送短信
        public abstract CommonReturn Send(string[] mobiles, SMS_Packet packet);
        //查询余额
        public abstract CommonReturn QueryBalance();
    }
    public class SMS_Packet
    {
        public int tlpId = 0;
        //部分旧接口仅支持文本
        public string message = "";
        //占位符替换
        public Dictionary<string, string> param = new Dictionary<string, string>();
        //返回的结果
        public object r_result = null;
    }
}
