using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoomLa.AppCode.Verify
{
    /// <summary>
    /// CMS内置验证码处理
    /// </summary>
    public class CMSCodeVerify : ICodeVerify
    {
        public static Dictionary<string, string> CodeDic = new Dictionary<string, string>();
        public bool Check(M_Comp_Verify model)
        {
            string key = model.sid;
            string s = model.token;
            bool flag = true;
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(s) || !CodeDic.ContainsKey(key) || string.IsNullOrEmpty(CodeDic[key])) { flag = false; }
            else if (!s.ToLower().Equals(CodeDic[key].ToLower()))
            {
                flag = false;
            }
            if (!string.IsNullOrEmpty(key) && CodeDic.ContainsKey(key)) { CodeDic.Remove(key); }
            return flag;
        }
    }
}
