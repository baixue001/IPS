using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoomLaCMS.Control
{
    public class C_MBEditor
    {
        //是否显示验证码
        public bool ShowVCode = true;
        /// <summary>
        /// 显示的标题文字
        /// </summary>
        public string Title = "";
        /// <summary>
        /// 用于提交给后端的内容控件名称
        /// </summary>
        public string ValueControlId = "MB_Content";
        public string VCodeControlId = "VCode";
        //内容,用于修改,以boundary切割
        public string Content = "";
        //前后缀,预约
        public string Prefix = "";
        public string Suffix = "";
        #region JS事件
        //执行完判断后的JS回调方法,即将执行提交时
        public string CallBack { get { return On_Submit; } set { On_Submit = value; } }//兼容
        public string On_Submit = "";
        //内容发生变更时
        public string On_Change = "";
        #endregion
    }
}
