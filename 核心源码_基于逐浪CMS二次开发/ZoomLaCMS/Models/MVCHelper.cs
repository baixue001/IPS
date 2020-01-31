using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ZoomLa.BLL;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS
{
    public class MVCHelper
    {
        private static IHtmlContent Raw(string html)
        {
            return new HtmlContentBuilder().AppendHtml(html);
        }
        /// <summary>
        /// 用于form表单
        /// </summary>
        /// <param name="action">要跳转的action,不带?号</param>
        /// <param name="Request">当前页面Request</param>
        /// <param name="param">需要附加的参数,不要以?或&开头</param>
        public static string GetAction(string action, HttpRequest Request, string param = "")
        {
            string query = "";
            if (Request.Query.Count > 0)
            {
                foreach (var item in Request.Query)
                {
                    query += string.Format("{0}={1}&", SafeSC.RemoveBadChar(item.Key), HttpUtility.UrlEncode(SafeSC.RemoveBadChar(Request.Query[item.Key])));
                }
                query = "?" + query.TrimEnd('&');
            }
            if (!string.IsNullOrEmpty(param))
            {
                query = string.IsNullOrEmpty(query) ? "?" + param : query + "&" + param;
            }
            return action + query;
        }
        public static IHtmlContent H_Radios(string name, string[] texts, string[] values, string selected = "", string attr = "")
        {
            string html = "";
            for (int i = 0; i < texts.Length; i++)
            {
                string text = texts[i];
                string value = values[i];
                string chked = "";
                if (string.IsNullOrEmpty(selected) && i == 0) { chked = "checked=\"checked\""; }
                else if (value.Equals(selected)) { chked = "checked=\"checked\""; }
                html += "<label><input type=\"radio\" name=\"" + name + "\" value=\"" + value + "\" " + chked + " " + attr + ">" + text + "</label>";
            }
            return  Raw(html);
        }
        public static IHtmlContent H_Radios(string name, DataTable dt, string vfield, string tfield, string selected = "")
        {
            string html = "";
            if (dt == null || dt.Rows.Count < 1) { return Raw("<div id=\"" + name + "\">无选项</div>"); }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                string text = DataConvert.CStr(dr[tfield]);
                string value = DataConvert.CStr(dr[vfield]);
                string chked = "";
                if (string.IsNullOrEmpty(selected) && i == 0) { chked = "checked=\"checked\""; }
                else if (value.Equals(selected)) { chked = "checked=\"checked\""; }
                html += "<label><input type=\"radio\" name=\"" + name + "\" value=\"" + value + "\" " + chked + ">" + text + "</label>";
            }
            return Raw(html);
        }
        public static IHtmlContent H_Check(string name, string text, string value, string init)
        {
            string html = string.Format("<input type='checkbox' id='{0}' name='{0}' value='{1}' {2} />",
                name, value, init.Equals(value) ? "checked='checked'" : "");
            html = "<label>" + html + text + "</label>";
            return Raw(html);
        }
        public static IHtmlContent H_Check(string name, bool check, object htmlAttributes)
        {
            var dictionary = ((IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes));
            string attrs = "";
            foreach (var item in dictionary)
            {
                attrs += string.Format(" {0}=\"{1}\" ", item.Key, item.Value);
            }
            string html = string.Format("<input type='checkbox' id='{0}' name='{0}' {1} {2}/>", name, (check ? "checked=\"checked\"" : ""), attrs);
            //html = "<label>" + html + text + "</label>";
            return Raw(html);
        }
        /// <summary>
        /// 生成CheckBoxList列表
        /// </summary>
        /// <param name="selected">1,2,3</param>
        /// <returns></returns>
        public static IHtmlContent H_Checks(string name, DataTable dt, string vField, string tField, string selected = "")
        {
            string html = "";
            string chkTlp = "<label><input type='checkbox' name='{0}' value='{1}' {3}/>{2}</label> ";
            foreach (DataRow dr in dt.Rows)
            {
                string value = DataConvert.CStr(dr[vField]);
                string chked = ("," + selected + ",").IndexOf(value) > -1 ? "checked=\"checked\"" : "";
                html += string.Format(chkTlp, name, value, dr[tField], chked);
            }
            return Raw(html);
        }
        public static SelectList ToSelectList(DataTable dt, string text, string value, string selected = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (DataRow dr in dt.Rows)
            {
                list.Add(new SelectListItem() { Text = DataConvert.CStr(dr[text]), Value = DataConvert.CStr(dr[value]) });
            }
            SelectList slist = new SelectList(list, "Value", "Text", selected);
            return slist;
        }
        public static SelectList ToSelectList(string[] textArr, string[] valueArr, string seled = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();
            for (int i = 0; i < textArr.Length; i++)
            {
                string text = textArr[i];
                string value = valueArr[i];
                list.Add(new SelectListItem() { Text = text, Value = value });
            }
            SelectList slist = new SelectList(list, "Value", "Text", seled);
            return slist;
        }
        public static string To(string name)
        {
            return "@" + name;
        }
    }
}
