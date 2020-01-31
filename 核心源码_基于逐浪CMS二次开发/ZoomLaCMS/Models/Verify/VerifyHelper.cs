using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoomLaCMS.Control;

namespace ZoomLa.AppCode.Verify
{
    public class VerifyHelper
    {
        public static TouClickVerify tcVerify = new TouClickVerify();
        private static CMSCodeVerify cmsVerify = new CMSCodeVerify();
        public static bool Check(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) { return false; }
                return Check(JsonConvert.DeserializeObject<M_Comp_Verify>(json));
            }
            catch (Exception ex) { throw new Exception(ex.Message+"|||"+json); }
        }
        public static bool Check(M_Comp_Verify model)
        {
            switch (model.vtype)
            {
                case 1:
                    return tcVerify.Check(model);
                case 0:
                default:
                    return cmsVerify.Check(model);
            }
        }
    }
}
