using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ZoomLa.Components;

public class PageCommon
{
    public static string GetTlpDP(string name, bool ispre = false, string preurl = "")
    {
        name = HttpUtility.UrlEncode(name);
        string url = "/Admin/Template/TemplateEdit?setTemplate=/template/" + SiteConfig.SiteOption.TemplateName + "/&filepath=/";
        StringBuilder builder = new StringBuilder();
        builder.Append("<div class='btn-group Template_btn' data-bind='" + name + "' id='" + name + "_body'>");
        builder.Append("<div class='input-group sysTlpDP'>");
        builder.Append("<button type='button' class='form-control dropdown-toggle sysTlpDP_input' data-toggle='dropdown' aria-expanded='false'>");
        builder.Append("<span class='gray_9'><i class='zi zi_questioncircle'></i> 点击选择模板</span> <span class='pull-right'><span class='caret'></span></span></button>");
        builder.Append("<ul class='dropdown-menu Template_files' role='menu'></ul>");
        //builder.Append("<input type='hidden' id='Tlp_Hid' name='Tlp_Hid' />");
        builder.Append("<div class='input-group-append'>");
        builder.Append("<input type='button' value='手工绑定' class='input-group-text' onclick='Tlp_ShowSel(\"" + name + "\");' />");
        builder.Append("<a href='javascript:;' onclick='Tlp_EditHtml(\"" + url + "\",\"" + name + "\");' title='编辑模板' class='input-group-text'>编辑模板</a>");
        builder.Append("</div>");
        builder.Append("</div>");

        if (ispre)
        { builder.Append("<a href='" + preurl + "' target='_blank' style='margin-left:5px;'><i class='zi zi_eye'></i></a>"); }
        return builder.ToString();
    }
    public static int GetPageCount(int pageSize, int itemCount)
    {
        int pageCount = 0;
        if (pageSize > 0 && itemCount > 0)
            pageCount = itemCount / pageSize + ((itemCount % pageSize > 0) ? 1 : 0);
        return pageCount;
    }

    //==================================================
    private static string TlpRep(string hrefTlp, string query, int page, string text, string title = "")
    {
        return hrefTlp.Replace("@query", query).Replace("@page", page.ToString()).Replace("@text", text).Replace("@title", title);
    }
    public static string CreatePageHtml(HttpContext ctx, int pageCount, int cPage, int pcount = 10, string hrefTlp = "")
    {
        string queryString = ctx.Request.QueryString.ToString();
        //cpage text
        //string hrefTlp = "";
        string liTlp = "<li class=\"page-item {0}\">{1}</li>";
        if (string.IsNullOrEmpty(hrefTlp))
        {
            //hrefTlp = "<a href='@querypage=@page' title='@title'>@text</a>";
            hrefTlp = "<a class=\"page-link\" href='@querypage=@page' title='@title'>@text</a>";
        }
        int i = 1, maxPage = cPage / pcount > 0 ? pcount * (cPage / pcount) + pcount : pcount;
        string url = queryString.Contains("?") ? queryString + "&" : queryString + "?";
        string query = Regex.Split(url, "page=")[0];
        cPage = cPage < 1 ? 1 : (cPage > pageCount ? pageCount : cPage);
        string pageHtml = "<ul class='pagination'  style=\"align-item: center;\">"
                        + string.Format(liTlp, (cPage > 1 ? "" : "disabled"), TlpRep(hrefTlp, query, 1, "&laquo;"));
        //+ "<li class='page-item'" + (cPage > 1 ? "" : "class='disabled'") + ">" +  + "</li>";
        if (maxPage > pcount) { i = (maxPage / pcount) * pcount - pcount; }//处理i值,让i值保持准确
        if (cPage >= pcount)
            pageHtml += "<li class='page-item'>" + TlpRep(hrefTlp, query, (maxPage - pcount * 2), "...<span class='sr-only'>(current)</span>") + "</li>";
        for (; i <= pageCount && i < maxPage; i++)
        {
            pageHtml += string.Format(liTlp, (cPage != i ? "" : "active"), TlpRep(hrefTlp, query, i, i.ToString() + "<span class='sr-only'>(current)</span>"));
            //pageHtml += "<li class='page-item' " + (cPage != i ? "" : "class='active'") + ">" + TlpRep(hrefTlp, query, i, i.ToString() + "<span class='sr-only'>(current)</span>") + "</li>";
        }
        if (pageCount > maxPage)
        {
            pageHtml += "<li class='page-item'>" + TlpRep(hrefTlp, query, maxPage, "...<span class='sr-only'>(current)</span>") + "</li>";
        }
        pageHtml += "<li class='page-item'>" + TlpRep(hrefTlp, query, pageCount, "&raquo;", "尾页") + "</li></ul>";
        return pageHtml;
    }
}