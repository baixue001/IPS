using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZoomLa.BLL;

namespace ZoomLaCMS.Models.Ask
{
    public class VM_AskInfo
    {
        B_Ask askBll = new B_Ask();
        B_GuestAnswer ansBll = new B_GuestAnswer();
        B_User buser = new B_User();
        /// <summary>
        /// 最佳回答采纳率
        /// </summary>
        public string Adoption = "0.00%";
        /// <summary>
        /// 已解决问题总数
        /// </summary>
        public int SolvedCount = 0;
        /// <summary>
        /// 待解决问题总数
        /// </summary>
        public int SolvingCount = 0;
        /// <summary>
        /// 用户总数
        /// </summary>
        public int UserCount = 0;
        public VM_AskInfo()
        {
            int ansSum = ansBll.getnum();
            Adoption = ansSum == 0 ? "0.00%" : (ansBll.IsExistInt(1) / ansSum * 100).ToString("0.00%");
            SolvedCount = askBll.IsExistInt("Status=2");
            SolvingCount = askBll.IsExistInt("Status=1");
            UserCount = buser.GetUserNameListTotal("");
        }
    }
}