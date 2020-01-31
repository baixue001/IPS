using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZoomLaCMS;

public class ComRE
{
    public static IHtmlContent Img_NoPic(string url, string css = "img_50")
    {
        string imgUrl = (url ?? "").ToLower();
        if (imgUrl.Contains("glyphicon ") || imgUrl.Contains("fa ")|| imgUrl.Contains("zi "))
        {
            return MvcHtmlString.Create(string.Format("<span class=\"{0}\"></span>", url));
        }
        return MvcHtmlString.Create(string.Format("<img src='{0}' class='" + css + "' onerror=\"this.error=null;this.src='/UploadFiles/nopic.svg';\" />", url));
    }
    public IHtmlContent Img_NoFace(string url, string css)
    {
        url = (url ?? "").ToLower();
        string tlp = "<img src='{0}' class='" + css + "' onerror=\"this.error=null;this.src='/Images/userface/noface.png';\" />";
        return MvcHtmlString.Create(string.Format(tlp, url));
    }
    /// <summary>
    /// 主用于别名与用户名的获取
    /// </summary>
    public static string GetNoEmptyStr(params string[] values)
    {
        foreach (string str in values)
        {
            if (!string.IsNullOrWhiteSpace(str)) return str;
        }
        return "";
    }
    public readonly static string Icon_OK = "<span style='color:green;' class='zi zi_check'></span>";
    public readonly static string Icon_Error = "<span style='color:red;' class='zi zi_times'></span>";
    public readonly static string Icon_Ban = "<span style='color:red;' class='zi zi_ban'></span>";
}