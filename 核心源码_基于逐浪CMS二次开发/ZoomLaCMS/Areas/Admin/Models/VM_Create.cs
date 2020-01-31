using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.Common;
using ZoomLa.Model;

namespace ZoomLaCMS.Areas.Admin.Models
{
    public class VM_Create
    {
        public string GetNodeType(DataRow dr)
        {
            switch (Convert.ToInt32(dr["NodeType"]))
            {
                case 0:
                    return "根节点";
                case 1:
                    return "栏目节点";
                case 2:
                    return "单页节点";
                case 3:
                    return "外部链接";
                default:
                    return "";
            }
        }
        public IHtmlContent ShowIcon(DataRow dr)
        {
            int nid = DataConverter.CLng(dr["NodeID"]);
            string nodeName = DataConverter.CStr(dr["NodeName"]);
            string nodeDir = DataConverter.CStr(dr["NodeDir"]);
            int childCount = DataConverter.CLng(dr["ChildCount"]);
            nodeName = "" + nodeName + "[" + nodeDir + "]";
            string html = "<img src='/Images/TreeLineImages/t.gif' border='0'>";
            if (childCount > 0)
            {
                html += "<span data-type='icon' class='zi zi_folders'></span><span>" + nodeName + "</span>";
            }
            else
            {
                html += "<span>" + nodeName + "</span>";
            }
            return MvcHtmlString.Create(html);
        }
        public IHtmlContent GetOper(DataRow dr)
        {
            int NodeID = Convert.ToInt32(dr["NodeID"]);
            int NodeType = Convert.ToInt32(dr["NodeType"]);
            int ChildCount = Convert.ToInt32(dr["ChildCount"]);
            string viewHtml = "";
            if (NodeType == (int)NodeEnum.Node || NodeType == (int)NodeEnum.SPage)//仅适用于栏目节点
            {
                int tempcount = 0;//栏目数量
                string indexurl = "";//首栏目地址
                string viewTlp = "<li><a href='/Class_" + NodeID + "/{0}' target='_blank'>{1}</a></li>";
                if (!string.IsNullOrEmpty(dr["IndexTemplate"].ToString())) { viewHtml += string.Format(viewTlp, "Default", "首页"); tempcount++; indexurl = "Default"; }
                if (!string.IsNullOrEmpty(dr["ListTemplateFile"].ToString())) { viewHtml += string.Format(viewTlp, "NodePage", "列表"); tempcount++; indexurl = "NodePage"; }
                if (!string.IsNullOrEmpty(dr["LastinfoTemplate"].ToString())) { viewHtml += string.Format(viewTlp, "NodeNews", "最新"); tempcount++; indexurl = "NodeNews"; }
                if (!string.IsNullOrEmpty(dr["HotinfoTemplate"].ToString())) { viewHtml += string.Format(viewTlp, "NodeHot", "热门"); tempcount++; indexurl = "NodeHot"; }
                if (!string.IsNullOrEmpty(dr["ProposeTemplate"].ToString())) { viewHtml += string.Format(viewTlp, "NodeElite", "推荐"); tempcount++; indexurl = "NodeElite"; }
                if (!string.IsNullOrEmpty(viewHtml) && tempcount > 1)
                {
                    viewHtml = "  <div class='dropdown' style='display:inline;'><a class=' dropdown-toggle'href='javascript:;' data-toggle='dropdown' aria-expanded='false' title='浏览列表'><span class='zi zi_squareDown'></span>浏览</a> <ul class='dropdown-menu' role='menu'>" + viewHtml + "</ul></div>";
                }
                else if (tempcount > 0)
                {
                    viewHtml = "  <a href='/Class_" + NodeID + "/" + indexurl + "' target='_blank' title='浏览首页'><span class='zi zi_squareRight'></span>浏览</a>";
                }
            }
            if (NodeType == (int)NodeEnum.SPage)
            {
                viewHtml = "<a href='/Class_" + NodeID + "/Default'  target='_blank' class='option_style'><i class='zi zi_globe' title='浏览'></i>浏览</a>";
            }
            return MvcHtmlString.Create(viewHtml);
        }
    }
}
