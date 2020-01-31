using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;

namespace ZoomLa.AppCode.SMS
{
    public class SMS_Old : SMS_Base
    {
        public override CommonReturn QueryBalance()
        {
            string message = SendWebSMS.GetBalance();
            return CommonReturn.Success(message);
        }

        public override CommonReturn Send(string[] mobiles, SMS_Packet packet)
        {
            foreach (string mobile in mobiles)
            {
                if (string.IsNullOrEmpty(mobile)) { continue; }
                SendWebSMS.SendMessage(mobile, packet.message);
            }
            return CommonReturn.Success();
        }
    }
}
