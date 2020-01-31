using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoomLa.AppCode.Verify
{
    public interface ICodeVerify
    {
        bool Check(M_Comp_Verify model);
    }
    public class M_Comp_Verify
    {
        public int vtype { get; set; }
        /// <summary>
        /// key或第三方的Sid
        /// </summary>
        public string sid { get; set; }
        /// <summary>
        /// 用户输入的验证码或第三方回发的Token
        /// </summary>
        public string token { get; set; }
    }
}
