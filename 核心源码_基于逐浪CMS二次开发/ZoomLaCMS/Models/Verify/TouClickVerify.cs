using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.BLL.Third;
using ZoomLa.Model.Third;

namespace ZoomLa.AppCode.Verify
{
    public class TouClickVerify:ICodeVerify
    {
        public string PUBKEY = "3b72851f-fc66-42d9-8a8d-0caf9f08d750";//公钥
        public string PRIKEY = "7bcf4755-9f8c-4298-a985-6080fe7b0e7a";//私钥
        public TouClickVerify()
        {

        }
        public void Refresh()
        {
            M_Third_PlatInfo thirdMod = B_Third_PlatInfo.SelByFlag("点触");
            if (thirdMod != null)
            {
                PUBKEY = thirdMod.APPKey;
                PRIKEY = thirdMod.APPSecret;
            }
        }
        public bool Check(M_Comp_Verify model)
        {
            //string token = RequestEx["token"];
            ////一次验证传递的参数,同一次验证一样
            //string sid = RequestEx["sid"];
            //-----------
            //sid = "5d5c6ac5-a5cc-4cba-a760-12320b48ee4e";
            string checkAddress = "sverify-5-2-0";
            //TouClickSDk.TouClick t = new TouClickSDk.TouClick();
            //TouClickSDk.Status status = t.check(model.sid, checkAddress, model.token, PUBKEY, PRIKEY);
            //Console.Write("checkAddress :" + checkAddress + ",token:" + token + ",sid:" + sid);
            //Console.Write("code :" + status.Code + ",message:" + status.Message);

            //if (status != null && status.Code == 0)
            //{
            //    //执行自己的程序逻辑
            //    t.callback(checkAddress, model.sid, model.token, PUBKEY, PRIKEY);
            //    //return CommonReturn.Success(status.Message);
            //    return true;
            //}
            return false;
        }
    }
}
