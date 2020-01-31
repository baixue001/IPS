using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    public static class HttpRequestExtensions
    {
        public static string RawUrl(this HttpRequest request)
        {
            return new StringBuilder()
                 .Append(request.PathBase)
                 .Append(request.Path)
                 .Append(request.QueryString)
                 .ToString();
        }
        //https://domain
        public static string FullUrl(this HttpRequest request)
        {
            return new StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
        }
        public static bool IsAjax(this HttpRequest request)
        {
            bool result = false;

            var xreq = request.Headers.ContainsKey("x-requested-with");
            if (xreq)
            {
                string xml = request.Headers["x-requested-with"];
                result = (xml.Equals("XMLHttpRequest"));
            }

            return result;
        }
        public static bool IsAjaxRequest(this HttpRequest request) { return IsAjax(request); }
        //public static string QueryString(this HttpRequest request,string name) { return GetParam(request,name); }
        //public static string Form(this HttpRequest request, string name) { return GetParam(request, name); }
        /// <summary>
        /// 前端使用,取地址栏中值,避免XXS
        /// </summary>
        /// <returns></returns>
        public static string GetParam(this HttpRequest request,string name)
        {
            string value = "";
            try
            {
                if (request.Method.Equals("POST"))
                {
                    Microsoft.Extensions.Primitives.StringValues temp = new Microsoft.Extensions.Primitives.StringValues();
                    if (request.Form.TryGetValue(name, out temp)) { value = temp.ToString(); }
                }
            }
            catch { }
            if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(request.Query[name]))
            {
                value = request.Query[name];
            }
            return value;
        }

        public static string UserAgent(this HttpRequest request)
        {
            return request.Headers["User-Agent"];
        }
    }
}