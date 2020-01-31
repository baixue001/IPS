using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoomLa.BLL.Pay
{
    public class PayHelper
    {
        public static string BuildForm(string url, Dictionary<string, string> dics, string method = "post", string taraget = "_blank")
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<form id='payform' name='payform' action='" + url + "' method=\"" + method + "\" target=\"" + taraget + "\">");
            if (dics != null)//有些支付要求用链接直接跳转
            {
                foreach (var item in dics)
                {
                    builder.Append("<input type=\"hidden\" name=\"" + item.Key + "\" value=\"" + item.Value + "\"/>");
                }
            }
            builder.Append("<input type='submit' value='确认支付'></form>");
            return builder.ToString();
        }
    }
}
