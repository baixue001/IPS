using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;


namespace ZoomLaCMS.Models.Common
{
    public class VM_Recursion
    {
        /// <summary>
        /// 当前数据表
        /// </summary>
        public DataTable dt =null;
        /// <summary>
        /// 原始数据表
        /// </summary>
        public DataTable alldt = null;
        public int depth = 0;
        public IHtmlContent H_LineIcon(int depth)
        {
            string space = "<img src=\"/Images/TreeLineImages/tree_line4.gif\" />";
            string html = "<img src=\"/Images/TreeLineImages/t.gif\" />";
            while (depth > 0)
            {
                html = space + html;
                depth--;
            }
            return MvcHtmlString.Create(html);
        }
    }
}